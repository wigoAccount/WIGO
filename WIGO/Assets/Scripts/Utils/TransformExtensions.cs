using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class TransformExtensions
{
    public static void DestroyChildren(this Transform parent)
    {
        foreach (Transform child in parent)
        {
            GameObject.Destroy(child.gameObject);
        }
    }

    public static Transform FindChildInHierarchy(this Transform transform, string name)
    {
        if (transform.name.ToLower() == name.ToLower())
        {
            return transform;
        }

        foreach (Transform child in transform)
        {
            Transform result = child.FindChildInHierarchy(name);

            if (result != null)
            {
                return result;
            }
        }

        return null;
    }
}
