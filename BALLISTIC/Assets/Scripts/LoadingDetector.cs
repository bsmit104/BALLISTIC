using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LoadingDetector : MonoBehaviour
{
    public static bool IsLoaded;

    void Start()
    {
        IsLoaded = true;
    }

    void OnDestroy()
    {
        IsLoaded = false;
    }
}
