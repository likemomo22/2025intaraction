using System;
using UnityEngine;
using UnityEngine.UI;

public class ToGetMaxValueButtonHandler : MonoBehaviour
{
    public GameObject RecentCanvas;
    public GameObject TargetCanvas;

    public Button PrintMaxButton;

    private void Start()
    {
        TargetCanvas.SetActive(false);
        
        PrintMaxButton.onClick.RemoveAllListeners(); // 先清除旧的监听器，防止重复绑定
        PrintMaxButton.onClick.AddListener(SwitchToPrintMaxValuesCanvas);
    }

    private void SwitchToPrintMaxValuesCanvas()
    {
        foreach (Transform child in RecentCanvas.transform)
        {
            child.gameObject.SetActive(false);
        }
        RecentCanvas.SetActive(false);
            
        TargetCanvas.SetActive(true);
    }
}
