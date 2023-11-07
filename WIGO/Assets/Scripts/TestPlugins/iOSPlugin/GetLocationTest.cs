using System;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using WIGO.Utility;

namespace WIGO.Test
{
    public class GetLocationTest : MonoBehaviour
    {
        [SerializeField] TMP_Text _latitudeText;
        [SerializeField] TMP_Text _longitudeText;
        [SerializeField] GameObject _getLocationButton;
        [SerializeField] GameObject _noPermissionTip;
        [Space]
        [SerializeField] Image _statusIcon;
        [SerializeField] Color _onColor;
        [SerializeField] Color _offColor;

        bool _inited;
        float _timer = 0f;

        public void OnUpdateLocationClick()
        {
            if (Input.location.status == LocationServiceStatus.Running)
            {
                var latitude = Input.location.lastData.latitude.ToString();
                var longitude = Input.location.lastData.longitude.ToString();
                _latitudeText.SetText(latitude);
                _longitudeText.SetText(longitude);
            }
            else
            {
                Debug.LogWarning("Location's not working");
            }
        }

        private void Start()
        {
            _timer = 0f;
#if UNITY_IOS && !UNITY_EDITOR
            MessageIOSHandler.OnAllowLocationPermission();
#endif
        }

        private void Update()
        {
            if (!_inited)
            {
                _timer += Time.unscaledDeltaTime;
                if (_timer >= 10f)
                {
                    _timer = 0f;
                    _inited = true;

                    _statusIcon.color = Color.yellow;
#if UNITY_IOS && !UNITY_EDITOR
                    StartCoroutine(CheckPermissionsIOSLocation((success) =>
                    {
                        _getLocationButton.SetActive(success);
                        _noPermissionTip.SetActive(!success);
                    }));
#endif

                    InvokeRepeating("CheckStatus", 1.5f, 2f);
                }
            }
        }

        void CheckStatus()
        {
            Color color = Input.location.status == LocationServiceStatus.Running ? _onColor : _offColor;
            _statusIcon.color = color;
        }

#if UNITY_IOS && !UNITY_EDITOR
        static IEnumerator CheckPermissionsIOSLocation(Action<bool> callback)
        {
            if (PermissionsRequestManager.HasLocationPermission())
            {
                callback?.Invoke(true);
                yield break;
            }

            Input.location.Start(1f, 5f);
            int maxWait = 1200;
            while (Input.location.status == LocationServiceStatus.Initializing && maxWait > 0)
            {
                yield return null;
                maxWait--;
            }

            if (maxWait < 1)
            {
                Debug.Log("Timed out location");
                callback?.Invoke(false);
                yield break;
            }

            if (Input.location.status == LocationServiceStatus.Failed)
            {
                Debug.Log("Unable to determine device location");
                callback?.Invoke(false);
            }
            else if (Input.location.status == LocationServiceStatus.Running)
            {
                callback?.Invoke(true);
            }
        }
#endif
    }
}
