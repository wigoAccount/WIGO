using System;
using UnityEngine;
using WIGO.Utility;

namespace WIGO.Userinterface
{
    public class PopupBottomPanel : MonoBehaviour
    {
        [SerializeField] PanelDragHandler _dragHandler;

        public void Init(float padding, Action onClose)
        {
            RectTransform panel = transform as RectTransform;
            panel.sizeDelta += Vector2.up * padding;
            _dragHandler.Init(onClose);
        }

        public void OpenPanel()
        {
            _dragHandler.OnOpen();
        }

        public void ClosePanel()
        {
            _dragHandler.OnClose();
        }

        public void OnSelectRU(bool value)
        {
            if (value)
            {
                LocalizeManager.ChangeLanguage(Language.RUS);
            }
        }

        public void OnSelectEN(bool value)
        {
            if (value)
            {
                LocalizeManager.ChangeLanguage(Language.ENG);
            }
        }
    }
}
