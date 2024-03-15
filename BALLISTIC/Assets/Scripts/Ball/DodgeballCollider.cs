using System.Collections;
using System.Collections.Generic;
using Fusion;
using UnityEngine;

/// <summary>
/// Client-sided script used by NetworkDodgeball to register collisions.
/// </summary>
public class DodgeballCollider : MonoBehaviour
{
    [HideInInspector] public NetworkDodgeball networkBall;

    public void OnCollisionEnter(Collision col)
    {
        AudioManager.Instance?.PlaySound("BallHit", gameObject);

        // Check if ball can kill players
        if (networkBall.IsDeadly && networkBall.enabled)
        {
            // Check if the collided GameObject is a player
            if (col.gameObject.CompareTag("Player"))
            {
                NetworkPlayer player = col.gameObject.GetComponent<NetworkPlayer>();

                // Ball cannot kill the player who threw it
                if (player?.GetRef == networkBall.Owner) return;

                if (player) // add " && networkBall.Runner.IsServer" to make collision detection host priority
                {
                    // Kill the player
                    networkBall.NetworkOnPlayerHit(player.GetRef);
                    player.ActivatePlayerRagdoll();
                }
            }
        }
    }
}

