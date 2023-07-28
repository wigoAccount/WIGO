using UnityEngine;
using UnityEngine.Networking;

namespace WIGO.Userinterface
{
    public class SettingsLevelMain : SettingsLevelWindow
    {
        [SerializeField] EmailConfig _config;
        [SerializeField] string _termsURL;
        [SerializeField] string _politicsURL;

        public void OnQuitAccountClick()
        {
            //var popupManager = ServiceLocator.Get<UIManager>().GetPopupManager();
            //List<PopupOption> options = new List<PopupOption>
            //{
            //    new PopupOption("Popup/NoAnswer", () => popupManager.CloseCurrentPopup()),
            //    new PopupOption("Popup/Quit", OnDeleteAccount, UIGameColors.RED_HEX)
            //};
            //popupManager.AddPopup("Popup/AskQuitAccount", options);
            ServiceLocator.Get<UIManager>().GetPopupManager().OpenBottomPanel(accept =>
            {
                if (accept)
                    OnDeleteAccount();
            });
        }

        public void ResetWindow()
        {
            foreach (var child in _children)
            {
                child.OnClose();
            }

            gameObject.SetActive(true);
        }

        public void OnOnboardingClick()
        {

        }

        public void OnFeedbackClick()
        {
            string url = string.Format("mailto:{0}?subject={1}&body={2}",
                UnityWebRequest.EscapeURL(_config.GetAddress()), UnityWebRequest.EscapeURL(_config.GetSubject()),
                UnityWebRequest.EscapeURL(_config.GetBody()));
            url = url.Replace("+", "%20");
            Application.OpenURL(url);
        }

        public void OnTermsOfUseClick()
        {
            Application.OpenURL(_termsURL);
        }

        public void OnPoliticsClick()
        {
            Application.OpenURL(_politicsURL);
        }

        async void OnDeleteAccount()
        {
            ServiceLocator.Get<UIManager>().GetPopupManager().ResetPopup();
            ServiceLocator.Get<UIManager>().SwitchTo(WindowId.START_SCREEN);

            var model = ServiceLocator.Get<GameModel>();
            await Core.NetService.TryDeleteAccount(model.ShortToken);
            model.Clear();
        }
    }
}
