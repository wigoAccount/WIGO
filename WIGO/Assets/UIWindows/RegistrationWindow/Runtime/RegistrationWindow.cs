using UnityEngine;
using DG.Tweening;
using WIGO.Core;
using System.Threading;
using WIGO.Utility;
using AppleAuth;
using AppleAuth.Enums;
using AppleAuth.Extensions;

namespace WIGO.Userinterface
{
    public class RegistrationWindow : UIWindow
    {
        [SerializeField] AbstractRegisterStep[] _steps;
        [SerializeField] UISelectableButton _nextButton;
        [SerializeField] GameObject _backButton;
        [SerializeField] RectTransform _content;
        [Space]
        [SerializeField] GameObject _signInAppleScreen;
        [SerializeField] GameObject _registrationWindow;
        [SerializeField] GameObject _loadingWindow;
        [SerializeField] GameObject _permissionsWindow;
        [SerializeField] GameObject _notificationsWindow;
        [SerializeField] GameObject _permissionsWaitStatus;
        [SerializeField] GameObject _permissionsButton;
        [Space]
        [SerializeField] GameObject[] _signInButtonStates;

        int _currentStep;
        bool _switching;

        string _appleId;
        string _tempToken;
        ProfileData _profile;
        CancellationTokenSource _cts;
        IAppleAuthManager _appleAuthManager;

        public override void OnOpen(WindowId previous)
        {
            _backButton.SetActive(false);
            _steps[0].SetPanelActive(true);
        }

