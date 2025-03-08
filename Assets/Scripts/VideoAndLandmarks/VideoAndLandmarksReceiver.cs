using System;
using System.Collections.Generic;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

public class VideoAndLandMarksReceiver : MonoBehaviour
{

    public Renderer planeRenderer;
    // public BananaManPoseController bananaManPoseController;

    private ClientWebSocket _webSocket;
    private Texture2D _videoTexture;

    private const int TextureWidth = 512;
    private const int TextureHeight = 512;

    private EnergyBarPositionController _energyBarPositionController;
    private EMGGraphManager _eMgGraphManager;
    
    //存储 idx 对应的游戏物体
    public Transform statusContainer0;
    public Transform statusContainer19;
    public Transform statusContainer20;
    
    private bool _isReceivingData=false;

    private void Awake()
    {
        _isReceivingData = false;
    }

   public   async void StartReceiving()
   {
       if (_isReceivingData) return;
       _isReceivingData = true;
       
        _webSocket=new ClientWebSocket();
        await _webSocket.ConnectAsync(new Uri("ws://127.0.0.1:8000/ws"),CancellationToken.None);
        Debug.Log("Connected to FastAPI WebSocket");
        
        //初始化绘制纹理
        // **确保 Texture2D 采用 RGBA32，支持透明度**
        _videoTexture = new Texture2D(TextureWidth, TextureHeight, TextureFormat.RGBA32, false);
        planeRenderer.material.mainTexture = _videoTexture;
        
        // SetMaterialToTransparent();
        
        //获取 EnergyBarController 组件
        _energyBarPositionController = GetComponent<EnergyBarPositionController>();
        
        //获取 EMGGraphManager 组件
        _eMgGraphManager=FindObjectOfType<EMGGraphManager>();
        
        //初始化 idx -> GameObject 映射
        _energyBarPositionController.statusContainers = new Dictionary<int, Transform>
        {
            { 12, statusContainer0 },
            { 14, statusContainer19 },
            { 10, statusContainer20 }
        };

        
        await ReceiveData();
    }

