using UnityEngine;

namespace WIGO.Userinterface
{
    public class OpeningVideoEventElement : VideoEventElement
    {
        //public override void OnVideoClick()
        //{
        //    switch (_currentMode)
        //    {
        //        case VideoMode.Muted:
        //            _player.SetDirectAudioMute(0, false);
        //            _currentMode = VideoMode.SoundOn;
        //            _soundStatusIcon.sprite = _soundSprites[1];
        //            break;
        //        case VideoMode.SoundOn:
        //            _player.SetDirectAudioMute(0, true);
        //            _currentMode = VideoMode.Muted;
        //            _soundStatusIcon.sprite = _soundSprites[0];
        //            break;
        //        case VideoMode.Paused:
        //            break;
        //        default:
        //            break;
        //    }
        //}

        public void OnOpenVideoFullScreen()
        {

        }

        public override void Clear()
        {
            base.Clear();
            _soundStatusIcon.sprite = _soundSprites[0];
        }
    }
}
