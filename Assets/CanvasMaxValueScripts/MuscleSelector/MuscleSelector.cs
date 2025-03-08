using UnityEngine;
using UnityEngine.UI;

    public class MuscleSelector:MonoBehaviour
    {
        public Button[] buttons;

        public GameObject MaxStatusContainer;
        private EmgReceiver _emgReceiver;

        private int _currentButtonIndex = -1;

        private void Start()
        {
            buttons = GetComponentsInChildren<Button>();

            for (int i = 0; i < buttons.Length; i++)
            {
                int index = i;
                buttons[i].onClick.AddListener(()=>HandleButtonClick(index));
            }

            if (MaxStatusContainer != null)
            {
                _emgReceiver = MaxStatusContainer.GetComponent<EmgReceiver>();
            }
        }
        private void HandleButtonClick(int buttonIndex)
        {
            Debug.Log($"Button {buttonIndex} is clicked");
            if (_emgReceiver == null)
            {
                Debug.LogError("EmgReceiver not found");
                return;
            }

            if (_currentButtonIndex==buttonIndex)
            {
                Debug.Log($"Button {buttonIndex} is already active");
                return;
            }
            // await _emgReceiver.StopReceiving();
            // _emgReceiver.StartReceiving(buttonIndex);
            //
            // _currentButtonIndex = buttonIndex;

            _emgReceiver.SetActiveChannel(buttonIndex);
        }
    }
