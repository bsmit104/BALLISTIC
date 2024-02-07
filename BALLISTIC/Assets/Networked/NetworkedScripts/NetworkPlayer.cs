using System.Collections;
using System.Collections.Generic;
using Fusion;
using UnityEngine;

/// <summary>
/// General event listener delegate.
/// </summary>
public delegate void Notify();

/// <summary>
/// Networked player controller, must be attached to the root game object of the player prefab.
/// </summary>
public class NetworkPlayer : NetworkBehaviour, IPlayerLeft
{
    private static NetworkPlayer _local;
    /// <summary>
    /// Get the NetworkPlayer instance assigned to the client.
    /// (e.g. returns a different instance depending on the computer it is run on).
    /// </summary>
    public static NetworkPlayer Local { get { return _local; } }

    private PlayerRef _playerRef = PlayerRef.None;
    public PlayerRef GetRef { 
        get 
        {
            if (_playerRef == PlayerRef.None)
            {
                foreach (var playerRef in Runner.ActivePlayers)
                {
                    if (Runner.TryGetPlayerObject(playerRef, out NetworkObject obj))
                    {
                        if (obj.gameObject.GetComponent<NetworkPlayer>() == this)
                        {
                            _playerRef = playerRef;
                        }
                    }
                }
            }
            return _playerRef; 
        } 
    }

    [Tooltip("The player's camera. Will be set active if the player instance is the local client. Should be deactivated by default.")]
    [SerializeField] private GameObject cmra;
    [SerializeField] private GameObject cinemachineCamera;

    // * Client-Sided Attributes ================================

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

    private Rigidbody rb;
    private Animator animator;

    // * ========================================================

    // * Networked Attributes ===================================

    private ChangeDetector detector;
    private Dictionary<string, Notify> networkChangeListeners;
    // Creates a map of event listeners for networked attribute changes.
    private void SetChangeListeners()
    {
        networkChangeListeners = new Dictionary<string, Notify>{
            // ? Example: { nameof(myAttribute), MyAttributeOnChange }
            // ...
        };
    }

    // ? Example:
    // [Networked] Type myAttribute { get; set; }
    // void MyAttributeOnChange() { ... }

    // ...

    // Detect changes, and trigger event listeners.
    public override void Render()
    {
        // foreach (var attrName in detector.DetectChanges(this))
        // {
        //     networkChangeListeners[attrName]();
        // }
    }

    // * ========================================================

    // * Join and Leave =========================================

    // Basically Start()
    public override void Spawned()
    {
        // Players do not need to be re-instantiated on scene changes
        DontDestroyOnLoad(gameObject);

        // Init change detector to current game state and set up listeners
        //detector = GetChangeDetector(ChangeDetector.Source.SimulationState);
        SetChangeListeners();

        // Get the Rigidbody component attached to the character
        rb = GetComponent<Rigidbody>();

        controller = GetComponent<CharacterController>();

        // Get the animator component from the attached animator object
        animator = GetComponentInChildren<Animator>();

        // Check if this player instance is the local client
        if (Object.HasInputAuthority)
        {
            _local = this;
            cmra.SetActive(true);
            cinemachineCamera.SetActive(true);
            //gameObject.GetComponent<ThirdPersonCam>().enabled = true;
            Debug.Log("Spawned Local Player");
        }
        else
        {
            Debug.Log("Spawned Remote Player");
        }
    }

    public void PlayerLeft(PlayerRef player)
    {
        if (player == Object.InputAuthority)
        {
            Runner.Despawn(Object);
        }
    }

    // * ========================================================

    // * Character Control ======================================

    public override void FixedUpdateNetwork()
    {
        NetworkInputData data;
        if (!GetInput(out data)) return;

        HandleMovement(data);
        HandleThrowBall(data);
    }

