using System;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

public class WebSocketUtils
{
    private ClientWebSocket _webSocket;
    public bool WebSocketStates;

    public async Task<bool> ConnectAsync(string uri)
    {
        _webSocket = new ClientWebSocket();

        try
        {
            Debug.Log("🔌 正在连接 WebSocket...");
            await _webSocket.ConnectAsync(new Uri(uri), CancellationToken.None);

            if (_webSocket.State == WebSocketState.Open)
            {
                Debug.Log("✅ WebSocket 连接成功");
                WebSocketStates = true;
                return true;
            }

            WebSocketStates = false;
            Debug.LogError("❌ WebSocket 连接失败: 未处于 Open 状态");
            return false;
        }
        catch (Exception ex)
        {
            Debug.LogError($"❌ WebSocket 连接错误: {ex.Message}");
            return false;
        }
    }

    public async Task SendMessageAsync(string message)
    {
        try
        {
            var jsonData = $"{{\"type\":\"userId\",\"userId\":\"{message}\"}}";
            var messageBuffer = Encoding.UTF8.GetBytes(jsonData);

            await _webSocket.SendAsync(new ArraySegment<byte>(messageBuffer), WebSocketMessageType.Text, true,
                CancellationToken.None);
            Debug.Log($"📤 发送消息: {message}");
        }
        catch (Exception ex)
        {
            Debug.LogError($"❌ 发送失败: {ex.Message}");
            throw;
        }
    }

    public async Task<string> ReceiveResponseAsync()
    {
        try
        {
            if (_webSocket == null || (_webSocket.State != WebSocketState.Open && _webSocket.State != WebSocketState.CloseReceived))
            {
                Debug.LogWarning($"⚠️ 无法接收消息，WebSocket 当前状态为: {_webSocket?.State}");
                return null;
            }
            
            var buffer = new byte[1024];
            var result = await _webSocket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);

            if (result.MessageType == WebSocketMessageType.Text)
            {
                var response = Encoding.UTF8.GetString(buffer, 0, result.Count);
                Debug.Log($"✅ 服务器响应: {response}");
                return response;
            }

            if (result.MessageType == WebSocketMessageType.Close || _webSocket.State == WebSocketState.CloseReceived)
            {
                Debug.Log("🔔 收到关闭请求或连接状态为 CloseReceived，开始关闭连接");
                await CloseConnectionAsync();
                return null;
            }

            return null;
        }
        catch (Exception ex)
        {
            Debug.LogError($"❌ 接收响应失败: {ex.Message}");
            throw; // 确保异常抛出
        }
    }

    public async Task SendCloseRequestAsync()
    {
        try
        {
            if (_webSocket != null && _webSocket.State == WebSocketState.Open)
            {
                var closeMessage = Encoding.UTF8.GetBytes("close");
                await _webSocket.SendAsync(new ArraySegment<byte>(closeMessage), WebSocketMessageType.Text, true,
                    CancellationToken.None);
                Debug.Log("📤 已发送关闭请求 ('close') 给服务器");
            }
            else
            {
                Debug.LogWarning($"⚠️ 关闭请求未发送，WebSocket 当前状态为: {_webSocket?.State}");
            }
        }
        catch (Exception ex)
        {
            Debug.LogError($"❌ 接收响应失败: {ex.Message}");
            throw; // 确保异常抛出
        }
    }

    public async Task CloseConnectionAsync()
    {
        if (_webSocket != null && (_webSocket.State == WebSocketState.Open || _webSocket.State == WebSocketState.CloseReceived))
        {
            try
            {
                await _webSocket.CloseAsync(WebSocketCloseStatus.NormalClosure, "Client closed", CancellationToken.None);
                Debug.Log("🔌 WebSocket 连接已关闭");
                WebSocketStates = false;
            }
            catch (Exception ex)
            {
                Debug.LogError($"❌ 关闭连接时发生错误: {ex.Message}");
            }
        }
        else
        {
            Debug.Log($"⚠️ WebSocket 无需关闭，当前状态: {_webSocket?.State}");
        }
    }
}