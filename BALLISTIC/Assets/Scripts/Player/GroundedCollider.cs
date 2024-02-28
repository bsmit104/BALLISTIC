using System;
using System.Collections;
using System.Collections.Generic;
using Palmmedia.ReportGenerator.Core.Parser.Analysis;
using UnityEngine;

public class GroundedCollider : MonoBehaviour
{
    private Rigidbody _rig = null;
    private Rigidbody Rig {
        get {
            if (_rig == null)
            {
                _rig = transform.parent.GetComponent<Rigidbody>();
            }
            return _rig;
        }
    }

    public bool IsGrounded 
    { 
        get { 
            return inContactWith > 0 && Rig.velocity.y <= 0; 
        } 
    }

    [SerializeField] private int inContactWith = 0;

    void OnTriggerEnter(Collider col)
    {
        if (col.gameObject.CompareTag("Surfaces") || col.gameObject.CompareTag("Floor"))
        {
            inContactWith++;
        }
    }

    void OnTriggerExit(Collider col)
    {
        if (col.gameObject.CompareTag("Surfaces") || col.gameObject.CompareTag("Floor"))
        {
            inContactWith--;
        }
    }

    public void Reset()
    {
        inContactWith = 0;
    }
}
