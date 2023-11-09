using UnityEngine;

namespace WIGO.Core
{
    public struct GameConsts
    {
        public static float WINDOW_DEFAULT_OPEN_DURATION = 0.4f;
        public static int RECORD_VIDEO_SECONDS = 15;

        public static Color GetRandomColor()
        {
            System.Random rnd = new System.Random();
            float r = (float)rnd.NextDouble();
            float g = (float)rnd.NextDouble();
            float b = (float)rnd.NextDouble();

            return new Color(r, g, b);
        }

        public static Location ParseLocation(string coordinates)
        {
            Location loc = new Location();
            string[] splited = coordinates.Replace("\"", "").Split(",");
            if (splited.Length > 1)
            {
                var longitude = splited[0];
                var latitude = splited[1];
                loc.latitude = latitude;
                loc.longitude = longitude;
                Debug.LogFormat("<color=yellow>MY LOCATION: Latitude: {0}\r\nLongitude: {1}</color>", latitude, longitude);
            }
            else
                Debug.LogWarningFormat("Can't split coordinates: {0}", coordinates);

            return loc;
        }
    }
}
