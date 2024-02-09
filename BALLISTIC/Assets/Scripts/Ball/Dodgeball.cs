using System.Collections;
using System.Collections.Generic;
using Fusion;
using UnityEngine;

/// <summary>
/// Client-sided script used by NetworkDodgeball to register collisions
/// </summary>
public class DodgeballCollider : MonoBehaviour
{
    private bool isDead = false; // Tracks if ball is "dead" (ball has hit the floor already)

    [HideInInspector] public NetworkDodgeball networkBall;

    private void OnCollisionEnter(Collision collider)
    {
        // Check if the collided GameObject has the "Player" tag
        if (!isDead && collider.gameObject.CompareTag("Player"))
        {
            // Activate the ragdoll for the player
            NetworkPlayer player = collider.gameObject.GetComponent<NetworkPlayer>();

            if (player?.GetRef == networkBall.owner) return;

            Debug.Log("hit player");
            if (player) // add " && networkBall.Runner.IsServer" to make collision detection host priority
            {
                Debug.Log("send message");
                player.ActivatePlayerRagdoll();
            }
        }
        else if (collider.gameObject.CompareTag("isGround"))
        {
            isDead = true;
        }
    }
}

