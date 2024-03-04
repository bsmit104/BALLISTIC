using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class drop : MonoBehaviour
{
    // public TextMeshProUGUI output;
    // Start is called before the first frame update
    public void HandleInputData(int val) {
        if (val == 0) {
            Lman.English = true;
            Lman.Chinese = false;
            Lman.Arabic = false;
            Lman.Spanish = false;
        }
        if (val == 1) {
            Lman.English = false;
            Lman.Chinese = true;
            Lman.Arabic = false;
            Lman.Spanish = false;
        }
        if (val == 2) {
            Lman.English = false;
            Lman.Chinese = false;
            Lman.Arabic = true;
            Lman.Spanish = false;
        }
        if (val == 3) {
            Lman.English = false;
            Lman.Chinese = false;
            Lman.Arabic = false;
            Lman.Spanish = true;
        }
    }
}