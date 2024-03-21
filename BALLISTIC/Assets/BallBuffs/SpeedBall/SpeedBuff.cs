using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpeedBuff : BallBuff
{
    [SerializeField] private float speedIncrease;
    [SerializeField] private float maxSpeed;
    [SerializeField] private int bounceLimit;

    private float originalSpeed;

    protected override void OnSpawnBuff(NetworkDodgeball ball)
    {
        originalSpeed = ball.ThrowSpeed;
        ball.BounceLimit = bounceLimit;
    }

    public override void OnBounce(Vector3 normal, Vector3 newDirection, int bounceCount, bool hitSurface)
    {
        Ball.ThrowSpeed = Mathf.Min(maxSpeed, Ball.ThrowSpeed + speedIncrease);
    }

    public override void OnNotDeadly()
    {
        Ball.ThrowSpeed = originalSpeed;
    }
}
