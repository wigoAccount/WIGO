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
            ClearTexture();

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
            _avatarImage.color = UIGameColors.transparent10;

            var avatar = await DownloadTextureAsync(url);
            if (avatar == null)
            {
                _background.gameObject.SetActive(true);
                _mask.SetActive(false);
                return;
            }

            SetPhotoSize(avatar);
            _avatarImage.color = Color.white;
            _avatarImage.texture = avatar;
        }

        public async void Setup(string url)
        {
            ClearTexture();

            if (string.IsNullOrEmpty(url))
            {
                Debug.LogWarning("Image URL is null");
                _background.gameObject.SetActive(true);
                _mask.SetActive(false);
                return;
            }

            _background.gameObject.SetActive(false);
            _mask.SetActive(true);
            _avatarImage.color = UIGameColors.transparent10;

            var avatar = await DownloadTextureAsync(url);
            if (avatar == null)
            {
                _background.gameObject.SetActive(true);
                _mask.SetActive(false);
                return;
            }

            SetPhotoSize(avatar);
            _avatarImage.color = Color.white;
            _avatarImage.texture = avatar;
        }

        public async void ChangeAvatar(string path, string copyPath)
        {
            Texture2D photo = null;
#if UNITY_IOS && !UNITY_EDITOR
            photo = NativeGallery.LoadImageAtPath(path, markTextureNonReadable: false);
#else
            photo = await DownloadLocalTextureAsync(path);
#endif

            if (photo != null)
            {
                var textureBytes = photo.EncodeToPNG();
                await File.WriteAllBytesAsync(copyPath, textureBytes);
                ServiceLocator.Get<GameModel>().UpdateMyAvatar(photo);
            }
            else
            {
                Debug.LogErrorFormat("Fail to load avatar image at path: {0}", path);
                _background.gameObject.SetActive(true);
                _mask.SetActive(false);
            }
        }

        private void Awake()
        {
            ServiceLocator.Get<GameModel>().OnUpdateAvatar += UpdateAvatarTexture;
        }

        private void OnDestroy()
        {
            ServiceLocator.Get<GameModel>().OnUpdateAvatar -= UpdateAvatarTexture;
            ClearTexture();
        }

        void UpdateAvatarTexture(Texture2D texture)
        {
            ClearTexture();
            SetPhotoSize(texture);
            _avatarImage.texture = texture;
            _background.gameObject.SetActive(false);
            _mask.SetActive(true);
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
            try
            {
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
            }
            catch (System.Exception ex)
            {
                Debug.LogErrorFormat("Error load image bytes. Exception: {0}", ex.Message);
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

        void ClearTexture()
        {
            if (_avatarImage.texture != null)
            {
                Destroy(_avatarImage.texture);
                _avatarImage.texture = null;
            }
        }
    }
}
