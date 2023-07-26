using System;
using UnityEngine;

namespace WIGO.Userinterface
{
    public enum RegisterStep
    {
        PhoneNumber,
        SmsAprove,
        Nickname,
        Birthday,
        Gender,
        Permissions,
        Notification
    }

    public class AbstractRegisterStep : MonoBehaviour
    {
        [SerializeField] RegisterStep _step;

        RectTransform _stepPanelRect;
        protected Action<bool> _isStepComplete;

        public RegisterStep GetStepType() => _step;
        public float GetPanelPosition() => _stepPanelRect.anchoredPosition.x - _stepPanelRect.rect.width / 2f;

        public void Initialize(Action<bool> isStepComplete)
        {
            _isStepComplete = isStepComplete;
            _stepPanelRect = transform as RectTransform;
        }

        public void SetPanelActive(bool active)
        {
            if (active)
            {
                _isStepComplete?.Invoke(CheckPanelComplete());
                return;
            }

            gameObject.SetActive(false);
        }

        public virtual bool CheckPanelComplete()
        {
            return true;
        }

        public virtual void ResetPanel()
        {

        }
    }
}
