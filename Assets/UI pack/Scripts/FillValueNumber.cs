using System;
using UnityEngine;
using UnityEngine.UI;
using Random = System.Random;

namespace UI_pack.Scripts
{
    public class FillValueNumber : MonoBehaviour
    {
        public string targetImageName = "TargetImage";
        public string targetTextName = "TargetText";

        private Image _targetImage;
        private Text _targetText;

        private Random _random = new Random();
        private int _randomInt;
        private float _timer = 0f;
        private void Start()
        {
            //查找子物体上的 Image 和 Text 组件
            _targetImage = transform.Find(targetImageName).GetComponent<Image>();
            _targetText = transform.Find(targetTextName).GetComponent<Text>();

            if (_targetImage == null || _targetText == null)
            {
                Debug.Log("Image or Text not found!");
            }
        }

        // Update is called once per frame
        void Update()
        {
            _timer += Time.deltaTime;
            if (_timer>=1f)
            {


                if (_targetImage != null && _targetText != null)
                {
                    _randomInt = _random.Next(0, 100);
                    _targetImage.fillAmount = _randomInt/100f;
                    // float amount = _targetImage.fillAmount * 100;
                    _targetText.text = _randomInt.ToString("F0");
                }

                _timer = 0f;
            }
        }
    }
}
