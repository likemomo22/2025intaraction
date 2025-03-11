
    using System;
    using UnityEngine;
    using UnityEngine.UI;

    public class EmgDataHandler:MonoBehaviour
    {
        
        private EMGGraphController _emgGraphController;

        private SmoothedValue smoothedValueProcessor;
        private float _smoothedValue;
        public float SmoothedValue => _smoothedValue; // 只读属性
        private float _targetFillAmount;

        public GameObject StatusContainer;
        private EMGUIPackController energyBar;
        private void Start()
        {
            smoothedValueProcessor = new SmoothedValue();
            //获取 EMGGraphController 组件
            _emgGraphController = GetComponent<EMGGraphController>();
            //用于改变能量条的fillAmount
            energyBar = StatusContainer.GetComponentInChildren<EMGUIPackController>();
            _targetFillAmount = 0;
        }

        private void Update()
        {
            _smoothedValue = smoothedValueProcessor.GetSmoothedValue(_targetFillAmount);
            _emgGraphController.updateEmgGraph(_smoothedValue);
            energyBar.UpdateImageAndText(_smoothedValue);
        }
    
        public void updateFillAmount(float emgData,int index)
        {
            float maxValue = 7000f;
            switch (index)
            {
                case 1:
                    maxValue = SetMaxValues.MaxChannel1;
                    break;
                case 2:
                    maxValue = SetMaxValues.MaxChannel2;
                    break;
                case 3:
                    maxValue = SetMaxValues.MaxChannel3;
                    break;
                default:
                    Debug.LogWarning($"⚠ 未知的 index: {index}");
                    break;
            }
            // 计算新的目标 fillAmount，归一化到 [0,1] 之间
            _targetFillAmount = Mathf.Clamp01(emgData / maxValue);
        }

      
    }
    