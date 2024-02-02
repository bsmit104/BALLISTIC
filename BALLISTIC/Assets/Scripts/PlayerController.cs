using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    public float speed = 30f;
    public float rotationSpeed = 1000f;

    private Rigidbody rb;
    private Animator animator;

    // Start is called before the first frame update
    void Start()
    {
        // Get the Rigidbody component attached to the character
        rb = GetComponent<Rigidbody>();

        // Get the animator component from the attached animator object
        animator = GetComponentInChildren<Animator>();
    }

    // Update is called once per frame
    void Update()
    {
        // Handle character movement in the Update method for better responsiveness
        HandleRotation();
        HandleMovement();
    }

    void HandleRotation()
    {
        // Get input from the player
        float horizontal = Input.GetAxis("Horizontal");

        // Rotate the character based on the input
        if (horizontal != 0f)
        {
            Quaternion deltaRotation = Quaternion.Euler(Vector3.up * horizontal * rotationSpeed * Time.deltaTime);
            rb.MoveRotation(rb.rotation * deltaRotation);
        }
    }

    void HandleMovement()
    {
        // Get input from the player
        float vertical = Input.GetAxis("Vertical");

        // Move the character forward based on input
        Vector3 movement = transform.forward * vertical;
        Vector3 velocity = movement * speed;
        rb.MovePosition(rb.position + velocity * Time.deltaTime);

        // set the animator isWalking bool to true only if the character is moving
        if (vertical == 0f)
            animator.SetBool("isWalking", false);
        else
            animator.SetBool("isWalking", true);

    }
}
