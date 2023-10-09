using System;

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

        public bool ContainsTag(int uid)
        {
            return uid == 0 || Array.Exists(tags, x => x == uid);
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
