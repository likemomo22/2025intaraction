using UnityEngine;
using UnityEngine.UI;

public class EMGUIPackController:MonoBehaviour
    {
        public string targetImageName = "TargetImage";
        public string targetTextName = "TargetText";

        private Image _targetImage;
        public  Image outerImage;
        private Text _targetText;
        private Color recentColor;

        private void Start()
        {
            //查找子物体上的 Image 和 Text 组件
            _targetImage = transform.Find(targetImageName)?.GetComponent<Image>();
            _targetText = transform.Find(targetTextName)?.GetComponent<Text>();
    
            if(_targetText==null||_targetImage==null)
            {
                Debug.Log("Image or Text not found!");
            }   
            recentColor = Color.black;
        }

        public void UpdateImageAndText(float smoothedValue)
        {

            // 更新 UI
            _targetImage.fillAmount = smoothedValue;
            if (outerImage&&this.gameObject.name!="EnergyCanvas1")
            {
                outerImage.fillAmount = smoothedValue;
            }

            Color newColor = Color.Lerp(recentColor, smoothedValue >= 0.4 ? Color.red : new Color(160f / 255f, 232f / 255f, 180f / 255f), Time.deltaTime * 5f);
            if (recentColor!=newColor&&this.gameObject.name!="EnergyCanvas1")
            {
                _targetImage.color = newColor;
                recentColor = newColor;
            }
            _targetText.text = (Mathf.Round(smoothedValue * 100 / 10) * 10).ToString("F0")+"%";
        }
    }
