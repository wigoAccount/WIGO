using WIGO.Userinterface;
using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Content/Storage/UI Windows Storage")]
public class UIWindowStorage : ScriptableObject
{
    [SerializeField] List<UIWindowData> _windows;

    public bool TryGetWindowPrefabById(WindowId id, out UIWindow prefab)
    {
        UIWindowData data = _windows.Find(x => x.GetId() == id);
        if (data != null)
        {
            prefab = data.GetPrefab();
            return true;
        }

        prefab = null;
        return false;
    }
}

[Serializable]
public class UIWindowData
{
    [SerializeField] WindowId _id;
    [SerializeField] UIWindow _windowPrefab;

    public WindowId GetId() => _id;
    public UIWindow GetPrefab() => _windowPrefab;
}
