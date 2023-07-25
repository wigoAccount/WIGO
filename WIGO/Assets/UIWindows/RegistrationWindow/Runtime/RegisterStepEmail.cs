using UnityEngine;
using TMPro;

namespace WIGO.Userinterface
{
    public class RegisterStepEmail : AbstractRegisterStep
    {
        [SerializeField] TMP_InputField _emailField;
        [SerializeField] GameObject _errorTip;

        bool _isComplete;

        public string GetEmail() => _emailField.text;

        public void OnEditEmail(string text)
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
            string email = _emailField.text;
            var trimmedEmail = email.Trim();

            if (trimmedEmail.EndsWith("."))
            {
                //_errorTip.SetActive(true);
                return false;
            }

            try
            {
                var addr = new System.Net.Mail.MailAddress(email);
                bool complete = addr.Address == trimmedEmail;
                //_errorTip.SetActive(!complete);
                return complete;
            }
            catch
            {
                //_errorTip.SetActive(true);
                return false;
            }
        }
    }
}
