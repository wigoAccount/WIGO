using UnityEngine;
using DG.Tweening;
using WIGO.Core;
using System.Threading;

namespace WIGO.Userinterface
{
    public class RegistrationWindow : UIWindow
    {
        [SerializeField] AbstractRegisterStep[] _steps;
        [SerializeField] UISelectableButton _nextButton;
        [SerializeField] RectTransform _content;

        int _currentStep;
        bool _switching;

        string _tempToken;
        string _longToken;
        string _shortToken;
        ProfileData _profile;
        CancellationTokenSource _cts;

        public override void OnOpen(WindowId previous)
        {
            _steps[0].SetPanelActive(true);
        }

        public void OnBackButtonClick()
        {
            if (_switching)
            {
                return;
            }

            if (_cts != null)
            {
                _cts.Cancel();
            }

            if (_currentStep == 0)
            {
                ServiceLocator.Get<UIManager>().CloseCurrent();
                return;
            }

            MoveStep(-1);
        }

        public async void OnNextButtonClick()
        {
            if (_switching)
            {
                return;
            }

            if (_currentStep == _steps.Length - 1)
            {
                _cts = new CancellationTokenSource();
                string email = ((RegisterStepEmail)_steps[2]).GetEmail();
                string username = ((RegisterStepUsername)_steps[3]).GetUsername();
                var updatedUser = await NetService.TryUpdateUser(_profile.uid, _shortToken, email, username, _cts.Token);
                if (_cts.IsCancellationRequested)
                {
                    Debug.Log("User cancelled creating profile");
                    _cts.Dispose();
                    return;
                }

                _cts.Dispose();
                if (updatedUser == null)
                {
                    // [TODO]: Show popup
                    Debug.LogError("Error create profile");
                    return;
                }

                ServiceLocator.Get<UIManager>().Open<FeedWindow>(WindowId.FEED_SCREEN);
                return;
            }

            switch (_steps[_currentStep].GetStepType())
            {
                case RegisterStep.PhoneNumber:
                    RegisterStepPhone stepPhone = _steps[_currentStep] as RegisterStepPhone;
                    if (stepPhone.CheckPanelComplete())
                    {
                        _cts = new CancellationTokenSource();
                        _nextButton.SetLoadingVisible(true);
                        _tempToken = await NetService.TryRegisterNewAccount(stepPhone.GetPhoneNumber(), _cts.Token);
                        _nextButton.SetLoadingVisible(false);
                        if (_cts.IsCancellationRequested)
                        {
                            Debug.Log("User cancelled registration");
                            _cts.Dispose();
                            return;
                        }

                        _cts.Dispose();
                        if (string.IsNullOrEmpty(_tempToken))
                        {
                            // [TODO]: Show popup
                            Debug.LogError("Wrong registration");
                            return;
                        }
                    }
                    break;
                case RegisterStep.SmsAprove:
                    RegisterStepSms stepAprove = _steps[_currentStep] as RegisterStepSms;
                    if (stepAprove.CheckPanelComplete())
                    {
                        _cts = new CancellationTokenSource();
                        _nextButton.SetLoadingVisible(true);
                        var data = await NetService.TryConfirmRegister(_tempToken, stepAprove.GetInputCode(), _cts.Token);
                        _longToken = data.ltoken;
                        _shortToken = data.stoken;
                        _profile = data.profile;
                        _nextButton.SetLoadingVisible(false);
                        if (_cts.IsCancellationRequested)
                        {
                            Debug.Log("User cancelled confirm sms");
                            _cts.Dispose();
                            return;
                        }

                        _cts.Dispose();
                        if (string.IsNullOrEmpty(_longToken))
                        {
                            // [TODO]: Show popup
                            Debug.LogError("Wrong confirm registration");
                            return;
                        }

                        ServiceLocator.Get<GameModel>().SaveLongToken(_longToken);
                        Debug.LogFormat("Long token: {0}\r\nShort token: {1}", _longToken, _shortToken);
                    }
                    
                    break;
                case RegisterStep.Email:
                case RegisterStep.Nickname:
                case RegisterStep.Birthday:
                    break;
                default:
                    break;
            }

            MoveStep(1);
        }

        protected override void Awake()
        {
            foreach (var step in _steps)
            {
                step.Initialize(_nextButton.SetEnabled);
            }
        }

        void MoveStep(int direction)
        {
            _steps[_currentStep + direction].gameObject.SetActive(true);
            float xPos = -_steps[_currentStep + direction].GetPanelPosition();
            _switching = true;
            Tween moveTween = _content.DOAnchorPosX(xPos, 0.2f).OnComplete(() =>
            {
                _currentStep += direction;
                _steps[_currentStep - direction].SetPanelActive(false);
                _steps[_currentStep].SetPanelActive(true);
                _switching = false;
            });
        }
    }
}
