using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace WIGO.Userinterface
{
    public class RegisterStepBirthday : AbstractRegisterStep
    {
        [SerializeField] TMP_InputField _dayIF;
        [SerializeField] TMP_InputField _monthIF;
        [SerializeField] TMP_InputField _yearIF;
        [SerializeField] Toggle _confirmToggle;
        [SerializeField] GameObject _errorTip;

        bool _dayComplete;
        bool _monthComplete;
        bool _yearComplete;

        public void OnEditDay(string text)
        {
            if (string.IsNullOrEmpty(text) && _dayComplete)
            {
                _dayComplete = false;
                _isStepComplete?.Invoke(false);
            }
            else if (!string.IsNullOrEmpty(text) && !_dayComplete)
            {
                _dayComplete = true;
                bool summaryComplete = _dayComplete && _monthComplete && _yearComplete && _confirmToggle.isOn;
                _isStepComplete?.Invoke(summaryComplete);
            }
        }

        public void OnEditMonth(string text)
        {
            if (string.IsNullOrEmpty(text) && _monthComplete)
            {
                _monthComplete = false;
                _isStepComplete?.Invoke(false);
            }
            else if (!string.IsNullOrEmpty(text) && !_monthComplete)
            {
                _monthComplete = true;
                bool summaryComplete = _dayComplete && _monthComplete && _yearComplete && _confirmToggle.isOn;
                _isStepComplete?.Invoke(summaryComplete);
            }
        }

        public void OnEditYear(string text)
        {
            if (string.IsNullOrEmpty(text) && _yearComplete)
            {
                _yearComplete = false;
                _isStepComplete?.Invoke(false);
            }
            else if (!string.IsNullOrEmpty(text) && !_yearComplete)
            {
                _yearComplete = true;
                bool summaryComplete = _dayComplete && _monthComplete && _yearComplete && _confirmToggle.isOn;
                _isStepComplete?.Invoke(summaryComplete);
            }
        }

        public void OnConfirm(bool value)
        {
            bool summaryComplete = _dayComplete && _monthComplete && _yearComplete && value;
            _isStepComplete?.Invoke(summaryComplete);
        }

        public override bool CheckPanelComplete()
        {
            bool summaryComplete = _dayComplete && _monthComplete && _yearComplete && _confirmToggle.isOn;
            //_errorTip.SetActive(!summaryComplete);
            return summaryComplete;
        }
    }
}
