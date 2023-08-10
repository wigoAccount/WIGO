using System;

namespace WIGO.Utility
{
    public enum NativeMessageType
    {
        Video,
        Location,
        MyLocation,
        Other
    }

    public static class MessageRouter
    {
        public static Action<NativeMessageType, string> onMessageReceive;

        public static void RouteMessage(NativeMessageType type, string message)
        {
            onMessageReceive?.Invoke(type, message);
        }
    }
}
