using System.IO;
using System.Threading;
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

        CancellationTokenSource _cts;

        public virtual async void Setup(ProfileData profile)
        {
            if (_avatarImage.texture != null)
            {
                Destroy(_avatarImage.texture);
                _avatarImage.texture = null;
            }

            if (profile == null)
            {
                Debug.LogWarning("User profile is null");
                return;
            }

            string url = profile.avatar;

            _background.color = profile.GetColor();
            _firstLetter.text = profile.firstname.Substring(0, 1);
            if (string.IsNullOrEmpty(url) || string.Compare(url, "null") == 0)
            {
                _background.gameObject.SetActive(true);
                _mask.SetActive(false);
                return;
            }

            _background.gameObject.SetActive(false);
            _mask.SetActive(true);

            var avatar = await DownloadTextureAsync(url);
            if (avatar == null)
            {
                _background.gameObject.SetActive(true);
                _mask.SetActive(false);
                return;
            }

            SetPhotoSize(avatar);
            _avatarImage.texture = avatar;
        }

        public async void Setup(string url)
        {
            if (_avatarImage.texture != null)
            {
                Destroy(_avatarImage.texture);
                _avatarImage.texture = null;
            }

            if (string.IsNullOrEmpty(url))
            {
                Debug.LogWarning("Image URL is null");
                _background.gameObject.SetActive(true);
                _mask.SetActive(false);
                return;
            }

            _background.gameObject.SetActive(false);
            _mask.SetActive(true);

            var avatar = await DownloadTextureAsync(url);
            if (avatar == null)
            {
                _background.gameObject.SetActive(true);
                _mask.SetActive(false);
                return;
            }

            SetPhotoSize(avatar);
            _avatarImage.texture = avatar;
        }

        public async void ChangeAvatar(string path)
        {
            var photo = await DownloadLocalTextureAsync(path);
            if (photo != null)
            {
                SetPhotoSize(photo);
                _avatarImage.texture = photo;
                _background.gameObject.SetActive(false);
                _mask.SetActive(true);
            }
            else
            {
                _background.gameObject.SetActive(true);
                _mask.SetActive(false);
            }
        }

        async Task<Texture2D> DownloadTextureAsync(string url)
        {
            _cts = new CancellationTokenSource();
            var texture = await ServiceLocator.Get<Core.S3ContentClient>().GetTexture(url, _cts.Token);

            if (_cts.IsCancellationRequested)
            {
                return null;
            }

            _cts.Dispose();
            _cts = null;
            return texture != null ? texture : null;
        }

        async Task<Texture2D> DownloadLocalTextureAsync(string url)
        {
            _cts = new CancellationTokenSource();
            var textureBytes = await File.ReadAllBytesAsync(url, _cts.Token);

            if (_cts.IsCancellationRequested)
            {
                return null;
            }

            _cts.Dispose();
            _cts = null;

            if (textureBytes != null && textureBytes.Length > 0)
            {
                var texture = TextureCreator.GetCompressedTexture();
                texture.LoadImage(textureBytes);
                return texture;
            }

            return null;
        }

        void SetPhotoSize(Texture2D texture)
        {
            var size = _avatarImage.rectTransform.rect.size;
            float aspect = (float)texture.width / texture.height;
            float width = aspect > 1f ? aspect * size.y : size.x;
            float height = aspect > 1f ? size.y : size.x / aspect;
            _avatarImage.rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, width);
            _avatarImage.rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, height);
        }
    }
}
