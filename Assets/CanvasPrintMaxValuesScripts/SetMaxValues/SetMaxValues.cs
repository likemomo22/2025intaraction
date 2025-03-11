
    using System;
    using System.Collections;
    using System.Net.WebSockets;
    using System.Text;
    using System.Threading;
    using System.Threading.Tasks;
    using Newtonsoft.Json;
    using UnityEngine;
    using UnityEngine.UI;

    public class SetMaxValues:MonoBehaviour
    {
        public Button confirmButton;
        public InputField userIdToGetMaxVInput;
        private ClientWebSocket _webSocket;
        private WebSocketUtils _webSocketUtils;

        public static float MaxChannel1 { get;  set; }
        public static float MaxChannel2 { get;  set; }
        public static float MaxChannel3 { get;  set; }

        private void Start()
        {
            _webSocketUtils = new WebSocketUtils();
            confirmButton.onClick.RemoveAllListeners();
            confirmButton.onClick.AddListener(OnConfirmButtonClicked);
        }

        private async void OnConfirmButtonClicked()
        {
            var userIdToGetMaxV = userIdToGetMaxVInput.text.Trim();
            if (string.IsNullOrEmpty(userIdToGetMaxV))
            {
                Debug.LogWarning("⚠️ 请输入用户 ID");
                return;
            }
            
            await CalculateMaxValue(userIdToGetMaxV);
        }
        private async Task CalculateMaxValue(string userIdToGetMaxV)
        {
            try
            {
                var isConnected = await _webSocketUtils.ConnectAsync("ws://127.0.0.1:8000/setMaxValue");
                
                if (isConnected)
                {
                    await _webSocketUtils.SendMessageAsync(new{type="userIdToGetMaxV",userId=userIdToGetMaxV});
                    // 接收确认消息
                    while (true)
                    {
                        var response = await _webSocketUtils.ReceiveResponseAsync();
                        if(!_webSocketUtils.WebSocketStates) break;
                        var message = JsonConvert.DeserializeObject<ResponseMessage>(response);
                        if (message.Type == "data")
                        {
                        ProcessReceivedData(message.Data);
                        }
                        
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.LogError($"❌ 发送失败: {ex.Message}");
            }
        }

        private void ProcessReceivedData(DataContent maxValueEachCh)
        {
            try
            {
                // 存储最大值到静态变量
                MaxChannel1 = maxValueEachCh.Channel1;
                MaxChannel2 = maxValueEachCh.Channel2;
                MaxChannel3 = maxValueEachCh.Channel3;

                Debug.Log($"🎯 最大值存储完成: Channel1={MaxChannel1}, Channel2={MaxChannel2}, Channel3={MaxChannel3}");
            }
            catch (Exception e)
            {
                Debug.LogError($"解析数据错误: {e.Message}");
            }
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
    
    public class ResponseMessage
    {
        public string Type { get; set; }
        public DataContent Data { get; set; }
    }

    public class DataContent
    {
        public float Channel1 { get; set; }
        public float Channel2 { get; set; }
        public float Channel3 { get; set; }
    }
    
