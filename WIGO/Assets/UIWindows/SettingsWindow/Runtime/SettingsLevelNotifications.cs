using UnityEngine;

namespace WIGO.Userinterface
{
    public class SettingsLevelNotifications : SettingsLevelWindow
    {
        [SerializeField] SwitchToggleButton[] _toggles;

        protected override void Awake()
        {
            base.Awake();
            var settings = ServiceLocator.Get<GameModel>().GetNotifications();
            foreach (var toggle in _toggles)
            {
                toggle.Subscribe(OnSwitchNotificationToggle);
            }

            _toggles[0].SetOn(settings.responses);
            _toggles[1].SetOn(settings.newMessages);
            _toggles[2].SetOn(settings.newEvent);
            _toggles[3].SetOn(settings.expireEvent);
            _toggles[4].SetOn(settings.areYouOK);
            _toggles[5].SetOn(settings.estimate);
        }

        void OnSwitchNotificationToggle(bool isOn)
        {
            NotificationSettings settings = new NotificationSettings()
            {
                responses = _toggles[0].IsOn(),
                newMessages = _toggles[1].IsOn(),
                newEvent = _toggles[2].IsOn(),
                expireEvent = _toggles[3].IsOn(),
                areYouOK = _toggles[4].IsOn(),
                estimate = _toggles[5].IsOn()
            };

            ServiceLocator.Get<GameModel>().SaveNotifications(settings);
        }
    }
}
