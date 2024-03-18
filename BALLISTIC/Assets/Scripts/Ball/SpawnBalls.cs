using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpawnBalls : MonoBehaviour
{

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.R))
        {
            var ball = NetworkBallManager.Instance.GetBall();
            ball.transform.position = Spawner.GetSpawnPoint(ball.Col.bounds);
        }

        if (Input.GetKeyDown(KeyCode.E))
        {
            SpawnDummies();
        }
    }

    void SpawnDummies()
    {
        var dummy = NetworkPlayerManager.Instance.GetDummy();
        dummy.transform.position = Spawner.GetSpawnPoint();
    }
}
