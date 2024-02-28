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
[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(CapsuleCollider))]
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
    public PlayerRef GetRef
    {
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
    [Tooltip("The max distance the camera will be from the player.")]
    [SerializeField] private float maxCmraDist;
    [Tooltip("The min distance the camera will be from the player.")]
    [SerializeField] private float minCmraDist;
    [Tooltip("How far the camera will sit off of the surface it is colliding with.")]
    [SerializeField] private float cmraWallOffset;
    [SerializeField] private float cmraShoulderOffset;
    public Cinemachine.AxisState xAxis, yAxis;
    private Transform cmraParent;
    private Transform cmraReferencePos;

    [Tooltip("The max distance aim target detection will be tested for.")]
    [SerializeField] private float aimDist;
    [Tooltip("The upward angle of the throw multiplier. Scales with distance to throw target.")]
    [SerializeField] private float arcMultiplier;
    public Vector3 LookTarget
    {
        get
        {
            if (Physics.Raycast(cmra.transform.position, cmra.transform.forward, out RaycastHit hit, aimDist, LayerMask.GetMask("Surfaces", "Players")))
            {
                return hit.point;
            }
            else
            {
                return cmra.transform.position + (cmra.transform.forward * aimDist);
            }
        }
    }

    [Header("Movement Settings")]
    public float walkSpeed = 2f;
    public float sprintSpeed = 5f;
    public float realSpeed = 0f;
    public float rotationSpeed = 10f;
    public float jumpImpulse;
    public GroundedCollider grounded;

    [HideInInspector] public Vector3 dir;
    float horizontal;
    float vertical;

    [Space]
    [Header("Ball Throwing")]
    public Transform throwPoint;            // Point from where the dodgeball is thrown
    public float throwForce = 10f;          // Force applied to the dodgeball when thrown
    public float throwCooldown = 1f;        // Cooldown duration between throws
    private float lastThrowTime;            // Time when the last throw happened

    [Space]
    [Header("Ball Pickup")]
    [SerializeField] public DodgeballPickup pickupCollider;

    // nearby balls list =====================

    // List of dodgeballs near player in "pickup" range
    private List<NetworkDodgeball> nearbyDodgeballs;

    public bool NearbyBallsContains(NetworkDodgeball ball)
    {
        return nearbyDodgeballs?.Contains(ball) ?? false;
    }

    public void AddNearbyBall(NetworkDodgeball ball)
    {
        nearbyDodgeballs?.Add(ball);
    }

    public void RemoveNearbyBall(NetworkDodgeball ball)
    {
        nearbyDodgeballs?.Remove(ball);
    }

    // ========================================

    private NetworkDodgeball heldBall;
    public bool IsHoldingBall { get { return heldBall != null; } }

    private Rigidbody rb;
    private Animator animator;

    private bool _isDummy = false;
    public bool isDummy
    {
        get { return _isDummy; }
        set
        {
            if (value)
            {
                // Instantiate list of nearby dodgeballs
                nearbyDodgeballs = new List<NetworkDodgeball>();

                // Set pickup collider
                pickupCollider.gameObject.SetActive(true);
                pickupCollider.player = this;

                Debug.Log("Spawned Dummy Player");
            }
            _isDummy = value;
        }
    }

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
            { nameof(isAlive), IsAliveOnChange },
            { nameof(isWalkingBack), IsWalkingBackOnChange },
            { nameof(isStrafingRight), IsStrafingRightOnChange },
            { nameof(isStrafingLeft), IsStrafingLeftOnChange },
            { nameof(isSprinting), IsSprintingOnChange },
            { nameof(isIdle), IsIdleOnChange },
            { nameof(isJumping), IsJumpingOnChange }
        };
    }

    // ? Example:
    // [Networked] Type myAttribute { get; set; }
    // void MyAttributeOnChange() { ... }

    // Position ======================================

    [HideInInspector] public NetworkPosition netPos;

    // ===============================================

    // Dead State ====================================

    [Networked, HideInInspector] public bool isAlive { get; set; } = true;
    void IsAliveOnChange()
    {
        if (isAlive)
        {
            animator.enabled = true;
            rb.isKinematic = false;
            rb.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
            GetComponent<CapsuleCollider>().enabled = true;
        }
        else
        {
            animator.enabled = false;
            rb.isKinematic = true;
            rb.collisionDetectionMode = CollisionDetectionMode.Discrete;
            GetComponent<CapsuleCollider>().enabled = false;
            if (Runner.IsServer) NetworkPlayerManager.Instance.PlayerDied(GetRef);
        }
    }

    // ===============================================

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

    [Networked, HideInInspector] public bool isJumping { get; set; }
    void IsJumpingOnChange() { animator.SetBool("isJump", isJumping); }

    // ===============================================

    // Detect changes, and trigger event listeners.
    public override void Render()
    {
        HandleLook();
        Debug.DrawLine(cmra.transform.position, LookTarget);

        foreach (var attrName in detector.DetectChanges(this))
        {
            if (networkChangeListeners.ContainsKey(attrName))
            {
                networkChangeListeners[attrName]();
            }
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

        // Get the animator component from the attached animator object
        animator = GetComponentInChildren<Animator>();

        // get the networked position
        netPos = GetComponentInChildren<NetworkPosition>();

        isAlive = true;

        // Check if this player instance is the local client
        if (Object.HasInputAuthority)
        {
            _local = this;
            gameObject.layer = LayerMask.NameToLayer("LocalPlayer");

            cmra.SetActive(true);
            cmraParent = cmra.transform.parent;
            cmraReferencePos = cmraParent.GetChild(1);
            cmraReferencePos.localPosition = cmra.transform.localPosition;

            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;

            // Instantiate list of nearby dodgeballs
            nearbyDodgeballs = new List<NetworkDodgeball>();

            // Set pickup collider
            pickupCollider.gameObject.SetActive(true);
            pickupCollider.player = this;

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

        if (!isAlive) return;

        HandleMovement(data);
        HandleThrowBall(data);
    }

    void HandleLook()
    {
        if (!Object.HasInputAuthority) return;

        xAxis.Update(Time.deltaTime);
        yAxis.Update(Time.deltaTime);

        UpdateLookDirection(xAxis.Value, yAxis.Value);

        Vector3 dir = (cmraReferencePos.position - cmraParent.position).normalized;
        if (Physics.Raycast(cmraParent.position, dir, out RaycastHit hit, maxCmraDist + cmraWallOffset, LayerMask.GetMask("Surfaces")))
        {
            float dist = Mathf.Max(minCmraDist, (hit.point - cmraParent.position).magnitude - cmraWallOffset);
            cmra.transform.position = cmraParent.position + (dir * dist);
            cmra.transform.position += (cmraShoulderOffset / dist) * transform.right;
        }
        else
        {
            cmra.transform.position = cmraParent.position + (dir * maxCmraDist);
        }
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

        // Start jump
        if (data.jumpButtonPressed && grounded.isGrounded)
        {
            grounded.isGrounded = false;
            rb.velocity = new Vector3(0, jumpImpulse, 0);
            isJumping = true;
        }
        else
        {
            isJumping = false;
        }

        // Move the character based on the input
        Vector3 movement = (transform.forward * vertical + transform.right * horizontal) * realSpeed * Runner.DeltaTime;
        rb.MovePosition(transform.position + movement);

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
        if (!Object.HasInputAuthority) return;

        // ensure heldBall is actually held
        if (heldBall && heldBall.transform.parent != throwPoint)
        {
            PickupBall(heldBall);
        }


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

            // If not already holding ball, pickup closest ball
            if (!IsHoldingBall)
            {
                NetworkDodgeball ball = FindClosestDodgeball();
                if (ball != null)
                {
                    PickupBall(ball);
                }
            }
            // If holding ball already, throw it
            else
            {
                ThrowBall(heldBall, LookTarget);
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
            if (dodgeball.Owner == PlayerRef.None)
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

    // Reset =================================

    public void Reset()
    {
        if (Runner.IsServer)
        {
            isAlive = true;
        }
        else
        {
            RPC_RequestReset();
        }
    }

    [Rpc(RpcSources.Proxies, RpcTargets.StateAuthority)]
    public void RPC_RequestReset()
    {
        isAlive = true;
    }

    // =======================================

    // Ragdoll ===============================

    /// <summary>
    /// Synchronously activate player ragdoll across all clients.
    /// </summary>
    public void ActivatePlayerRagdoll()
    {
        if (NetworkLevelManager.Instance.IsAtLobby) return;

        if (Runner.IsServer)
        {
            RPC_EnforcePlayerRagdoll();
        }
        else
        {
            RagdollActivation(); // client-sided prediction
            if (Object.HasInputAuthority)
            {
                RPC_NotifyPlayerRagdoll();
            }
            else
            {
                RPC_RequestPlayerRagdoll();
            }
        }
    }

    private void RagdollActivation()
    {
        isAlive = false;
        animator.enabled = false;
        rb.isKinematic = true;
        rb.collisionDetectionMode = CollisionDetectionMode.Discrete;
        GetComponent<CapsuleCollider>().enabled = false;
    }

    // enforce ragdoll activation from host to clients
    [Rpc(RpcSources.StateAuthority, RpcTargets.All, HostMode = RpcHostMode.SourceIsServer)]
    public void RPC_EnforcePlayerRagdoll()
    {
        RagdollActivation();
    }

    // send request to host to activate the player's ragdoll
    [Rpc(RpcSources.Proxies, RpcTargets.StateAuthority)]
    public void RPC_RequestPlayerRagdoll()
    {
        RPC_EnforcePlayerRagdoll();
    }

    // send notification to host to activate the local player's ragdoll
    [Rpc(RpcSources.InputAuthority, RpcTargets.StateAuthority, HostMode = RpcHostMode.SourceIsHostPlayer)]
    public void RPC_NotifyPlayerRagdoll()
    {
        RPC_EnforcePlayerRagdoll();
    }

    // =====================================

    // Look Direction ======================

    private void UpdateLookDirection(float yRot, float pitch)
    {
        transform.eulerAngles = new Vector3(transform.eulerAngles.x, yRot, transform.eulerAngles.z);
        cmraParent.localRotation = Quaternion.Euler(new Vector3(pitch, 0, 0));
        RPC_UpdateLookDirection(yRot);
    }

    [Rpc(RpcSources.InputAuthority, RpcTargets.StateAuthority, HostMode = RpcHostMode.SourceIsHostPlayer)]
    public void RPC_UpdateLookDirection(float yRot)
    {
        transform.eulerAngles = new Vector3(transform.eulerAngles.x, yRot, transform.eulerAngles.z);
    }

    // ======================================

    // Ball Pickup ==========================

    private void ApplyPickupBall(NetworkDodgeball ball)
    {
        if (IsHoldingBall && ball.NetworkID != heldBall.NetworkID)
        {
            ApplyDropBall();
        }
        heldBall = ball;
        ball.IsHeld = true;
        ball.SetOwner(GetRef);
        ball.transform.position = throwPoint.position;
        ball.transform.localPosition = Vector3.zero;
    }

    public void PickupBall(NetworkDodgeball ball)
    {
        // RPCs require network ID
        NetworkId networkID = ball.NetworkID;

        if (Runner.IsServer)
        {
            RPC_EnforcePickupBall(networkID); // if call was made on the host, tell everyone to do the thing
        }
        else
        {
            //ApplyPickupBall(ball); // local prediction so that the client doesn't have to wait
            RPC_RequestPickupBall(networkID); // tell the host that everyone should do this thing
        }
    }

    [Rpc(RpcSources.StateAuthority, RpcTargets.All, HostMode = RpcHostMode.SourceIsServer, TickAligned = false)]
    public void RPC_EnforcePickupBall(NetworkId networkID) // this function is executed on everyone's computer
    {
        if (Runner.TryFindObject(networkID, out var obj))
        {
            ApplyPickupBall(obj.GetComponent<NetworkDodgeball>());
        }
    }

    [Rpc(RpcSources.InputAuthority, RpcTargets.StateAuthority, HostMode = RpcHostMode.SourceIsHostPlayer, TickAligned = false)]
    public void RPC_RequestPickupBall(NetworkId networkID) // this function is executed on the host's computer
    {
        RPC_EnforcePickupBall(networkID);
    }

    // ======================================

    // Ball Throw ==========================

    private void ApplyThrowBall(NetworkDodgeball ball, Vector3 targetPos)
    {
        if (!IsHoldingBall) return;
        heldBall = null;
        ball.IsHeld = false;
        ball.transform.SetParent(null);
        ball.transform.position = throwPoint.position + transform.forward; // ball a bit in front of player so doesn't immediately collide with hand
        Vector3 diff = targetPos - ball.transform.position;
        Vector3 arc = new Vector3(0, arcMultiplier * diff.magnitude, 0);
        ball.Rig.AddForce((diff.normalized + arc) * throwForce, ForceMode.Impulse);
    }

    public void ThrowBall(NetworkDodgeball ball, Vector3 targetPos)
    {
        // RPCs require network ID
        NetworkId networkID = ball.NetworkID;

        if (Runner.IsServer)
        {
            RPC_EnforceThrowBall(networkID, targetPos); // if call was made on the host, tell everyone to do the thing
        }
        else
        {
            //ApplyThrowBall(ball, targetPos); // local prediction so that the client doesn't have to wait
            RPC_RequestThrowBall(networkID, targetPos); // tell the host that everyone should do this thing
        }
    }

    [Rpc(RpcSources.StateAuthority, RpcTargets.All, HostMode = RpcHostMode.SourceIsServer, TickAligned = false)]
    public void RPC_EnforceThrowBall(NetworkId networkID, Vector3 targetPos) // this function is executed on everyone's computer
    {
        if (Runner.TryFindObject(networkID, out var obj))
        {
            ApplyThrowBall(obj.GetComponent<NetworkDodgeball>(), targetPos);
        }
    }

    [Rpc(RpcSources.InputAuthority, RpcTargets.StateAuthority, HostMode = RpcHostMode.SourceIsHostPlayer, TickAligned = false)]
    public void RPC_RequestThrowBall(NetworkId networkID, Vector3 targetPos) // this function is executed on the host's computer
    {
        RPC_EnforceThrowBall(networkID, targetPos);
    }

    // ======================================

    // Drop Ball ============================

    private void ApplyDropBall()
    {
        if (!IsHoldingBall) return;
        heldBall.IsHeld = false;
        heldBall.transform.SetParent(null);
        heldBall = null;
    }

    public void DropBall()
    {
        if (Runner.IsServer)
        {
            RPC_EnforceDropBall();
        }
        else
        {
            RPC_RequestDropBall();
        }
    }

    [Rpc(RpcSources.StateAuthority, RpcTargets.All, HostMode = RpcHostMode.SourceIsServer, TickAligned = false)]
    public void RPC_EnforceDropBall()
    {
        ApplyDropBall();
    }

    [Rpc(RpcSources.InputAuthority, RpcTargets.StateAuthority, HostMode = RpcHostMode.SourceIsHostPlayer, TickAligned = false)]
    public void RPC_RequestDropBall()
    {
        RPC_EnforceDropBall();
    }

    // ======================================
}
