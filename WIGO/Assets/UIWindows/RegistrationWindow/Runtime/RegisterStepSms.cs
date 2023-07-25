using TMPro;
using UnityEngine;

namespace WIGO.Userinterface
{
    public class RegisterStepSms : AbstractRegisterStep
    {
        [SerializeField] TMP_InputField _codeField;
        [SerializeField] GameObject _errorTip;

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
            //_errorTip.SetActive(!complete);
            return complete;
        }
    }
}
