using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Fusion;
using UnityEngine.InputSystem;
using UnityEngine.Rendering.Universal;

/// <summary>
/// Networked object to manage dodgeball. Spawn and release using the NetworkBallManager.
/// </summary>
[RequireComponent(typeof(DodgeballCollider))]
[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(TrailRenderer))]
[RequireComponent(typeof(MeshRenderer))]
public class NetworkDodgeball : NetworkBehaviour
{
    // * Client-Sided Attributes =======================================
    private int layerMarkerVisible;
    private int layerMarkerHidden;

    /// <summary>
    /// The client-sided script responsible for detecting collisions.
    /// </summary>
    public DodgeballCollider BallCol { get { return ballCol; } }
    private DodgeballCollider ballCol;

    /// <summary>
    /// The ball's rigidbody.
    /// </summary>
    public Rigidbody Rig { get { return rig; } }
    private Rigidbody rig;

    /// <summary>
    /// The ball's collider.
    /// </summary>
    public Collider Col { get { return col; } }
    private Collider col;

    /// <summary>
    /// The ball's Trail Renderer.
    /// </summary>
    public TrailRenderer Trail { get { return trail; } }
    private TrailRenderer trail;

    /// <summary>
    /// The ball's mesh renderer.
    /// </summary>
    public MeshRenderer Rend { get { return rend; } }
    private MeshRenderer rend;

    private Material normalMat;
    [SerializeField] private Material pickupMat;

    /// <summary>
    /// Returns the NetworkId associated with the NetworkObject attached to the ball.
    /// </summary>
    public NetworkId NetworkID 
    {
        get {
            if (!netId.IsValid)
            {
                netId = GetComponent<NetworkObject>().Id;
            }
            return netId;
        }
    }
    private NetworkId netId;

    /// <summary>
    /// The network position component responsible for synchronizing this ball's transform state.
    /// </summary>
    public NetworkPosition NetPos
    {
        get
        {
            if (netPos == null)
            {
                netPos = GetComponent<NetworkPosition>();
            }
            return netPos;
        }
    }
    private NetworkPosition netPos;

    /// <summary>
    /// Returns true if the ball kill a player on collision.
    /// </summary>
    public bool IsDeadly { get { return isDeadly; } }
    private bool isDeadly = false;

    // * ===============================================================

    // * Networked Attributes ==========================================

    /// <summary>
    /// The player who threw the ball, or is currently holding it.
    /// Use IsHeld to see if the ball is currently held by a player.
    /// </summary>
    public PlayerRef Owner { get { return owner; } }
    private PlayerRef owner = PlayerRef.None;

    /// <summary>
    /// If the ball is currently held by a player. Use Owner to see who is 
    /// currently holding it.
    /// </summary>
    public bool IsHeld 
    { 
        get { return isHeld; } 
        set {
            isHeld = value;
            if (isHeld)
            {
                Rig.isKinematic = true;
                Rig.detectCollisions = false;
            }
            else
            {
                Rig.isKinematic = false;
                Rig.detectCollisions = true;
            }
        }
    }
    /// <summary>
    /// DO NOT USE. Use IsHeld getter & setter instead.
    /// </summary>
    [Networked, HideInInspector] public bool isHeld { get; set; }

    // * ===============================================================

    // * Spawning and Despawning =======================================

    private Vector3 originalScale;

    public override void Spawned()
    {
        // Dodgeballs will be recycled between levels
        DontDestroyOnLoad(gameObject);

        // Cache marker collision layers for displaying them through walls
        layerMarkerVisible = LayerMask.NameToLayer("BallMarkerVisible");
        layerMarkerHidden = LayerMask.NameToLayer("BallMarkerHidden");

        // Cache collider script
        ballCol = GetComponent<DodgeballCollider>();
        ballCol.networkBall = this;

        // Cache components
        rig = GetComponent<Rigidbody>();
        col = GetComponent<Collider>();
        trail = GetComponent<TrailRenderer>();
        rend = GetComponent<MeshRenderer>();

        // Default to inactive, wait for manager to activate it
        gameObject.SetActive(false);

        // Cache initial values for resets
        originalSpeed = throwSpeed;
        originalDeadlyTime = deadlyTime;
        originalBounceLimit = bounceLimit;
        normalMat = rend.material;
        originalScale = transform.localScale;
    }

