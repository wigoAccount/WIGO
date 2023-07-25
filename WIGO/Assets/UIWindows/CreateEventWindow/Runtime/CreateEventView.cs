using UnityEngine;
using UnityEngine.UI;
using WIGO.Core;
using DG.Tweening;
using TMPro;

namespace WIGO.Userinterface
{
    public class CreateEventView : UIWindowView<UIWindowModel>
    {
        [SerializeField] Image[] _companyCountButtons;
        [SerializeField] Image[] _genderButtons;
        [Space]
        [SerializeField] GameObject _emptyLocation;
        [SerializeField] GameObject _fullLocation;
        [SerializeField] TMP_Text _locationLabel;
        [SerializeField] TMP_Text _addressLabel;

        Sequence _genderSequence;
        Sequence _sizeSequence;

        public void SelectGender(int prevIndex, int index)
        {
            Image previous = _genderButtons[prevIndex];
            Image next = _genderButtons[index];
            
            CancelGenderAnimation();

            _genderSequence = DOTween.Sequence();
            _genderSequence.Append(previous.DOFade(0f, 0.2f))
                .Join(next.DOFade(1f, 0.2f))
                .OnComplete(() => _genderSequence = null);
        }

        public void SelectGroupSize(int prevIndex, int index)
        {
            Image previous = prevIndex == 0 ? null : _companyCountButtons[prevIndex - 1];
            Image next = _companyCountButtons[index - 1];

            CancelSizeAnimation();

            _sizeSequence = DOTween.Sequence();
            _sizeSequence.Append(next.DOFade(1f, 0.2f))
                .OnComplete(() => _sizeSequence = null);

            if (previous != null)
            {
                _sizeSequence.Join(previous.DOFade(0f, 0.2f));
            }
        }

        public void ResetView(EventGenderType gender, EventGroupSizeType size)
        {
            Image currentGender = _genderButtons[(int)gender];
            Image currentSize = size == EventGroupSizeType.None ? null : _companyCountButtons[(int)size - 1];

            UIGameColors.SetTransparent(currentGender);
            if (currentSize != null)
            {
                UIGameColors.SetTransparent(currentSize);
            }

            UIGameColors.SetTransparent(_genderButtons[0], 1f);
            _emptyLocation.SetActive(true);
            _fullLocation.SetActive(false);
        }

        public void SetLocation(string coordinates, string location)
        {
            _emptyLocation.SetActive(false);
            _fullLocation.SetActive(true);
            //_locationLabel.text = placeName;
            //_addressLabel.text = placeAddress;

            Debug.LogFormat("1: {0}\r\n2: {1}", coordinates, location);
            try
            {
                string[] splited = coordinates.Replace("\"", "").Split(",");
                if (splited.Length > 1)
                {
                    var latitude = splited[0];
                    var longitude = splited[1];
                    Debug.LogFormat("<color=cyan>Latitude: {0}\r\nLongitude: {1}</color>", latitude, longitude);
                }
                else
                    Debug.LogWarningFormat("Can't split coordinates: {0}", coordinates);
            }
            catch (System.Exception)
            {
                Debug.LogWarningFormat("Wrong coordinates format");
            }

            try
            {
                string address = string.Empty;
                if (location.Contains("YandexMap"))
                {
                    int start = Mathf.Clamp(location.IndexOf(@"\") + 2, 0, int.MaxValue);
                    int end = Mathf.Clamp(location.IndexOf(@"\", start) - 1, 0, int.MaxValue);
                    address = location.Substring(start, end - start + 1);
                }
                else
                {
                    address = location.Replace("\"", "");
                }
                
                Debug.LogFormat("<color=magenta>Location: {0}</color>", address);
                _addressLabel.text = address;
            }
            catch (System.Exception)
            {
                Debug.LogWarningFormat("Wrong location format");
            }
        }

        void CancelGenderAnimation()
        {
            if (_genderSequence == null)
            {
                _genderSequence.Kill();
                _genderSequence = null;
            }
        }

        void CancelSizeAnimation()
        {
            if (_sizeSequence == null)
            {
                _sizeSequence.Kill();
                _sizeSequence = null;
            }
        }
    }

    [System.Serializable]
    public struct LocationParams
    {
        public double longitude;
        public double latitude;
    }
}
