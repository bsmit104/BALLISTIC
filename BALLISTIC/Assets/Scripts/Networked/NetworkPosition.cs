using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Fusion;

/// <summary>
/// Replacement for NetworkTransform. Gives host complete authority over transforms.
/// DO NOT EDIT THIS SCRIPT UNLESS YOU KNOW WHAT YOU'RE DOING.
/// </summary>
public class NetworkPosition : NetworkBehaviour
{
    private ChangeDetector detector;

    // Synced transform properties
    [Networked, HideInInspector] public Vector3 position { get; set; }
    [Networked, HideInInspector] public Vector3 rotation { get; set; }

    // Synced rigidbody properties for local simulation
    [Networked, HideInInspector] public Vector3 velocity { get; set; }
    [Networked, HideInInspector] public Vector3 angVelocity { get; set; }

    // If false, clients will not adhere to host simulation
    [Networked, HideInInspector] public bool networkEnabled { get; set; }

    // If this transform has a parent, then follow the parent, not the host's simulation
    private bool hasParent = false;

    // Override to force the client to follow the host's simulation
    [HideInInspector] public bool ForceUpdate = false;

    private Rigidbody rig;

    public override void Spawned()
    {
        detector = GetChangeDetector(ChangeDetector.Source.SimulationState);
        rig = GetComponent<Rigidbody>();
    }

    public override void Render()
    {
        hasParent = transform.parent != null;

        if (Runner.IsServer)
        {
            networkEnabled = !hasParent;
        }

        // Do not apply host's previous tick to their current tick
        // OR ignore host's state if the local transform has a parent, and isn't forced to update
        if (Runner.IsServer || (hasParent && !ForceUpdate)) return;

        // Apply all networked properties on client simulations
        foreach (var attrName in detector.DetectChanges(this))
        {
            switch (attrName)
            {
                // Apply transform properties
                case nameof(position):
                    transform.position = position;
                    break;
                case nameof(rotation):
                    transform.eulerAngles = rotation;
                    break;

                // Apply rigidbody properties for local simulation
                case nameof(velocity):
                    if (rig && !rig.isKinematic) rig.velocity = velocity;
                    break;
                case nameof(angVelocity):
                    if (rig && !rig.isKinematic) rig.angularVelocity = angVelocity;
                    break;
                
                // Host can tell clients if they should be ignoring updates
                case nameof(networkEnabled):
                    hasParent = !networkEnabled;
                    break;
            }
        }
    }

    // Regularly check if the client needs to be forced to match the current state
    const float REFRESH_RATE = 3f;
    float refreshTimer = 0f;

    public override void FixedUpdateNetwork()
    {
        // Only the host will set networked properties
        if (!Runner.IsServer) return;

        // Get values from host's transform and rigidbody
        position = transform.position;
        rotation = transform.eulerAngles;
        if (rig)
        {
            velocity = rig.velocity;
            angVelocity = rig.angularVelocity;
        }

        if (refreshTimer <= 0)
        {
            // If the object isn't moving, then clients could be de-synced from the host
            if (velocity == Vector3.zero || angVelocity == Vector3.zero)
            {
                // So force clients to match the host
                RPC_EnforceState(position, rotation, velocity, angVelocity);
            } 
            refreshTimer = REFRESH_RATE;
        }

        refreshTimer -= Runner.DeltaTime;
    }

    [Rpc(RpcSources.StateAuthority, RpcTargets.All, HostMode = RpcHostMode.SourceIsServer)]
    public void RPC_EnforceState(Vector3 pos, Vector3 rot, Vector3 vel, Vector3 angVel)
    {
        // Only enforce state on clients, 
        // and on objects that don't have parents,
        // aren't forced to update,
        // and have their network position script enabled
        if (Runner.IsServer || (hasParent && !ForceUpdate) || !enabled) return;
        transform.position = pos;
        transform.eulerAngles = rot;
        if (rig && !rig.isKinematic)
        {
            rig.velocity = vel;
            rig.angularVelocity = angVel;
        }
    }
}
