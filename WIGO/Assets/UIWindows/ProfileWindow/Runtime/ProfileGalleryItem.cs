using System.IO;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;
using WIGO.Core;

namespace WIGO.Userinterface
{
    public class ProfileGalleryItem : MonoBehaviour
    {
        [SerializeField] RawImage _photo;
        [SerializeField] GameObject _addButton;

        CancellationTokenSource _cts;
        string _url = "https://i.pinimg.com/736x/6f/5a/21/6f5a212ab85e159a590a9d1a917f6eeb--basketball-art-basketball-players.jpg";

        public void SetEmpty()
        {
            _photo.gameObject.SetActive(false);
            _addButton.SetActive(true);
            Destroy(_photo.texture);
            _photo.texture = null;
            _cts = null;
        }

        public async void SetPhoto(string path)
        {
            _photo.gameObject.SetActive(true);
            _addButton.SetActive(false);
            _cts = new CancellationTokenSource();
            var photo = await NetService.GetRemoteTexture(_url, _cts.Token);

            if (_cts.IsCancellationRequested || photo == null)
            {
                return;
            }

            _cts = null;
            _photo.texture = photo;
            SetPhotoSize(photo);
        }

        public void OnAddClick()
        {
#if UNITY_EDITOR
            var path = UnityEditor.EditorUtility.OpenFilePanel("Show all images (.png)", "D:/Downloads/Pathfinder/Characters", "png");
            //var files = Directory.GetFiles("D:/Downloads/Pathfinder/Characters");
            if (string.IsNullOrEmpty(path))
                return;

            OnAddImage(path);
#elif UNITY_ANDROID || UNITY_IOS
            NativeGallery.GetImageFromGallery(OnAddImage);
#endif
        }

        public void OnClear()
        {
            Destroy(_photo.texture);
            _photo.texture = null;
        }

        private void OnDisable()
        {
            _cts?.Cancel();
            _cts = null;
        }

        async void OnAddImage(string path)
        {
            var photo = await DownloadTextureAsync(path);
            if (photo != null)
            {
                _addButton.SetActive(false);
                _photo.gameObject.SetActive(true);
                _photo.texture = photo;

                SetPhotoSize(photo);
            }
        }

        public async Task<string> UploadPhoto()
        {
            await Task.Delay(400);
            return null;
        }

        async Task<Texture2D> DownloadTextureAsync(string url)
        {
            var textureBytes = await File.ReadAllBytesAsync(url);
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
            var size = _photo.rectTransform.rect.size;
            float aspect = (float)texture.width / texture.height;
            float width = aspect > 1f ? aspect * size.y : size.x;
            float height = aspect > 1f ? size.y : size.x / aspect;
            _photo.rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, width);
            _photo.rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, height);
        }
    }
}
