using System;
using System.Collections.Generic;
using UnityEngine;
using WIGO.Userinterface;

namespace WIGO.Core
{
    [Serializable]
    public class AbstractEvent
    {
        public string uid;
        public string created;
        public string about;
        public string video;
        public string preview;
        public ProfileData author;
        public Location location;
        public string address;
        public int waiting;

        public float AspectRatio { get
            {
                float.TryParse(preview, out float aspect);
                if (aspect <= 0f)
                    aspect = 9f / 16f;
                return aspect;
            } 
        }

        public virtual bool IsResponse() => false;
    }

    [Serializable]
    public class Event : AbstractEvent
    {
        public enum EventStatus
        {
            active,
            closed
        }

        public string status;
        public string title;
        public int duration;
        public string area;
        public int[] tags;

        public EventStatus GetStatus()
        {
            if (Enum.TryParse(status, out EventStatus enumType))
            {
                return enumType;
            }

            return EventStatus.active;
        }
    }

    [Serializable]
    public class Request : AbstractEvent
    {
        public enum RequestStatus
        {
            decline,
            wait,
            accept
        }

        public string status;
        public Event @event;

        public RequestStatus GetStatus()
        {
            if (Enum.TryParse(status, out RequestStatus enumType))
            {
                return enumType;
            }

            return RequestStatus.wait;
        }

        public override bool IsResponse() => true;
    }

    [Serializable]
    public struct Location
    {
        public string longitude;
        public string latitude;

        public override string ToString()
        {
            return $"{longitude},{latitude}";
        }
    }

    [Serializable]
    public class EventCard
    {
        [SerializeField] string _id;
        [SerializeField] string _userId;
        [TextArea]
        [SerializeField] string _description;
        [SerializeField] List<string> _hashtags;
        [SerializeField] string _videoPath;
        [SerializeField] float _videoAspect;
        [SerializeField] int _distanceTime;
        [SerializeField] int _remainingTime;
        [SerializeField] int _participants;
        [SerializeField] string _location;
        [SerializeField] string _status;
        [SerializeField] bool _response;

        public string GetId() => _id;
        // [TODO]: replace later
        public string GetUser() => _userId;
        public string GetDescription() => _description;
        public IReadOnlyList<string> GetHashtags() => _hashtags;
        public string GetVideoPath() => _videoPath;
        public float GetVideoAspect() => _videoAspect;
        public int CalculateDistanceTime() => _distanceTime;
        public int GetRemainingTime() => _remainingTime;
        public EventGroupSizeType GetGroupSizeType() => _participants > 1 ? EventGroupSizeType.Group : EventGroupSizeType.Single;
        public string GetLocation() => _location;
        public EventStatus GetStatus()
        {
            if (Enum.TryParse(_status, out EventStatus enumType))
            {
                return enumType;
            }

            return EventStatus.NotAccepted;
        }
        public bool IsResponse() => _response;

        public void UpdateStatus(EventStatus status) => _status = status.ToString();

        public bool HasCategory(EventCategory category)
        {
            return _hashtags.Exists(x => string.Compare(x, category.ToString()) == 0);
        }

        public static EventCard CreateEmpty()
        {
            var empty = new EventCard();
            empty.SetParams("2un0jt746qkp98", 4);
            return empty;
        }

        public void SetParams(string userId, int participants)
        {
            _id = "82gam325skweh9";
            _userId = userId;
            _participants = participants;
            _remainingTime = UnityEngine.Random.Range(300, 3600);
            _status = ((EventStatus)UnityEngine.Random.Range(0, 3)).ToString();
            _description = "Let’s have a party today! Join me here";
            _response = UnityEngine.Random.Range(0f, 1f) > 0.5f;

            // temp
            _videoPath = "FakeProfile/IMG_9674.MP4";
            _videoAspect = 0.5472f;
        }

        public void SetResponse() => _response = true;
    }

    public enum EventGroupSizeType
    {
        None,
        Single,
        Group
    }

    public enum EventGenderType
    {
        Any,
        Female,
        Male
    }

    public enum EventStatus
    {
        NotAccepted,
        Accepted,
        Watched,
        Denied
    }
}
