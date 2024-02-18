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
            ball.transform.position = new Vector3(0, 1, 0.75f);
        }

        if (Input.GetKeyDown(KeyCode.E))
        {
            SpawnDummies();
        }
    }

    void SpawnDummies()
    {
        var dummy = NetworkPlayerManager.Instance.GetDummy();
        dummy.transform.position = new Vector3(0, 0, 0);
    }
}
