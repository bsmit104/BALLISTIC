using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Fusion;
using UnityEngine.InputSystem;

/// <summary>
/// Networked object to manage dodgeball. Spawn and release using the NetworkBallManager.
/// </summary>
[RequireComponent(typeof(DodgeballCollider))]
[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(TrailRenderer))]
public class NetworkDodgeball : NetworkBehaviour
{
    // * Client-Sided Attributes =======================================

    private DodgeballCollider ballCol;

    /// <summary>
    /// The ball's rigidbody.
    /// </summary>
    public Rigidbody Rig { get { return rig; } }
    private Rigidbody rig;

    /// <summary>
    /// The ball's collider.
    /// </summary>
    public SphereCollider Col { get { return col; } }
    private SphereCollider col;

    /// <summary>
    /// The ball's Trail Renderer.
    /// </summary>
    public TrailRenderer Trail { get { return trail; } }
    private TrailRenderer trail;

    /// <summary>
    /// Returns the NetworkId associated with the NetworkObject attached to the ball.
    /// </summary>
    public NetworkId NetworkID 
    {
        get {
            if (!_id.IsValid)
            {
                _id = GetComponent<NetworkObject>().Id;
            }
            return _id;
        }
    }
    private NetworkId _id;

    public NetworkPosition NetPos
    {
        get
        {
            if (_netPos == null)
            {
                _netPos = GetComponent<NetworkPosition>();
            }
            return _netPos;
        }
    }
    private NetworkPosition _netPos;

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
        get { return _isHeld; } 
        set {
            _isHeld = value;
            if (_isHeld)
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
    [Networked, HideInInspector] public bool _isHeld { get; set; }

    // * ===============================================================

    // * Spawning and Despawning =======================================

    public override void Spawned()
    {
        DontDestroyOnLoad(gameObject);
        ballCol = GetComponent<DodgeballCollider>();
        ballCol.networkBall = this;
        rig = GetComponent<Rigidbody>();
        col = GetComponent<SphereCollider>();
        trail = GetComponent<TrailRenderer>();
        gameObject.SetActive(false);
    }

    /// <summary>
    /// Reset any attributes for this NetworkDodgeball. 
    /// Used by the NetworkBallManager to reset dodgeballs returned by GetBall().
    /// </summary>
    public NetworkDodgeball Reset()
    {
        NetworkSetOwner(PlayerRef.None);
        transform.position = Vector3.zero;
        rig.velocity = Vector3.zero;
        rig.angularVelocity = Vector3.zero;
        return this;
    }

    // * ===============================================================

    // * Throw Physics =================================================

    [Tooltip("The constant speed the ball will travel at will it is deadly.")]
    [SerializeField] private float throwSpeed;
    [Tooltip("The duration the ball will be deadly for after being thrown.")]
    [SerializeField] private float deadlyTime;
    [Tooltip("The ball will stop being deadly after bouncing this many times.")]
    [SerializeField] private int bounceLimit;

    private float deadlyTimer;
    private Vector3 travelDir;
    private int bounceCount;

    public override void FixedUpdateNetwork()
    {
        if (IsDeadly)
        {
            //Rig.AddForce(Vector3.up * Physics.gravity.magnitude * (1f - deadlyGravity));
            Rig.velocity = travelDir * throwSpeed;

            Vector3 start = transform.position;
            Vector3 dir = Rig.velocity.normalized;
            float dist = Rig.velocity.magnitude * Runner.DeltaTime + Col.bounds.extents.x;
            if (Physics.Raycast(start, dir, out RaycastHit hit, dist, LayerMask.GetMask("Surfaces")))
            {
                travelDir = Vector3.Reflect(travelDir, hit.normal);
                bounceCount++;
            }
        }
    }

    public void Throw(Vector3 dir)
    {
        Rig.velocity = dir * throwSpeed;
        travelDir = dir;
        isDeadly = true;
        deadlyTimer = deadlyTime;
        bounceCount = 0;
    }

    private void Update()
    {
        setTrail();
        if (deadlyTimer > 0)
        {
            deadlyTimer -= Time.deltaTime;
            if (deadlyTimer <= 0 || bounceCount >= bounceLimit)
            {
                if (Runner.IsServer)
                {
                    NetworkSetOwner(PlayerRef.None);
                }
            }
        }
    }

    public void setTrail()
    {
        trail.emitting = IsDeadly;
        //trail.material.color = NetworkPlayerManager.Instance.GetColor(Owner).color;
    }

    // * ===============================================================

    // * Ball-Buff Events ==============================================

    

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
        if (player == PlayerRef.None)
        {
            transform.SetParent(null);
            isDeadly = false;
        }
        else
        {
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
