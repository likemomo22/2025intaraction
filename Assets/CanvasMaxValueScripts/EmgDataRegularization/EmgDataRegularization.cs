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
            float maxChannel1 = PrintMaxValues.MaxChannel1 > 0 ? PrintMaxValues.MaxChannel1 : 7000f;
            float maxChannel2 = PrintMaxValues.MaxChannel2 > 0 ? PrintMaxValues.MaxChannel2 : 7000f;
            float maxChannel3 = PrintMaxValues.MaxChannel3 > 0 ? PrintMaxValues.MaxChannel3 : 7000f;
            
            float[] result = new float[emgDatas.Length];
            for (int i=0; i < emgDatas.Length; i++)
            {
                float maxValue = (i == 0) ? maxChannel1 : (i == 1) ? maxChannel2 : maxChannel3;
                result[i] = Mathf.Clamp01(emgDatas[i] / maxValue);
            }

            return result;
        }
    }
