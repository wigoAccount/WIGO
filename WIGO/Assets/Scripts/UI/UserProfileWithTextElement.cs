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

        protected override void Awake()
        {
            base.Awake();
            ServiceLocator.Get<GameModel>().OnUpdateProfile += UpdateUsername;
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();
            ServiceLocator.Get<GameModel>().OnUpdateProfile -= UpdateUsername;
        }

        void UpdateUsername()
        {
            var profile = ServiceLocator.Get<GameModel>().GetMyProfile();
            _username.SetText(profile == null ? string.Empty : profile.firstname);
        }
    }
}
