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
        transform.SetParent(null);
    }

    public void DeactivateRagdoll()
    {
        DeactivateColliders();
        transform.SetParent(parent);
        transform.localPosition = Vector3.zero;
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
        if ((col = root.gameObject.GetComponent<Collider>()) != null && NetworkPlayer.Local.Runner.IsServer)
        {
            col.enabled = state;
            root.gameObject.GetComponent<Rigidbody>().isKinematic = !state;
        }
        NetworkPosition pos = null;
        if ((pos = root.gameObject.GetComponent<NetworkPosition>()) != null)
        {
            pos.enabled = state;
            pos.ForceUpdate = true;
        }

        for (int i = 0; i < root.childCount; i++)
        {
            RecurseColliders(root.GetChild(i), state);
        }
    }
}
