using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace CanvasMaxValueScripts.ImageChanger
{
    public class ImageChanger:MonoBehaviour
    {
        private Image _imageComponent;
        public Sprite[] imageSprites;
        
        private void Start()
        {
            _imageComponent = GetComponent<Image>();
            
            if (_imageComponent == null)
            {
                Debug.LogError("ImageChanger: 没有找到 Image 组件！");
            }
            
        }

        public void ChangeImage(int picNum)
        {
                _imageComponent.sprite = imageSprites[picNum];
        }
    }
}
