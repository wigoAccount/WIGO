using Amazon.Runtime;
using Amazon.S3;
using Amazon.S3.Model;
using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

namespace WIGO.Core
{
    public enum ContentType
    {
        PHOTO,
        VIDEO,
        PREVIEW
    }

    public class S3ContentClient
    {
        S3DataConfig _s3Config;
        AmazonS3Client _s3Client;

        const int EXPIRE_LINK_MINUTES = 10;

        public S3ContentClient(S3DataConfig dataConfig)
        {
            _s3Config = dataConfig;
            var credentials = new BasicAWSCredentials(_s3Config.GetAccessKey(), _s3Config.GetSecretKey());
            var config = new AmazonS3Config
            {
                ServiceURL = string.Format("https://{0}", _s3Config.GetYandexCloudURL())
            };
            _s3Client = new AmazonS3Client(credentials, config);
        }

        public string GetVideoURL(string videoName)
        {
            var request = new GetPreSignedUrlRequest()
            {
                BucketName = _s3Config.GetBucketName(),
                Key = videoName,
                Expires = DateTime.Now.AddMinutes(EXPIRE_LINK_MINUTES)
            };

            var url = _s3Client.GetPreSignedURL(request);
            return url;
        }

        public async Task<Texture2D> GetTexture(string filename, CancellationToken token = default)
        {
            try
            {
                var response = await _s3Client.GetObjectAsync(_s3Config.GetBucketName(), filename, token);
                if (response.HttpStatusCode == System.Net.HttpStatusCode.OK)
                {
                    using StreamReader reader = new StreamReader(response.ResponseStream);
                    using var memstream = new MemoryStream();
                    await reader.BaseStream.CopyToAsync(memstream);
                    var bytes = memstream.ToArray();

                    if (bytes == null || bytes.Length == 0)
                    {
                        Debug.LogWarningFormat("Loaded null bytes from S3 at path '{0}'", filename);
                        return null;
                    }

                    var texture = TextureCreator.GetRGBwithoutAlphaTexture();
                    if (texture.LoadImage(bytes))
                    {
                        texture.Apply();
                        return texture;
                    }

                    Debug.LogWarningFormat("Can't convert bytes to texture 2d from '{0}'", filename);
                    return null;
                }

                Debug.LogWarningFormat("Request status isn't OK: {0}", response.HttpStatusCode);
                return null;
            }
            catch (Exception ex)
            {
                Debug.LogErrorFormat("Get avatar '{0}' request failed. Error: {1}", filename, ex.Message);
                return null;
            }
        }

        public async Task<string> UploadFile(string filePath, ContentType fileType, CancellationToken token = default)
        {
            string extension = Path.GetExtension(filePath);
            var stream = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.Read);
            string bucketName = _s3Config.GetBucketName();
            string name = CreateFileName(fileType, extension);
            var request = new PutObjectRequest()
            {
                BucketName = bucketName,
                Key = name,
                InputStream = stream,
                CannedACL = S3CannedACL.Private
            };

            var response = await _s3Client.PutObjectAsync(request, token);
            if (response.HttpStatusCode == System.Net.HttpStatusCode.OK)
            {
                Debug.Log("File successfully uploated");
                return name;
            }

            Debug.LogErrorFormat("Fail to upload file. Status: {0}", response.HttpStatusCode.ToString());
            return null;
        }

        string CreateFileName(ContentType type, string extension)
        {
            string userId = ServiceLocator.Get<GameModel>().GetUserId();
            string folderName = _s3Config.GetFolderName(type);
            return $"{folderName}/{DateTime.Now.ToString("MM_dd_yyyy_HH-mm-ss")}_{type.ToString()}_{userId}{extension}";
        }
    }
}
