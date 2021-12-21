using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace FleekStudio
{
    public class AppUI : MonoBehaviour
    {
        [SerializeField] private AppManager _appManager;
        
        [SerializeField] private Button _cycleCameraButton;
        [SerializeField] private Button _cycleEyebrowButton;
        
        void Start()
        {
        }

        private void OnEnable()
        {
            _cycleEyebrowButton.onClick.AddListener(HandleCycleEyebrowButtonOnClick);
        }

        private void OnDisable()
        {
            _cycleEyebrowButton.onClick.RemoveListener(HandleCycleEyebrowButtonOnClick);
        }

        private void Update()
        {

        }

        private void HandleCycleEyebrowButtonOnClick()
        {
            _appManager.NextEyebrow();
        }
    }
}

