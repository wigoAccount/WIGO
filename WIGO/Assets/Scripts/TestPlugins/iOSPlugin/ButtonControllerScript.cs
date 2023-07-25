using AOT;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Runtime.InteropServices;

public class ButtonControllerScript : MonoBehaviour
{
    #region open controllers
    [DllImport("__Internal")]
    private static extern void startSwiftCameraController();
    
    [DllImport("__Internal")]
    private static extern void startSwiftMapController();
    
    [DllImport("__Internal")]
    private static extern void startSwiftTestController();
    #endregion

    #region delegates
    public delegate void SwiftTestPluginVideoDidSave(string value);
    [DllImport("__Internal")]
    private static extern void setSwiftTestPluginVideoDidSave(SwiftTestPluginVideoDidSave callBack);

    public delegate void SwiftTestPluginLocationDidSend(string value);
    [DllImport("__Internal")]
    private static extern void setSwiftTestPluginLocationDidSend(SwiftTestPluginLocationDidSend callBack);
    #endregion

    #region delegate handlers
    [MonoPInvokeCallback(typeof(SwiftTestPluginVideoDidSave))]
    public static void setSwiftTestPluginVideoDidSave(string value)
    {
        Debug.Log("SwiftTestPlugin video url " + value);
    }
    
    [MonoPInvokeCallback(typeof(SwiftTestPluginLocationDidSend))]
    public static void setSwiftTestPluginLocationDidSend(string value)
    {
        Debug.Log("SwiftTestPlugin current location: " + value);
    }
    #endregion
    
    
    public void OnPressCameraButton()
    {
        startSwiftCameraController();
    }
    
    public void OnPressMapButton()
    {
        startSwiftMapController();
    }
    
    public void OnPressTestButton()
    {
        startSwiftTestController();
    }
}