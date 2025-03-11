using UnityEngine;

public static class EmgDataRegularization
    {
        public static float[] getRegulatedEmgData(float[] emgDatas)
        {
            if (emgDatas == null || emgDatas.Length == 0)
            {
                return new float[0];
            }

            // 读取每个通道的最大值
            float maxChannel1 = SetMaxValues.MaxChannel1 > 0 ? SetMaxValues.MaxChannel1 : 7000f;
            float maxChannel2 = SetMaxValues.MaxChannel2 > 0 ? SetMaxValues.MaxChannel2 : 7000f;
            float maxChannel3 = SetMaxValues.MaxChannel3 > 0 ? SetMaxValues.MaxChannel3 : 7000f;
            
            float[] result = new float[emgDatas.Length];
            for (int i=0; i < emgDatas.Length; i++)
            {
                float maxValue = (i == 0) ? maxChannel1 : (i == 1) ? maxChannel2 : maxChannel3;
                result[i] = Mathf.Clamp01(emgDatas[i] / maxValue);
            }

            return result;
        }
    }
