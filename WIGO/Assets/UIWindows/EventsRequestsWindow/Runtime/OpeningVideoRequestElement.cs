using System;
using UnityEngine;
using TMPro;
using WIGO.Core;
using System.Threading;
using System.Threading.Tasks;

namespace WIGO.Userinterface
{
    public class OpeningVideoRequestElement : VideoEventElement
    {
        [SerializeField] TMP_Text _usernameLabel;
        [SerializeField] TMP_Text _scoreEventLabel;
        [SerializeField] TMP_Text _scoreRequestLabel;
        [SerializeField] TMP_Text _descLabel;
        [SerializeField] TMP_Text _distanceLabel;

        Action<OpeningVideoRequestElement, bool> _onManageRequest;

        public void Setup(Request card, Action<OpeningVideoRequestElement, bool> onManageRequest)
        {
            if (card == null)
            {
                Debug.LogError("Request data is null");
                return;
            }

            _onManageRequest = onManageRequest;
            _descLabel.text = card.about;
            _distanceLabel.text = string.Format("{0} min from me", card.waiting);
            SetupUserInfo(card.author);
            SetupVideo(card.video, card.AspectRatio);
        }

        public override void OnVideoClick()
        {
            switch (_currentMode)
            {
                case VideoMode.Muted:
                    _player.SetDirectAudioMute(0, false);
                    _currentMode = VideoMode.SoundOn;
                    _soundStatusIcon.sprite = _soundSprites[1];
                    break;
                case VideoMode.SoundOn:
                    _player.SetDirectAudioMute(0, true);
                    _currentMode = VideoMode.Muted;
                    _soundStatusIcon.sprite = _soundSprites[0];
                    break;
                case VideoMode.Paused:
                    break;
                default:
                    break;
            }
        }

        public void OnOpenVideoFullScreen()
        {

        }

        public override void Clear()
        {
            base.Clear();
            _soundStatusIcon.sprite = _soundSprites[0];
        }

        public void OnManageRequest(bool accept)
        {
            _onManageRequest?.Invoke(this, accept);
        }

        void SetupUserInfo(ProfileData profile)
        {
            _usernameLabel.text = profile.nickname;
            var scoreEvents = Math.Round(UnityEngine.Random.Range(0f, 5f), 1).ToString();
            var scoreRequests = Math.Round(UnityEngine.Random.Range(0f, 5f), 1).ToString();

            _scoreEventLabel.text = scoreEvents;
            _scoreRequestLabel.text = scoreRequests;
        }
    }
}
