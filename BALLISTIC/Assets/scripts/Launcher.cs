using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Launcher : MonoBehaviour
{
    [SerializeField] public float jumpForce = 50.0f;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            Rigidbody playerRigidbody = other.GetComponent<Rigidbody>();

            if (playerRigidbody != null)
            {
                Debug.Log("Jumping");
                Vector3 launch = new Vector3(0, 1, 1);
                playerRigidbody.AddForce(launch * jumpForce, ForceMode.Impulse);
            }
        }
    }
}
