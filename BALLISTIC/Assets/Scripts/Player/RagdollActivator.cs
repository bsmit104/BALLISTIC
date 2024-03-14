using System.Collections;
using System.Collections.Generic;
using Fusion;
using UnityEngine;

public class RagdollActivator : MonoBehaviour
{
    private Transform parent;
    private NetworkPlayer player;

    private NetworkDodgeball ragdollBall;
    private BallBuff ragdollBuff;

    public void Init(NetworkPlayer player)
    {
        parent = transform.parent;
        this.player = player;
        ragdollBuff = GetComponent<RobotRagdoll>();
        ragdollBuff.Title = player.Color.colorName + " Robot";
        ragdollBuff.Description = "You wouldn't dare...";
    }
    
    public void ActivateRagdoll()
    {
        ActivateColliders();
        transform.SetParent(null);
        
        if (player.Runner.IsServer)
        {
            ragdollBall = NetworkBallManager.Instance.GetBall();
            RagdollActivatorMessages.RPC_SendRagdollBall(player.Runner, player.GetRef, ragdollBall.NetworkID);
        }
    }

    public void SetRagdollBall(NetworkId ballId)
    {
        StartCoroutine(FindBall(ballId));
    }

    IEnumerator FindBall(NetworkId ballId)
    {
        float timer = 10f;

        NetworkObject ballObj = null;
        while (!player.Runner.TryFindObject(ballId, out ballObj) && timer > 0)
        {
            timer -= Time.deltaTime;
            yield return null;
        }

        if (ballObj)
        {
            ragdollBall = ballObj.GetComponent<NetworkDodgeball>();
            ragdollBall.GetComponent<MeshRenderer>().enabled = false;
            // ragdollBall.Col.enabled = false;
            ragdollBall.Rig.isKinematic = true;
            // ragdollBall.Rig.detectCollisions = false;
            ragdollBall.transform.SetParent(transform);
            ragdollBall.transform.localPosition = Vector3.zero;
            ragdollBall.SetBuff(ragdollBuff);
        }
    }

    public void DeactivateRagdoll()
    {
        if (ragdollBall)
        {
            ragdollBall.GetComponent<MeshRenderer>().enabled = true;
            ragdollBall.Col.enabled = true;
            ragdollBall.Rig.isKinematic = false;
            ragdollBall.Rig.detectCollisions = true;
            ragdollBall.transform.SetParent(null);
            ragdollBall = null;
        }
        DeactivateColliders();
        transform.SetParent(parent);
        transform.localPosition = Vector3.zero;
    }

    private void ActivateColliders()
    {
        RecurseColliders(transform, true);
    }

    private void DeactivateColliders()
    {
        RecurseColliders(transform, false);
    }

    private void RecurseColliders(Transform root, bool state)
    {
        Collider col = null;
        if ((col = root.gameObject.GetComponent<Collider>()) != null && NetworkPlayer.Local.Runner.IsServer)
        {
            col.enabled = state;
            var rb = root.gameObject.GetComponent<Rigidbody>();
            if (rb)
            {
                rb.isKinematic = !state;
            }
        }
        NetworkPosition pos = null;
        if ((pos = root.gameObject.GetComponent<NetworkPosition>()) != null)
        {
            pos.enabled = state;
            pos.ForceUpdate = true;
        }

        for (int i = 0; i < root.childCount; i++)
        {
            RecurseColliders(root.GetChild(i), state);
        }
    }

    public void OnPickup()
    {
        SetCollisionLayers(transform, LayerMask.GetMask("Players", "LocalPlayer", "BallMarkerVisible", "BallMarkerHidden"));
    }

    public void OnThrow()
    {
        SetCollisionLayers(transform, LayerMask.GetMask("BallMarkerVisible", "BallMarkerHidden"));
    }

    public void SetCollisionLayers(Transform root, LayerMask ignoreLayers)
    {
        Collider col = null;
        if ((col = root.gameObject.GetComponent<Collider>()) != null && NetworkPlayer.Local.Runner.IsServer)
        {
            col.excludeLayers = ignoreLayers;
        }

        for (int i = 0; i < root.childCount; i++)
        {
            SetCollisionLayers(root.GetChild(i), ignoreLayers);
        }
    }

    void OnCollisionEnter(Collision col)
    {
        ragdollBall?.BallCol.OnCollisionEnter(col);
    }
}

public class RagdollActivatorMessages : SimulationBehaviour
{
    [Rpc]
    public static void RPC_SendRagdollBall(NetworkRunner runner, PlayerRef player, NetworkId ballId)
    {
        NetworkPlayerManager.Instance.GetPlayer(player).RagdollActivator.SetRagdollBall(ballId);
    }
}
