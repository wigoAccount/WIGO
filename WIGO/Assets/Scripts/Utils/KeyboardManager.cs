using UnityEngine;
using WIGO.Userinterface;

namespace WIGO.Utility
{
    public class KeyboardManager
    {
        float _screenHeight;
        const float KEYBOARD_HEIGHT = 0.36f;

        public KeyboardManager()
        {
            _screenHeight = ServiceLocator.Get<UIManager>().GetCanvasSize().y;
        }

        public KeyboardManager(float height)
        {
            _screenHeight = height;
        }

        public float GetKeyboardHeight()
        {
            float relative = GetKeyboardRelativeHeight();
            return _screenHeight * relative;
        }

        float GetKeyboardRelativeHeight()
        {
#if UNITY_EDITOR
            return KEYBOARD_HEIGHT;
#elif UNITY_ANDROID
            using AndroidJavaClass UnityClass = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
            AndroidJavaObject View = UnityClass.GetStatic<AndroidJavaObject>("currentActivity").Get<AndroidJavaObject>("mUnityPlayer").Call<AndroidJavaObject>("getView");

            using AndroidJavaObject rect = new AndroidJavaObject("android.graphics.Rect");
            View.Call("getWindowVisibleDisplayFrame", rect);
            return (float)(Screen.height - rect.Call<int>("height")) / Screen.height;
#else
            float heightInPixels = TouchScreenKeyboard.area.height * Screen.height / Display.main.systemHeight;
            return heightInPixels / Screen.height;
#endif
        }
    }
}