    /// <summary>
    /// Reset any attributes for this NetworkDodgeball. 
    /// Used by the NetworkBallManager to reset dodgeballs returned by GetBall().
    /// </summary>
    public NetworkDodgeball Reset(int newBuff)
    {
        NetworkSetOwner(PlayerRef.None);
        transform.position = Vector3.zero;
        transform.localScale = originalScale;
        rig.velocity = Vector3.zero;
        rig.angularVelocity = Vector3.zero;
        NetworkSetBuff(newBuff);
        return this;
    }

    // * ===============================================================

    // * Throw Physics =================================================

    [Tooltip("The constant speed the ball will travel at will it is deadly.")]
    [SerializeField] private float throwSpeed;
    private float originalSpeed; // caches the original speed for resets

    /// <summary>
    /// The speed the ball will travel at when thrown, must be greater than 0.
    /// </summary>
    public float ThrowSpeed {
        get { return throwSpeed; }
        set { throwSpeed = Mathf.Max(0f, value); }
    }

    [Tooltip("The duration the ball will be deadly for after being thrown.")]
    [SerializeField] private float deadlyTime;
    private float originalDeadlyTime; // caches the original deadly time for resets

    /// <summary>
    /// The max duration the ball will be deadly for after being thrown, must be greater than 0.
    /// </summary>
    public float DeadlyTime {
        get { return deadlyTime; }
        set { deadlyTime = Mathf.Max(0f, value); }
    }

    [Tooltip("The ball will stop being deadly after bouncing this many times.")]
    [SerializeField] private int bounceLimit;
    private int originalBounceLimit; // caches the original bounce limit for resets

    /// <summary>
    /// The max number of times the ball will bounce before becoming not deadly, 
    /// must be greater than or equal to 1.
    /// </summary>
    public int BounceLimit {
        get { return bounceLimit; }
        set { bounceLimit = Mathf.Max(1, value); }
    }

    private float deadlyTimer; // Time left before ball becomes not deadly
    private Vector3 travelDir; // Direction ball is traveling while deadly
    private int bounceCount;   // Number of times the ball has bounced since being throw

    public override void FixedUpdateNetwork()
    {
        if (!gameObject.activeInHierarchy) return;

        if (IsDeadly) // Apply on throw physics
        {
            // Travel at a constant speed
            Rig.velocity = travelDir * throwSpeed;

            // Look-ahead for bounce collisions
            Vector3 start = transform.position;
            Vector3 dir = Rig.velocity.normalized;
            float dist = Rig.velocity.magnitude * Runner.DeltaTime + Col.bounds.extents.x;
            if (Physics.Raycast(start, dir, out RaycastHit hit, dist, LayerMask.GetMask("Surfaces", "BallMarkerVisible", "BallMarkerHidden")))
            {
                // Reflect ball off of surface if a bounce is detected
                travelDir = Vector3.Reflect(travelDir, hit.normal);
                bounceCount++;
                NetworkOnBounce(hit.normal, travelDir, bounceCount, !hit.collider.gameObject.CompareTag("Dodgeball"));
            }
            buff?.WhileDeadly(travelDir);
        }
        else
        {
            buff?.WhileNotDeadly();
            if (IsHeld)
            {
                buff?.WhileHeld(NetworkPlayerManager.Instance.GetPlayer(Owner));
            }
        }
    }

    /// <summary>
    /// Activates the ball's throw state.
    /// </summary>
    /// <param name="dir">The initial throw direction.</param>
    public void Throw(Vector3 dir)
    {
        Rig.velocity = dir * throwSpeed;
        travelDir = dir;
        isDeadly = true;
        deadlyTimer = deadlyTime;
        bounceCount = 0;
        NetworkOnThrow(Owner, dir);
        SetTrailColor();
    }

