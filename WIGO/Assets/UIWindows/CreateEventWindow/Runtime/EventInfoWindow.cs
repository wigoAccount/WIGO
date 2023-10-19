using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using WIGO.Core;

namespace WIGO.Userinterface
{
    public abstract class EventInfoWindow : UIWindow
    {
        [SerializeField] RectTransform _previewMask;
        [SerializeField] protected RawImage _preview;
        [SerializeField] protected TMP_InputField _descIF;
        [SerializeField] TMP_Text _counterDescLabel;
        [SerializeField] TMP_Text _sendButton;
        [SerializeField] Image _overlay;
        [SerializeField] GameObject _loader;
        [SerializeField] GameObject _doneElement;
        [SerializeField] protected WindowAnimator _animator;
        [SerializeField] Texture2D _tempPreview;
        [SerializeField] CreateEventFailMessage _failMessage;

        protected string _videoPreviewPath;
        protected string _videoPath;
        protected float _videoAspect;

        const float MAX_PREVIEW_HEIGHT = 164f;

        public override void OnOpen(WindowId previous)
        {
            _animator.OnOpen();
        }

        public override void OnBack(WindowId previous, Action callback = null)
        {
            ClearWindow();
            callback?.Invoke();
        }

        public virtual void Setup(string path)
        {
            if (!string.IsNullOrEmpty(path))
            {
                _videoPath = path;
#if UNITY_EDITOR
                var preview = _tempPreview;
#elif UNITY_ANDROID || UNITY_IOS
                var preview = NativeCamera.GetVideoThumbnail(_videoPath);
#endif
                _videoAspect = (float)preview.width / preview.height;
                float previewHeight = _previewMask.rect.width / _videoAspect;
                float height = Mathf.Min(previewHeight, MAX_PREVIEW_HEIGHT);
                _previewMask.sizeDelta = new Vector2(_previewMask.sizeDelta.x, height);
                _preview.texture = preview;
                _preview.rectTransform.sizeDelta = new Vector2(_preview.rectTransform.sizeDelta.x, previewHeight);
            }

            _descIF.SetTextWithoutNotify(string.Empty);
            UIGameColors.SetTransparent(_sendButton, 0.4f);
            _counterDescLabel.text = $"{_descIF.characterLimit}/{_descIF.characterLimit}";
        }

        public void OnBackButtonClick()
        {
            ServiceLocator.Get<UIManager>().CloseCurrent();
        }

        public virtual void OnEditDescText(string text)
        {
            int count = text.Length;
            int remaining = _descIF.characterLimit - count;
            _counterDescLabel.text = $"{remaining}/{_descIF.characterLimit}";
        }

        public virtual async void OnPublishClick()
        {
            if (IsAvailable())
            {
                var created = await CreateEventOrResponse();
                if (created == null)
                {
                    return;
                }

                await Task.Delay(1200);
                ServiceLocator.Get<UIManager>().SwitchTo(WindowId.FEED_SCREEN);
            }
        }

        protected override void Awake()
        {
            _failMessage.Init();
        }

        protected virtual Task<AbstractEvent> CreateEventOrResponse()
        {
            Debug.Log("Send new event...");

            UIGameColors.SetTransparent(_overlay, 0.8f);
            _overlay.gameObject.SetActive(true);
            _loader.SetActive(true);

            return null;
        }

        protected virtual void ClearWindow()
        {
#if !UNITY_EDITOR
            Destroy(_preview.texture);
            if (string.IsNullOrEmpty(_videoPreviewPath))
            {
                if (File.Exists(_videoPreviewPath))
                    File.Delete(_videoPreviewPath);
                
                _videoPreviewPath = string.Empty;
            }
#endif
            _videoPath = null;
            _overlay.gameObject.SetActive(false);
            _doneElement.SetActive(false);
            _failMessage.Close(true);
        }

        protected virtual bool IsAvailable()
        {
            return false;
        }

        protected void CheckIfAvailable()
        {
            float alpha = IsAvailable() ? 1f : 0.4f;
            UIGameColors.SetTransparent(_sendButton, alpha);
        }

        protected void ShowResult(bool success)
        {
            _loader.SetActive(false);

            if (success)
            {
                UIGameColors.SetTransparent(_overlay, 1f);
                _doneElement.SetActive(true);
#if (UNITY_ANDROID || UNITY_IOS) && !UNITY_EDITOR
                if (File.Exists(_videoPath))
                    File.Delete(_videoPath);
#endif
                return;
            }

            _overlay.gameObject.SetActive(false);
            _failMessage.Show();
            Debug.LogError("Fail create event");
        }

        protected async Task<string> UploadVideo(string filePath)
        {
            CancellationTokenSource cts = new CancellationTokenSource();
            cts.CancelAfter(16000);
            string videoName = await ServiceLocator.Get<S3ContentClient>().UploadFile(filePath, ContentType.VIDEO, cts.Token);
            if (cts.IsCancellationRequested)
            {
                cts.Dispose();
                return null;
            }

            return videoName;
        }

        protected async Task<string> UploadPreview()
        {
            if (_preview.texture != null)
            {
                byte[] textureBytes = ((Texture2D)_preview.texture).EncodeToPNG();
                string fileName = $"{DateTime.Now.ToString("MM_dd_yyyy_HH-mm-ss")}_preview.png";
                _videoPreviewPath = Path.Combine(Application.persistentDataPath, fileName);
                await File.WriteAllBytesAsync(_videoPreviewPath, textureBytes);

                CancellationTokenSource cts = new CancellationTokenSource();
                cts.CancelAfter(16000);
                string previewKey = await ServiceLocator.Get<S3ContentClient>().UploadFile(_videoPreviewPath, ContentType.PREVIEW, cts.Token);
                if (cts.IsCancellationRequested)
                {
                    cts.Dispose();
                    return null;
                }

                return previewKey;
            }

            return null;
        }
    }
}
