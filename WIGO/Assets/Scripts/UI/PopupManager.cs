using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using Crystal;

namespace WIGO.Userinterface
{
    public class PopupManager : MonoBehaviour
    {
        [SerializeField] NotificationErrorDatabase _errorsDatabase;
        [SerializeField] Image _overlay;
        [SerializeField] PopupWindowElement _popupPrefab;
        [SerializeField] NotificationMessageElement _notificationAlertPrefab;
        [SerializeField] PopupBottomPanel _bottomPanel;
        [SerializeField] SafeArea _safeArea;

        PopupWindowElement _currentPopup;
        NotificationMessageElement _notification;

        public void AddErrorNotification(int errorId)
        {
            if (_notification != null)
            {
                return;
            }

            string message = _errorsDatabase.GetErrorMessageWithId(errorId);
            _notification = Instantiate(_notificationAlertPrefab, _safeArea.transform);
            _notification.Setup(message, () => _notification = null);
        }

        public void AddPopup(string titleKey, IEnumerable<PopupOption> options)
        {
            _currentPopup = Instantiate(_popupPrefab, _safeArea.transform);
            _currentPopup.Setup(titleKey, options);

            UIGameColors.SetTransparent(_overlay);
            _overlay.gameObject.SetActive(true);
            _overlay.DOFade(0.8f, 0.28f);
        }

        public void OpenBottomPanel(Action<bool> answerCallback)
        {
            _bottomPanel.OpenPanel(answerCallback);
            UIGameColors.SetTransparent(_overlay);
            _overlay.gameObject.SetActive(true);
            _overlay.DOFade(0.8f, 0.28f);
        }

        public void CloseCurrentPopup()
        {
            if (_currentPopup != null)
            {
                _currentPopup.OnClose();
                _currentPopup = null;
                _overlay.DOFade(0f, 0.28f).OnComplete(() => _overlay.gameObject.SetActive(false));
            }
        }

        public void ResetPopup()
        {
            UIGameColors.SetTransparent(_overlay);
            _overlay.gameObject.SetActive(false);

            if (_currentPopup != null)
            {
                Destroy(_currentPopup.gameObject);
                _currentPopup = null;
            }

            _bottomPanel.gameObject.SetActive(false);
        }

        private void Start()
        {
            _bottomPanel.Init(_safeArea.GetSafeAreaBottomPadding(), () =>
            {
                _overlay.DOFade(0f, 0.28f).OnComplete(() => _overlay.gameObject.SetActive(false));
            });
        }
    }

    public struct PopupOption
    {
        public string descKey { get; private set; }
        public Action onOptionSelect { get; private set; }
        public Color color { get; private set; }

        public PopupOption(string desc, Action callback, string colorHex = "#1188E4")
        {
            descKey = desc;
            onOptionSelect = callback;
            color = ColorUtility.TryParseHtmlString(colorHex, out Color col) ? col : UIGameColors.Blue;
        }
    }
}
