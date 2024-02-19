using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GroundedCollider : MonoBehaviour
{
    public bool isGrounded 
    { 
        get { return inContactWith > 0; } 
        set { inContactWith = value ? 1 : 0; }
    }

    [SerializeField] private int inContactWith = 0;

    void OnTriggerEnter(Collider col)
    {
        if (col.gameObject.CompareTag("Floor"))
        {
            inContactWith++;
        }
    }

    void OnTriggerExit(Collider col)
    {
        if (col.gameObject.CompareTag("Floor"))
        {
            inContactWith = Math.Max(0, inContactWith - 1);
        }
    }
}
