using UnityEngine;
using System.Runtime.InteropServices;
using TMPro;

public class Showcase : MonoBehaviour
{
    [SerializeField] TMP_Text _statusLabel;

#if UNITY_IOS
    [DllImport("__Internal")]
    private static extern void NativeiOSCode_runNativeCode(string input);
#endif

    void Start()
    {
        runNativeiOSCode("Run Native iOS Code in Unity");
    }

    void runNativeiOSCode(string input)
    {
#if UNITY_IOS && !UNITY_EDITOR
        NativeiOSCode_runNativeCode(input);
        _statusLabel.text = $"Status: {input}";
#else
        Debug.LogWarning("No iOS Device!!");
        _statusLabel.text = "Status: No iOS Device!";
#endif
    }
}