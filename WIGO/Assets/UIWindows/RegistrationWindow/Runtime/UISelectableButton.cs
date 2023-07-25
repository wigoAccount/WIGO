using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace WIGO.Userinterface
{
    public class UISelectableButton : MonoBehaviour
    {
        [SerializeField] Image _button;
        [SerializeField] TMP_Text _label;
        [SerializeField] GameObject _spinner;

        public void SetEnabled(bool enabled)
        {
            _button.raycastTarget = enabled;
            _button.color = enabled ? UIGameColors.Blue : UIGameColors.transparent20;
            UIGameColors.SetTransparent(_label, enabled ? 1f : 0.5f);
        }

        public void SetLoadingVisible(bool value)
        {
            _button.raycastTarget = !value;
            _spinner.SetActive(value);
        }
    }
}
