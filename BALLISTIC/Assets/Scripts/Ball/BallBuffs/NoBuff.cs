using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Empty buff, doesn't do anything special.
/// </summary>
public class NoBuff : BallBuff
{
    protected override void OnSpawnBuff(NetworkDodgeball ball) {}

    public override void OnThrow(NetworkPlayer thrower, Vector3 throwDirection) {}

    public override void OnBounce(Vector3 normal, Vector3 newDirection, int bounceCount) {}

    public override void WhileDeadly(Vector3 curDirection) {}

    public override void OnPlayerHit(NetworkPlayer player) {}

    public override void OnPickup(NetworkPlayer player) {}

    public override void OnDropped(NetworkPlayer player) {}

    public override void OnNotDeadly() {}

    public override void WhileNotDeadly() {}

    public override void WhileHeld(NetworkPlayer player) {}
}
