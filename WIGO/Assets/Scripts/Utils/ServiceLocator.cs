using System;
using UnityEngine;

public static class ServiceLocator
{
    struct Container<TObject> where TObject : class
    {
        public static TObject instance;
    }

    public class Scope<TObject> : IDisposable where TObject : class
    {
        TObject _old;

        public Scope(TObject value)
        {
            _old = ServiceLocator.Get<TObject>();
            ServiceLocator._Set<TObject>(value);
        }

        public void Dispose()
        {
            ServiceLocator._Set<TObject>(_old);
        }
    }

    public static TObject Get<TObject>() where TObject : class
    {
        return Container<TObject>.instance;
    }

    public static void Set<TObject>(TObject value) where TObject : class
    {
        if (Container<TObject>.instance != null)
        {
            Debug.LogErrorFormat("Service already set. '{0}'", typeof(TObject).FullName);
            return;
        }

        Container<TObject>.instance = value;
    }

    public static void Unset<TObject>(TObject value) where TObject : class
    {
        if (!Container<TObject>.instance.Equals(value))
        {
            Debug.LogErrorFormat("Unset failed. Service is not the same. '{0}'", typeof(TObject).FullName);
            return;
        }

        Release<TObject>();
    }

    public static void Release<TObject>() where TObject : class
    {
        Container<TObject>.instance = null;
    }

    static void _Set<TObject>(TObject value) where TObject : class
    {
        Container<TObject>.instance = value;
    }
}
