using System;
using TMPro;
using UnityEngine;

namespace WIGO.Userinterface
{
    public class PopupOptionElement : MonoBehaviour
    {
        [SerializeField] TMP_Text _label;

        Action _onClickOption;

        public void Setup(string key, Action callback, Color color)
        {
            _label.SetText(key);
            _onClickOption = callback;
            _label.color = color;
        }

        public void OnClickOption() => _onClickOption?.Invoke();
    }
}
