using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Fusion;

public class LoadingDetector : MonoBehaviour
{
    public static bool IsLoaded;

    void Start()
    {
        IsLoaded = true;
        LevelLoadedMessage.RPC_ClientHasLoaded(NetworkRunner.Instances[0]);
    }

    void OnDestroy()
    {
        IsLoaded = false;
    }
}

public class LevelLoadedMessage : SimulationBehaviour
{
    [Rpc]
    public static void RPC_ClientHasLoaded(NetworkRunner runner)
    {
        NetworkLevelManager.Instance.ClientLoaded();
    }
}
