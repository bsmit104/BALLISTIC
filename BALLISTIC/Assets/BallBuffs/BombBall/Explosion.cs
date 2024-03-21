using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Explosion : MonoBehaviour
{
    [SerializeField] private float activeTime;

    private float timer;

    void Awake()
    {
        timer = activeTime;
    }

    void Update()
    {
        timer -= Time.deltaTime;
        if (timer <= 0)
        {
            Destroy(gameObject);
        }
    }

    void OnTriggerEnter(Collider col)
    {
        // Check if the collided GameObject is a player
        if (col.gameObject.CompareTag("Player"))
        {
            NetworkPlayer player = col.gameObject.GetComponent<NetworkPlayer>();

            if (player) // add " && networkBall.Runner.IsServer" to make collision detection host priority
            {
                // Kill the player
                player.ActivatePlayerRagdoll();
            }
        }
    }
}
