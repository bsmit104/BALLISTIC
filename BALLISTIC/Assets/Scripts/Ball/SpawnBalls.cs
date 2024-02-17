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
            ball.transform.position = Vector3.up;
        }

        if (Input.GetKeyDown(KeyCode.E))
        {
            NetworkPlayerManager.Instance.GetDummy();
        }
    }
}
