using System;
using UnityEngine;
using UnityEngine.UI;

public class FeedBackBarChart : MonoBehaviour
{
    public RectTransform bar1; 
    public RectTransform bar2;
    public RectTransform barBack;
    public RectTransform barBack1;

    public Text muscle2Text;
    public Text muscle3Text;
    // 设置柱状图高度的最大值 (像素)
    private float maxBarHeight = 600f;

    private void Start()
    {
        // 初始化时设置柱状图从底部开始增长
        RectTransform[] bars = { bar1, bar2 ,barBack,barBack1};
        foreach (var bar in bars)
        {
            bar.pivot = new Vector2(0.5f, 0f);
        }
        SetFixedMaxBarHeight();
    }
    private void SetFixedMaxBarHeight()
    {
        if (barBack != null)
            barBack.sizeDelta = new Vector2(barBack.sizeDelta.x, maxBarHeight);

        if (barBack1 != null)
            barBack1.sizeDelta = new Vector2(barBack1.sizeDelta.x, maxBarHeight);

        Debug.Log($"🎯 固定背景条高度为: {maxBarHeight}px");
    }
    // 设置柱状图数据
    public void SetBarValues(float value1, float value2)
    {
        float height1 = value1 * maxBarHeight;
        float height2 = value2 * maxBarHeight;

        // 设置柱状图高度，直接使用比例计算
        if (bar1 != null)
            bar1.sizeDelta = new Vector2(bar1.sizeDelta.x, height1);

        if (bar2 != null)
            bar2.sizeDelta = new Vector2(bar2.sizeDelta.x, height2);
        

        // 设置显示百分比，乘以 100 转为百分比显示
        muscle2Text.text = Mathf.RoundToInt(value1 * 100) + "%";
        muscle3Text.text = Mathf.RoundToInt(value2 * 100) + "%";
        
    }
  
}