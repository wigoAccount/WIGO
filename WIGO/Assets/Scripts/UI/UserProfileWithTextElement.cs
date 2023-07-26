using UnityEngine;
using TMPro;

namespace WIGO.Userinterface
{
    public class UserProfileWithTextElement : UserProfileElement
    {
        [SerializeField] TMP_Text _username;

        public override void Setup(ProfileData profile)
        {
            _username.text = profile == null ? string.Empty : profile.firstname;
            base.Setup(profile);
        }
    }
}
