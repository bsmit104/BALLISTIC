using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Fusion;

/// <summary>
/// Replacement for NetworkTransform, but only synchronizes position and rotation.
/// </summary>
public class NetworkPosition : NetworkBehaviour
{
    private ChangeDetector detector;

    [Networked, HideInInspector] public Vector3 position { get; set; }
    [Networked, HideInInspector] public Vector3 rotation { get; set; }
    [Networked, HideInInspector] public Vector3 velocity { get; set; }
    [Networked, HideInInspector] public bool networkEnabled { get; set; }

    private bool hasParent = false;

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

        if (Runner.IsServer || hasParent) return;

        foreach (var attrName in detector.DetectChanges(this))
        {
            switch (attrName)
            {
                case nameof(position):
                    transform.position = position;
                    break;
                case nameof(rotation):
                    transform.eulerAngles = rotation;
                    break;
                case nameof(velocity):
                    if (rig ?? !rig.isKinematic) rig.velocity = velocity;
                    break;
                case nameof(networkEnabled):
                    hasParent = !networkEnabled;
                    break;
            }
        }
    }

    const float REFRESH_RATE = 3f;
    float refreshTimer = 0f;

    public override void FixedUpdateNetwork()
    {
        if (!Runner.IsServer) return;

        position = transform.position;
        rotation = transform.eulerAngles;
        velocity = rig?.velocity ?? Vector3.zero;

        if (refreshTimer <= 0)
        {
            RPC_EnforceState(position, rotation, velocity);
            refreshTimer = REFRESH_RATE;
        }

        refreshTimer -= Runner.DeltaTime;
    }

    [Rpc(RpcSources.StateAuthority, RpcTargets.All, HostMode = RpcHostMode.SourceIsServer)]
    public void RPC_EnforceState(Vector3 pos, Vector3 rot, Vector3 vel)
    {
        if (Runner.IsServer) return;
        position = pos;
        rotation = rot;
        velocity = vel;
    }
}