        public override void OnClose(WindowId next, System.Action callback = null)
        {
            _currentStep = 0;
            _switching = false;
            _tempToken = string.Empty;
            _profile = null;

            for (int i = 0; i < _steps.Length; i++)
            {
                _steps[i].ResetPanel();
                _steps[i].gameObject.SetActive(i == 0);
            }

            _nextButton.SetLoadingVisible(false);
            _nextButton.SetEnabled(false);
            _content.anchoredPosition = new Vector2(0f, _content.anchoredPosition.y);

            _signInAppleScreen.SetActive(true);
            _registrationWindow.SetActive(false);
            _loadingWindow.SetActive(false);
            _permissionsWindow.SetActive(false);
            _notificationsWindow.SetActive(false);
            _permissionsButton.SetActive(true);
            _permissionsWaitStatus.SetActive(false);
            _signInButtonStates[0].SetActive(true);
            _signInButtonStates[1].SetActive(false);
            callback?.Invoke();
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

        public void OnSignInClick()
        {
            if (_switching)
            {
                return;
            }

            _switching = true;
            _signInButtonStates[0].SetActive(false);
            _signInButtonStates[1].SetActive(true);

#if UNITY_IOS && !UNITY_EDITOR
            var loginArgs = new AppleAuthLoginArgs(LoginOptions.IncludeEmail | LoginOptions.IncludeFullName);
            _appleAuthManager.LoginWithAppleId(
                loginArgs,
                credential =>
                {
                    _appleId = credential.User;
                    _switching = false;
                    _signInAppleScreen.SetActive(false);
                    _registrationWindow.SetActive(true);
                },
                error =>
                {
                    _switching = false;
                    _signInButtonStates[0].SetActive(true);
                    _signInButtonStates[1].SetActive(false);
                    var authorizationErrorCode = error.GetAuthorizationErrorCode();
                    Debug.LogWarning("Sign in with Apple failed " + authorizationErrorCode.ToString() + " " + error.ToString());
                });
#else
            _appleId = "Unity test apple id";
            _switching = false;
            _signInAppleScreen.SetActive(false);
            _registrationWindow.SetActive(true);
#endif
        }

        public async void OnNextButtonClick()
        {
            if (_switching)
            {
                return;
            }

            var model = ServiceLocator.Get<GameModel>();
            switch (_steps[_currentStep].GetStepType())
            {
                case RegisterStep.PhoneNumber:
                    RegisterStepPhone stepPhone = _steps[_currentStep] as RegisterStepPhone;
                    if (stepPhone.CheckPanelComplete())
                    {
                        _cts = new CancellationTokenSource();
                        _nextButton.SetLoadingVisible(true);

                        var data = await NetService.TryRegisterNewAccount(stepPhone.GetPhoneNumber(), _appleId, _cts.Token);
                        _nextButton.SetLoadingVisible(false);
                        if (_cts.IsCancellationRequested)
                        {
                            Debug.Log("User cancelled registration");
                            _cts.Dispose();
                            _cts = null;
                            return;
                        }

                        _cts.Dispose();
                        _cts = null;
                        if (string.IsNullOrEmpty(data.ltoken) || string.IsNullOrEmpty(data.stoken) || data.profile == null)
                        {
                            Debug.LogError("Wrong confirm registration");
                            return;
                        }

                        _profile = data.profile;
                        model.SaveTokens(data.ltoken, data.stoken, data.links);
                    }
                    break;
                case RegisterStep.SmsAprove:
                    RegisterStepSms stepAprove = _steps[_currentStep] as RegisterStepSms;
                    if (stepAprove.CheckPanelComplete())
                    {
                        _cts = new CancellationTokenSource();
                        _nextButton.SetLoadingVisible(true);

                        var data = await NetService.TryConfirmRegister(_tempToken, stepAprove.GetInputCode(), _cts.Token);
                        _nextButton.SetLoadingVisible(false);
                        if (_cts.IsCancellationRequested)
                        {
                            Debug.Log("User cancelled confirm sms");
                            _cts.Dispose();
                            _cts = null;
                            return;
                        }

                        _cts.Dispose();
                        _cts = null;
                        if (string.IsNullOrEmpty(data.ltoken) || string.IsNullOrEmpty(data.stoken) || data.profile == null)
                        {
                            Debug.LogError("Wrong confirm registration");
                            return;
                        }

                        _profile = data.profile;
                        model.SaveTokens(data.ltoken, data.stoken, data.links);
                    }
                    
                    break;
                case RegisterStep.Nickname:
                    RegisterStepUsername stepName = _steps[_currentStep] as RegisterStepUsername;
                    if (stepName.CheckPanelComplete())
                    {
                        _profile.firstname = stepName.GetUsername();
                    }
                    break;
                case RegisterStep.Birthday:
                    RegisterStepBirthday stepBirthday = _steps[_currentStep] as RegisterStepBirthday;
                    if (stepBirthday.CheckPanelComplete())
                    {
                        var birthday = stepBirthday.GetBirthday();
                        _cts = new CancellationTokenSource();
                        _nextButton.SetLoadingVisible(true);

                        bool isInvalid = await NetService.CheckBirthdayInvalid(birthday, model.ShortToken, _cts.Token);
                        _nextButton.SetLoadingVisible(false);
                        if (_cts.IsCancellationRequested)
                        {
                            Debug.Log("User cancelled registration");
                            _cts.Dispose();
                            _cts = null;
                            return;
                        }

                        _cts.Dispose();
                        _cts = null;
                        if (isInvalid)
                        {
                            Debug.LogError("Wrong birthday format");
                            return;
                        }

                        _profile.birthday = birthday;
                    }
                    break;
                case RegisterStep.Gender:
                    RegisterStepGender stepGender = _steps[_currentStep] as RegisterStepGender;
                    ContainerData gender = stepGender.GetSelectedGender();

                    string userUpdJson = "{\"birthday\":" + $"\"{_profile.birthday}\"," +
                                        "\"firstname\":" + $"\"{_profile.firstname}\"," +
                                        "\"gender\":" + $"\"{gender.uid}\"}}";

                    _cts = new CancellationTokenSource();
                    _registrationWindow.SetActive(false);
                    _loadingWindow.SetActive(true);
                    Debug.LogFormat("<color=cyan>UPD: {0}</color>", userUpdJson);

                    var updProfile = await NetService.TryUpdateUser(userUpdJson, model.ShortToken, _cts.Token);
                    _loadingWindow.SetActive(false);
                    if (_cts.IsCancellationRequested)
                    {
                        Debug.Log("User cancelled registration");
                        _cts.Dispose();
                        _cts = null;
                        _registrationWindow.SetActive(true);
                        return;
                    }

                    _cts.Dispose();
                    _cts = null;
                    if (updProfile == null)
                    {
                        Debug.LogError("Wrong update profile");
                        _registrationWindow.SetActive(true);
                        return;
                    }

                    _profile = updProfile;
                    model.SaveProfile(updProfile);
                    model.FinishRegister();
                    //_permissionsWindow.SetActive(true);

                    _notificationsWindow.SetActive(true);
#if UNITY_IOS && !UNITY_EDITOR
                    MessageIOSHandler.OnAllowLocationPermission();
#endif
                    return;
                case RegisterStep.Permissions:
                case RegisterStep.Notification:
                    break;
                default:
                    break;
            }

            MoveStep(1);
        }

        public void OnAskPermissions()
        {
            _permissionsButton.SetActive(false);
            _permissionsWaitStatus.SetActive(true);
            PermissionsRequestManager.RequestPermissionLocation((result) =>
            {
                _permissionsWaitStatus.SetActive(false);
                _permissionsButton.SetActive(true);
                if (result)
                {
                    _permissionsWindow.SetActive(false);
                    _notificationsWindow.SetActive(true);
                }
            });
        }

        public void OnSetNotifications(bool accept)
        {
            NotificationSettings settings = new NotificationSettings()
            {
                areYouOK = accept,
                estimate = accept,
                expireEvent = accept,
                newEvent = accept,
                newMessages = accept,
                responses = accept
            };

            ServiceLocator.Get<GameModel>().SaveNotifications(settings);
            ServiceLocator.Get<UIManager>().Open<FeedWindow>(WindowId.FEED_SCREEN);
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
                _backButton.SetActive(_currentStep > 0);
                _switching = false;
            });
        }
    }
}
