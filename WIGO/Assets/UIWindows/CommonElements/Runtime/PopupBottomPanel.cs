using System;
using UnityEngine;

namespace WIGO.Userinterface
{
    public class PopupBottomPanel : MonoBehaviour
    {
        [SerializeField] PanelDragHandler _dragHandler;

        Action<bool> _onAnswer;

        public void Init(float padding, Action onClose)
        {
            RectTransform panel = transform as RectTransform;
            panel.sizeDelta += Vector2.up * padding;
            _dragHandler.Init(onClose);
        }

        public void OpenPanel(Action<bool> callback = null)
        {
            _onAnswer = callback;
            _dragHandler.OnOpen();
        }

        public void ClosePanel()
        {
            _dragHandler.OnClose();
        }

        public void OnPositiveAnswer()
        {
            _onAnswer?.Invoke(true);
        }

        //public void OnSelectRU(bool value)
        //{
        //    if (value)
        //    {
        //        LocalizeManager.ChangeLanguage(Language.RUS);
        //    }
        //}

        //public void OnSelectEN(bool value)
        //{
        //    if (value)
        //    {
        //        LocalizeManager.ChangeLanguage(Language.ENG);
        //    }
        //}
    }
}
