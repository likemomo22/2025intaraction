using UnityEngine;
using UnityEngine.UI;

public class GragidentUI : MonoBehaviour
{
    public Image targetImage;
    public Gradient colorGradient; // 在Inspector面板设置渐变

    public float gradientPosition = 0.5f; // 取渐变中的哪个颜色 (0~1)

    void Start()
    {
        if (targetImage != null && colorGradient != null)
        {
            targetImage.color = colorGradient.Evaluate(gradientPosition);
        }
    }
}