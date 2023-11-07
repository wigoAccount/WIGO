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
            PermissionsRequestManager.RequestPermissionLocation((success) =>
            {
                _getLocationButton.SetActive(success);
                _noPermissionTip.SetActive(!success);
            });

            InvokeRepeating("CheckStatus", 0.5f, 2f);
        }

        void CheckStatus()
        {
            Color color = Input.location.status == LocationServiceStatus.Running ? _onColor : _offColor;
            _statusIcon.color = color;
        }
    }
}
