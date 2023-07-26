using System.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace WIGO.Userinterface
{
    public class UserProfileElement : MonoBehaviour
    {
        [SerializeField] GameObject _mask;
        [SerializeField] RawImage _avatarImage;
        [SerializeField] Image _background;
        [SerializeField] TMP_Text _firstLetter;

        public virtual async void Setup(ProfileData profile)
        {
            if (_avatarImage.texture != null)
            {
                Destroy(_avatarImage.texture);
                _avatarImage.texture = null;
            }

            if (profile == null)
            {
                Debug.LogError("User profile is null");
                return;
            }

            string url = profile.avatar;

            if (string.IsNullOrEmpty(url))
            {
                _background.color = profile.GetColor();
                _firstLetter.text = profile.nickname.Substring(0, 1);
                _background.gameObject.SetActive(true);
                _mask.SetActive(false);
                return;
            }

            _background.gameObject.SetActive(false);
            _mask.SetActive(true);

            var avatar = await DownloadTextureAsync(url);
            _avatarImage.texture = avatar;
        }

        async Task<Texture2D> DownloadTextureAsync(string url)
        {
            string fullPath = System.IO.Path.Combine(Application.streamingAssetsPath, url);
            var textureBytes = await System.IO.File.ReadAllBytesAsync(fullPath);

            if (textureBytes != null && textureBytes.Length > 0)
            {
                var texture = TextureCreator.GetCompressedTexture();
                texture.LoadImage(textureBytes);
                return texture;
            }

            return null;
        }
    }
}
