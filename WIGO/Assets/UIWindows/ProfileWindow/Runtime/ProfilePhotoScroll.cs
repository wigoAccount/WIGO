using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using WIGO.Core;
using DG.Tweening;
using System.Threading.Tasks;

namespace WIGO.Userinterface
{
    public class ProfilePhotoScroll : MonoBehaviour
    {
        [SerializeField] RawImage[] _photos;
        [SerializeField] Texture2D _defaultTexture;
        [SerializeField] ProfilePhotoPointElement _pointsElement;

        List<Texture2D> _profilePhotos = new List<Texture2D>();
        int _selected;
        bool _isSwiping;

        public async void Setup(ProfileAvatar data)
        {
            if (data.GetAllPhotos().Count > 0)
            {
                _pointsElement.Setup(data.GetAllPhotos().Count, data.GetSelectedIndex());

                foreach (var url in data.GetAllPhotos())
                {
                    var photo = await DownloadTextureAsync(url);
                    _profilePhotos.Add(photo);
                }

                _selected = data.GetSelectedIndex();
                SetPhoto(0, _selected - 1);
                SetPhoto(1, _selected);
                SetPhoto(2, _selected + 1);
            }
        }

        public void Clear()
        {
            _pointsElement.Clear();
            foreach (var photo in _profilePhotos)
            {
                Destroy(photo);
            }

            _profilePhotos.Clear();

            _photos[0].rectTransform.anchoredPosition = Vector2.left * 360f;
            _photos[1].rectTransform.anchoredPosition = Vector2.zero;
            _photos[2].rectTransform.anchoredPosition = Vector2.right * 360f;
        }

        public void MoveTo(int direction)
        {
            if (_isSwiping)
            {
                return;
            }

            _isSwiping = true;
            int next = _selected - direction;
            if (next < 0)
            {
                next = _profilePhotos.Count - 1;
            }
            else if (next >= _profilePhotos.Count)
            {
                next = 0;
            }
            _selected = next;
            _pointsElement.SetSelected(_selected);

            var animation = DOTween.Sequence();
            foreach (var photo in _photos)
            {
                float pos = photo.rectTransform.anchoredPosition.x + direction * 360f;
                animation.Join(photo.rectTransform.DOAnchorPosX(pos, 0.32f));
            }

            animation.OnComplete(() =>
            {
                if (direction > 0)
                {
                    _photos[2].rectTransform.anchoredPosition = Vector2.left * 360f;
                    var tmp = _photos[2];
                    _photos[2] = _photos[1];
                    _photos[1] = _photos[0];
                    _photos[0] = tmp;
                    SetPhoto(0, _selected - 1);
                }
                else
                {
                    _photos[0].rectTransform.anchoredPosition = Vector2.right * 360f;
                    var tmp = _photos[0];
                    _photos[0] = _photos[1];
                    _photos[1] = _photos[2];
                    _photos[2] = tmp;
                    SetPhoto(2, _selected + 1);
                }

                _isSwiping = false;
            });
        }

        async Task<Texture2D> DownloadTextureAsync(string url)
        {
            var textureBytes = await System.IO.File.ReadAllBytesAsync(url);

            if (textureBytes != null && textureBytes.Length > 0)
            {
                var texture = TextureCreator.GetCompressedTexture();
                texture.LoadImage(textureBytes);
                return texture;
            }

            return null;
        }

        void SetPhoto(int imageIndex, int photoIndex)
        {
            if (photoIndex < 0)
            {
                photoIndex = _profilePhotos.Count - 1;
            }
            else if (photoIndex >= _profilePhotos.Count)
            {
                photoIndex = 0;
            }

            if (_profilePhotos[photoIndex] == null)
            {
                _photos[imageIndex].texture = _defaultTexture;
            }
            else
            {
                _photos[imageIndex].texture = _profilePhotos[photoIndex];
            }
        }
    }
}
