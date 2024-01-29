using System.IO;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
using WIGO.Core;

namespace WIGO.Userinterface
{
    public class ProfileGalleryItem : MonoBehaviour
    {
        [SerializeField] UserProfileElement _profile;

        Texture2D _cachedOldAvatar;
        CancellationTokenSource _cts;
        string _selectedPhotoPath;

        public void OnAddClick()
        {
#if UNITY_EDITOR
            var path = UnityEditor.EditorUtility.OpenFilePanel("Show all images (.png)", "D:/Downloads/Pathfinder/Characters", "png");
            if (string.IsNullOrEmpty(path))
                return;

            OnAddImage(path);
#elif UNITY_ANDROID || UNITY_IOS
            NativeGallery.GetImageFromGallery(OnAddImage);
#endif
        }

        public void OnClear()
        {
            CheckOldCopy();
            _cts?.Cancel();
            _cts?.Dispose();
            _cts = null;
        }

        public void CacheCurrentAvatar()
        {
            if (_cachedOldAvatar != null)
            {
                Destroy(_cachedOldAvatar);
                _cachedOldAvatar = null;
            }

            var avatar = _profile.GetCachedTexture();
            if (avatar != null)
            {
                _cachedOldAvatar = new Texture2D(avatar.width, avatar.height);
                _cachedOldAvatar.LoadImage(avatar.EncodeToPNG(), false);
            }
        }

        public void ApplyCachedAvatar()
        {
            ServiceLocator.Get<GameModel>().UpdateMyAvatar(_cachedOldAvatar);
        }

        private void OnDisable()
        {
            OnClear();
        }

        void OnAddImage(string path)
        {
            CheckOldCopy();
            _selectedPhotoPath = Path.Combine(Application.persistentDataPath, Path.GetFileNameWithoutExtension(path) + ".png");
            _profile.ChangeAvatar(path, _selectedPhotoPath);
        }

        public async Task<string> UploadPhoto()
        {
            if (string.IsNullOrEmpty(_selectedPhotoPath))
            {
                return null;
            }

            _cts = new CancellationTokenSource();
            _cts.CancelAfter(8000);
            var photo = await ServiceLocator.Get<S3ContentClient>().UploadFile(_selectedPhotoPath, ContentType.PHOTO, _cts.Token);

            if (_cts.IsCancellationRequested)
            {
                return null;
            }

            _cts.Dispose();
            _cts = null;
            return photo;
        }

        void CheckOldCopy()
        {
            if (!string.IsNullOrEmpty(_selectedPhotoPath) && File.Exists(_selectedPhotoPath))
            {
                File.Delete(_selectedPhotoPath);
            }
            _selectedPhotoPath = null;
        }
    }
}
