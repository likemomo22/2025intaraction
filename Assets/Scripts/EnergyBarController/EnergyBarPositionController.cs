using System.Collections.Generic;
using UnityEngine;

public class EnergyBarPositionController : MonoBehaviour
{
    public Dictionary<int, Transform> statusContainers;

    private void Awake()
    {
        // 确保字典初始化，避免 `null` 问题
        statusContainers = new Dictionary<int, Transform>();
    }  
    public void UpdatePositionInPlane(int idx,Vector3 localPos)
    {
        if (statusContainers.ContainsKey(idx) && statusContainers[idx] != null)
        {
            statusContainers[idx].localPosition = localPos;
        }
        else
        {
            Debug.LogError($"idx {idx} 没有对应的 statusContainer！");
        }        
    }
}