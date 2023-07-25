using TMPro;
using UnityEngine;

namespace WIGO.Userinterface
{
    public class RegisterStepUsername : AbstractRegisterStep
    {
        [SerializeField] TMP_InputField _usernameField;
        [SerializeField] GameObject _errorTip;

        bool _isComplete;

        public string GetUsername() => _usernameField.text;

        public void OnEditUsername(string text)
        {
            if (string.IsNullOrEmpty(text) && _isComplete)
            {
                _isComplete = false;
                _isStepComplete?.Invoke(false);
            }
            else if (!string.IsNullOrEmpty(text) && !_isComplete)
            {
                _isComplete = true;
                _isStepComplete?.Invoke(true);
            }
        }

        public override bool CheckPanelComplete()
        {
            bool complete = !string.IsNullOrEmpty(_usernameField.text);
            //_errorTip.SetActive(!complete);
            return complete;
        }

        public void OnClearNameClick()
        {
            _usernameField.SetTextWithoutNotify(string.Empty);
            if (_isComplete)
            {
                _isComplete = false;
                _isStepComplete?.Invoke(false);
            }
        }
    }
}
