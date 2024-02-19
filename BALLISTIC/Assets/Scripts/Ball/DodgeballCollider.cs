using System.Collections;
using System.Collections.Generic;
using Fusion;
using UnityEngine;

/// <summary>
/// Client-sided script used by NetworkDodgeball to register collisions
/// </summary>
public class DodgeballCollider : MonoBehaviour
{
    [HideInInspector] public NetworkDodgeball networkBall;

    private void OnCollisionEnter(Collision collider)
    {
        // Check if the collided GameObject has the "Player" tag
        if (collider.gameObject.CompareTag("Player") && networkBall.owner != PlayerRef.None)
        {
            // Activate the ragdoll for the player
            NetworkPlayer player = collider.gameObject.GetComponent<NetworkPlayer>();

            if (player?.GetRef == networkBall.owner) return;

            Debug.Log("hit player");
            if (player) // add " && networkBall.Runner.IsServer" to make collision detection host priority
            {
                player.ActivatePlayerRagdoll();
            }
        }
        else if (collider.gameObject.CompareTag("Floor"))
        {
            networkBall.owner = PlayerRef.None;
        }
    }
}

