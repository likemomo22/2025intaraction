using System;
using System.Net.WebSockets;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

public class CameraDataReceiver : MonoBehaviour
{
    public Renderer planeRenderer;  // 将平面对象的Renderer组件拖入此处
    private ClientWebSocket webSocket;
    private Texture2D receivedTexture;

    async void Start()
    {
        // 初始化一个空的纹理，用于显示接收到的图像
        receivedTexture = new Texture2D(2, 2);

        // 创建 WebSocket 连接
        webSocket = new ClientWebSocket();
        await webSocket.ConnectAsync(new Uri("ws://127.0.0.1:8000/ws"), CancellationToken.None);
        Debug.Log("Connected to FastAPI WebSocket");

        // 开始接收数据
        await ReceiveData();
    }

    private async Task ReceiveData()
    {
        byte[] buffer = new byte[1024 * 1024];  // 假设单帧图像不超过1MB

        while (webSocket.State == WebSocketState.Open)
        {
            var result = await webSocket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);

            if (result.MessageType == WebSocketMessageType.Binary)
            {
                // 将接收到的字节数据加载为纹理
                receivedTexture.LoadImage(buffer);
                receivedTexture.Apply();

                // 将纹理应用到平面的材质
                planeRenderer.material.mainTexture = receivedTexture;
            }
            else if (result.MessageType == WebSocketMessageType.Close)
            {
                await webSocket.CloseAsync(WebSocketCloseStatus.NormalClosure, string.Empty, CancellationToken.None);
                Debug.Log("WebSocket closed");
            }

            await Task.Delay(100);  // 等待一小段时间后继续接收
        }
    }

    private void OnDestroy()
    {
        webSocket?.Dispose();
    }
}