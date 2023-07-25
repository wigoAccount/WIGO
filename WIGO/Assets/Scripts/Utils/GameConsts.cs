using System.Collections.Generic;
using UnityEngine;
using WIGO.Userinterface;

namespace WIGO.Core
{
    public struct GameConsts
    {
        public static float WINDOW_DEFAULT_OPEN_DURATION = 0.4f;
        public static int RECORD_VIDEO_SECONDS = 15;

        public static string GetCategoryLabel(EventCategory category)
        {
            switch (category)
            {
                case EventCategory.All:
                    return "All";
                case EventCategory.Party:
                    return "Party";
                case EventCategory.Outside:
                    return "Outside";
                case EventCategory.Sport:
                    return "Sport";
                case EventCategory.Other:
                    return "Other";
                default:
                    return "All";
            }
        }

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
