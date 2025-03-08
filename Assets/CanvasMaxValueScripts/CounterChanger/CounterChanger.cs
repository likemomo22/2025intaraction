using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace CanvasMaxValueScripts.CounterChanger
{
    public class CounterChanger : MonoBehaviour
    {
        private Text _textComponent;
        public String[] textString;
        public float changeInterval = 3f;
        private int _currentIndex = 0;

        private void Start()
        {
            _textComponent = GetComponent<Text>();

            if (_textComponent == null)
            {
                Debug.LogError("Not Found Text Component");
                return;
            }

            if (textString.Length > 0)
            {
                StartCoroutine(ChangeTextRoutine());
            }
            else
            {
                Debug.LogError("No text assigned to CounterChanger");
            }
        }

        IEnumerator ChangeTextRoutine()
        {
            while (true)
            {
                yield return new WaitForSeconds(changeInterval);

                //切换到下一个text
                _currentIndex = (_currentIndex + 1) % textString.Length;
                _textComponent.text = textString[_currentIndex];
            }
        }
    }
}

