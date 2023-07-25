using System.Collections.Generic;

namespace WIGO.Userinterface
{
    public class SettingsLevelMain : SettingsLevelWindow
    {
        public void OnQuitAccountClick()
        {
            var popupManager = ServiceLocator.Get<UIManager>().GetPopupManager();
            List<PopupOption> options = new List<PopupOption>
            {
                new PopupOption("Popup/NoAnswer", () => popupManager.CloseCurrentPopup()),
                new PopupOption("Popup/Quit", () => popupManager.CloseCurrentPopup(), UIGameColors.RED_HEX)
            };
            popupManager.AddPopup("Popup/AskQuitAccount", options);
        }

        public void ResetWindow()
        {
            foreach (var child in _children)
            {
                child.OnClose();
            }

            gameObject.SetActive(true);
        }
    }
}
