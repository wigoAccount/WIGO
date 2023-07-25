using UnityEngine;
using UnityEngine.UI;

namespace WIGO.Userinterface
{
    public static class UIGameColors
    {
        public static Color MessageSentColor
        {
            get { return ColorUtility.TryParseHtmlString(messageSentColor, out Color color) ? color : Color.white; }
        }

        public static Color Blue
        {
            get { return ColorUtility.TryParseHtmlString(blue, out Color color) ? color : Color.white; }
        }

        public static Color Red
        {
            get { return ColorUtility.TryParseHtmlString(RED_HEX, out Color color) ? color : Color.white; }
        }

        public static Color Gray
        {
            get { return ColorUtility.TryParseHtmlString(gray, out Color color) ? color : Color.white; }
        }

        public static Color Purple
        {
            get { return ColorUtility.TryParseHtmlString(purple, out Color color) ? color : Color.white; }
        }

        public static Color Green
        {
            get { return ColorUtility.TryParseHtmlString(green, out Color color) ? color : Color.white; }
        }

        public readonly static Color transparent20 = new Color(255f / 255f, 255f / 255f, 255f / 255f, 0.2f);
        public readonly static Color transparent10 = new Color(255f / 255f, 255f / 255f, 255f / 255f, 0.1f);
        public readonly static Color transparentBlue = new Color(17f / 255f, 136f / 255f, 238f / 255f, 0.3f);

        public const string RED_HEX = "#F82943";
        const string messageSentColor = "#828282";
        const string blue = "#1188EE";
        const string gray = "#9D9D9D";
        const string purple = "#A845D7";
        const string green = "#41BC46";

        public static void SetTransparent<T>(T _object, float alpha = 0f) where T : MaskableGraphic
        {
            _object.color = new Color(_object.color.r, _object.color.g, _object.color.b, alpha);
        }
    }
}
