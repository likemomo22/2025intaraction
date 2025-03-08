using System;
using System.Collections;
using System.Collections.Generic;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

public class ReadId : MonoBehaviour
{
    public Button confirmButton;
    public InputField userIdInput;
    private ClientWebSocket _webSocket;

    private void Start()
    {
        confirmButton.onClick.AddListener(() => StartCoroutine(SendUserIdCoroutine()));
    }

    private IEnumerator SendUserIdCoroutine()
    {
        var userId = userIdInput.text.Trim();
        if (string.IsNullOrEmpty(userId))
        {
            Debug.LogWarning("⚠️ 请输入用户 ID");
            yield break;
        }
        
        Task sendTask = SendUserId(userId);
        yield return new WaitUntil(() => sendTask.IsCompleted);
    }

    private async Task SendUserId(string userId)
    {
        try
        {
            _webSocket = new ClientWebSocket();
            Debug.Log("🔌 正在连接 WebSocket...");
            await _webSocket.ConnectAsync(new Uri("ws://127.0.0.1:8000/sendUserId"), CancellationToken.None);
            Debug.Log("✅ WebSocket 连接成功");

            if (_webSocket.State == WebSocketState.Open)
            {
                await SendUserIdMessage(userId);
                await ReceiveResponse();
            }
        }
        catch (Exception ex)
        {
            Debug.LogError($"❌ 发送失败: {ex.Message}");
        }
    }

    private async Task SendUserIdMessage(string userId)
    {
        string jsonData = $"{{\"type\":\"userId\",\"userId\":\"{userId}\"}}";

        byte[] bytes = Encoding.UTF8.GetBytes(jsonData);
        
        await _webSocket.SendAsync(new ArraySegment<byte>(bytes), WebSocketMessageType.Text, true, CancellationToken.None);
        Debug.Log($"📤 已发送用户 ID: {userId}");
    }

    private async Task ReceiveResponse()
    {
        byte[] buffer = new byte[1024];
        var result = await _webSocket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);

        if (result.MessageType == WebSocketMessageType.Text)
        {
            string response = Encoding.UTF8.GetString(buffer, 0, result.Count);
            Debug.Log($"✅ 服务器响应: {response}");

            // **发送 "close" 让服务器端主动关闭**
            byte[] closeMessage = Encoding.UTF8.GetBytes("close");
            await _webSocket.SendAsync(new ArraySegment<byte>(closeMessage), WebSocketMessageType.Text, true, CancellationToken.None);
        }

        // 关闭 WebSocket
        await _webSocket.CloseAsync(WebSocketCloseStatus.NormalClosure, "Client closed", CancellationToken.None);
        Debug.Log("🔌 WebSocket 连接已关闭");
    }
}
