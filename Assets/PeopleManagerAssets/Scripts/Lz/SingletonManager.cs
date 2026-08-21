using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SingletonManager<T> : MonoBehaviour where T : MonoBehaviour
{
    private static T instance;
    public static T Ins { get { return GetInstance(); } }
    private static T GetInstance()
    {
        if (instance == null)
        {
            if ((T)FindObjectOfType(typeof(T)) != null)
            {
                instance = (T)FindObjectOfType(typeof(T));
            }
            else
            {
                GameObject go = new GameObject("" + typeof(T));
                instance = go.AddComponent<T>();
                DontDestroyOnLoad(instance);
            }
        }
        return instance;
    }

    protected virtual void Awake()
    {
        instance = this as T;
    }

    protected virtual void OnDestroy()
    {
        instance = null;
    }

    protected virtual void OnDisable()
    {
        
    }
    protected virtual void Update()
    {
        
    }
}

