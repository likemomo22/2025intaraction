using System;
using System.Collections.Generic;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.UI;
using Debug = UnityEngine.Debug;

public class VideoAndLandMarksReceiver : MonoBehaviour
{
    private const int TextureWidth = 512;
    private const int TextureHeight = 512;
    public Button stopButton;

    public Renderer planeRenderer;

    //存储 idx 对应的游戏物体
    public Transform statusContainer0;
    public Transform statusContainer19;
    public Transform statusContainer20;

    private readonly int pointSize = 3;
    private EMGGraphManager _eMgGraphManager;

    private EnergyBarPositionController _energyBarPositionController;

    // private bool _isReceivingData;

    private Texture2D _videoTexture;
    // public BananaManPoseController bananaManPoseController;
    
    private WebSocketUtils _webSocketUtils;

    private async void Start()
    {
        stopButton.onClick.AddListener(StopWebSocket);
        _webSocketUtils = new WebSocketUtils();

        await StartReceiving();
    }
    
    // private byte[] FromBase64String(string base64String)
    // {
    //     try
    //     {
    //         return Convert.FromBase64String(base64String);
    //     }
    //     catch (Exception e)
    //     {
    //         Debug.LogError($"Base64 解码失败: {e.Message}");
    //         return null;
    //     }
    // }

    private async void OnDestroy()
    {
        if (_webSocketUtils.WebSocketIsOpened)
        {
            await _webSocketUtils.SendCloseRequestAsync();
        }
        // await StopReceiving();
    }
    
    private async void OnDisable()
    {
        if (_webSocketUtils.WebSocketIsOpened)
        {
            await _webSocketUtils.SendCloseRequestAsync();
        }
        // await StopReceiving();
    }

    private async void StopWebSocket()
    {
        if (_webSocketUtils.WebSocketIsOpened) await _webSocketUtils.SendCloseRequestAsync();
    }

    public async Task StartReceiving()
    {

        //初始化绘制纹理
        // **确保 Texture2D 采用 RGBA32，支持透明度**
        _videoTexture = new Texture2D(TextureWidth, TextureHeight, TextureFormat.RGBA32, false);
        planeRenderer.material.mainTexture = _videoTexture;

        // SetMaterialToTransparent();

        //获取 EnergyBarController 组件
        _energyBarPositionController = GetComponent<EnergyBarPositionController>();

        //获取 EMGGraphManager 组件
        _eMgGraphManager = FindObjectOfType<EMGGraphManager>();

        //初始化 idx -> GameObject 映射
        _energyBarPositionController.statusContainers = new Dictionary<int, Transform>
        {
            { 12, statusContainer0 },
            { 14, statusContainer19 },
            { 10, statusContainer20 }
        };
        
        
        try
        {
            var isConnected = await _webSocketUtils.ConnectAsync("ws://127.0.0.1:8000/ws");
            if (isConnected) 
            {
                await _webSocketUtils.ReceiveLoopAsync(ProcessReceivedData, 1024 * 600);
            }
        }
        catch (Exception ex)
        {
            Debug.LogError($"❌ 失败: {ex.Message}");
        }


        // await ReceiveData();
    }

    private void ProcessReceivedData(string jsonString)
    {
        if (string.IsNullOrEmpty(jsonString))
        {
            Debug.LogError("⚠️ JSON 数据为空，无法解析！");
            return;
        }
        if (_videoTexture == null)
        {
            Debug.LogWarning("⚠️ _videoTexture 为空，重新初始化！");
            _videoTexture = new Texture2D(TextureWidth, TextureHeight, TextureFormat.RGBA32, false);
            planeRenderer.material.mainTexture = _videoTexture;
        }
        //解析 JSON
        var combinedData = JsonUtility.FromJson<CombinedData>(jsonString);
        var videoData = FromHexString(combinedData.video);

        DrawLandmarks(combinedData.landmarks, videoData);
        _eMgGraphManager.UpdateGraphs(combinedData.emgDatas);
    }

    private void LoadTransparentTexture(byte[] videoData)
    {
        if (videoData == null || videoData.Length == 0)
        {
            Debug.LogWarning("Received empty video data.");
            return;
        }

        // **优化：只更新 `videoTexture`，不重新创建**
        if (_videoTexture.LoadImage(videoData)) _videoTexture.Apply();
    }

