using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
using WIGO.Core;

namespace WIGO.Userinterface
{
    public class ProfileGalleryItem : MonoBehaviour
    {
        [SerializeField] UserProfileElement _profile;

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
            _selectedPhotoPath = null;
            _cts?.Cancel();
            _cts?.Dispose();
            _cts = null;
        }

        private void OnDisable()
        {
            OnClear();
        }

        void OnAddImage(string path)
        {
            _selectedPhotoPath = path;
            _profile.ChangeAvatar(path);
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
    }
}
