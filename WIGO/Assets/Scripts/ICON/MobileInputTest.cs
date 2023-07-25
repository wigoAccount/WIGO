using System.Collections;
using UnityEngine;
using TMPro;

public class MobileInputTest : MonoBehaviour
{
    [SerializeField] RectTransform _canvas;
    [SerializeField] TMP_InputField _inputField;

    RectTransform _fieldRect;
    float _ratio;
    bool _controlHeight;

    public void OnActivateIF(string text)
    {
        TouchScreenKeyboard.Android.consumesOutsideTouches = false;
        _controlHeight = true;
        Debug.Log("Activated");
    }

    public void OnDeactivateIF(string text)
    {
        Debug.Log("Deactivated");
        StartCoroutine(DelayDeactivation());
    }

    public void OnSendMessage()
    {
        _inputField.text = string.Empty;
    }

    public void ManualCloseInput()
    {
        _inputField.DeactivateInputField();
    }

    private void Start()
    {
        _fieldRect = _inputField.transform as RectTransform;
        _ratio = _canvas.sizeDelta.y / Screen.height;
        TouchScreenKeyboard.Android.consumesOutsideTouches = false;
    }

    private void LateUpdate()
    {
        if (_controlHeight)
        {
            int keyboardHeightPx = GetKeyboardHeight();
            float height = keyboardHeightPx * _ratio;
            _fieldRect.anchoredPosition = Vector2.up * height;
        }
    }

    int GetKeyboardHeight(bool includeInput = false)
    {
#if UNITY_EDITOR
        return 0;
#elif UNITY_ANDROID
        using AndroidJavaClass UnityClass = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
        AndroidJavaObject View = UnityClass.GetStatic<AndroidJavaObject>("currentActivity").Get<AndroidJavaObject>("mUnityPlayer").Call<AndroidJavaObject>("getView");

        using AndroidJavaObject Rct = new AndroidJavaObject("android.graphics.Rect");
        View.Call("getWindowVisibleDisplayFrame", Rct);

        return Screen.height - Rct.Call<int>("height");
        //using (AndroidJavaClass unityClass = new AndroidJavaClass("com.unity3d.player.UnityPlayer"))
        //{
        //    AndroidJavaObject unityPlayer = unityClass.GetStatic<AndroidJavaObject>("currentActivity").Get<AndroidJavaObject>("mUnityPlayer");
        //    AndroidJavaObject view = unityPlayer.Call<AndroidJavaObject>("getView");
        //    AndroidJavaObject dialog = unityPlayer.Get<AndroidJavaObject>("mSoftInputDialog");
        //    if (view == null || dialog == null)
        //        return 0;

        //    var decorHeight = 0;
        //    if (includeInput)
        //    {
        //        AndroidJavaObject decorView = dialog.Call<AndroidJavaObject>("getWindow").Call<AndroidJavaObject>("getDecorView");
        //        if (decorView != null)
        //            decorHeight = decorView.Call<int>("getHeight");
        //    }

        //    using AndroidJavaObject rect = new AndroidJavaObject("android.graphics.Rect");
        //    view.Call("getWindowVisibleDisplayFrame", rect);
        //    return Screen.height - rect.Call<int>("height") + decorHeight;
        //}
#elif UNITY_IOS
        return (int)TouchScreenKeyboard.area.height;
#endif
    }

    public int GetKeyboardSize()
    {
        using AndroidJavaClass UnityClass = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
        AndroidJavaObject View = UnityClass.GetStatic<AndroidJavaObject>("currentActivity").Get<AndroidJavaObject>("mUnityPlayer").Call<AndroidJavaObject>("getView");

        using AndroidJavaObject Rct = new AndroidJavaObject("android.graphics.Rect");
        View.Call("getWindowVisibleDisplayFrame", Rct);

        return Screen.height - Rct.Call<int>("height");
    }

    IEnumerator DelayDeactivation()
    {
        yield return new WaitForSeconds(0.4f);
        _controlHeight = false;
    }
}
