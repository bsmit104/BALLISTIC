using System;
using System.Collections;
using System.Collections.Generic;
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
    private List<Collider> cols = new List<Collider>();

    void OnTriggerEnter(Collider col)
    {
        if (col.gameObject.CompareTag("Surfaces") || col.gameObject.CompareTag("Floor"))
        {
            cols.Add(col);
            inContactWith = cols.Count;
        }
    }

    void OnTriggerExit(Collider col)
    {
        if (col.gameObject.CompareTag("Surfaces") || col.gameObject.CompareTag("Floor"))
        {
            cols.Remove(col);
            inContactWith = cols.Count;
        }
    }

    public void Reset()
    {
        for (int i = 0; i < cols.Count; i++) 
        {
            if (cols[i] == null)
            {
                cols.RemoveAt(i);
                i--;
            }
        }
        inContactWith = cols.Count;
    }
}
