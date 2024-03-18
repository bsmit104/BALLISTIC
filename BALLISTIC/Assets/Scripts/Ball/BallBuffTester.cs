using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BallBuffTester : MonoBehaviour
{
    [Tooltip("Will be given to all dodgeballs in the testing scene.")]
    [SerializeField] private BallBuff buffPrefab;
    [Tooltip("The number of balls that will be spawned.")]
    [SerializeField] private int ballCount;

    void Start()
    {
        StartCoroutine(ReplaceBuffs());
    }

    IEnumerator ReplaceBuffs()
    {
        while (NetworkLevelManager.Instance == null)
        {
            yield return null;
        }

        while (NetworkLevelManager.Instance.IsResetting)
        {
            yield return null;
        }

        NetworkBallManager.Instance.ReleaseAllBalls();

        for (int i = 0; i < ballCount; i++)
        {
            var ball = NetworkBallManager.Instance.GetBall();
            ball.SetBuff(Instantiate(buffPrefab));
            ball.transform.position = Spawner.GetSpawnPoint(ball.Col.bounds);
        }
    }
}
