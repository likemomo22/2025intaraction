
    using System;
    using Unity.VisualScripting;
    using UnityEngine;
    using UnityEngine.UI;

    public class SliderManager:MonoBehaviour
    {
        public Slider slider1;
        public Slider slider2;
        public Slider slider3;

        public Text valueText1;
        public Text valueText2;
        public Text valueText3;

        private float _maxValue1 = 10000f;


        private void Start()
        {
            slider1.maxValue = _maxValue1;
            slider2.maxValue = _maxValue1;
            slider3.maxValue = _maxValue1;
            
            UpdateValue1(SetMaxValues.MaxChannel1);
            UpdateValue2(SetMaxValues.MaxChannel2);
            UpdateValue3(SetMaxValues.MaxChannel3);
                
            slider1.onValueChanged.AddListener(UpdateValue1);
            slider2.onValueChanged.AddListener(UpdateValue2);
            slider3.onValueChanged.AddListener(UpdateValue3);
        }

        private void UpdateValue1(float value)
        {
            valueText1.text = value.ToString("F2");
            slider1.value = value;
            SetMaxValues.MaxChannel1 = value;
            Debug.Log(SetMaxValues.MaxChannel1);
        }
        void UpdateValue2(float value)
        {
            valueText2.text =value.ToString("F2");
            slider2.value = value;
            SetMaxValues.MaxChannel2 = value;
        }

        void UpdateValue3(float value)
        {
            valueText3.text = value.ToString("F2");       
            slider3.value = value;
            SetMaxValues.MaxChannel3 = value;
        }
    }
