using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace WIGO.Userinterface
{
    public class SettingsLevelRegion : SettingsLevelWindow
    {
        public void OnLanguageSelect()
        {
            ServiceLocator.Get<UIManager>().GetPopupManager().OpenBottomPanel(null);
        }
    }
}
