using UnityEngine;
using UnityEngine.UI;

public class SecondStepNextButtonHandler:MonoBehaviour
{
    public GameObject RecentCanvas;
    public GameObject TargetCanvas;
    public GameObject HumanOnly;
    public Button NextButton;
    public GameObject Plane;
    private VideoReceiver _videoReceiver;

    private void Start()
    {
        TargetCanvas.SetActive(false);
        HumanOnly.SetActive(false);

        if (Plane != null)
        {
            _videoReceiver = Plane.GetComponent<VideoReceiver>();
        }
        else
        {
            Debug.LogError("Plane is null! Cannot find VideoAndLandMarksReceiver.");
        }
        
        NextButton.onClick.RemoveAllListeners(); // 先清除旧的监听器，防止重复绑定
        NextButton.onClick.AddListener(SwitchToGameCanvas);
    }

    private void SwitchToGameCanvas()
    {
        foreach (Transform child in RecentCanvas.transform)
        {
            child.gameObject.SetActive(false);
        }
        RecentCanvas.SetActive(false);
            
        TargetCanvas.SetActive(true);
        HumanOnly.SetActive(true);
        
        if (_videoReceiver !=null)
        {
            _videoReceiver.StartReceiving();
        }
        else
        {
            Debug.LogError("Not Found VideoAndLandMarksReceiver");
        }
    }
}