    private void Update()
    {
        if (!gameObject.activeInHierarchy) return;

        // Set VFX
        SetTrail();
        SetMarker();

        // Count down deadly timer
        if (deadlyTimer > 0)
        {
            deadlyTimer -= Time.deltaTime;
            if (deadlyTimer <= 0 || bounceCount >= bounceLimit)
            {
                // Become not deadly, only distributed by host
                if (Runner.IsServer)
                {
                    NetworkSetOwner(PlayerRef.None);
                    NetworkOnNotDeadly();
                }
                deadlyTimer = 0;
            }
        }
    }

    /// <summary>
    /// Activate or deactivate the ball's trail effect.
    /// </summary>
    public void SetTrail()
    {
        if (trail) trail.emitting = IsDeadly;
    }

    /// <summary>
    /// Set the ball's trail color to match its owner.
    /// </summary>
    public void SetTrailColor()
    {
        var g = new Gradient();
        List<GradientColorKey> modifiedColorKeys = new List<GradientColorKey>();

        foreach (GradientColorKey k in trail.colorGradient.colorKeys)
        {
            GradientColorKey modifiedKey = new GradientColorKey(NetworkPlayerManager.Instance.GetColor(Owner).color, k.time);
            modifiedColorKeys.Add(modifiedKey);
        }
        
        g.SetKeys(modifiedColorKeys.ToArray(), trail.colorGradient.alphaKeys);

        trail.colorGradient = g;
    }

    /// <summary>
    /// Update whether the player should be able to see the ball through walls.
    /// It should not be seen through walls if it is deadly, or held by a player.
    /// </summary>
    public void SetMarker()
    {
        gameObject.layer = IsDeadly || IsHeld ? layerMarkerHidden : layerMarkerVisible;
    }

    public void SetPickupMaterial()
    {
        rend.material = pickupMat;
    }

    public void SetNormalMaterial()
    {
        rend.material = normalMat;
    }

    // * ===============================================================

    // * Delegates =====================================================

    public delegate void DodgeballEvent(NetworkDodgeball ball);

    public delegate void ThrowEvent(NetworkPlayer player, Vector3 dir);

    public delegate void BounceEvent(Vector3 normal, Vector3 newDirection, int bounceCount, bool hitSurface);

    public delegate void PlayerEvent(NetworkPlayer player);

    public delegate void Notify();

    // * ===============================================================

    // * Ball-Buff Events ==============================================

    private BallBuff buff;

    /// <summary>
    /// The index of this ball's buff in the NetworkBallManager.ballBuffs array.
    /// Use with GetBuff(BuffIndex) to get a new instance of this ball buff.
    /// </summary>
    public int BuffIndex { get { return buffIndex; } }
    private int buffIndex;

    // -----------------

    /// <summary>
    /// Invoked when the ball is activated in the level, and has been given a new ball buff.
    /// </summary>
    public event DodgeballEvent OnSpawned;

    /// <summary>
    /// Called by host to tell all clients to add the specified ball buff to this ball.
    /// </summary>
    /// <param name="buffInd">The ball buff's index in the NetworkBallManager.ballBuffs array.</param>
    public void NetworkSetBuff(int buffInd)
    {
        if (Runner.IsServer)
        {
            RPC_SetBuff(buffInd);
        }
    }

    [Rpc(RpcSources.StateAuthority, RpcTargets.All, HostMode = RpcHostMode.SourceIsServer)]
    public void RPC_SetBuff(int buffInd)
    {
        SetBuff(buffInd);
    }

    /// <summary>
    /// Adds the given ball buff to this ball.
    /// </summary>
    /// <param name="buffInd">The ball buff's index in the NetworkBallManager.ballBuffs array.</param>
    public void SetBuff(int buffInd)
    {
        // Reset to default values
        throwSpeed = originalSpeed;
        deadlyTime = originalDeadlyTime;
        bounceLimit = originalBounceLimit;

        // Remove the ball's old buff
        if (this.buff != null && this.buff.transform.parent == transform)
        {
            Destroy(this.buff.gameObject);
            this.buff = null;
        }

        // Add the new buff
        buffIndex = buffInd;
        BallBuff buff = NetworkBallManager.Instance.GetBuff(buffInd);
        this.buff = buff;
        buff.transform.SetParent(transform);
        buff.OnSpawn(this);

        // Invoke spawn event
        OnSpawned?.Invoke(this);
    }

