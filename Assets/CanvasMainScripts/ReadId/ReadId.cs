using System;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

public class ReadId : MonoBehaviour
{
    public Button confirmButton;
    public InputField userIdInput;
    private WebSocketUtils _webSocketUtils;

    private void Start()
    {
        _webSocketUtils = new WebSocketUtils();
        confirmButton.onClick.AddListener(OnConfirmButtonClicked);
    }

    private async void OnConfirmButtonClicked()
    {
        var userId = userIdInput.text.Trim();
        if (string.IsNullOrEmpty(userId))
        {
            Debug.LogWarning("⚠️ 请输入用户 ID");
            return;
        }

        await SendUserId(userId);
    }

    private async Task SendUserId(string userId)
    {
        try
        {
            var isConnected = await _webSocketUtils.ConnectAsync("ws://127.0.0.1:8000/sendUserId");

            if (isConnected)
            {
                //1. 发送消息
                await _webSocketUtils.SendMessageAsync(userId);
                //2. 接收消息
                var response = await _webSocketUtils.ReceiveResponseAsync();
                //3. 发送关闭请求
                await _webSocketUtils.SendCloseRequestAsync();
                //4. 等待服务器确认关闭
                await _webSocketUtils.ReceiveResponseAsync();
            }
        }
        catch (Exception ex)
        {
            Debug.LogError($"❌ 发送失败: {ex.Message}");
        }
        finally
        {
            if (_webSocketUtils.WebSocketStates)
                await _webSocketUtils.CloseConnectionAsync();
        }
    }
}