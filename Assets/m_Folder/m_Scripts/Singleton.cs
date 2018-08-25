using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Singleton<T> : MonoBehaviour where T:MonoBehaviour{
    private static T _instance;

    public static T Instance
    {
        get
        {
            if(null == _instance)
            {
                string insName = string.Format("{0}",typeof(T));
                _instance = new GameObject(insName).AddComponent<T>();
            }
            return _instance;
        }
    }

}
