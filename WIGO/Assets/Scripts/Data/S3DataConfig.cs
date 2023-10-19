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
        [SerializeField] string _previewFolderName;

        public string GetYandexCloudURL() => _yandexCloudURL;
        public string GetAccessKey() => _accessKey;
        public string GetSecretKey() => _secretKey;
        public string GetBucketName() => _bucketName;

        public string GetFolderName(ContentType type)
        {
            return type switch
            {
                ContentType.PHOTO => _photoFolderName,
                ContentType.VIDEO => _videoFolderName,
                ContentType.PREVIEW => _previewFolderName,
                _ => null,
            };
        }
    }
}
