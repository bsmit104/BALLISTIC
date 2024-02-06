using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [Header("Movement Settings")]
    public float walkSpeed = 2f;
    public float sprintSpeed = 5f;
    public float realSpeed = 0f;
    public float rotationSpeed = 10f;

    [HideInInspector] public Vector3 dir;
    float horizontal;
    float vertical;
    CharacterController controller;

    public GameObject dodgeballPrefab;      // Reference to your dodgeball prefab
    public Transform throwPoint;            // Point from where the dodgeball is thrown
    public float throwForce = 10f;          // Force applied to the dodgeball when thrown
    // private bool isThrowing = false;        // Flag to check if the player is currently throwing
    public float throwCooldown = 1f;        // Cooldown duration between throws

    private float lastThrowTime;            // Time when the last throw happened

    [Header("Input Settings")]

    private Rigidbody rb;
    private Animator animator;

    // Start is called before the first frame update
    void Start()
    {
        // Get the Rigidbody component attached to the character
        rb = GetComponent<Rigidbody>();

        controller = GetComponent<CharacterController>();

        // Get the animator component from the attached animator object
        animator = GetComponentInChildren<Animator>();
    }

    // Update is called once per frame
    void Update()
    {
        // Handle character movement in the Update method for better responsiveness
        // HandleRotation();
        HandleMovement();
        HandleThrowBall();
    }

    void HandleRotation()
    {
        horizontal = Input.GetAxis("Horizontal");
        vertical = Input.GetAxis("Vertical");

        // Player rotates with A and D keys
        //if (horizontal != 0f)
        //{
        //    // Rotate the character based on the input
        //    Quaternion deltaRotation = Quaternion.Euler(Vector3.up * horizontal * rotationSpeed * Time.deltaTime);
        //    rb.MoveRotation(rb.rotation * deltaRotation);
        //}

        // Player rotates with camera
        if (horizontal != 0f || vertical != 0f)
        {
            // Get the camera's forward direction without the y-component
            Vector3 cameraForward = Camera.main.transform.forward;
            cameraForward.y = 0f;
            cameraForward.Normalize();

            // Calculate the rotation angle based on the input and camera direction
            float targetAngle = Mathf.Atan2(horizontal, vertical) * Mathf.Rad2Deg + Camera.main.transform.eulerAngles.y;

            // Smoothly interpolate towards the target rotation
            Quaternion targetRotation = Quaternion.Euler(0f, targetAngle, 0f);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
        }

        /*
        // Trigger the turnLeft animation when rotating left and standing still
        if (horizontal < 0f && rb.velocity.magnitude == 0f)
        {
            animator.SetTrigger("turnLeft");
        }

        // Trigger the turnRight animation when rotating right and standing still
        if (horizontal > 0f && rb.velocity.magnitude == 0f)
        {
            animator.SetTrigger("turnRight");
        }
        */
    }

    void HandleMovement()
    {
        horizontal = Input.GetAxis("Horizontal");
        vertical = Input.GetAxis("Vertical");

        bool isMovingForward = vertical > 0f;
        bool isMovingBackward = vertical < 0f;
        bool isStrafingRight = horizontal > 0f;
        bool isStrafingLeft = horizontal < 0f;
        bool isSprinting = isMovingForward && Input.GetKey(KeyCode.LeftShift);

        // Set the speed based on the input
        float realSpeed = isSprinting ? sprintSpeed : walkSpeed;

        // Move the character based on the input
        Vector3 movement = (transform.forward * vertical + transform.right * horizontal) * realSpeed * Time.deltaTime;
        rb.MovePosition(rb.position + movement);

        // Trigger the walk animation when moving forward and not holding the sprint key
        animator.SetBool("isWalking", isMovingForward && !isSprinting && !isMovingBackward);

        // Trigger the walkBackwards animation when moving backward and not holding the sprint key
        animator.SetBool("isWalkingBack", isMovingBackward && !isSprinting && !isMovingForward);

        // Trigger the rightStrafe animation when moving right
        animator.SetBool("isStrafingRight", isStrafingRight);

        // Trigger the leftStrafe animation when moving left
        animator.SetBool("isStrafingLeft", isStrafingLeft);

        // Trigger the sprint animation when moving forward and holding the sprint key
        animator.SetBool("isSprinting", isSprinting);

        // Trigger the idle animation when standing still and not pressing any movement keys
        animator.SetBool("isIdle", vertical == 0 && horizontal == 0);
    }

    // helper function to check if the player is currently throwing
    bool IsThrowing()
    {
        return Time.time - lastThrowTime < throwCooldown;
    }

    IEnumerator EnableColliderAfterDelay(GameObject dodgeball)
    {
        // Wait for 0.25 seconds
        yield return new WaitForSeconds(0.25f);

        // Enable the dodgeball's collider after the delay
        dodgeball.GetComponent<SphereCollider>().enabled = true;
    }

    void HandleThrowBall()
    {
        // Check if the player is not currently throwing and the mouse button is pressed
        if (Input.GetMouseButtonDown(0) && !IsThrowing())
        {
            // Set the last throw time to the current time
            lastThrowTime = Time.time;

            // instantiate the dodgeball at the throw point
            GameObject dodgeball = Instantiate(dodgeballPrefab, throwPoint.position, throwPoint.rotation);
            // disable the dodgeballs collider for .25 seconds
            dodgeball.GetComponent<SphereCollider>().enabled = false;

            // Apply force to throw the dodgeball
            Rigidbody dodgeballRb = dodgeball.GetComponent<Rigidbody>();
            // find the children of the player object called "mixamorig:Spine1"
            Transform spine1 = transform.Find("Animated/mixamorig:Hips/mixamorig:Spine/mixamorig:Spine1");
            // apply the force in the direction of the mixamo:Spine1 bone

            dodgeballRb.AddForce(spine1.forward * throwForce, ForceMode.Impulse);

            // Start the coroutine to enable the collider after a delay
            StartCoroutine(EnableColliderAfterDelay(dodgeball));
        }
    }



    // Don't worry about all of this for now - this was for playing an actual throw animation
    /*

    IEnumerator DisableAndEnableCollider()
    {
        // Disable the collider
        dodgeballPrefab.GetComponent<SphereCollider>().enabled = false;

        // Wait for a short duration (you can adjust this value based on your needs)
        yield return new WaitForSeconds(1f); // Adjust the duration as needed

        // Enable the collider after waiting
        dodgeballPrefab.GetComponent<SphereCollider>().enabled = true;

        // Set the throwing flag to false
        isThrowing = false;
    }

    void HandleThrowBall()
    {
        // Check if the player is not currently throwing and the mouse button is pressed
        if (!isThrowing && Input.GetMouseButtonDown(0))
        {
            // Set the throwing flag to true
            isThrowing = true;

            // Start the coroutine for disabling and enabling the collider
            StartCoroutine(DisableAndEnableCollider());

            float vertical = Input.GetAxis("Vertical");
            float horizontal = Input.GetAxis("Horizontal");

            animator.SetBool("isStrongThrow", true);

            // Wait until the animation is finished then throw the dodgeball
            StartCoroutine(ThrowDodgeballWithDelay());
        }
    }

    IEnumerator ThrowDodgeballWithDelay()
    {
        // instantiate the dodgeball at the throw point, but disable the rigidbodys gravity
        GameObject dodgeball = Instantiate(dodgeballPrefab, throwPoint.position, throwPoint.rotation);
        // make it a child of the throw point
        dodgeball.transform.parent = throwPoint;
        dodgeball.GetComponent<Rigidbody>().useGravity = false;

        // Wait until the strong throw animation is finished
        yield return new WaitForSeconds(1.5f); // Adjust the duration as needed

        // Enable the rigidbodys gravity
        dodgeball.GetComponent<Rigidbody>().useGravity = true;

        // Apply force to throw the dodgeball
        Rigidbody dodgeballRb = dodgeball.GetComponent<Rigidbody>();
        // apply a forward force relative to the throw point
        dodgeballRb.AddForce(throwPoint.forward * throwForce, ForceMode.Impulse);
    }
    */
}
