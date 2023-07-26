using Crystal;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using WIGO.Core;

namespace WIGO.Userinterface
{
    public class ChatWindowView : MonoBehaviour
    {
        [SerializeField] RectTransform _header;
        [SerializeField] RectTransform _bottomPanel;
        [SerializeField] SafeArea _safeArea;
        [SerializeField] RectTransform _messagePanel;
        [Space]
        [SerializeField] TMP_Text _usernameLabel;
        [SerializeField] UserProfileElement _userProfile;

        public void Init()
        {
            _header.sizeDelta += Vector2.up * _safeArea.GetSafeAreaUpperPadding();
            _bottomPanel.sizeDelta += Vector2.up * _safeArea.GetSafeAreaBottomPadding();
        }

        public void SetupInfo(ProfileData profile)
        {
            _usernameLabel.text = profile.nickname;
            _userProfile.Setup(profile);
        }

        public void OnOpenKeyboard()
        {

        }

        public void OnCloseKeyboard()
        {

        }
    }
}
