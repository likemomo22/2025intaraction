using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class ThirdStepNextButtonHandler:MonoBehaviour
{
    public GameObject RecentCanvas;
    public GameObject TargetCanvas;
    public GameObject Human;
    public GameObject HumanOnly;
    public Button NextButton;
    public GameObject Plane;
    private VideoAndLandMarksReceiver _videoAndLandMarksReceiver;

    private void Start()
    {
        TargetCanvas.SetActive(false);
        Human.SetActive(false);
        if (Plane != null)
        {
            _videoAndLandMarksReceiver = Plane.GetComponent<VideoAndLandMarksReceiver>();
        }
        else
        {
            Debug.LogError("Plane is null! Cannot find VideoAndLandMarksReceiver.");
        }
        
        NextButton.onClick.RemoveAllListeners(); // 先清除旧的监听器，防止重复绑定
        NextButton.onClick.AddListener(() => StartCoroutine(SwitchToGameCanvas()));
    }

    private IEnumerator  SwitchToGameCanvas()
    {
        foreach (Transform child in RecentCanvas.transform)
        {
            child.gameObject.SetActive(false);
        }

        if (HumanOnly != null)
        {
            HumanOnly.SetActive(false);
        }
        RecentCanvas.SetActive(false);
            
        yield return new WaitForSeconds(1f);

        TargetCanvas.SetActive(true);
        Human.SetActive(true);

        if (_videoAndLandMarksReceiver !=null)
        {
            _videoAndLandMarksReceiver.StartReceiving();
        }
        else
        {
            Debug.LogError("Not Found VideoAndLandMarksReceiver");
        }
    }
}
