using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerShooting : MonoBehaviour
{
    public GameObject dodgeballPrefab;  // Reference to your dodgeball prefab
    public float shootingForce = 10f;   // Adjust the force as needed

    // Update is called once per frame
    void Update()
    {
        // Check for mouse click
        if (Input.GetMouseButtonDown(0))
        {
            ShootDodgeball();
        }
    }

    void ShootDodgeball()
    {
        // Create a ray from the camera to the mouse position
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;

        // Check if the ray hits something
        if (Physics.Raycast(ray, out hit))
        {
            // Instantiate the dodgeball prefab
            GameObject dodgeball = Instantiate(dodgeballPrefab, transform.position, Quaternion.identity);

            // Calculate the direction to shoot
            Vector3 direction = (hit.point - transform.position).normalized;

            // Apply force to the dodgeball
            Rigidbody rb = dodgeball.GetComponent<Rigidbody>();
            if (rb != null)
            {
                rb.AddForce(direction * shootingForce, ForceMode.Impulse);
            }

            // Optional: You might want to add some rotation to the dodgeball for a more natural look
            dodgeball.transform.rotation = Quaternion.LookRotation(direction);

            // Destroy the dodgeball after a certain time (adjust as needed)
            Destroy(dodgeball, 5f);
        }
    }
}