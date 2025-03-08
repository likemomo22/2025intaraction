using System;
using System.Collections.Generic;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

public class VideoReceiver : MonoBehaviour
{
    public Renderer planeRenderer;
    // public BananaManPoseController bananaManPoseController;

    private ClientWebSocket _webSocket;
    private Texture2D _videoTexture;

    private const int TextureWidth = 512;
    private const int TextureHeight = 512;

    private void Awake()
    {
  
    }

   public async void StartReceiving()
   {

       
        _webSocket=new ClientWebSocket();
        await _webSocket.ConnectAsync(new Uri("ws://127.0.0.1:8000/getVideoData"),CancellationToken.None);
        Debug.Log("Connected to FastAPI WebSocket");
        
        //初始化绘制纹理
        // **确保 Texture2D 采用 RGBA32，支持透明度**
        _videoTexture = new Texture2D(TextureWidth, TextureHeight, TextureFormat.RGBA32, false);
        planeRenderer.material.mainTexture = _videoTexture;

        SetMaterialToTransparent();
        
        await ReceiveData();
    }

    private async Task ReceiveData()
    {
        byte[] buffer = new byte[1024 * 512];

        while (_webSocket.State == WebSocketState.Open)
        {
            var result = await _webSocket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);

            if (result.MessageType == WebSocketMessageType.Text)
            {
                string jsonString = Encoding.UTF8.GetString(buffer, 0, result.Count);

                //解析 JSON
                CombinedData combinedData=JsonUtility.FromJson<CombinedData>(jsonString);

                byte[] videoData = FromHexString(combinedData.video);
                
                //背景透明化
                LoadTransparentTexture(videoData);
                //在纹理上绘制点
                DrawLandmarks(videoData);
                // 传递数据给 Banana Man 让其调整姿势
                // if (bananaManPoseController != null)
                // {
                //     bananaManPoseController.UpdatePose(combinedData.landmarks);
                // }
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

    private void DrawLandmarks(byte[] videoData)
    {
        _videoTexture.LoadImage(videoData);
        _videoTexture.Apply();
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
public class VideoData
{
    public string video;
}