    /// <summary>
    /// Adds the given ball buff to this ball.
    /// </summary>
    /// <param name="buff">The ball buff instance to attach to this ball.</param>
    public void SetBuff(BallBuff buff)
    {
        // Reset to default values
        throwSpeed = originalSpeed;
        deadlyTime = originalDeadlyTime;
        bounceLimit = originalBounceLimit;

        // Remove the ball's old buff
        if (this.buff != null)
        {
            Destroy(this.buff.gameObject);
            this.buff = null;
        }

        // Add the new buff
        this.buff = buff;
        buff.transform.SetParent(transform);
        buff.OnSpawn(this);

        // Invoke spawn event
        OnSpawned?.Invoke(this);
    }

    // Materials set by ball buff

    public void SetMaterial(Material material)
    {
        rend.material = material;
        normalMat = material;
    }

    public void SetPickupMaterial(Material material)
    {
        pickupMat = material;
    }

    // ------------
    
    /// <summary>
    /// Invoked when the ball is thrown.
    /// </summary>
    public event ThrowEvent OnThrow;

    private void NetworkOnThrow(PlayerRef player, Vector3 throwDirection)
    {
        if (Runner.IsServer)
        {
            RPC_OnThrow(player, throwDirection);
        }
    }

    [Rpc(RpcSources.StateAuthority, RpcTargets.All, HostMode = RpcHostMode.SourceIsServer)]
    public void RPC_OnThrow(PlayerRef player, Vector3 throwDirection)
    {
        var netPlayer = NetworkPlayerManager.Instance.GetPlayer(player);
        buff?.OnThrow(netPlayer, throwDirection);
        OnThrow?.Invoke(netPlayer, throwDirection);
    }

    // ------------

    /// <summary>
    /// Invoked when the ball bounces on a surface, while it is deadly.
    /// </summary>
    public event BounceEvent OnBounce;

    private void NetworkOnBounce(Vector3 normal, Vector3 newDirection, int bounceCount, bool hitSurface)
    {
        if (Runner.IsServer)
        {
            RPC_OnBounce(normal, newDirection, bounceCount, hitSurface);
        }
    }

    [Rpc(RpcSources.StateAuthority, RpcTargets.All, HostMode = RpcHostMode.SourceIsServer)]
    public void RPC_OnBounce(Vector3 normal, Vector3 newDirection, int bounceCount, bool hitSurface)
    {
        buff?.OnBounce(normal, newDirection, bounceCount, hitSurface);
        OnBounce?.Invoke(normal, newDirection, bounceCount, hitSurface);
    }

    // ------------

    /// <summary>
    /// Invoked when the ball hits a player while it is deadly.
    /// </summary>
    public event PlayerEvent OnPlayerHit;

    public void NetworkOnPlayerHit(PlayerRef player)
    {
        if (Runner.IsServer)
        {
            RPC_OnPlayerHit(player);
        }
    }

    [Rpc(RpcSources.StateAuthority, RpcTargets.All, HostMode = RpcHostMode.SourceIsServer)]
    public void RPC_OnPlayerHit(PlayerRef player)
    {
        buff?.OnPlayerHit(NetworkPlayerManager.Instance.GetPlayer(player));
        OnPlayerHit?.Invoke(NetworkPlayerManager.Instance.GetPlayer(player));
    }

    // ------------

    /// <summary>
    /// Invoked when the ball is picked up by a player.
    /// </summary>
    public event PlayerEvent OnPickup;

    public void NetworkOnPickup(PlayerRef player)
    {
        if (Owner == NetworkPlayer.Local.GetRef)
        {
            NetworkBallManager.Instance.DisplayBuffText(buff.Title, buff.Description);
        }
        if (Runner.IsServer)
        {
            RPC_OnPickup(player);
        }
    }

