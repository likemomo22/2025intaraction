using System;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using CanvasMaxValueScripts.ImageChanger;
using UnityEngine;
using UnityEngine.UI;

public class EmgReceiver : MonoBehaviour
{
    private CancellationTokenSource _cancellationTokenSource;

    private bool _isReceiving;

    private MaxEnergyBarHandler _maxEnergyBarHandler;
    private ImageChanger _imageChanger; 
        
    private float _regulatedEmgData;
    private SmoothedValue _smoothedValueProcessor;
    private ClientWebSocket _webSocket;
    private float[] processedEmgData;
    private int testIndex;
    private int _recentIndex;

    private async void Start()
    {
        _smoothedValueProcessor = new SmoothedValue();
        processedEmgData = new float[0];
        _maxEnergyBarHandler = FindObjectOfType<MaxEnergyBarHandler>();
        _imageChanger = FindObjectOfType<ImageChanger>();
        _recentIndex = -1;

        await StartReceiving();
    }

    private void Update()
    {
        if (_isReceiving) _maxEnergyBarHandler.UpDateFillAmount(_regulatedEmgData, testIndex);
        if (_recentIndex != testIndex)
        {
            _imageChanger.ChangeImage(testIndex);
            _recentIndex = testIndex;
        }
    }

    private async void OnDestroy()
    {
       await StopReceiving();
    }
    
    private async void OnDisable()
    {
        if (_webSocket != null && _webSocket.State == WebSocketState.Open)
        {
            await _webSocket.CloseAsync(WebSocketCloseStatus.NormalClosure, "Clint closed", CancellationToken.None);
        }
    }
    public async Task StartReceiving()
    {
        if (_isReceiving) return;
        _isReceiving = true;

        _cancellationTokenSource = new CancellationTokenSource();

        _webSocket = new ClientWebSocket();
        await _webSocket.ConnectAsync(new Uri("ws://127.0.0.1:8000/getMaxValue"), CancellationToken.None);
        Debug.Log("Connected to FastAPI WebSocket");

        await ReceiveData( _cancellationTokenSource.Token);
    }

    private async Task ReceiveData( CancellationToken cancellationToken)
    {
        var buffer = new byte[1024 * 512];

        while (_webSocket.State == WebSocketState.Open)
        {
            if (cancellationToken.IsCancellationRequested)
            {
                Debug.Log("Stopping WebSocket Data Receiving");
                break;
            }

            var result = await _webSocket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);

            if (result.MessageType == WebSocketMessageType.Text)
            {
                var jsonString = Encoding.UTF8.GetString(buffer, 0, result.Count);

                var emgData = JsonUtility.FromJson<EmgData>(jsonString);
                processedEmgData = EmgDataRegularization.getRegulatedEmgData(emgData.emgDatas);
                Debug.Log("test: "+testIndex);
                _regulatedEmgData = _smoothedValueProcessor.GetSmoothedValue(processedEmgData[testIndex]);
            }
        }
    }

    public void SetActiveChannel(int newIndex)
    {
        if (testIndex == newIndex)
        {
            Debug.Log($"Channel {newIndex} is already active");
            return;
        }

        Debug.Log($"Switching to Channel {newIndex}");
        testIndex = newIndex;
    }

    public async Task StopReceiving()
    {
        if (!_isReceiving)
            return;

        await Task.Yield(); // ✅ 避免 `CS1998` 警告

        _isReceiving = false;

        if (_cancellationTokenSource != null)
        {
            _cancellationTokenSource.Cancel();
            _cancellationTokenSource.Dispose();
            _cancellationTokenSource = null;
        }

        if (_webSocket != null && _webSocket.State == WebSocketState.Open)
        {
            await _webSocket.CloseAsync(WebSocketCloseStatus.NormalClosure, "Clint closed", CancellationToken.None);
            Debug.Log("WebSocket closed");
        }
    }
}

[Serializable]
public class EmgData
{
    public float[] emgDatas;
}