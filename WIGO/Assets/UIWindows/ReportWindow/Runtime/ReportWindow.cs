using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace WIGO.Userinterface
{
    public class ReportWindow : UIWindow
    {
        public void OnBackButtonClick()
        {
            ServiceLocator.Get<UIManager>().CloseCurrent();
        }
    }
}
