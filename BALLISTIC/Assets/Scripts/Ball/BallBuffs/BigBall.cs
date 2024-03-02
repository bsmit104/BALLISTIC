using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BigBall : BallBuff
{
    [Space]
    [Header("Big Ball Buff")]
    [SerializeField] float throwScale;
    [SerializeField] float scaleChangeDuration;

    Vector3 originalScale;

    Coroutine scaleChangeTween;

    protected override void OnSpawnBuff(NetworkDodgeball ball) 
    {
        originalScale = ball.transform.localScale;
    }

    public override void OnThrow(NetworkPlayer thrower, Vector3 throwDirection) 
    {
        if (scaleChangeTween != null)
        {
            StopCoroutine(scaleChangeTween);
        }
        scaleChangeTween = StartCoroutine(Grow());
    }

    IEnumerator Grow()
    {
        float speed = (throwScale - originalScale.x) / scaleChangeDuration;

        while (Ball.transform.localScale.x < throwScale)
        {
            Ball.transform.localScale += Vector3.one * speed * Time.deltaTime;
            yield return null;
        }
        Ball.transform.localScale = Vector3.one * throwScale;
    }

    public override void OnBounce(Vector3 normal, Vector3 newDirection, int bounceCount) {}

    public override void WhileDeadly(Vector3 curDirection) {}

    public override void OnPlayerHit(NetworkPlayer player) {}

    public override void OnPickup(NetworkPlayer player) {}

    public override void OnDropped(NetworkPlayer player) {}

    public override void OnNotDeadly() 
    {
        if (scaleChangeTween != null)
        {
            StopCoroutine(scaleChangeTween);
        }
        scaleChangeTween = StartCoroutine(Shrink());
    }

    IEnumerator Shrink()
    {
        float speed = (originalScale.x - throwScale) / scaleChangeDuration;

        while (Ball.transform.localScale.x > originalScale.x)
        {
            Ball.transform.localScale += Vector3.one * speed * Time.deltaTime;
            yield return null;
        }
        Ball.transform.localScale = originalScale;
    }

    public override void WhileNotDeadly() {}

    public override void WhileHeld(NetworkPlayer player) {}
}
