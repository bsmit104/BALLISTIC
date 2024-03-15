using System.Collections;
using System.Collections.Generic;
using Fusion;
using UnityEngine;
using UnityEngine.Rendering.Universal;

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
                // Search for a player ref that's mapped to this
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

    /// <summary>
    /// Returns true if the player is currently alive.
    /// </summary>
    public bool IsAlive { get { return _isAlive; } }
    private bool _isAlive = true;

    /// <summary>
    /// Returns the color assigned to this player.
    /// </summary>
    public PlayerColor Color
    {
        get
        {
            return NetworkPlayerManager.Instance.GetColor(GetRef);
        }
    }

    public void SetColor(Material mat)
    {
        SetColorRecursive(transform, mat);
    }

    private void SetColorRecursive(Transform root, Material mat)
    {
        if (root.TryGetComponent<Renderer>(out var renderer))
        {
            renderer.material = mat;
        }

        for (int i = 0; i < root.childCount; i++)
        {
            SetColorRecursive(root.GetChild(i), mat);
        }
    }

    public void SetLayer(string Layer)
    {
        SetLayerRecursive(transform, LayerMask.NameToLayer(Layer));
    }

    private void SetLayerRecursive(Transform root, int Layer)
    {
        root.gameObject.layer = Layer;

        for (int i = 0; i < root.childCount; i++)
        {
            Debug.Log(Layer);
            Debug.Log(root);
            SetLayerRecursive(root.GetChild(i), Layer);
        }
    }


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
    [Tooltip("How far the camera will swing out from the player as the camera is drawn in.")]
    [SerializeField] private float cmraShoulderOffset;
    [SerializeField] private Cinemachine.AxisState xAxis, yAxis;

    private Transform cmraParent; // parent transform for the camera
    private Transform cmraReferencePos; // where the ideal position for the camera is, used as a reference

    /// <summary>
    /// Returns true if the player's HUD is currently visible.
    /// </summary>
    public bool IsHUDActive { get { return cmra?.transform.GetChild(0).gameObject.activeInHierarchy ?? false; } }

    /// <summary>
    /// Activate or deactivate the player's HUD.
    /// </summary>
    public void SetHUDActive(bool state)
    {
        cmra?.transform.GetChild(0).gameObject.SetActive(state);
    }

    [Header("Movement Settings")]
    [Tooltip("The speed the player will walk at.")]
    [SerializeField] private float walkSpeed;
    [Tooltip("The speed the player will run at.")]
    [SerializeField] private float sprintSpeed;
    [Tooltip("Controls jump height.")]
    [SerializeField] private float jumpImpulse;
    [Tooltip("Collider script for checking if the player is grounded.")]
    [SerializeField] private GroundedCollider grounded;
    [Tooltip("Script used to activate and deactivate the player's ragdoll. Should be attached to the hip joint.")]
    [SerializeField] private RagdollActivator ragdollActivator;

    /// <summary>
    /// Returns the ragdoll activator for this player.
    /// </summary>
    public RagdollActivator RagdollActivator { get { return ragdollActivator; } }

    // Cache WASD movement input values
    private float horizontal;
    private float vertical;

    public Vector2 InputDir { get { return new Vector2(horizontal, vertical); } }

    [Space]
    [Header("Ball Throwing")]
    [Tooltip("Point from where the dodgeball is parented to when held.")]
    public Transform throwPoint;
    [Tooltip("Cooldown duration between click inputs.")]
    public float actionCooldown;

    private float lastThrowTime; // Time when the last throw happened

    [Tooltip("The max distance aim target detection will be tested for.")]
    [SerializeField] private float aimDist;

    /// <summary>
    /// Returns the global position of the point the player is looking at.
    /// </summary>
    public Vector3 LookTarget
    {
        get
        {
            // Raycast from the camera out to the center of the screen
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

    [Space]
    [Header("Ball Pickup")]
    [Tooltip("The collider script used to determine what balls are near the player.")]
    [SerializeField] public DodgeballPickup pickupCollider;

    [Space]
    [Header("Markers")]
    [SerializeField] private ScriptableRendererFeature markerRender;

    // nearby balls list =====================

    // List of dodgeballs near player in "pickup" range
    private List<NetworkDodgeball> nearbyDodgeballs;

    // ========================================

    private NetworkDodgeball prevClosestBall;
    private NetworkDodgeball heldBall;

    /// <summary>
    /// Returns true if the player is currently holding a ball.
    /// </summary>
    public bool IsHoldingBall { get { return heldBall != null; } }

    // Cache components
    private Rigidbody rb;
    private Collider col;
    private Animator animator;

    private bool _isDummy = false;
    public bool IsDummy
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
            { nameof(isWalkingBack), IsWalkingBackOnChange },
            { nameof(isStrafingRight), IsStrafingRightOnChange },
            { nameof(isStrafingLeft), IsStrafingLeftOnChange },
            { nameof(isSprinting), IsSprintingOnChange },
            { nameof(isIdle), IsIdleOnChange },
            { nameof(isJumping), IsJumpingOnChange },
            { nameof(isCrouching), IsCrouchingOnChange },
            { nameof(isCrouchingForward), IsCrouchingForwardOnChange },
            { nameof(isCrouchingBackward), IsCrouchingBackwardOnChange },
            { nameof(isCrouchingRight), IsCrouchingRightOnChange },
            { nameof(isCrouchingLeft), IsCrouchingLeftOnChange }
        };
    }

    // ? Example:
    // [Networked] Type myAttribute { get; set; }
    // void MyAttributeOnChange() { ... }

    // Position ======================================

    [HideInInspector] public PlayerPosition netPos;

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
    void IsIdleOnChange()
    {
        animator.SetBool("isIdle", isIdle);
        if (!isIdle)
        {
            AudioManager.Instance?.PlaySound("Footsteps", gameObject);
            Debug.Log("footsteps active");
        }
        else
        {
            AudioManager.Instance?.StopSound("Footsteps", gameObject);
            Debug.Log("footsteps inactive");
        }
            
    }

    [Networked, HideInInspector] public bool isJumping { get; set; }
    void IsJumpingOnChange() { animator.SetBool("isJump", isJumping); }

    [Networked, HideInInspector] public bool isCrouching { get; set; }
    void IsCrouchingOnChange() { animator.SetBool("isCrouching", isCrouching); }

    [Networked, HideInInspector] public bool isCrouchingForward { get; set; }
    void IsCrouchingForwardOnChange() { animator.SetBool("isCrouchingForward", isCrouchingForward); }

    [Networked, HideInInspector] public bool isCrouchingBackward { get; set; }
    void IsCrouchingBackwardOnChange() { animator.SetBool("isCrouchingBackward", isCrouchingBackward); }

    [Networked, HideInInspector] public bool isCrouchingRight { get; set; }
    void IsCrouchingRightOnChange() { animator.SetBool("isCrouchingRight", isCrouchingRight); }

    [Networked, HideInInspector] public bool isCrouchingLeft { get; set; }
    void IsCrouchingLeftOnChange() { animator.SetBool("isCrouchingLeft", isCrouchingLeft); }

    // ===============================================

    // Detect changes, and trigger event listeners.
    public override void Render()
    {
        // Apply camera controls per frame
        HandleLook();
        Debug.DrawLine(cmra.transform.position, LookTarget);

        // Call network event listeners
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
        col = GetComponent<Collider>();

        // Get the animator component from the attached animator object
        animator = GetComponentInChildren<Animator>();

        // get the networked position
        netPos = GetComponentInChildren<PlayerPosition>();

        _isAlive = true;

        // Check if this player instance is the local client
        if (Object.HasInputAuthority)
        {
            _local = this;
            SetLayer("LocalPlayer");
            netPos.HasAuthority = true;

            // Set up local camera
            cmra.SetActive(true);
            cmraParent = cmra.transform.parent;
            cmraReferencePos = cmraParent.GetChild(1);
            cmraReferencePos.localPosition = cmra.transform.localPosition;

            // Lock and remove cursor
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;

            // Instantiate list of nearby dodgeballs
            nearbyDodgeballs = new List<NetworkDodgeball>();

            // Set pickup collider
            pickupCollider.gameObject.SetActive(true);
            pickupCollider.player = this;

            StartCoroutine(PickupTweenCheck());
        }

        // Wait for this object to be associated with a PlayerRef
        StartCoroutine(SetPlayerRef());
    }

    IEnumerator SetPlayerRef()
    {
        // Wait for this object's PlayerRef to be assigned
        while (GetRef.PlayerId == -1)
        {
            yield return null;
        }

        // Init player number specific stuff
        SetColor(Color.material);
        RagdollActivator.Init(this);
        if (Object.HasInputAuthority)
        {
            NetworkSetPosition(Spawner.GetSpawnPoint());
        }

        Debug.Log("Spawned " + GetRef.PlayerId + " player");
    }

    // Despawn self on disconnect
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

        // Can only pickup and throw balls when alive
        if (!IsAlive) return;

        HandleThrowBall(data);
    }

    // Look Direction ======================

    void HandleLook()
    {
        // Camera control is applied locally
        if (!Object.HasInputAuthority) return;

        // Ignore if the player is paused
        if (NetworkRunnerCallbacks.Instance.IsPaused) return;

        // Update cinemachine axis states
        xAxis.Update(Time.deltaTime);
        yAxis.Update(Time.deltaTime);

        // Send update to host
        UpdateLookDirection(xAxis.Value, yAxis.Value);

        // Apply camera offset so it doesn't phase through walls
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

    private void UpdateLookDirection(float yRot, float pitch)
    {
        // Apply local rotations
        transform.eulerAngles = new Vector3(transform.eulerAngles.x, yRot, transform.eulerAngles.z);
        cmraParent.localRotation = Quaternion.Euler(new Vector3(pitch, 0, 0));
    }

    // ======================================

    void HandleMovement(NetworkInputData data)
    {
        horizontal = data.horizontal;
        vertical = data.vertical;

        // Regular movement (jog) --------------
        bool isMovingForward = vertical > 0f;
        bool isMovingBackward = vertical < 0f;
        isStrafingRight = horizontal > 0f;
        isStrafingLeft = horizontal < 0f;

        // Crouch movement ---------------------
        // start crouching
        if (data.crouchButtonPressed && !isSprinting)
        {
            // TODO: adjust collider height and center of player to be smaller
            isCrouching = true;
        } // stop crouching
        else
        {
            // TODO: adjust collider height and center of player to be normal
            isCrouching = false;
        }
        bool isCrouchForward = isCrouching && isMovingForward;
        bool isCrouchBackward = isCrouching && isMovingBackward;
        bool isCrouchRight = isCrouching && isStrafingRight;
        bool isCrouchLeft = isCrouching && isStrafingLeft;

        // Sprint movement
        isSprinting = isMovingForward && data.sprintButtonPressed;

        // Set the speed based on the input
        float realSpeed = isSprinting ? sprintSpeed : walkSpeed;

        // Start jump
        if (data.jumpButtonPressed && grounded.IsGrounded && !isCrouching)
        {
            rb.velocity = new Vector3(0, jumpImpulse, 0);
            isJumping = true;
        }
        else
        {
            isJumping = false;
        }

        // Move the character based on the input
        Vector3 movement = (transform.forward * vertical + transform.right * horizontal) * realSpeed;
        ApplyMovement(movement);

        // Trigger the walk animations when moving in that direction and not holding the sprint key
        isWalking = isMovingForward && !isSprinting && !isMovingBackward;
        isWalkingBack = isMovingBackward && !isSprinting && !isMovingForward;

        // Trigger the crouch animations when crouching in that direction and not holding the sprint key
        isCrouchingForward = isCrouchForward && !isSprinting && !isCrouchBackward;
        isCrouchingBackward = isCrouchBackward && !isSprinting && !isCrouchForward;
        isCrouchingRight = isCrouchRight && !isSprinting && !isCrouchLeft;
        isCrouchingLeft = isCrouchLeft && !isSprinting && !isCrouchRight;

        // Trigger the idle animation when standing still and not pressing any movement keys
        isIdle = vertical == 0 && horizontal == 0;
    }

    // dont think about it, it works and that's all that matters
    private void ApplyMovement(Vector3 movement)
    {
        Vector3 centerDir = movement.normalized;
        Vector2 lowDir2D = Rotate(new Vector2(centerDir.x, centerDir.z), -45f);
        Vector3 lowDir = new Vector3(lowDir2D.x, 0, lowDir2D.y);
        Vector2 highDir2D = Rotate(new Vector2(centerDir.x, centerDir.z), 45f);
        Vector3 highDir = new Vector3(highDir2D.x, 0, highDir2D.y);

        Vector3[] centerStarts = {
            transform.position + Vector3.up * col.bounds.extents.y + centerDir * col.bounds.extents.x, // torso
            transform.position + Vector3.up * col.bounds.size.y * 0.9f + centerDir * col.bounds.extents.x, // head
            transform.position + Vector3.up * col.bounds.size.y * 0.6f + centerDir * col.bounds.extents.x, // shoulders
            transform.position + Vector3.up * col.bounds.size.y * 0.3f + centerDir * col.bounds.extents.x, // knees
            transform.position + Vector3.up * col.bounds.size.y * 0.1f + centerDir * col.bounds.extents.x, // feet
        };

        Vector3[] lowStarts = {
            transform.position + Vector3.up * col.bounds.extents.y + lowDir * col.bounds.extents.x, // torso
            transform.position + Vector3.up * col.bounds.size.y * 0.95f + lowDir * col.bounds.extents.x, // head
            transform.position + Vector3.up * col.bounds.size.y * 0.6f + lowDir * col.bounds.extents.x, // shoulders
            transform.position + Vector3.up * col.bounds.size.y * 0.3f + lowDir * col.bounds.extents.x, // knees
            transform.position + Vector3.up * col.bounds.size.y * 0.1f + lowDir * col.bounds.extents.x, // feet
        };

        Vector3[] highStarts = {
            transform.position + Vector3.up * col.bounds.extents.y + highDir * col.bounds.extents.x, // torso
            transform.position + Vector3.up * col.bounds.size.y * 0.95f + highDir * col.bounds.extents.x, // head
            transform.position + Vector3.up * col.bounds.size.y * 0.6f + highDir * col.bounds.extents.x, // shoulders
            transform.position + Vector3.up * col.bounds.size.y * 0.3f + highDir * col.bounds.extents.x, // knees
            transform.position + Vector3.up * col.bounds.size.y * 0.1f + highDir * col.bounds.extents.x, // feet
        };

        float dist = movement.magnitude * Runner.DeltaTime * 2f;

        RaycastHit hit;
        for (int i = 0; i < centerStarts.Length; i++)
        {
            if (Physics.Raycast(centerStarts[i], centerDir, out hit, dist, LayerMask.GetMask("Surfaces")))
            {
                Vector2 move = new Vector2(movement.x, movement.z);
                Vector2 hitPerp = new Vector2(hit.normal.z, -hit.normal.x);
                float dot = Vector2.Dot(move, hitPerp);
                Vector2 newDir = hitPerp * dot;
                movement = new Vector3(newDir.x, 0, newDir.y);
            }
            else if (Physics.Raycast(lowStarts[i], lowDir, out hit, dist, LayerMask.GetMask("Surfaces")))
            {
                Vector2 move = new Vector2(movement.x, movement.z);
                Vector2 hitPerp = new Vector2(hit.normal.z, -hit.normal.x);
                float dot = Vector2.Dot(move, hitPerp);
                Vector2 newDir = hitPerp * dot;
                movement = new Vector3(newDir.x, 0, newDir.y);
            }
            else if (Physics.Raycast(highStarts[i], highDir, out hit, dist, LayerMask.GetMask("Surfaces")))
            {
                Vector2 move = new Vector2(movement.x, movement.z);
                Vector2 hitPerp = new Vector2(hit.normal.z, -hit.normal.x);
                float dot = Vector2.Dot(move, hitPerp);
                Vector2 newDir = hitPerp * dot;
                movement = new Vector3(newDir.x, 0, newDir.y);
            }
            if (grounded.IsGrounded && i == centerStarts.Length - 2)
            {
                break;
            }
        }

        rb.MovePosition(transform.position + movement * Runner.DeltaTime);
    }

    // Helper function for cursed code above
    Vector2 Rotate(Vector2 v, float angle)
    {
        return new Vector2(
            v.x * Mathf.Cos(angle * Mathf.Deg2Rad) - v.y * Mathf.Sin(angle * Mathf.Deg2Rad),
            v.x * Mathf.Sin(angle * Mathf.Deg2Rad) + v.y * Mathf.Cos(angle * Mathf.Deg2Rad)
        );
    }

    bool IsOnCooldown()
    {
        return Time.time - lastThrowTime < actionCooldown;
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
        if (data.throwButtonPressed && !alreadyPressed && !IsOnCooldown()) // IsThrowing() checks for cooldown
        {
            // Set the last throw time to the current time
            lastThrowTime = Time.time;
            alreadyPressed = true;

            // If not already holding ball, pickup closest ball
            if (!IsHoldingBall)
            {
                pickupCollider.GetAllDodgeballs(ref nearbyDodgeballs);
                NetworkDodgeball ball = FindClosestDodgeball();
                SetBallMaterial(ball);
                if (ball != null)
                {
                    AudioManager.Instance?.PlaySound("BallPickup", gameObject);
                    SetBallMaterial(null);
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
            RPC_EnforceReset();
        }
    }

    [Rpc(RpcSources.StateAuthority, RpcTargets.All, HostMode = RpcHostMode.SourceIsServer, TickAligned = false)]
    public void RPC_EnforceReset()
    {
        _isAlive = true;
        animator.enabled = true;
        rb.isKinematic = false;
        rb.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
        GetComponent<CapsuleCollider>().enabled = true;
        netPos.enabled = true;
        ApplyDropBall();
        RagdollActivator.DeactivateRagdoll();
        grounded.Reset();
        SetHUDActive(true);
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
        _isAlive = false;
        animator.enabled = false;
        if (this != Local) // Enter spectator mode if the local player dies
        {
            rb.isKinematic = true;
            rb.collisionDetectionMode = CollisionDetectionMode.Discrete;
            GetComponent<CapsuleCollider>().enabled = false;
        }
        else
        {
            netPos.enabled = false;
            SetHUDActive(false);
        }
        ApplyDropBall();
        RagdollActivator.ActivateRagdoll();
        if (Runner.IsServer) NetworkPlayerManager.Instance.PlayerDied(GetRef);
        SetBallMaterial(null);
    }

    // enforce ragdoll activation from host to clients
    [Rpc(RpcSources.StateAuthority, RpcTargets.All, HostMode = RpcHostMode.SourceIsServer)]
    public void RPC_EnforcePlayerRagdoll()
    {
        if (!IsAlive) return;
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
        ball.NetworkOnPickup(GetRef);
    }

    public void PickupBall(NetworkDodgeball ball)
    {
        // RPCs require network ID
        NetworkId networkID = ball.NetworkID;

        if (Runner.IsServer)
        {
            RPC_EnforcePickupBall(networkID);
        }
        else
        {
            RPC_RequestPickupBall(networkID);
        }
    }

    [Rpc(RpcSources.StateAuthority, RpcTargets.All, HostMode = RpcHostMode.SourceIsServer, TickAligned = false)]
    public void RPC_EnforcePickupBall(NetworkId networkID)
    {
        if (Runner.TryFindObject(networkID, out var obj))
        {
            ApplyPickupBall(obj.GetComponent<NetworkDodgeball>());
        }
    }

    [Rpc(RpcSources.InputAuthority, RpcTargets.StateAuthority, HostMode = RpcHostMode.SourceIsHostPlayer, TickAligned = false)]
    public void RPC_RequestPickupBall(NetworkId networkID)
    {
        // If host has already received a request to pick up the same ball, deny request
        if (Runner.TryFindObject(networkID, out var obj))
        {
            if ((obj.GetComponent<NetworkDodgeball>()?.IsHeld) ?? false)
            {
                return;
            }
        }
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
        ball.Throw(diff.normalized);
    }

    public void ThrowBall(NetworkDodgeball ball, Vector3 targetPos)
    {
        // RPCs require network ID
        NetworkId networkID = ball.NetworkID;

        if (Runner.IsServer)
        {
            RPC_EnforceThrowBall(networkID, targetPos);
        }
        else
        {
            RPC_RequestThrowBall(networkID, targetPos);
        }
    }

    [Rpc(RpcSources.StateAuthority, RpcTargets.All, HostMode = RpcHostMode.SourceIsServer, TickAligned = false)]
    public void RPC_EnforceThrowBall(NetworkId networkID, Vector3 targetPos)
    {
        if (Runner.TryFindObject(networkID, out var obj))
        {
            ApplyThrowBall(obj.GetComponent<NetworkDodgeball>(), targetPos);
        }
    }

    [Rpc(RpcSources.InputAuthority, RpcTargets.StateAuthority, HostMode = RpcHostMode.SourceIsHostPlayer, TickAligned = false)]
    public void RPC_RequestThrowBall(NetworkId networkID, Vector3 targetPos)
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
        heldBall.SetOwner(PlayerRef.None);
        heldBall.NetworkOnDropped(GetRef);
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

    // Enforce Position =====================

    public void NetworkSetPosition(Vector3 position)
    {
        if (netPos.HasAuthority)
        {
            RPC_EnforcePosition(position);
        }
        else
        {
            RPC_RequestPosition(position);
        }
    }

    [Rpc(RpcSources.InputAuthority, RpcTargets.All, HostMode = RpcHostMode.SourceIsHostPlayer)]
    public void RPC_EnforcePosition(Vector3 position)
    {
        transform.position = position;
    }

    [Rpc(RpcSources.All, RpcTargets.All, HostMode = RpcHostMode.SourceIsHostPlayer)]
    public void RPC_RequestPosition(Vector3 position)
    {
        if (netPos.HasAuthority)
        {
            RPC_EnforcePosition(position);
        }
    }

    // * ==================================================

    // * VFX ==============================================

    // Markers ==============================

    private void Update()
    {
        if (this == Local)
        {
            SetMarkers();
        }
    }

    private void SetMarkers()
    {
        markerRender.SetActive(!IsHoldingBall);
    }

    // Tween ================================

    // Check if there is a ball in range that needs to start tweening
    private IEnumerator PickupTweenCheck()
    {
        while (true)
        {
            if (_isAlive && !IsHoldingBall)
            {
                pickupCollider.GetAllDodgeballs(ref nearbyDodgeballs);
                NetworkDodgeball ball = FindClosestDodgeball();
                SetBallMaterial(ball);
            }
            yield return new WaitForSeconds(0.1f);
        }
    }

    // Stop tweening the previous ball and start tweening the new one
    private void SetBallMaterial(NetworkDodgeball ball)
    {
        if (ball != prevClosestBall)
        {
            if (prevClosestBall != null)
            {
                prevClosestBall.SetNormalMaterial();
            }
            if (ball != null)
            {
                ball.SetPickupMaterial();
            }
            prevClosestBall = ball;
        }
    }

}
