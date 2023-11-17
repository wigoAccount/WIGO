using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace WIGO.Userinterface
{
    public class RegisterStepGender : AbstractRegisterStep
    {
        [SerializeField] Image[] _buttons;
        [SerializeField] TMP_Text[] _labels;

        Gender _selectedGender = Gender.none;

        public ContainerData GetSelectedGender() => new ContainerData() { uid = 2 - (int)_selectedGender, name = _selectedGender.ToString() };

        public void OnGenderSelect(int index)
        {
            int lastIndex = (int)_selectedGender;
            Gender gender = (Gender)index;
            if (gender == _selectedGender)
            {
                return;
            }

            _selectedGender = gender;
            if (lastIndex < 2)
            {
                _buttons[lastIndex].color = UIGameColors.Gray;
                _labels[lastIndex].color = UIGameColors.Gray;
            }
            
            _buttons[index].color = UIGameColors.Blue;
            _labels[index].color = UIGameColors.Blue;
            _isStepComplete?.Invoke(true);
        }

        public override void ResetPanel()
        {
            _selectedGender = Gender.female;
            _buttons[1].color = UIGameColors.Gray;
            _labels[1].color = UIGameColors.Gray;
            _buttons[0].color = UIGameColors.Gray;
            _labels[0].color = UIGameColors.Gray;
        }

        public override bool CheckPanelComplete()
        {
            return _selectedGender != Gender.none;
        }
    }
}