    private void SetMaterialToTransparent()
    {
        var mat = planeRenderer.material;

        // **切换到 Transparent 模式**
        mat.SetFloat("_Mode", 3);
        mat.SetInt("_SrcBlend", (int)BlendMode.SrcAlpha);
        mat.SetInt("_DstBlend", (int)BlendMode.OneMinusSrcAlpha);
        mat.SetInt("_ZWrite", 0);
        mat.DisableKeyword("_ALPHATEST_ON");
        mat.EnableKeyword("_ALPHABLEND_ON");
        mat.DisableKeyword("_ALPHAPREMULTIPLY_ON");
        mat.renderQueue = 3000;

        // **保持 Alpha 透明**
        var matColor = mat.color;
        matColor.a = 1f;
        mat.color = matColor;
    }

    private void DrawLandmarks(Landmark[] landmarks, byte[] videoData)
    {
        _videoTexture.LoadImage(videoData);
        var landmarkDict = new Dictionary<int, Landmark>();
        foreach (var landmark in landmarks) landmarkDict[landmark.idx] = landmark;

        foreach (var landmark in landmarks)
        {
            var idx = landmark.idx;
            var x = landmark.x;
            var y = landmark.y;

            // 处理14，10 的转换
            if (idx == 14 && landmarkDict.ContainsKey(16))
            {
                x = (landmark.x + landmarkDict[16].x) / 2;
                y = (landmark.y + landmarkDict[16].y) / 2;
            }
            else if (idx == 10 && landmarkDict.ContainsKey(12))
            {
                x = (landmark.x + landmarkDict[12].x) / 2;
                y = (landmark.y + landmarkDict[12].y) / 2;
            }

            var pixelX = Mathf.RoundToInt(x * _videoTexture.width);
            var pixelY = Mathf.RoundToInt((1.0f - y) * _videoTexture.height);

            DrawPoint(idx, pixelX, pixelY, Color.red);
        }

        _videoTexture.Apply();
    }

    private void DrawPoint(int idx, int x, int y, Color color)
    {
        if (x < 0 || x >= _videoTexture.width || y < 0 || y >= _videoTexture.height) return;

        for (var offsetX = -pointSize; offsetX <= pointSize; offsetX++)
        for (var offsetY = -pointSize; offsetY <= pointSize; offsetY++)
        {
            var px = x + offsetX;
            var py = y + offsetY;

            if (px >= 0 && px < _videoTexture.width && py >= 0 && py < _videoTexture.height)
                if (_energyBarPositionController.statusContainers.ContainsKey(idx) &&
                    _energyBarPositionController.statusContainers[idx] != null)
                {
                    var localPos = ConvertPixelToLocal(px, py);
                    _energyBarPositionController.UpdatePositionInPlane(idx, localPos);
                }
            // videoTexture.SetPixel(px, py, color);
        }
    }

    private Vector3 ConvertPixelToLocal(int px, int py)
    {
        // 获取 `Plane` 真实大小，避免 `Scale` 影响坐标计算
        var planeBounds = planeRenderer.bounds;
        var planeWidth = planeBounds.size.x; // 真实 X 轴宽度
        var planeHeight = planeBounds.size.y; // 真实 Z 轴高度

        // 归一化像素坐标 (0 - 1)
        var normalizedX = (float)px / _videoTexture.width;
        var normalizedY = (float)py / _videoTexture.height;

        // 计算 `localPosition`（在 `Plane` 内）
        var localX = (0.5f - normalizedX) * planeWidth;
        var localZ = (0.5f - normalizedY) * planeHeight; //用 `Z` 代替 `Y`

        return new Vector3(localX, 1, localZ);
    }


    private byte[] FromHexString(string hexString)
    {
        if (string.IsNullOrEmpty(hexString) || hexString.Length % 2 != 0)
        {
            Debug.LogError("❌ 无效的十六进制字符串！");
            return new byte[0];
        }

        try
        {
            var bytes = new byte[hexString.Length / 2];
            for (var i = 0; i < bytes.Length; i++)
            {
                var byteValue = hexString.Substring(i * 2, 2);
                bytes[i] = Convert.ToByte(byteValue, 16);
            }
            return bytes;
        }
        catch (Exception ex)
        {
            Debug.LogError($"❌ HexString 转换失败: {ex.Message}");
            return new byte[0];
        }
    }
}


[Serializable]
public class CombinedData
{
    public Landmark[] landmarks;
    public string video;
    public float[] emgDatas;
}

[Serializable]
public class LandmarkData
{
    public Landmark[] landmarks;
}

[Serializable]
public class Landmark
{
    public int idx;
    public float x;
    public float y;
    public float z;
}