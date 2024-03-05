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

    public void OnCollisionEnter(Collision col)
    {
        AudioManager.Instance.PlaySound("BallHit", gameObject);
        // Check if the collided GameObject has the "Player" tag
        if (networkBall.IsDeadly && networkBall.enabled)
        {
            if (col.gameObject.CompareTag("Player"))
            {
                // Activate the ragdoll for the player
                NetworkPlayer player = col.gameObject.GetComponent<NetworkPlayer>();

                if (player?.GetRef == networkBall.Owner) return;

                if (player) // add " && networkBall.Runner.IsServer" to make collision detection host priority
                {
                    networkBall.OnPlayerHit(player.GetRef);
                    player.ActivatePlayerRagdoll();
                }
            }
        }
    }
}