    [Rpc(RpcSources.StateAuthority, RpcTargets.All, HostMode = RpcHostMode.SourceIsServer)]
    public void RPC_OnPickup(PlayerRef player)
    {
        buff?.OnPickup(NetworkPlayerManager.Instance.GetPlayer(player));
        OnPickup?.Invoke(NetworkPlayerManager.Instance.GetPlayer(player));
    }

    // ------------

    /// <summary>
    /// Invoked when a player drops the ball.
    /// </summary>
    public event PlayerEvent OnDropped;

    public void NetworkOnDropped(PlayerRef player)
    {
        if (Runner.IsServer)
        {
            RPC_OnDropped(player);
        }
    }

    [Rpc(RpcSources.StateAuthority, RpcTargets.All, HostMode = RpcHostMode.SourceIsServer)]
    public void RPC_OnDropped(PlayerRef player)
    {
        buff?.OnDropped(NetworkPlayerManager.Instance.GetPlayer(player));
        OnDropped?.Invoke(NetworkPlayerManager.Instance.GetPlayer(player));
    }

    // ------------

    /// <summary>
    /// Invoked when the ball becomes not deadly.
    /// </summary>
    public event Notify OnNotDeadly;

    private void NetworkOnNotDeadly()
    {
        if (Runner.IsServer)
        {
            RPC_OnNotDeadly();
        }
    }

    [Rpc(RpcSources.StateAuthority, RpcTargets.All, HostMode = RpcHostMode.SourceIsServer)]
    public void RPC_OnNotDeadly()
    {
        buff?.OnNotDeadly();
        OnNotDeadly?.Invoke();
    }

    // ------------

    // * ===============================================================

    // * Remote Procedure Calls ========================================

    // set active ==============================

    /// <summary>
    /// Use instead of gameObject.SetActive(). Ensures game object is in the same state 
    /// across all clients.
    /// </summary>
    /// <param name="state">The active state the game object will be set to.</param>
    public void NetworkSetActive(bool state)
    {
        if (Runner.IsServer)
        {
            RPC_EnforceSetActive(state);
        }
        else
        {
            gameObject.SetActive(state);
            RPC_RequestSetActive(state);
        }
    }

    [Rpc(RpcSources.StateAuthority, RpcTargets.All, HostMode = RpcHostMode.SourceIsServer)]
    public void RPC_EnforceSetActive(bool state)
    {
        gameObject.SetActive(state);
    }

    [Rpc(RpcSources.Proxies, RpcTargets.StateAuthority, HostMode = RpcHostMode.SourceIsHostPlayer)]
    public void RPC_RequestSetActive(bool state)
    {
        RPC_EnforceSetActive(state);
    }

    // ========================================

    // set owner ==============================

    public void SetOwner(PlayerRef player)
    {
        owner = player;

        // Set the ball's parent to null on drop
        if (player == PlayerRef.None)
        {
            transform.SetParent(null);
            isDeadly = false;
        }
        else
        {
            // Set the ball's parent to the player's hand on pickup
            transform.SetParent(NetworkPlayerManager.Instance.GetPlayer(owner).throwPoint);
        }
    }

    /// <summary>
    /// Sets the owner of the ball across all clients.
    /// Use PlayerRef.None to signal the ball has been dropped.
    /// </summary>
    /// <param name="player">The player who owns the ball.</param>
    public void NetworkSetOwner(PlayerRef player)
    {
        if (Runner.IsServer)
        {
            RPC_EnforceSetOwner(player);
        }
        else
        {
            SetOwner(player);
            RPC_RequestSetOwner(player);
        }
    }

    [Rpc(RpcSources.StateAuthority, RpcTargets.All, HostMode = RpcHostMode.SourceIsServer)]
    public void RPC_EnforceSetOwner(PlayerRef player)
    {
        SetOwner(player);
    }

    [Rpc(RpcSources.Proxies, RpcTargets.StateAuthority, HostMode = RpcHostMode.SourceIsHostPlayer)]
    public void RPC_RequestSetOwner(PlayerRef player)
    {
        RPC_EnforceSetOwner(player);
    }

    // =========================================
}
