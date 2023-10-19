using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
using WIGO.Core;
using WIGO.Utility;
using Event = WIGO.Core.Event;

namespace WIGO.Userinterface
{
    public class CreateEventWindow : EventInfoWindow
    {
        [SerializeField] CreateEventView _view;
        [SerializeField] HashtagDoubleScroll _hashtagScroll;
        [Multiline]
        [SerializeField] string _testCoordinates;
        [SerializeField] string _testLocation;

        EventGenderType _selectedGender = EventGenderType.Any;
        EventGroupSizeType _selectedSizeType = EventGroupSizeType.None;
        Location _location = new Location();
        string _address;
        bool _locationSelected;

        const int EVENT_WAITING_SECONDS = 1800;

        public override void OnReopen(WindowId previous, UIWindowModel cachedModel)
        {
            _animator.OnReopen();
        }

        public override void CloseUnactive()
        {
            ClearWindow();
        }

        public override void Setup(string path)
        {
            base.Setup(path);
            _hashtagScroll.CreateHashtags(CheckIfAvailable);
        }

        public void OnSelectGender(int index)
        {
            int prevIndex = (int)_selectedGender;
            if (index == prevIndex)
            {
                return;
            }

            _selectedGender = (EventGenderType)index;
            _view.SelectGender(prevIndex, index);
            CheckIfAvailable();
        }

        public void OnSelectGroupSize(int index)
        {
            int prevIndex = (int)_selectedSizeType;
            if (index == prevIndex)
            {
                return;
            }

            _selectedSizeType = (EventGroupSizeType)index;
            _view.SelectGroupSize(prevIndex, index);
            CheckIfAvailable();
        }

        public void OnLocationSelectClick()
        {
#if UNITY_EDITOR
            string sum = string.Join("\r\n", _testCoordinates, _testLocation);
            OnLocationSelected(sum);
#elif UNITY_IOS
            MessageIOSHandler.OnPressMapButton();
#endif
        }

        public void OnEndEditDesc(string text)
        {
            CheckIfAvailable();
        }

        protected override void Awake()
        {
            base.Awake();
            MessageRouter.onMessageReceive += OnReceiveMessage;
        }

        protected void OnDestroy()
        {
            MessageRouter.onMessageReceive -= OnReceiveMessage;
        }

        protected override void ClearWindow()
        {
            base.ClearWindow();
            _hashtagScroll.Clear();
            _view.ResetView(_selectedGender, _selectedSizeType);
            _selectedSizeType = EventGroupSizeType.None;
            _selectedGender = EventGenderType.Any;
            _location = new Location();
            _address = string.Empty;
        }

        protected override async Task<AbstractEvent> CreateEventOrResponse()
        {
            base.CreateEventOrResponse();

            string preview = await UploadPreview();
            string video = await UploadVideo(_videoPath);
            if (string.IsNullOrEmpty(video) || string.IsNullOrEmpty(preview))
            {
                Debug.LogError("Fail upload video or preview");
                return null;
            }

            int genderIndex = (int)_selectedGender;
            CreateEventRequest request = new CreateEventRequest()
            {
                title = "My new event",
                about = _descIF.text,
                waiting = EVENT_WAITING_SECONDS,
                duration = 120,
                gender = genderIndex,
                location = _location,
                address = _address,
                area = _address,
                video = video,
                video_aspect = _videoAspect.ToString(),
                preview = preview,
                tags_add = _hashtagScroll.GetSelectedCategories().ToArray()
            };

            var model = ServiceLocator.Get<GameModel>();
            var cts = new CancellationTokenSource();
            cts.CancelAfter(8000);
            var myEvent = await NetService.TryCreateEvent(request, model.GetUserLinks().data.address, model.ShortToken, cts.Token);
            model.SetMyEvent(myEvent);
            ShowResult(myEvent != null);

            return myEvent;
        }

        protected override bool IsAvailable()
        {
            int categoriesCount = _hashtagScroll.GetSelectedCategories().Count();
            return _locationSelected && !string.IsNullOrEmpty(_descIF.text) && categoriesCount > 0;
        }

        void OnLocationSelected(string value)
        {
            _locationSelected = !string.IsNullOrEmpty(value);
            string[] splitData = value.Split("\r\n");
            if (splitData.Length > 1)
            {
                var location = ParseLocation(splitData[1], splitData[0]);
                _location = location.Item1;
                _view.SetLocation(location.Item2);
                CheckIfAvailable();
            }
            else
            {
                Debug.LogErrorFormat("Can't split data: {0}", value);
            }
        }

        void OnReceiveMessage(NativeMessageType type, string message)
        {
            switch (type)
            {
                case NativeMessageType.Video:
                    break;
                case NativeMessageType.Location:
                    OnLocationSelected(message);
                    break;
                case NativeMessageType.MyLocation:
                case NativeMessageType.Other:
                    Debug.LogFormat("Message: {0}", message);
                    break;
                default:
                    break;
            }
        }

        (Location, string) ParseLocation(string coordinates, string location)
        {
            Location selectedLocation = new Location();
            Debug.LogFormat("1: {0}\r\n2: {1}", coordinates, location);
            try
            {
                string[] splited = coordinates.Replace("\"", "").Split(",");
                if (splited.Length > 1)
                {
                    var longitude = splited[0];
                    var latitude = splited[1];
                    selectedLocation.latitude = latitude.ToString();
                    selectedLocation.longitude = longitude.ToString();
                    Debug.LogFormat("<color=cyan>Latitude: {0}\r\nLongitude: {1}</color>", latitude, longitude);
                }
                else
                    Debug.LogWarningFormat("Can't split coordinates: {0}", coordinates);
            }
            catch (System.Exception)
            {
                Debug.LogWarningFormat("Wrong coordinates format");
            }

            try
            {
                if (location.Contains("YandexMap"))
                {
                    int start = Mathf.Clamp(location.IndexOf(@"\") + 2, 0, int.MaxValue);
                    int end = Mathf.Clamp(location.IndexOf(@"\", start) - 1, 0, int.MaxValue);
                    _address = location.Substring(start, end - start + 1);
                }
                else
                {
                    _address = location.Replace("\"", "");
                }

                Debug.LogFormat("<color=magenta>Location: {0}</color>", _address);
            }
            catch (System.Exception)
            {
                Debug.LogWarningFormat("Wrong location format");
            }

            return (selectedLocation, _address);
        }
    }
}
