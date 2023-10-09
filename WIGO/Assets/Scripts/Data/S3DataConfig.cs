using UnityEngine;

namespace WIGO.Core
{
    [CreateAssetMenu(menuName = "Content/Configs/S3 Data config")]
    public class S3DataConfig : ScriptableObject
    {
        [SerializeField] string _yandexCloudURL;
        [SerializeField] string _accessKey;
        [SerializeField] string _secretKey;
        [Space]
        [SerializeField] string _bucketName;
        [SerializeField] string _photoFolderName;
        [SerializeField] string _videoFolderName;

        public string GetYandexCloudURL() => _yandexCloudURL;
        public string GetAccessKey() => _accessKey;
        public string GetSecretKey() => _secretKey;
        public string GetBucketName() => _bucketName;
        public string GetPhotoFolderName() => _photoFolderName;
        public string GetVideoFolderName() => _videoFolderName;
    }
}
