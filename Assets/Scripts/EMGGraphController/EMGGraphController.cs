using System;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

public class EMGGraphController : MonoBehaviour
{
    public DD_DataDiagram dataDiagram;
    private GameObject emgLine1;
    private DD_CoordinateAxis _coordinateAxis;

    private Color recentColor;
    
    void Start()
    {
        if (dataDiagram == null)
            dataDiagram = GetComponent<DD_DataDiagram>();
        dataDiagram.m_MaxPointNum = 500;
        dataDiagram.m_CentimeterPerCoordUnitX = 0.1f;  // ✅ 让 X 轴显示更多历史数据
        dataDiagram.m_CentimeterPerMark = 1.0f;

        emgLine1 = dataDiagram.AddLine("EMG1", new Color(1f,1f,0f));

        recentColor = Color.black;
    }

    public void updateEmgGraph(float emgData)
    {
        float emgValue = emgData;
        // Debug.Log($"[EmgDataHandler] Attached to: {gameObject.name}::::::"+emgData);

        Color newColor = Color.Lerp(recentColor, emgValue >= 0.4 ? Color.red : new Color(160f / 255f, 232f / 255f, 180f / 255f), Time.deltaTime * 5f);
        if (recentColor!=newColor&&this.gameObject.name!="part1")
        {
            dataDiagram.SetGraphBackgroundColor(newColor);
            recentColor = newColor;
        }
        dataDiagram.InputPoint(emgLine1, new Vector2(1, emgValue));
        
    }
}