    void HandleMovement(NetworkInputData data)
    {
        horizontal = data.horizontal;
        vertical = data.vertical;

        bool isMovingForward = vertical > 0f;
        bool isMovingBackward = vertical < 0f;
        bool isStrafingRight = horizontal > 0f;
        bool isStrafingLeft = horizontal < 0f;
        bool isSprinting = isMovingForward && data.sprintButtonPressed;

        // Set the speed based on the input
        float realSpeed = isSprinting ? sprintSpeed : walkSpeed;

        // Move the character based on the input
        Vector3 movement = (transform.forward * vertical + transform.right * horizontal) * realSpeed;
        transform.position += movement * Runner.DeltaTime;

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

    bool alreadyPressed = false;

    void HandleThrowBall(NetworkInputData data)
    {
        if (!data.throwButtonPressed)
        {
            alreadyPressed = false;
        }
        if (data.throwButtonPressed && !alreadyPressed && !IsThrowing())
        {
            // Set the last throw time to the current time
            lastThrowTime = Time.time;

            // instantiate the dodgeball at the throw point
            GameObject dodgeball = Instantiate(dodgeballPrefab, throwPoint.position + (transform.forward * 0.5f), throwPoint.rotation);
            dodgeball.GetComponent<Dodgeball>().source = gameObject;
            dodgeball.GetComponent<Dodgeball>().runner = Runner;
            // disable the dodgeballs collider for .25 seconds
            //dodgeball.GetComponent<SphereCollider>().enabled = false;

            // Apply force to throw the dodgeball
            Rigidbody dodgeballRb = dodgeball.GetComponent<Rigidbody>();
            // find the children of the player object called "mixamorig:Spine1"
            //Transform spine1 = transform.Find("Animated/mixamorig:Hips/mixamorig:Spine/mixamorig:Spine1");
            // apply the force in the direction of the mixamo:Spine1 bone

            dodgeballRb.AddForce((transform.forward + new Vector3(0, 0.05f, 0)) * throwForce, ForceMode.Impulse);

            // Start the coroutine to enable the collider after a delay
            //StartCoroutine(EnableColliderAfterDelay(dodgeball));
        }
    }

    // * ========================================================

    // * Remote Procedure Calls =================================

    public void ActivatePlayerRagdoll()
    {
        RPC_ActivatePlayerRagdoll(GetRef);
    }


    [Rpc(RpcSources.StateAuthority, RpcTargets.All, HostMode = RpcHostMode.SourceIsServer)]
    public void RPC_ActivatePlayerRagdoll(PlayerRef player)
    {
        if (player != GetRef)
        {
            return;
        }

        animator.enabled = false;

        // get the player controller script from the parent object
        enabled = false;

        // get and disable the player's rigidbody and collider
        GetComponent<Rigidbody>().isKinematic = true;
        GetComponent<Rigidbody>().collisionDetectionMode = CollisionDetectionMode.Discrete;
        GetComponent<CapsuleCollider>().enabled = false;

        // if (Object.HasInputAuthority)
        // {
        //     // get cinemachine camera and change the look at target to the player's head
        //     Cinemachine.CinemachineFreeLook freeLook = GetComponentInChildren<Cinemachine.CinemachineFreeLook>();

        //     // find the child in children that is called "LookTargetOnDeath"
        //     Transform newLookTarget = transform.Find("Animated/mixamorig:Hips/LookTargetOnDeath");
        //     // set the cinemachine look at target to newLookTarget
        //     freeLook.m_Follow = newLookTarget;
        //     freeLook.m_LookAt = newLookTarget;

        //     // ===== Zoom out to have a dramatic effect for the death =====
        //     // increase the height for the bottom rig/middle rig/top rig
        //     freeLook.m_Orbits[0].m_Height = Mathf.Lerp(freeLook.m_Orbits[0].m_Height, 2, 2f);
        //     freeLook.m_Orbits[1].m_Height = Mathf.Lerp(freeLook.m_Orbits[1].m_Height, 4, 2f);
        //     freeLook.m_Orbits[2].m_Height = Mathf.Lerp(freeLook.m_Orbits[2].m_Height, 6, 2f);

        //     // increase the radius for the bottom rig/middle rig/top rig
        //     freeLook.m_Orbits[0].m_Radius = Mathf.Lerp(freeLook.m_Orbits[0].m_Radius, 5, 2f);
        //     freeLook.m_Orbits[1].m_Radius = Mathf.Lerp(freeLook.m_Orbits[1].m_Radius, 6, 2f);
        //     freeLook.m_Orbits[2].m_Radius = Mathf.Lerp(freeLook.m_Orbits[2].m_Radius, 7, 2f);
        //     // =============================================================
        // }
    }
}
