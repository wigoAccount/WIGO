using System;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace WIGO.Userinterface
{
    public class ProfileInfoArea : MonoBehaviour
    {
        [SerializeField] TMP_Text _displayNameLabel;
        [SerializeField] TMP_Text _usernameLabel;
        [SerializeField] TMP_Text _aboutLabel;
        [SerializeField] CategoryEventElement _tagPrefab;
        [SerializeField] RectTransform _tagsContent;

        [SerializeField] RawImage[] _photoes;

        ProfileData _currentProfile;

        const float SPACING = 12f;
        const float PADDINGS = 16f;

        public float GetHeight() => ((RectTransform)transform).rect.height;

        public void Setup(ProfileData profile, Action callback = null)
        {
            _currentProfile = profile;
            UpdateInfo(callback);
        }

        public async void UpdateInfo(Action callback = null)
        {
            _displayNameLabel.text = _currentProfile.firstname;
            _usernameLabel.text = $"@{_currentProfile.nickname}";
            _aboutLabel.text = string.IsNullOrEmpty(_currentProfile.about) ? "Информация о себе" : _currentProfile.about;

            float screenWidth = ServiceLocator.Get<UIManager>().GetCanvasSize().x;
            _tagsContent.DestroyChildren();
            float xPos = 16f;
            float yPos = -36f;
            foreach (var tag in _currentProfile.tags)
            {
                var tagElement = Instantiate(_tagPrefab, _tagsContent);
                tagElement.Setup(tag.name);
                RectTransform tagRect = tagElement.transform as RectTransform;
                float width = tagElement.GetWidth();

                if (screenWidth - PADDINGS * 2 - xPos < width)
                {
                    xPos = 16f;
                    yPos -= tagRect.sizeDelta.y + SPACING;
                }

                tagRect.anchoredPosition = new Vector2(xPos, yPos);
                xPos += tagElement.GetWidth() + SPACING;
            }
            _tagsContent.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, -yPos + 32f);

            foreach (var photo in _photoes)
            {
                Destroy(photo.texture);
                photo.texture = null;
                photo.transform.parent.gameObject.SetActive(false);
            }
            var photoes = _currentProfile.photos;
            if (photoes == null || photoes.Length == 0)
            {
                callback?.Invoke();
                return;
            }

            int index = 0;
            foreach (var url in photoes)
            {
                var photo = await DownloadTextureAsync(url);
                if (photo == null)
                    continue;

                _photoes[index].texture = photo;
                _photoes[index].transform.parent.gameObject.SetActive(true);
                SetPhotoSize(_photoes[index], photo);
                index++;
            }

            callback?.Invoke();
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

        void SetPhotoSize(RawImage image, Texture2D texture)
        {
            var size = image.rectTransform.rect.size;
            float aspect = (float)texture.width / texture.height;
            float width = aspect > 1f ? aspect * size.y : size.x;
            float height = aspect > 1f ? size.y : size.x / aspect;
            image.rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, width);
            image.rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, height);
        }
    }
}
