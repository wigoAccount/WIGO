using System;
using TMPro;
using UnityEngine;
using WIGO.Utility;

namespace WIGO.Userinterface
{
    public class PopupOptionElement : MonoBehaviour
    {
        [SerializeField] LocalizeItem _label;

        Action _onClickOption;

        public void Setup(string key, Action callback, Color color)
        {
            _label.SetupKey(key);
            _onClickOption = callback;
            _label.GetComponent<TMP_Text>().color = color;
        }

        public void OnClickOption() => _onClickOption?.Invoke();
    }
}
