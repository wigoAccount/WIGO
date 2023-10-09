using UnityEngine;

namespace WIGO.Core
{
    public struct GameConsts
    {
        public static float WINDOW_DEFAULT_OPEN_DURATION = 0.4f;
        public static int RECORD_VIDEO_SECONDS = 15;
        public static string EDITOR_TEST_VIDEO = "FakeProfile/IMG_9672.MP4";

        public static Color GetRandomColor()
        {
            System.Random rnd = new System.Random();
            float r = (float)rnd.NextDouble();
            float g = (float)rnd.NextDouble();
            float b = (float)rnd.NextDouble();

            return new Color(r, g, b);
        }
    }
}
