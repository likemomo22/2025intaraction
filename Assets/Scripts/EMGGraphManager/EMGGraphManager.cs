using System;
using UnityEngine;

public class EMGGraphManager : MonoBehaviour
{
    public string graphName1 = "part1";
    public string graphName2 = "part2";
    public string graphName3 = "part3";
    
    private RectTransform graph1;
    private RectTransform graph2;
    private RectTransform graph3;

    private EmgDataHandler emgDataHandler1;
    private EmgDataHandler emgDataHandler2;
    private EmgDataHandler emgDataHandler3;

    private Canvas canvas;

    
    private Vector2 originalScreenSize;

    private void Start()
    {
        emgDataHandler1 = GameObject.Find(graphName1)?.GetComponent<EmgDataHandler>();
        emgDataHandler2 = GameObject.Find(graphName2)?.GetComponent<EmgDataHandler>();
        emgDataHandler3 = GameObject.Find(graphName3)?.GetComponent<EmgDataHandler>();

        graph1 = emgDataHandler1?.GetComponent<RectTransform>();
        graph2 = emgDataHandler2?.GetComponent<RectTransform>();
        graph3 = emgDataHandler3?.GetComponent<RectTransform>();

        originalScreenSize = new Vector2(Screen.width, Screen.height);
        AdjustGraphSize();
    }

    private void Update()
    {
        if (Screen.width != originalScreenSize.x || Screen.height != originalScreenSize.y)
        {
            originalScreenSize = new Vector2(Screen.width, Screen.height);
            AdjustGraphSize();
        }
    }

    private void AdjustGraphSize()
    {
        if (graph1 == null || graph2 == null || graph3 == null)
        {
            Debug.LogError("graph1, graph2 或 graph3 为空，调整大小失败！");
            return;
        }
        RectTransform canvasRect = GetComponent<RectTransform>();

        float canvasWidth = canvasRect.rect.width;
        float canvasHeight = canvasRect.rect.height;

        float graphWidth = canvasWidth * 0.51f;  // **占 `Canvas` 左侧 `70%`**
        float graphHeight = canvasHeight * 0.28f; // **占 `Canvas` 高度 `25%`**
        float spacing = canvasHeight * 0.05f; // **竖直间隔**

        // **设定 UI 组件大小**
        graph1.sizeDelta = new Vector2(graphWidth, graphHeight);
        graph2.sizeDelta = new Vector2(graphWidth, graphHeight);
        graph3.sizeDelta = new Vector2(graphWidth, graphHeight);

        // **固定 UI 在 `Canvas` 左侧**
        graph1.anchorMin = new Vector2(0.5f, 0.5f);
        graph1.anchorMax = new Vector2(0.5f, 0.5f);

        graph2.anchorMin = new Vector2(0.5f, 0.5f);
        graph2.anchorMax = new Vector2(0.5f, 0.5f);

        graph3.anchorMin = new Vector2(0.5f, 0.5f);
        graph3.anchorMax = new Vector2(0.5f, 0.5f);

        // **调整 X 坐标**
        float graphX = -canvasWidth*0.234f;  // **图表完全贴左侧**

        // **调整 Y 坐标（竖直排列）**
        graph1.anchoredPosition = new Vector2(graphX, graphHeight + spacing);  // 顶部
        graph2.anchoredPosition = new Vector2(graphX, 0);                      // 居中
        graph3.anchoredPosition = new Vector2(graphX, -graphHeight - spacing); // 底部
    }



    public  void UpdateGraphs(float[] emgDatas)
    {
        emgDataHandler1.updateFillAmount(emgDatas[0],1);
        emgDataHandler2.updateFillAmount(emgDatas[1],2);
        emgDataHandler3.updateFillAmount(emgDatas[2],3);

    }
}