using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RobotRagdoll : BallBuff
{
    private RagdollActivator ragdoll;

    protected override void OnSpawnBuff(NetworkDodgeball ball)
    {
        ragdoll = GetComponent<RagdollActivator>();
    }

    public override void OnPickup(NetworkPlayer player)
    {
        ragdoll.OnPickup();
        ragdoll.transform.position = ragdoll.transform.GetChild(0).position;
        ragdoll.transform.SetParent(Ball.transform);
    }

    public override void WhileHeld(NetworkPlayer player)
    {
        transform.position = Ball.transform.position - (transform.up * 0.3f);
    }

    public override void OnThrow(NetworkPlayer thrower, Vector3 direction)
    {
        ragdoll.OnThrow();
    }

    public override void OnDropped(NetworkPlayer player)
    {
        ragdoll.OnThrow();
        Ball.Rig.isKinematic = true;
        ragdoll.transform.SetParent(null);
        Ball.transform.SetParent(transform);
        Ball.transform.localPosition = Vector3.zero;
    }

    public override void WhileDeadly(Vector3 curDirection)
    {
        ragdoll.transform.position = Ball.transform.position - (transform.up * 0.3f);
    }

    public override void OnNotDeadly()
    {
        Ball.Rig.isKinematic = true;
        ragdoll.transform.SetParent(null);
        Ball.transform.SetParent(transform);
        Ball.transform.localPosition = Vector3.zero;
    }
}
