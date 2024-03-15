using System.Collections;
using System.Collections.Generic;
using Fusion;
using JetBrains.Annotations;
using UnityEngine;

/// <summary>
/// Responsible for synchronizing player transform values.
/// Gives the local player state authority over their player object's transform.
/// </summary>
public class PlayerPosition : NetworkBehaviour
{
    private Rigidbody rig;

    // True if this object is the local player
    public bool HasAuthority = false;

    public override void Spawned()
    {
        rig = GetComponent<Rigidbody>();
    }

    public override void FixedUpdateNetwork()
    {
        if (!HasAuthority) return;

        RPC_EnforceState(transform.position, transform.eulerAngles.y, rig.velocity.y);
    }

    [Rpc(RpcSources.All, RpcTargets.All, HostMode = RpcHostMode.SourceIsHostPlayer, Channel = RpcChannel.Unreliable)]
    public void RPC_EnforceState(Vector3 position, float yRotation, float yVelocity)
    {
        if (HasAuthority) return;
        transform.position = position;
        transform.eulerAngles = new Vector3(transform.eulerAngles.x, yRotation, transform.eulerAngles.z);
        rig.velocity = new Vector3(rig.velocity.x, yVelocity, rig.velocity.z);
    }
}
