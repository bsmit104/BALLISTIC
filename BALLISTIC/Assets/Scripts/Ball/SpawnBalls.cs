using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpawnBalls : MonoBehaviour
{
    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.R))
        {
            NetworkDodgeball ball = NetworkBallManager.Instance.GetBall();
            ball.transform.position = new Vector3(0,10,0);
            Debug.Log(NetworkBallManager.Instance.Runner.IsServer);
        }
    }
}
