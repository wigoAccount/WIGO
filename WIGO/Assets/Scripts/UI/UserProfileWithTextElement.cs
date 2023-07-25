using UnityEngine;
using TMPro;

namespace WIGO.Userinterface
{
    public class UserProfileWithTextElement : UserProfileElement
    {
        [SerializeField] TMP_Text _username;

        public void Setup(string url, string displayname)
        {
            _username.text = displayname;
            Setup(url);
        }
    }
}
