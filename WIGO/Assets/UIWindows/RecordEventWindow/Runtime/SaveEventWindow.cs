using System.Globalization;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
using WIGO.Core;

namespace WIGO.Userinterface
{
    public class SaveEventWindow : EventInfoWindow
    {
        Core.Event _acceptedEvent;
        bool _available;

        public override void Setup(string path)
        {
            base.Setup(path);
            _available = false;
        }

        public void Setup(string path, Core.Event accepted)
        {
            _acceptedEvent = accepted;
            Setup(path);
        }

        public override void OnEditDescText(string text)
        {
            base.OnEditDescText(text);

            if (_available && string.IsNullOrEmpty(text))
            {
                _available = false;
                CheckIfAvailable();
            }
            else if (!_available && !string.IsNullOrEmpty(text))
            {
                _available = true;
                CheckIfAvailable();
            }
        }

        protected override void ClearWindow()
        {
            base.ClearWindow();
            _acceptedEvent = null;
        }

        protected override bool IsAvailable()
        {
            return _available;
        }

        protected override async Task<AbstractEvent> CreateEventOrResponse()
        {
            if (_acceptedEvent == null)
            {
                Debug.LogError("Event you want make request to is null");
                return null;
            }

            base.CreateEventOrResponse();

            string preview = await UploadPreview();
            string video = await UploadVideo(_videoPath);
            if (string.IsNullOrEmpty(video) || string.IsNullOrEmpty(preview))
            {
                Debug.LogError("Fail upload video or preview");
                return null;
            }

            CreateResponseRequest request = new CreateResponseRequest()
            {
                eventid = _acceptedEvent.uid,
                about = _descIF.text,
                video = video,
                preview = preview,
                video_aspect = _videoAspect.ToString(CultureInfo.InvariantCulture)
            };

            var model = ServiceLocator.Get<GameModel>();
            var cts = new CancellationTokenSource();
            cts.CancelAfter(8000);

            var myRequest = await NetService.TryCreateRequest(request, model.GetUserLinks().data.address, model.ShortToken, cts.Token);
            ShowResult(myRequest != null);

            return myRequest;
        }
    }
}
