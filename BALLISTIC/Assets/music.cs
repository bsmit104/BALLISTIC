using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class music : MonoBehaviour
{
    
    public static music isPlay;

    public void Awake() {

        if (isPlay == null)
        {
            // Keep this GameObject alive between scene loads
            DontDestroyOnLoad(gameObject);
            isPlay = this;
        }
        else
        {
            // Destroy duplicate GameObjects
            Destroy(gameObject);
        }
        DontDestroyOnLoad(gameObject);
    }

    // public void AdjustVolume(float volume) {
    //     gameObject.volume = volume;
    // }
}
