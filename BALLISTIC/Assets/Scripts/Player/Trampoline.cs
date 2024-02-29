using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Trampoline : MonoBehaviour
{
    private void OnCollisionEnter(Collision collision)
    {
        if (collision.collider.CompareTag("Player"))
        {
            BouncePlayer(collision.collider.GetComponent<Rigidbody>());
        }
    }

    private void BouncePlayer(Rigidbody playerRigidbody)
    {
        playerRigidbody.AddForce(transform.forward * 3000, ForceMode.Impulse);
    }
}
