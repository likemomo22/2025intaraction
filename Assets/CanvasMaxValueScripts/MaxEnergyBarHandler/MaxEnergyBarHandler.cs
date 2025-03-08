using System;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class MaxEnergyBarHandler:MonoBehaviour
{
    public string targetImageName = "TargetImage";
    public string targetText = "TargetText";
    public string outerImageName = "outerImage";

    private Image _targetImage;
    private Text _targetText;
    private Image _outerImage;
    Color recentColor;

    private void Start()
    {
        _targetImage = transform.Find(targetImageName)?.GetComponent<Image>();
        _targetText = transform.Find(targetText)?.GetComponent<Text>();
        _outerImage = GetComponentsInChildren<Image>(true).FirstOrDefault(img => img.name == outerImageName);        if(_targetText==null||_targetImage==null)
        {
            Debug.Log("Image or Text not found!");
        }
        recentColor = Color.black;
    }

    public void UpDateFillAmount(float regulatedEmgData,int testIndex)
    {
        if (testIndex == 0)
        {
            _outerImage.enabled = false;
        }
        else
        {
            _outerImage.enabled = true;
        }
        _targetImage.fillAmount = regulatedEmgData;
            _outerImage.fillAmount = regulatedEmgData;

        Color newColor = Color.Lerp(recentColor, regulatedEmgData >= 0.5 ? Color.red : new Color(160f / 255f, 232f / 255f, 180f / 255f),
            Time.deltaTime * 5f);
        if (recentColor != newColor)
        {
            _targetImage.color = newColor;
            recentColor = newColor;
        }
        _targetText.text = (Mathf.Round(regulatedEmgData * 100 / 10) * 10).ToString("F0")+"%";
    }
}