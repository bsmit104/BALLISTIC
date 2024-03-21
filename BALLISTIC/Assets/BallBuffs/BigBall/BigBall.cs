using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BigBall : BallBuff
{
    [Space]
    [Header("Big Ball Buff")]
    [SerializeField] float throwScale;
    [SerializeField] float scaleChangeDuration;
    [SerializeField] float throwSpeed;
    [SerializeField] int bounceLimit;

    Vector3 originalScale;

    Coroutine scaleChangeTween;

    protected override void OnSpawnBuff(NetworkDodgeball ball) 
    {
        originalScale = ball.transform.localScale;
        ball.ThrowSpeed = throwSpeed;
        ball.BounceLimit = bounceLimit;
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
}
