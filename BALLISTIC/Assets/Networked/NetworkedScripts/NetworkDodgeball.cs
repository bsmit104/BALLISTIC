using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Fusion;

/// <summary>
/// Networked object to manage dodgeball. Spawn and release using the NetworkBallManager.
/// </summary>
[RequireComponent(typeof(DodgeballCollider))]
[RequireComponent(typeof(Rigidbody))]
public class NetworkDodgeball : NetworkBehaviour
{
    // * Client-Sided Attributes =======================================

    private DodgeballCollider ballCol;

    private Rigidbody rig;
    public Rigidbody GetRigidbody()
    {
        return rig;
    }

    private SphereCollider col;
    public SphereCollider Col { get { return col; } }

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
            { nameof(owner), SourceOnChange }
        };
    }

    // ? Example:
    // [Networked] Type myAttribute { get; set; }
    // void MyAttributeOnChange() { ... }

    /// <summary>
    /// Sets who threw the ball, or is currently holding it.
    /// </summary>
    [Networked, HideInInspector] public PlayerRef owner { get; set; }
    void SourceOnChange() 
    { 
        //TODO: idk something is gonna have to go here, but whatever
    }

    [Networked, HideInInspector] public bool isHeld { get; set; }

    // Detect changes, and trigger event listeners.
    public override void Render()
    {
        foreach (var attrName in detector.DetectChanges(this))
        {
            if (networkChangeListeners.ContainsKey(attrName))
            {
                networkChangeListeners[attrName]();
            } 
        }
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
        gameObject.SetActive(false);
    }

    /// <summary>
    /// Reset any attributes for this NetworkDodgeball. 
    /// Used by the NetworkBallManager to reset dodgeballs returned by GetBall().
    /// 
    /// </summary>
    public NetworkDodgeball Reset()
    {
        owner = PlayerRef.None;
        transform.position = Vector3.zero;
        rig.velocity = Vector3.zero;
        return this;
    }

    // * ===============================================================

    // * Remote Procedure Calls ========================================

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
        Debug.Log("received disable signal");
    }

    [Rpc(RpcSources.Proxies, RpcTargets.StateAuthority, HostMode = RpcHostMode.SourceIsHostPlayer)]
    public void RPC_RequestSetActive(bool state)
    {
        RPC_EnforceSetActive(state);
    }


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
}
