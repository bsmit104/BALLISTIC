using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// I want this script to turn off the player animator component when a key is pressed
public class ActivateRagdoll : MonoBehaviour
{
    public Animator animator;

    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        // Check for a key press
        if (Input.GetKeyDown(KeyCode.R))
        {
            // Toggle the Animator component on/off
            animator.enabled = !animator.enabled;
        }
    }

    // This function is called when the dodgeball collides with the player
    private void OnCollisionEnter(Collision collider)
    {
        // Check if the player collided with the trigger
        if (collider.gameObject.CompareTag("Dodgeball"))
        {
            // Disable the animator component
            animator.enabled = false;
        }
    }
}
