using TMPro;
using UnityEngine;

namespace WIGO.Userinterface
{
    public class RegisterStepSms : AbstractRegisterStep
    {
        [SerializeField] TMP_InputField _codeField;

        bool _isComplete;

        public string GetInputCode() => _codeField.text;

        public void OnEditCode(string text)
        {
            if (text.Length < 4 && _isComplete)
            {
                _isComplete = false;
                _isStepComplete?.Invoke(false);
            }
            else if (text.Length == 4 && !_isComplete)
            {
                _isComplete = true;
                _isStepComplete?.Invoke(true);
            }
        }

        public override bool CheckPanelComplete()
        {
            bool complete = _codeField.text.Length == 4;
            return complete;
        }

        public override void ResetPanel()
        {
            _codeField.SetTextWithoutNotify(string.Empty);
            _isComplete = false;
        }
    }
}
