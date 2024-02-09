using System.Collections;
using System.Collections.Generic;
using Fusion;
using Fusion.Sockets;
using UnityEngine;
using System;

/// <summary>
/// Manages instances of NetworkPlayers, each joined client will have a NetworkPlayer assigned to represent them in-game.
/// </summary>
public class NetworkPlayerManager : MonoBehaviour, INetworkRunnerCallbacks
{
    private static NetworkPlayerManager _instance = null;
    /// <summary>
    /// Returns the singleton instance of the local NetworkPlayerManager.
    /// </summary>
    public static NetworkPlayerManager Instance { get { return _instance; } }

    private NetworkRunner runner;
    private NetworkBallManager ballManager;

    void Awake()
    {
        if (_instance != null && _instance != this)
        {
            Debug.LogError("NetworkPlayerManager singleton instantiated twice");
            Destroy(this);
        }
        _instance = this;

        runner = GetComponent<NetworkRunner>();
        if (runner == null)
        {
            Debug.LogError("No NetworkRunner found");
        }

        ballManager = GetComponent<NetworkBallManager>();
        if (ballManager == null)
        {
            Debug.LogError("No NetworkBallManager found");
        }
        ballManager.Init(runner);
    }


    [Tooltip("Prefab that will be instantiated for each player, this has the character controller")]
    [SerializeField] private NetworkPrefabRef playerPrefab;
    private Dictionary<PlayerRef, NetworkPlayer> spawnedPlayers = new Dictionary<PlayerRef, NetworkPlayer>();

    /// <summary>
    /// Get the NetworkPlayer linked to the given playerRef
    /// </summary>
    /// <param name="playerRef">Synchronized, unique player identifier</param>
    /// <returns>NetworkPlayer instance</returns>
    public static NetworkPlayer GetPlayer(PlayerRef playerRef)
    {
        return _instance.spawnedPlayers[playerRef];
    }

    // * Network Events =========================================

    public void OnPlayerJoined(NetworkRunner runner, PlayerRef player) 
    {
        if (runner.IsServer)
        {
            // Create a unique position for the player
            Vector3 spawnPosition = GetSpawnPosition(player.RawEncoded);
            NetworkObject networkPlayerObject = runner.Spawn(playerPrefab, spawnPosition, Quaternion.identity, player);
            // Keep track of the player avatars for easy access
            spawnedPlayers.Add(player, networkPlayerObject.gameObject.GetComponent<NetworkPlayer>());
            runner.SetPlayerObject(player, networkPlayerObject);

            Debug.Log($"Added player no. {player.RawEncoded}");
        }
        else
        {
            Debug.Log($"Player {player.RawEncoded} Joined The Game");
        }
    }

    // TODO: create a better player spawn position method
    private Vector3 GetSpawnPosition(int playerNum)
    {
        return new Vector3(playerNum * 3, 1, 0);
    }

    public void OnPlayerLeft(NetworkRunner runner, PlayerRef player) 
    { 
        if (spawnedPlayers.TryGetValue(player, out NetworkPlayer networkPlayer))
        {
            runner.Despawn(networkPlayer.gameObject.GetComponent<NetworkObject>());
            spawnedPlayers.Remove(player);
        }
    }


    public void OnInput(NetworkRunner runner, NetworkInput input) 
    { 
        var data = new NetworkInputData();

        data.horizontal = Input.GetAxis("Horizontal");
        data.vertical = Input.GetAxis("Vertical");
        data.sprintButtonPressed = Input.GetKey(KeyCode.LeftShift);
        data.throwButtonPressed = Input.GetMouseButton(0);

        input.Set(data);
    }

    // * Other Network Events ===================================================
    // TODO: Handle all events

    public void OnInputMissing(NetworkRunner runner, PlayerRef player, NetworkInput input) { }

    public void OnShutdown(NetworkRunner runner, ShutdownReason shutdownReason) 
    { 
        Application.Quit();
    }

    public void OnConnectedToServer(NetworkRunner runner) { }
    
    public void OnDisconnectedFromServer(NetworkRunner runner, NetDisconnectReason reason) 
    {
        Application.Quit();
    }

    public void OnConnectRequest(NetworkRunner runner, NetworkRunnerCallbackArgs.ConnectRequest request, byte[] token) { }

    public void OnConnectFailed(NetworkRunner runner, NetAddress remoteAddress, NetConnectFailedReason reason) 
    { 
        Application.Quit();
    }

    public void OnUserSimulationMessage(NetworkRunner runner, SimulationMessagePtr message) { }
    public void OnSessionListUpdated(NetworkRunner runner, List<SessionInfo> sessionList) { }
    public void OnCustomAuthenticationResponse(NetworkRunner runner, Dictionary<string, object> data) { }
    public void OnHostMigration(NetworkRunner runner, HostMigrationToken hostMigrationToken) { }
    public void OnSceneLoadDone(NetworkRunner runner) { }
    public void OnSceneLoadStart(NetworkRunner runner) { }
    public void OnObjectExitAOI(NetworkRunner runner, NetworkObject obj, PlayerRef player){ }
    public void OnObjectEnterAOI(NetworkRunner runner, NetworkObject obj, PlayerRef player){ }
    public void OnReliableDataReceived(NetworkRunner runner, PlayerRef player, ReliableKey key, ArraySegment<byte> data){ }
    public void OnReliableDataProgress(NetworkRunner runner, PlayerRef player, ReliableKey key, float progress){ }
}
