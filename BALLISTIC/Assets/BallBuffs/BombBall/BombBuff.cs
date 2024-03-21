using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BombBuff : BallBuff
{
    [SerializeField] private GameObject explosion;

    protected override void OnSpawnBuff(NetworkDodgeball ball)
    {
        ball.BounceLimit = 1;
    }

    public override void OnBounce(Vector3 normal, Vector3 newDirection, int bounceCount, bool hitSurface)
    {
        var newExplosion = Instantiate(explosion);
        newExplosion.transform.position = Ball.transform.position;
    }
}
