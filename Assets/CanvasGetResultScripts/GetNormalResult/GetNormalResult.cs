using System;
    using System.Collections;
    using System.Net.WebSockets;
    using System.Text;
    using System.Threading;
    using System.Threading.Tasks;
    using UnityEngine;
    using UnityEngine.UI;

    public class GetNormalResult:MonoBehaviour
    {
        public Button getFeedbackButton;
        private ClientWebSocket _webSocket;
        
        public FeedBackBarChart _feedBackBarChart;

        private void Start()
        {
            getFeedbackButton.onClick.AddListener(() => StartCoroutine(PrintFeedbackCoroutine()));
        }

        private IEnumerator PrintFeedbackCoroutine()
        {
            Task calculateTask = PrintFeedback();
            yield return new WaitUntil(() => calculateTask.IsCompleted);
        }
        private async Task PrintFeedback()
        {
            try
            {
                _webSocket = new ClientWebSocket();
                Debug.Log("🔌 正在连接 WebSocket...");
                await _webSocket.ConnectAsync(new Uri("ws://127.0.0.1:8000/calculateRMSNormalRatio"), CancellationToken.None);
                Debug.Log("✅ WebSocket 连接成功");
                
                if (_webSocket.State == WebSocketState.Open)
                {
                    await printResult();
                }
                
            }
            catch (Exception ex)
            {
                Debug.LogError($"❌ 发送失败: {ex.Message}");
            }
        }

        private async Task printResult()
        {
            byte[] buffer = new byte[4096];
            var receiveResult = await _webSocket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);
            string receivedData = Encoding.UTF8.GetString(buffer, 0, receiveResult.Count);

            Debug.Log($"📡 收到数据: {receivedData}");
            try
            {
                // 去除方括号，手动解析为 JSON 格式
                string jsonData = "{\"values\":" + receivedData + "}";
            
                // 使用 JsonUtility 解析数据
                FeedbackData feedbackData = JsonUtility.FromJson<FeedbackData>(jsonData);

                if (feedbackData.values.Length >= 2)
                {
                    float value1 = feedbackData.values[0];
                    float value2 = feedbackData.values[1];
                    _feedBackBarChart.SetBarValues(value1, value2);
                }
                else
                {
                    Debug.LogError("⚠️ 数据格式不正确，缺少值");
                }
            }
            catch (System.Exception e)
            {
                Debug.LogError($"❌ 数据解析错误: {e.Message}");
            }
            
            if (receiveResult.MessageType == WebSocketMessageType.Text)
            {
                string response = Encoding.UTF8.GetString(buffer, 0, receiveResult.Count);
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
    