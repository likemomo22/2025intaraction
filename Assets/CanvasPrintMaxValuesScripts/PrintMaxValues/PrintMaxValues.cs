
    using System;
    using System.Collections;
    using System.Net.WebSockets;
    using System.Text;
    using System.Threading;
    using System.Threading.Tasks;
    using UnityEngine;
    using UnityEngine.UI;

    public class PrintMaxValues:MonoBehaviour
    {
        public Button confirmButton;
        public InputField userIdToGetMaxVInput;
        private ClientWebSocket _webSocket;
        
        public static float MaxChannel1 { get;  set; }
        public static float MaxChannel2 { get;  set; }
        public static float MaxChannel3 { get;  set; }

        private void Start()
        {
            confirmButton.onClick.RemoveAllListeners();
            confirmButton.onClick.AddListener(() => StartCoroutine(CalculateMaxValueCoroutine()));
        }

        private IEnumerator CalculateMaxValueCoroutine()
        {
            var userIdToGetMaxV = userIdToGetMaxVInput.text.Trim();
            
            if (string.IsNullOrEmpty(userIdToGetMaxV))
            {
                Debug.LogWarning("⚠️ 请输入用户 ID");
                yield break;
            }
            
            Task calculateTask = CalculateMaxValue(userIdToGetMaxV);
            yield return new WaitUntil(() => calculateTask.IsCompleted);
        }
        private async Task CalculateMaxValue(string userIdToGetMaxV)
        {
            try
            {
                _webSocket = new ClientWebSocket();
                Debug.Log("🔌 正在连接 WebSocket...");
                await _webSocket.ConnectAsync(new Uri("ws://127.0.0.1:8000/readMaxValue"), CancellationToken.None);
                Debug.Log("✅ WebSocket 连接成功");


                if (_webSocket.State == WebSocketState.Open)
                {
                    await SendUserIdToGetMaxValue(userIdToGetMaxV);
                    // 接收确认消息
                    var confirmBuffer = new byte[4096];
                    var confirmResult = await _webSocket.ReceiveAsync(new ArraySegment<byte>(confirmBuffer), CancellationToken.None);
                    string confirmMessage = Encoding.UTF8.GetString(confirmBuffer, 0, confirmResult.Count);
                    Debug.Log($"✅ 服务器确认: {confirmMessage}");
                }
                
                // 创建一个 4KB 缓冲区接收数据
                byte[] buffer = new byte[10026];
                var receiveResult = await _webSocket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);
                string receivedData = Encoding.UTF8.GetString(buffer, 0, receiveResult.Count);
                
                Debug.Log($"📡 收到数据: {receivedData}");
                
                ProcessReceivedData(receivedData);
            }
            catch (Exception ex)
            {
                Debug.LogError($"❌ 发送失败: {ex.Message}");
            }
        }

        private async Task SendUserIdToGetMaxValue(string userIdToGetMaxV)
        {
            string jsonData = $"{{\"type\":\"userIdToGetMaxV\",\"userIdToGetMaxV\":\"{userIdToGetMaxV}\"}}";

            byte[] bytes = Encoding.UTF8.GetBytes(jsonData);
        
            await _webSocket.SendAsync(new ArraySegment<byte>(bytes), WebSocketMessageType.Text, true, CancellationToken.None);
            Debug.Log($"📤 已发送用户 ID: {userIdToGetMaxV}");
            
        }

        private void ProcessReceivedData(string jsonData)
        {
            try
            {
                var dataDict = JsonUtility.FromJson<MaxValueData>(jsonData);

                // 存储最大值到静态变量
                MaxChannel1 = dataDict.Channel1;
                MaxChannel2 = dataDict.Channel2;
                MaxChannel3 = dataDict.Channel3;

                Debug.Log($"🎯 最大值存储完成: Channel1={MaxChannel1}, Channel2={MaxChannel2}, Channel3={MaxChannel3}");
            }
            catch (Exception e)
            {
                Debug.LogError($"解析数据错误: {e.Message}");
            }
        }

        [Serializable]
        private class MaxValueData
        {
            public float Channel1;
            public float Channel2;
            public float Channel3;
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
