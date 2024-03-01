using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Fusion;

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

    // * ===============================================================

    // * Networked Attributes ==========================================

    private ChangeDetector detector;
    private Dictionary<string, Notify> networkChangeListeners;
    // Creates a map of event listeners for networked attribute changes.
    private void SetChangeListeners()
    {
        networkChangeListeners = new Dictionary<string, Notify>{
            // ? Example: { nameof(myAttribute), MyAttributeOnChange }
        };
    }

    // ? Example:
    // [Networked] Type myAttribute { get; set; }
    // void MyAttributeOnChange() { ... }

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
    [Networked, HideInInspector] public bool isHeld { get; set; }

    // Detect changes, and trigger event listeners.
    public override void Render()
    {
        // foreach (var attrName in detector.DetectChanges(this))
        // {
        //     if (networkChangeListeners.ContainsKey(attrName))
        //     {
        //         networkChangeListeners[attrName]();
        //     } 
        // }
    }

    // * ===============================================================

    // * Spawning and Despawning =======================================

    public override void Spawned()
    {
        DontDestroyOnLoad(gameObject);
        SetChangeListeners();
        detector = GetChangeDetector(ChangeDetector.Source.SimulationState);
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
        return this;
    }

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
        }
        else
        {
            transform.SetParent(NetworkPlayerManager.GetPlayer(owner).throwPoint);
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

    public void setTrail()
    {
            trail.emitting = owner != PlayerRef.None && !isHeld;
    }

    // =========================================

    // add force ===============================

    /// <summary>
    /// Wrapper around Rigidbody.AddForce(). Applies to client simulation for prediction, and sends the 
    /// function call to the host for synchronization.
    /// </summary>
    /// <param name="force">The force vector passed to Rigidbody.AddForce()</param>
    public void NetworkAddForce(Vector3 force)
    {
        if (Runner.IsServer)
        {
            RPC_EnforceAddForce(force);
        }
        else
        {
            rig.AddForce(force, ForceMode.Impulse);
            RPC_RequestAddForce(force);
        }
    }

    [Rpc(RpcSources.StateAuthority, RpcTargets.All, HostMode = RpcHostMode.SourceIsServer)]
    public void RPC_EnforceAddForce(Vector3 force)
    {
        rig.AddForce(force, ForceMode.Impulse);
    }

    [Rpc(RpcSources.Proxies, RpcTargets.StateAuthority, HostMode = RpcHostMode.SourceIsHostPlayer)]
    public void RPC_RequestAddForce(Vector3 force)
    {
        RPC_EnforceAddForce(force);
    }

    private void Update()
    {
        setTrail();
    }

}
