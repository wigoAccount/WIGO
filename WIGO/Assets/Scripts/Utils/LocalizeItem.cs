using System;
using TMPro;
using UnityEngine;

namespace WIGO.Utility
{
    [RequireComponent(typeof(TMP_Text))]
    public class LocalizeItem : MonoBehaviour
    {
        public Action onTextChanged;

        [SerializeField] string _key;

        TMP_Text _textComponent;
        bool _subscribed;
        string _value;

        public string GetKey() => _key;
        public void SetupKey(string key)
        {
            _key = string.IsNullOrEmpty(key) ? "OSK/Empty" : key;
        }

        public float GetPreferedWidth() => _textComponent.preferredWidth;
        public float GetPreferedHeight() => _textComponent.preferredHeight;

        public void ChangeLanguageText(string text)
        {
            if (text.Contains("%val"))
            {
                _textComponent.text = text.Replace("%val", _value);
            }
            else
            {
                _textComponent.text = text.Replace("<br>", "\r\n");
            }
            
            onTextChanged?.Invoke();
        }

        public void UpdateKey(string newKey)
        {
            if (!_subscribed)
            {
                _textComponent = GetComponent<TMP_Text>();
            }

            _key = string.IsNullOrEmpty(newKey) ? "Common/PasteText" : newKey;
            string text = LocalizeManager.GetTextWithKey(_key);
            ChangeLanguageText(text);
        }

        public void SetupValue(string value) => _value = value;

        private void Start()
        {
            _textComponent = GetComponent<TMP_Text>();
            LocalizeManager.Subscribe(this);
            _subscribed = true;
        }

        private void OnDestroy()
        {
            LocalizeManager.Unscribe(this);
            _subscribed = false;
        }
    }
}