    private async Task ReceiveData()
    {
        byte[] buffer = new byte[1024 * 600];

        while (_webSocket.State == WebSocketState.Open)
        {
            var result = await _webSocket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);
            // Debug.Log($"Received message size: {result.Count} bytes");

            if (result.MessageType == WebSocketMessageType.Text)
            {
                string jsonString = Encoding.UTF8.GetString(buffer, 0, result.Count);

                //解析 JSON
                CombinedData combinedData=JsonUtility.FromJson<CombinedData>(jsonString);

                byte[] videoData = FromHexString(combinedData.video);
                // byte[] videoData = FromBase64String(combinedData.video);

                //背景透明化
                // LoadTransparentTexture(videoData);
                //在纹理上绘制点
                DrawLandmarks(combinedData.landmarks,videoData);
                // 传递数据给 Banana Man 让其调整姿势
                // if (bananaManPoseController != null)
                // {
                //     bananaManPoseController.UpdatePose(combinedData.landmarks);
                // }
                _eMgGraphManager.UpdateGraphs(combinedData.emgDatas);
                
            }
            else if (result.MessageType == WebSocketMessageType.Close)
            {
                await _webSocket.CloseAsync(WebSocketCloseStatus.NormalClosure, string.Empty, CancellationToken.None);
                Debug.Log("WebSocket closed");
            }

            
        }
    }

    private void LoadTransparentTexture(byte[] videoData)
    {
        if (videoData == null || videoData.Length == 0)
        {
            Debug.LogWarning("Received empty video data.");
            return;
        }

        // **优化：只更新 `videoTexture`，不重新创建**
        if (_videoTexture.LoadImage(videoData))
        {
            _videoTexture.Apply();
        }
    }

    private void SetMaterialToTransparent()
    {
        Material mat = planeRenderer.material;
        
        // **切换到 Transparent 模式**
        mat.SetFloat("_Mode", 3);
        mat.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
        mat.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
        mat.SetInt("_ZWrite", 0);
        mat.DisableKeyword("_ALPHATEST_ON");
        mat.EnableKeyword("_ALPHABLEND_ON");
        mat.DisableKeyword("_ALPHAPREMULTIPLY_ON");
        mat.renderQueue = 3000;

        // **保持 Alpha 透明**
        Color matColor = mat.color;
        matColor.a = 1f;
        mat.color = matColor;
    }

    private void DrawLandmarks(Landmark[] landmarks, byte[] videoData)
    {
        _videoTexture.LoadImage(videoData);
        Dictionary<int, Landmark> landmarkDict = new Dictionary<int, Landmark>();
        foreach (var landmark in landmarks)
        {
            landmarkDict[landmark.idx] = landmark;
        }

        foreach (var landmark in landmarks)
        {
            int idx = landmark.idx;
            float x = landmark.x;
            float y = landmark.y;
        
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

            int pixelX = Mathf.RoundToInt(x * _videoTexture.width);
            int pixelY = Mathf.RoundToInt((1.0f - y) * _videoTexture.height);
        
            DrawPoint(idx, pixelX, pixelY, Color.red);
        }

        _videoTexture.Apply();
    }

    private int pointSize = 3;
    private void DrawPoint(int idx,int x, int y,Color color)
    {
       if(x<0||x>=_videoTexture.width||y<0||y>=_videoTexture.height) return;

       for (int offsetX = -pointSize; offsetX <= pointSize; offsetX++)
       {
           for (int offsetY = -pointSize; offsetY <= pointSize; offsetY++)
           {
               int px = x + offsetX;
               int py = y + offsetY;

               if (px >= 0 && px < _videoTexture.width && py >= 0 && py < _videoTexture.height)
               {
                   if (_energyBarPositionController.statusContainers.ContainsKey(idx)&&_energyBarPositionController.statusContainers[idx]!=null)
                   {
                       Vector3 localPos = ConvertPixelToLocal(px, py);
                       _energyBarPositionController.UpdatePositionInPlane(idx,localPos);
                   }

                   // videoTexture.SetPixel(px, py, color);
               }
           }
       }
    }

    private Vector3 ConvertPixelToLocal(int px, int py)
    {
        // 获取 `Plane` 真实大小，避免 `Scale` 影响坐标计算
        Bounds planeBounds = planeRenderer.bounds;
        float planeWidth = planeBounds.size.x;  // 真实 X 轴宽度
        float planeHeight = planeBounds.size.y; // 真实 Z 轴高度

        // 归一化像素坐标 (0 - 1)
        float normalizedX = (float)px / _videoTexture.width;
        float normalizedY = (float)py / _videoTexture.height;

        // 计算 `localPosition`（在 `Plane` 内）
        float localX = (0.5f-normalizedX) * planeWidth ;
        float localZ = (0.5f - normalizedY) * planeHeight ;  //用 `Z` 代替 `Y`

        return new Vector3(localX, 1, localZ);
    }

    
    private byte[] FromHexString(string hexString)
    {
        if (hexString.Length % 2 != 0)
            throw new ArgumentException("Invalid hex string length");
    
        byte[] bytes = new byte[hexString.Length / 2];
        for (int i = 0; i < bytes.Length; i++)
        {
            string byteValue = hexString.Substring(i * 2, 2);
            bytes[i] = Convert.ToByte(byteValue, 16);
        }
        return bytes;
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
        if (_webSocket != null && _webSocket.State == WebSocketState.Open)
        {
            await _webSocket.CloseAsync(WebSocketCloseStatus.NormalClosure, "Clint closed", CancellationToken.None);
        }
    }
    private async void OnDisable()
    {
        if (_webSocket != null && _webSocket.State == WebSocketState.Open)
        {
            await _webSocket.CloseAsync(WebSocketCloseStatus.NormalClosure, "Clint closed", CancellationToken.None);
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
