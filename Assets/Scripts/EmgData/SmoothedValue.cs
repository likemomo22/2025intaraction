using UnityEngine;

public class SmoothedValue
    {
        float smoothedValue;
        private float _smoothingFactor = 5f;
        public float GetSmoothedValue(float _targetFillAmount)
        {
            // 使用 Mathf.Lerp 让 _smoothedValue 平滑逼近 _targetFillAmount
            smoothedValue = Mathf.Lerp(smoothedValue, _targetFillAmount, Time.deltaTime * _smoothingFactor);
            return smoothedValue;
        }
    }
