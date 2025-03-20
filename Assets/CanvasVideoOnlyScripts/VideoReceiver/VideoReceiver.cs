using System;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.UI;

public class VideoReceiver : MonoBehaviour
{
    public Button toGameCanvasButton;

    private const int TextureWidth = 512;
    private const int TextureHeight = 512;
    public Renderer planeRenderer;
    private Texture2D _videoTexture;

    private WebSocketUtils _webSocketUtils;

    private async void Start()
    {
        toGameCanvasButton.onClick.AddListener(StopWebSocket);
        
        _webSocketUtils = new WebSocketUtils();
        
        //初始化绘制纹理
        // **确保 Texture2D 采用 RGBA32，支持透明度**
        _videoTexture = new Texture2D(TextureWidth, TextureHeight, TextureFormat.RGBA32, false);
        planeRenderer.material.mainTexture = _videoTexture;

        // SetMaterialToTransparent();

        await StartReceiving();
    }

    private async void  StopWebSocket()
    {
        if (_webSocketUtils.WebSocketIsOpened)
        {
            await _webSocketUtils.SendCloseRequestAsync();
        }
    }

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
    public async Task StartReceiving()
    {
        try
        {
            var isConnected = await _webSocketUtils.ConnectAsync("ws://127.0.0.1:8000/getVideoData");

            if (isConnected)
            {
                await _webSocketUtils.ReceiveLoopAsync(ProcessReceivedData, 1024 * 512);
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
        //解析 JSON
        var combinedData = JsonUtility.FromJson<CombinedData>(jsonString);

        var videoData = FromHexString(combinedData.video);

        //背景透明化
        // LoadTransparentTexture(videoData);
        //在纹理上绘制点
        DrawLandmarks(videoData);
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

    private void DrawLandmarks(byte[] videoData)
    {
        _videoTexture.LoadImage(videoData);
        _videoTexture.Apply();
    }

    private byte[] FromHexString(string hexString)
    {
        if (hexString.Length % 2 != 0)
            throw new ArgumentException("Invalid hex string length");

        var bytes = new byte[hexString.Length / 2];
        for (var i = 0; i < bytes.Length; i++)
        {
            var byteValue = hexString.Substring(i * 2, 2);
            bytes[i] = Convert.ToByte(byteValue, 16);
        }

        return bytes;
    }
}

[Serializable]
public class VideoData
{
    public string video;
}