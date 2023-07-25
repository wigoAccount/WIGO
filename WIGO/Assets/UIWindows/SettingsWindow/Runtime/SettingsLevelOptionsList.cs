using System.Collections.Generic;

namespace WIGO.Userinterface
{
    public class SettingsLevelOptionsList : SettingsLevelWindow
    {
        public void OnReportClick()
        {
            var popupManager = ServiceLocator.Get<UIManager>().GetPopupManager();
            List<PopupOption> options = new List<PopupOption>();
            options.Add(new PopupOption("Popup/Spam", () => popupManager.CloseCurrentPopup()));
            options.Add(new PopupOption("Popup/NotWorking", () =>
            {
                popupManager.CloseCurrentPopup();
                ServiceLocator.Get<UIManager>().Open<ReportWindow>(WindowId.REPORT_SCREEN);
            }));
            options.Add(new PopupOption("Popup/SendFeedback", () => popupManager.CloseCurrentPopup()));
            options.Add(new PopupOption("Popup/Cancel", () => popupManager.CloseCurrentPopup(), UIGameColors.RED_HEX));
            popupManager.AddPopup("Popup/AskProblem", options);
        }
    }
}
