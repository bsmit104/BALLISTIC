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
    /// <summary>
    /// Get the NetworkPlayer instance assigned to the client.
    /// (e.g. returns a different instance depending on the computer it is run on).
    /// </summary>
    public static NetworkPlayer Local { get { return _local; } }
    private static NetworkPlayer _local;

    /// <summary>
    /// Returns the PlayerRef associated with this player object.
    /// </summary>
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
    private PlayerRef _playerRef = PlayerRef.None;


    // * Client-Sided Attributes ================================

    [Header("Camera Controls")]
    [Tooltip("The player's camera. Will be set active if the player instance is the local client. Should be deactivated by default.")]
    [SerializeField] private GameObject cmra;
    [SerializeField] private GameObject cinemachineCamera;
    public Cinemachine.AxisState xAxis, yAxis;
    [SerializeField] Transform camFollowPos;

    [Header("Movement Settings")]
    public float walkSpeed = 2f;
    public float sprintSpeed = 5f;
    public float realSpeed = 0f;
    public float rotationSpeed = 10f;

    [HideInInspector] public Vector3 dir;
    float horizontal;
    float vertical;
    CharacterController controller;

    [Space]
    [Header("Ball Throwing")]
    public Transform throwPoint;            // Point from where the dodgeball is thrown
    public float throwForce = 10f;          // Force applied to the dodgeball when thrown
    public float throwCooldown = 1f;        // Cooldown duration between throws

    private float lastThrowTime;            // Time when the last throw happened

    // Ball Pickup
    [Space]
    public List<NetworkDodgeball> nearbyDodgeballs; // List of dodgeballs near player in "pickup" range
    [SerializeField] public GameObject pickupCollider;

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
            { nameof(isWalking), IsWalkingOnChange },
            { nameof(isWalkingBack), IsWalkingBackOnChange },
            { nameof(isStrafingRight), IsStrafingRightOnChange },
            { nameof(isStrafingLeft), IsStrafingLeftOnChange },
            { nameof(isSprinting), IsSprintingOnChange },
            { nameof(isIdle), IsIdleOnChange }
        };
    }

    // ? Example:
    // [Networked] Type myAttribute { get; set; }
    // void MyAttributeOnChange() { ... }

    // Animator Bools ================================

    [Networked, HideInInspector] public bool isWalking { get; set; }
    void IsWalkingOnChange() { animator.SetBool("isWalking", isWalking); }

    [Networked, HideInInspector] public bool isWalkingBack { get; set; }
    void IsWalkingBackOnChange() { animator.SetBool("isWalkingBack", isWalkingBack); }

    [Networked, HideInInspector] public bool isStrafingRight { get; set; }
    void IsStrafingRightOnChange() { animator.SetBool("isStrafingRight", isStrafingRight); }

    [Networked, HideInInspector] public bool isStrafingLeft { get; set; }
    void IsStrafingLeftOnChange() { animator.SetBool("isStrafingLeft", isStrafingLeft); }

    [Networked, HideInInspector] public bool isSprinting { get; set; }
    void IsSprintingOnChange() { animator.SetBool("isSprinting", isSprinting); }

    [Networked, HideInInspector] public bool isIdle { get; set; }
    void IsIdleOnChange() { animator.SetBool("isIdle", isIdle); }

    // ===============================================

    // Detect changes, and trigger event listeners.
    public override void Render()
    {
        HandleLook();
        foreach (var attrName in detector.DetectChanges(this))
        {
            networkChangeListeners[attrName]();
        }
    }

    // * ========================================================

    // * Join and Leave =========================================

    // Basically Start()
    public override void Spawned()
    {
        // Players do not need to be re-instantiated on scene changes
        DontDestroyOnLoad(gameObject);

        // Init change detector to current game state and set up listeners
        detector = GetChangeDetector(ChangeDetector.Source.SimulationState);
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

            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;

            // Instantiate list of nearby dodgeballs
            nearbyDodgeballs = new List<NetworkDodgeball>();

            // Set pickup collider
            pickupCollider.SetActive(true);

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

    void HandleLook()
    {
        if (!Object.HasInputAuthority) return;
        
        xAxis.Update(Time.deltaTime);
        yAxis.Update(Time.deltaTime);

        camFollowPos.localEulerAngles = new Vector3(yAxis.Value, camFollowPos.localEulerAngles.y, camFollowPos.localEulerAngles.z);
        UpdateLookDirection(xAxis.Value);
    }

    void HandleMovement(NetworkInputData data)
    {
        horizontal = data.horizontal;
        vertical = data.vertical;

        bool isMovingForward = vertical > 0f;
        bool isMovingBackward = vertical < 0f;
        isStrafingRight = horizontal > 0f;
        isStrafingLeft = horizontal < 0f;
        isSprinting = isMovingForward && data.sprintButtonPressed;

        // Set the speed based on the input
        float realSpeed = isSprinting ? sprintSpeed : walkSpeed;

        // Move the character based on the input
        Vector3 movement = (transform.forward * vertical + transform.right * horizontal) * realSpeed * Runner.DeltaTime;
        rb.MovePosition(transform.position + movement);
        //transform.position += movement * Runner.DeltaTime;

        // Trigger the walk animation when moving forward and not holding the sprint key
        isWalking = isMovingForward && !isSprinting && !isMovingBackward;

        // Trigger the walkBackwards animation when moving backward and not holding the sprint key
        isWalkingBack = isMovingBackward && !isSprinting && !isMovingForward;

        // Trigger the idle animation when standing still and not pressing any movement keys
        isIdle = vertical == 0 && horizontal == 0;
    }

    bool IsThrowing()
    {
        return Time.time - lastThrowTime < throwCooldown;
    }

    // since input is applied on FixedUpdate, OnKey/ButtonDown events are unreliable.
    // instead track whether OnPress has been registered, and reset when OnRelease.
    bool alreadyPressed = false;

    void HandleThrowBall(NetworkInputData data)
    {
        // reset pressed state on button release
        if (!data.throwButtonPressed)
        {
            alreadyPressed = false;
        }

        // apply input, given that this is the first update that the current button press has been read on
        if (data.throwButtonPressed && !alreadyPressed && !IsThrowing()) // IsThrowing() checks for cooldown
        {
            // Set the last throw time to the current time
            lastThrowTime = Time.time;

            // TODO Pickup: replace with pickup and throw mechanics
            if (Object.HasInputAuthority)
            {
                NetworkDodgeball ball = FindClosestDodgeball();
                if (ball != null)
                {
                    ball.owner = GetRef;
                    PickupBall(ball);
                }
            }
        }
    }

    // Iterates through all dodgeballs in list of nearby balls and returns the closest one to the player AND isn't already "owned" by another player
    NetworkDodgeball FindClosestDodgeball()
    {
        NetworkDodgeball closestDodgeball = null;
        float closestDistance = float.MaxValue;

        foreach (NetworkDodgeball dodgeball in nearbyDodgeballs)
        {
            if (dodgeball.owner == PlayerRef.None)
            {
                float distance = Vector3.Distance(transform.position, dodgeball.transform.position);

                if (distance < closestDistance)
                {
                    closestDistance = distance;
                    closestDodgeball = dodgeball;
                }
            }
        }

        return closestDodgeball;
    }

    // * ========================================================

    // * Remote Procedure Calls =================================


    // Ragdoll ===============================

    /// <summary>
    /// Synchronously activate player ragdoll across all clients.
    /// </summary>
    public void ActivatePlayerRagdoll()
    {
        if (Runner.IsServer)
        {
            RPC_EnforcePlayerRagdoll(GetRef);
        }
        else
        {
            RagdollActivation(); // client-sided prediction
            RPC_RequestPlayerRagdoll(GetRef);
        }
    }

    private void RagdollActivation()
    {
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

    // enforce ragdoll activation from host to clients
    [Rpc(RpcSources.StateAuthority, RpcTargets.All, HostMode = RpcHostMode.SourceIsServer)]
    public void RPC_EnforcePlayerRagdoll(PlayerRef player)
    {
        if (player != GetRef)
        {
            return;
        }

        RagdollActivation();
    }

    // set request to host to activate the player's ragdoll
    [Rpc(RpcSources.Proxies, RpcTargets.StateAuthority, HostMode = RpcHostMode.SourceIsHostPlayer)]
    public void RPC_RequestPlayerRagdoll(PlayerRef player)
    {
        RPC_EnforcePlayerRagdoll(player);
    }

    // =====================================

    // Look Direction ======================

    private void UpdateLookDirection(float yRot)
    {
        transform.eulerAngles = new Vector3(transform.eulerAngles.x, yRot, transform.eulerAngles.z);
        RPC_UpdateLookDirection(yRot, GetRef);
    }

    [Rpc(RpcSources.InputAuthority, RpcTargets.StateAuthority, HostMode = RpcHostMode.SourceIsHostPlayer)]
    public void RPC_UpdateLookDirection(float yRot, PlayerRef player)
    {
        if (GetRef != player) return;
        transform.eulerAngles = new Vector3(transform.eulerAngles.x, yRot, transform.eulerAngles.z);
    }

    // ======================================

    // Ball Pickup ==========================
    
    private void ActualPickupBall(NetworkDodgeball ball)
    {
        Debug.Log("Ball picked up");
        ball.transform.SetParent(throwPoint);
        ball.GetRigidbody().isKinematic = true;
        ball.GetRigidbody().detectCollisions = false;
    }

    public void PickupBall(NetworkDodgeball ball)
    {
        // RPCs require network ID
        NetworkId networkID = ball.GetComponent<NetworkObject>().Id;

        if (Runner.IsServer)
        {
            RPC_EnforcePickupBall(networkID); // if call was made on the host, tell everyone to do the thing
        }
        else
        {
            ActualPickupBall(ball); // local prediction so that the client doesn't have to wait
            RPC_RequestPickupBall(networkID); // tell the host that everyone should do this thing
        }
    }

    [Rpc(RpcSources.StateAuthority, RpcTargets.All, HostMode = RpcHostMode.SourceIsServer)]
    public void RPC_EnforcePickupBall(NetworkId networkID) // this function is executed on everyone's comupter
    {
        if (Runner.TryFindObject(networkID, out var obj))
        {
            ActualPickupBall(obj.GetComponent<NetworkDodgeball>());
        }
    }

    [Rpc(RpcSources.Proxies, RpcTargets.StateAuthority, HostMode = RpcHostMode.SourceIsHostPlayer)]
    public void RPC_RequestPickupBall(NetworkId networkID) // this function is executed on the host's computer
    {
        RPC_EnforcePickupBall(networkID);
    }

    // ======================================
}
