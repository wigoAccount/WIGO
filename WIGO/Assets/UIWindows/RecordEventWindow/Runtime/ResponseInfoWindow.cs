using System.Collections;
using TMPro;
using UnityEngine;

namespace WIGO.Userinterface
{
    public class ResponseInfoWindow : UIWindow
    {
        [SerializeField] WindowAnimator _animator;
        [SerializeField] RectTransform _window;
        [SerializeField] RectTransform _photo;
        [SerializeField] TMP_Text _title;
        [SerializeField] TMP_Text _subtitle;
        [SerializeField] TMP_Text _description;
        [TextArea]
        [SerializeField] string _eventTitleText;
        [TextArea]
        [SerializeField] string _responseTitleText;
        [SerializeField] string _eventSubtitleText;
        [SerializeField] string _responseSubtitleText;
        [TextArea]
        [SerializeField] string _eventDescText;
        [TextArea]
        [SerializeField] string _responseDescText;

        bool _isResponseInfo;

        public override void OnOpen(WindowId previous)
        {
            _animator.OnOpen();
        }

        public void Setup(bool response = false)
        {
            _isResponseInfo = response;
            _title.text = response ? _responseTitleText : _eventTitleText;
            _subtitle.text = response ? _responseSubtitleText : _eventSubtitleText;
            _description.text = response ? _responseDescText : _eventDescText;

            StartCoroutine(SetCorrectSize());
        }

        public void OnContinueClick()
        {
            //ServiceLocator.Get<UIManager>().Open<RecordEventWindow>(WindowId.RECORD_EVENT_SCREEN, (window) => window.Setup(_isResponseInfo));
        }

        public void OnBackClick()
        {
            ServiceLocator.Get<UIManager>().CloseCurrent();
        }

        IEnumerator SetCorrectSize()
        {
            yield return new WaitForEndOfFrame();

            float height = _window.rect.height - (_description.rectTransform.anchoredPosition.y +
                _description.rectTransform.sizeDelta.y - _photo.anchoredPosition.y + 28f);
            _photo.sizeDelta = new Vector2(_photo.sizeDelta.x, height);
        }
    }
}
