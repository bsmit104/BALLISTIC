using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Fusion;

public class DodgeballPickup : MonoBehaviour
{

    [HideInInspector] public NetworkPlayer player;

    private Collider _col = null;
    private Collider Col { get {
        if (_col == null)
        {
            _col = GetComponent<Collider>();
        }
        return _col;
    }}

    public void GetAllDodgeballs(ref List<NetworkDodgeball> balls)
    {
        balls.Clear();
        Collider[] cols = Physics.OverlapSphere(transform.position, Col.bounds.extents.z);
        foreach (Collider col in cols)
        {
            if (col.CompareTag("Dodgeball"))
            {
                NetworkDodgeball ball = col.GetComponent<NetworkDodgeball>();
                Vector3 diff = ball.transform.position - player.transform.position;
                if (!balls.Contains(ball) && ball.Owner == PlayerRef.None
                    && !Physics.Raycast(player.transform.position, diff.normalized, diff.magnitude, LayerMask.GetMask("Surfaces")))
                {
                    balls.Add(ball);
                }
            }
        }
    }

    // Add balls to list of available balls when colliding with pickup trigger
    // void OnTriggerEnter(Collider other)
    // {
    //     if (other.CompareTag("Dodgeball"))
    //     {
    //         NetworkDodgeball ball = other.GetComponent<NetworkDodgeball>();
    //         if (!player.NearbyBallsContains(ball) && ball.Owner == PlayerRef.None)
    //         {
    //             player.AddNearbyBall(ball);
    //         }
    //     }
    // }

    // // Remove balls from list of available balls when no longer colliding with pickup trigger
    // void OnTriggerExit(Collider other)
    // {
    //     if (other.CompareTag("Dodgeball") && player.NearbyBallsContains(other.GetComponent<NetworkDodgeball>()))
    //     {
    //         player.RemoveNearbyBall(other.GetComponent<NetworkDodgeball>());
    //     }
    // }
}


