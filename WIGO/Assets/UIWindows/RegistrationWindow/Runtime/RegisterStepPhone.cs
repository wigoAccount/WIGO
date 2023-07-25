using UnityEngine;
using TMPro;

namespace WIGO.Userinterface
{
    public class RegisterStepPhone : AbstractRegisterStep
    {
        [SerializeField] TMP_InputField _phoneField;
        [SerializeField] GameObject _errorTip;

        bool _isComplete;

        public string GetPhoneNumber() => _phoneField.text;

        public void OnEditNumber(string text)
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
            bool complete = !string.IsNullOrEmpty(_phoneField.text);
            //_errorTip.SetActive(!complete);
            return complete;
        }
    }
}
