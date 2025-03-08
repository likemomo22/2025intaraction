using System;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

public class MPLandmarksReceiver : MonoBehaviour
{

    public Renderer planeRenderer;
    private ClientWebSocket webSocket;
    private Texture2D drawTexture;
    private Color[] clearColor;

    private const int textureWidth = 512;
    private const int textureHeight = 512;
    async void Start()
    {
        webSocket=new ClientWebSocket();
        await webSocket.ConnectAsync(new Uri("ws://127.0.0.1:8000/ws"),CancellationToken.None);
        Debug.Log("Connected to FastAPI WebSocket");
        
        //初始化绘制纹理
        drawTexture = new Texture2D(textureWidth, textureHeight, TextureFormat.RGBA32, false);
        clearColor = new Color[textureWidth * textureHeight];
        for (int i = 0; i < clearColor.Length; i++)
        {
            clearColor[i]=Color.clear;
        }

        planeRenderer.material.mainTexture = drawTexture;

        await ReceiveData();
    }

    private async Task ReceiveData()
    {
        byte[] buffer = new byte[1024 * 1024];

        while (webSocket.State == WebSocketState.Open)
        {
            var result = await webSocket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);

            if (result.MessageType == WebSocketMessageType.Text)
            {
                string jsonString = Encoding.UTF8.GetString(buffer, 0, result.Count);
                Debug.Log("Received JSON: "+jsonString);

                LandmarkData landmarkData = JsonUtility.FromJson<LandmarkData>(jsonString);

                //在纹理上绘制点
                DrawLandmarks(landmarkData.landmarks);
                // foreach (var landmark in landmarkData.landmarks)
                // {
                //     Debug.Log($"Landmark {landmark.idx}: x={landmark.x}, y={landmark.y}, z={landmark.z}");
                // }
            }
            else if (result.MessageType == WebSocketMessageType.Close)
            {
                await webSocket.CloseAsync(WebSocketCloseStatus.NormalClosure, string.Empty, CancellationToken.None);
                Debug.Log("WebSocket closed");
            }

            await Task.Delay(100);
        }
        
        throw new NotImplementedException();
    }

    private void DrawLandmarks(Landmark[] landmarks)
    {
        drawTexture.SetPixels(clearColor);

        foreach (var landmark in landmarks)
        {
            int x = Mathf.RoundToInt(landmark.x * textureWidth);
            int y = Mathf.RoundToInt(landmark.y * textureHeight);
            
            DrawPoint(x,y,Color.red);
        }
        drawTexture.Apply();
    }

    private int pointSize = 3;
    private void DrawPoint(int x, int y,Color color)
    {
       if(x<0||x>=textureWidth||y<0||y>=textureHeight) return;

       for (int offsetX = -pointSize; offsetX <= pointSize; offsetX++)
       {
           for (int offsetY = -pointSize; offsetY <= pointSize; offsetY++)
           {
               int px = x + offsetX;
               int py = y + offsetY;

               if (px >= 0 && px < textureWidth && py >= 0 && py < textureHeight)
               {
                   drawTexture.SetPixel(px, py, color);
               }
           }
       }
    }
}

