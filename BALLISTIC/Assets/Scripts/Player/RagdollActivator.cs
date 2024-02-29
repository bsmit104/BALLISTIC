using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RagdollActivator : MonoBehaviour
{
    private Transform parent;

    void Awake()
    {
        parent = transform.parent;
    }
    
    public void ActivateRagdoll()
    {
        ActivateColliders();
        //transform.SetParent(null);
    }

    public void DeactivateRagdoll()
    {
        DeactivateColliders();
        // transform.SetParent(parent);
        // transform.localPosition = Vector3.zero;
    }

    private void ActivateColliders()
    {
        RecurseColliders(transform, true);
    }

    private void DeactivateColliders()
    {
        RecurseColliders(transform, false);
    }

    private void RecurseColliders(Transform root, bool state)
    {
        Collider col = null;
        if ((col = root.gameObject.GetComponent<Collider>()) != null)
        {
            col.enabled = state;
        }

        for (int i = 0; i < root.childCount; i++)
        {
            RecurseColliders(root.GetChild(i), state);
        }
    }
}
