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
    public Button PrintMaxValueButton;
    private WebSocketUtils _webSocketUtils;
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
        PrintMaxValueButton.onClick.AddListener(StopWebSocket);
        _webSocketUtils = new WebSocketUtils();
        _smoothedValueProcessor = new SmoothedValue();
        processedEmgData = new float[0];
        _maxEnergyBarHandler = FindObjectOfType<MaxEnergyBarHandler>();
        _imageChanger = FindObjectOfType<ImageChanger>();
        _recentIndex = -1;

        await StartReceiving();
    }

    private async void  StopWebSocket()
    {
        if (_webSocketUtils.WebSocketIsOpened)
        {
            await _webSocketUtils.SendCloseRequestAsync();
        }
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
        if (_webSocketUtils.WebSocketIsOpened)
        {
            await _webSocketUtils.SendCloseRequestAsync();
        }
       // await StopReceiving();
    }
    
    private async void OnDisable()
    {
        if (_webSocketUtils.WebSocketIsOpened)
        {
            await _webSocketUtils.SendCloseRequestAsync();
        }
        // await StopReceiving();
    }
    public async Task StartReceiving()
    {
        if (_isReceiving) return;
        _isReceiving = true;

        _cancellationTokenSource = new CancellationTokenSource();

        try
        {
            var isConnect = await _webSocketUtils.ConnectAsync("ws://127.0.0.1:8000/getMaxValue");
            if (isConnect)
            {
                await _webSocketUtils.ReceiveLoopAsync(ProcessReceivedData,1024*512);
            }
        }
        catch (Exception ex)
        {
            Debug.LogError($"❌ 失败: {ex.Message}");
        }
    }

    private void ProcessReceivedData(string jsonString)
    {
        var emgData = JsonUtility.FromJson<EmgData>(jsonString);
        processedEmgData = EmgDataRegularization.getRegulatedEmgData(emgData.emgDatas);
        Debug.Log("test: "+testIndex);
        _regulatedEmgData = _smoothedValueProcessor.GetSmoothedValue(processedEmgData[testIndex]);
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

    // public async Task StopReceiving()
    // {
    //     if (!_isReceiving)
    //         return;
    //
    //     await Task.Yield(); // ✅ 避免 `CS1998` 警告
    //
    //     _isReceiving = false;
    //
    //     if (_cancellationTokenSource != null)
    //     {
    //         _cancellationTokenSource.Cancel();
    //         _cancellationTokenSource.Dispose();
    //         _cancellationTokenSource = null;
    //     }
    //     
    // }
}

[Serializable]
public class EmgData
{
    public float[] emgDatas;
}