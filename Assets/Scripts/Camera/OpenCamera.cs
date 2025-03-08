using System;
using UnityEngine;
using UnityEngine.UI;


    public class OpenCamera :MonoBehaviour
    {

        private WebCamTexture _webCamTexture;

        private void Start()
        {
            WebCamDevice[] devices = WebCamTexture.devices;
            if (devices.Length > 0)
            {
                _webCamTexture = new WebCamTexture(devices[0].name);

                Renderer renderer = GetComponent<Renderer>();
                renderer.material.mainTexture = _webCamTexture;
                
                _webCamTexture.Play();
            }
            else
            {
                Debug.LogWarning("No camera found!");
            }
        }

        private void OnDestroy()
        {
            if (_webCamTexture != null && _webCamTexture.isPlaying)
            {
                _webCamTexture.Stop();
            }
        }
    }
