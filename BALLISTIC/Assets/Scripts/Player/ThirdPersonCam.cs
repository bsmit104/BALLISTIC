using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;

public class ThirdPersonCam : MonoBehaviour
{
    /*
    [Header("References")]
    public Transform orientation;
    public Transform player;

    public float rotationSpeed = 10f;
    public float mouseSensitivity = 2f;

    */

    public Cinemachine.AxisState xAxis, yAxis;
    [SerializeField] Transform camFollowPos;

    // Sensitivity for horizontal and vertical movement
    public float horizontalSensitivity = 2f;
    public float verticalSensitivity = 2f;

    // Clamp vertical rotation to prevent flipping
    public float minYAngle = -80f;
    public float maxYAngle = 80f;

    // Start is called before the first frame update
    void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    // Update is called once per frame
    private void Update()
    {
        // Get input for horizontal and vertical rotation
        float mouseX = Input.GetAxis("Mouse X");
        float mouseY = Input.GetAxis("Mouse Y");

        // Update Cinemachine.AxisState values
        xAxis.Update(Time.deltaTime * horizontalSensitivity * mouseX);
        yAxis.Update(Time.deltaTime * verticalSensitivity * mouseY);

        // Clamp vertical rotation to prevent flipping
        float clampedYAngle = Mathf.Clamp(camFollowPos.localEulerAngles.x + yAxis.Value, minYAngle, maxYAngle);
        camFollowPos.localEulerAngles = new Vector3(clampedYAngle, camFollowPos.localEulerAngles.y, camFollowPos.localEulerAngles.z);

        /*
        // Rotate player based on mouse input
        float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity;
        player.Rotate(Vector3.up * mouseX);

        // Rotate orientation based on player rotation
        Vector3 viewDirection = player.position - new Vector3(transform.position.x, player.position.y, transform.position.z);
        orientation.forward = viewDirection.normalized;

        // Apply vertical rotation to the camera based on mouse input
        float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity;
        Vector3 currentRotation = transform.localRotation.eulerAngles;
        float newRotationX = Mathf.Clamp(currentRotation.x - mouseY, -90f, 90f);
        transform.localRotation = Quaternion.Euler(newRotationX, currentRotation.y, currentRotation.z);
        */
    }

    private void LateUpdate()
    {
        //camFollowPos.localEulerAngles = new Vector3(yAxis.Value, camFollowPos.localEulerAngles.y, camFollowPos.localEulerAngles.z);
        //transform.eulerAngles = new Vector3(transform.eulerAngles.x, xAxis.Value, transform.eulerAngles.z);

        // Rotate the camera horizontally
        transform.eulerAngles = new Vector3(transform.eulerAngles.x, xAxis.Value, transform.eulerAngles.z);
    }
}
