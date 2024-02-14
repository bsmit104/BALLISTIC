using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Fusion;

public class DodgeballPickup : MonoBehaviour
{

    public NetworkPlayer player;


    // Add balls to list of available balls when colliding with pickup trigger
    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Dodgeball"))
        {
            NetworkDodgeball ball = other.GetComponent<NetworkDodgeball>();
            if (!player.nearbyDodgeballs.Contains(ball) && ball.owner == PlayerRef.None)
            {
                player.nearbyDodgeballs.Add(ball);
            }
        }
    }

    // Remove balls from list of available balls when no longer colliding with pickup trigger
    void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Dodgeball") && player.nearbyDodgeballs.Contains(other.GetComponent<NetworkDodgeball>()))
        {
            player.nearbyDodgeballs.Remove(other.GetComponent<NetworkDodgeball>());
        }
    }
}


