using UnityEngine;
using TMPro;

namespace WIGO.Userinterface
{
    public class RegisterStepPhone : AbstractRegisterStep
    {
        [SerializeField] TMP_InputField _phoneField;

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
            return complete;
        }

        public override void ResetPanel()
        {
            _phoneField.SetTextWithoutNotify(string.Empty);
        }
    }
}
