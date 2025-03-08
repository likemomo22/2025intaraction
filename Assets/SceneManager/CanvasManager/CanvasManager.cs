using UnityEngine;
using UnityEngine.UI;

public class CanvasManager : MonoBehaviour
{
    public GameObject MainCanvas;
    public GameObject GameCanvas;
    public GameObject GetMaxValuesCanvas;
    public GameObject VideoCanvas;
    public GameObject Human;
    public GameObject HumanOnly;
    public GameObject GetResultCanvas;
    public Button startButton;
    public Button getMaxValueButton;
    public Button startCamButton;
    public Button getResultButton;
    public GameObject Plane;
    public GameObject HumanPlane;
    public GameObject MaxStatusContainer;
    private EmgReceiver _emgReceiver;
    private VideoAndLandMarksReceiver _videoAndLandMarksReceiver;
    private VideoReceiver _videoReceiver;

    private void Start()
    {
        if (!MainCanvas.activeSelf)
        {
            MainCanvas.SetActive(true);

        }
        GameCanvas.SetActive(false);
        Human.SetActive(false);

        GetMaxValuesCanvas.SetActive(false);

        VideoCanvas.SetActive(false);
        HumanOnly.SetActive(false);
        
        GetResultCanvas.SetActive(false);

        startButton.onClick.AddListener(SwitchToGameCanvas);
        getMaxValueButton.onClick.AddListener(SwitchToGetMaxValuesCanvas);
        startCamButton.onClick.AddListener(SwitchToVideoCanvas);
        getResultButton.onClick.AddListener(SwitchToResultCanvas);
    }

    private void SwitchToResultCanvas()
    {
        MainCanvas.SetActive(false);
        GetResultCanvas.SetActive(true);
    }

    private void SwitchToVideoCanvas()
    {
        MainCanvas.SetActive(false);

        VideoCanvas.SetActive(true);
        HumanOnly.SetActive(true);
        
        if (_videoReceiver == null)
            _videoReceiver = HumanPlane.GetComponent<VideoReceiver>();
        if (_videoReceiver != null)
            _videoReceiver.StartReceiving();
        else
            Debug.LogError("Not Found VideoReceiver");
    }

    private void SwitchToGetMaxValuesCanvas()
    {
        MainCanvas.SetActive(false);

        GetMaxValuesCanvas.SetActive(true);
    }

    private void SwitchToGameCanvas()
    {
        MainCanvas.SetActive(false);

        GameCanvas.SetActive(true);
        Human.SetActive(true);

        if (_videoAndLandMarksReceiver == null)
            _videoAndLandMarksReceiver = Plane.GetComponent<VideoAndLandMarksReceiver>();
        if (_videoAndLandMarksReceiver != null)
            _videoAndLandMarksReceiver.StartReceiving();
        else
            Debug.LogError("Not Found VideoAndLandMarksReceiver");
    }
}