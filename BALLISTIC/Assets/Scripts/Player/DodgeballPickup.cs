using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Fusion;

public class DodgeballPickup : MonoBehaviour
{

    [HideInInspector] public NetworkPlayer player;

    // Add balls to list of available balls when colliding with pickup trigger
    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Dodgeball"))
        {
            NetworkDodgeball ball = other.GetComponent<NetworkDodgeball>();
            if (!player.NearbyBallsContains(ball) && ball.Owner == PlayerRef.None)
            {
                player.AddNearbyBall(ball);
            }
        }
    }

    // Remove balls from list of available balls when no longer colliding with pickup trigger
    void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Dodgeball") && player.NearbyBallsContains(other.GetComponent<NetworkDodgeball>()))
        {
            player.RemoveNearbyBall(other.GetComponent<NetworkDodgeball>());
        }
    }
}


