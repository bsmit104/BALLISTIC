using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Fusion;
using Fusion.Sockets;
using System;
using Unity.VisualScripting;

/// <summary>
/// Implements all network events, and initializes managers.
/// </summary>
public class NetworkRunnerCallbacks : MonoBehaviour, INetworkRunnerCallbacks
{
    /// <summary>
    /// The singleton instance of the NetworkRunnerCallbacks.
    /// </summary>
    public static NetworkRunnerCallbacks Instance { get { return _instance; } }
    private static NetworkRunnerCallbacks _instance = null;

    // * Managers =========================================

    private NetworkRunner runner;
    private NetworkPlayerManager playerManager;
    private NetworkBallManager ballManager;
    private NetworkLevelManager levelManager;

    /// <summary>
    /// Returns true if managers have not been initialized yet.
    /// </summary>
    public bool NotInitialized { get { return notInitialized; } }
    private bool notInitialized = true;

    void Awake()
    {
        if (_instance != null && _instance != this)
        {
            Debug.LogError("NetworkRunnerCallbacks singleton instantiated twice.");
            Destroy(gameObject);
            return;
        }
        _instance = this;
    }

    private void InitManagers()
    {
        runner = GetComponent<NetworkRunner>();
        if (runner == null)
        {
            Debug.LogError("NetworkRunner not found.");
        }

        playerManager = GetComponent<NetworkPlayerManager>();
        if (playerManager == null)
        {
            Debug.LogError("No NetworkPlayerManager instance found.");
        }

        ballManager = GetComponent<NetworkBallManager>();
        if (ballManager == null)
        {
            Debug.LogError("No NetworkBallManager instance found.");
        }

        levelManager = GetComponent<NetworkLevelManager>();
        if (levelManager == null)
        {
            Debug.LogError("No NetworkLevelManager instance found.");
        }

        playerManager.Init(runner, levelManager);
        ballManager.Init(runner);
        levelManager.Init(runner, playerManager, ballManager);
        notInitialized = false;
    }

    // * ==================================================

    // * Joining and Leaving: Host Controlled =============

    public void OnPlayerJoined(NetworkRunner runner, PlayerRef player)
    {
        if (runner.IsServer && NotInitialized)
        {
            InitManagers();
        }

        if (runner.IsServer)
        {
            playerManager.SpawnPlayer(player).SetColor(playerManager.GetColor(player).material);

            Debug.Log($"Added player no. {player.PlayerId}");
        }
        else
        {
            Debug.Log($"Player {player.PlayerId} Joined The Game");
        }
    }

    public void OnPlayerLeft(NetworkRunner runner, PlayerRef player)
    {
        playerManager.DespawnPlayer(player);
    }

    // * ==================================================

    // * Input Reading: Client-Sided ======================

    public void OnInput(NetworkRunner runner, NetworkInput input)
    {
        var data = new NetworkInputData();

        data.horizontal = Input.GetAxis("Horizontal");
        data.vertical = Input.GetAxis("Vertical");
        data.sprintButtonPressed = Input.GetKey(KeyCode.LeftShift);
        data.throwButtonPressed = Input.GetMouseButton(0);
        data.testButtonPressed = Input.GetKey(KeyCode.R);
        data.jumpButtonPressed = Input.GetKey(KeyCode.Space);
        data.crouchButtonPressed = Input.GetKey(KeyCode.C);

        input.Set(data);
    }

    public void OnInputMissing(NetworkRunner runner, PlayerRef player, NetworkInput input) { }

    // * ==================================================

    // * On Connection & On Disconnection =================

    public void OnShutdown(NetworkRunner runner, ShutdownReason shutdownReason)
    {
        Application.Quit();
    }

    public void OnConnectedToServer(NetworkRunner runner)
    {
        InitManagers();
        notInitialized = true;
    }

    public void OnDisconnectedFromServer(NetworkRunner runner, NetDisconnectReason reason)
    {
        Application.Quit();
    }

    public void OnConnectRequest(NetworkRunner runner, NetworkRunnerCallbackArgs.ConnectRequest request, byte[] token)
    {
        if (playerManager.LobbyHasSpace)
        {
            request.Accept();
        }
        else
        {
            request.Refuse();
        }
    }

    public void OnConnectFailed(NetworkRunner runner, NetAddress remoteAddress, NetConnectFailedReason reason)
    {
        Application.Quit();
    }

    // * ==================================================

    // * Scene Loading Events =============================

    public void OnSceneLoadDone(NetworkRunner runner) { }
    public void OnSceneLoadStart(NetworkRunner runner) { }

    // * ==================================================

    // * Other Stuff: Not Important =======================

    public void OnUserSimulationMessage(NetworkRunner runner, SimulationMessagePtr message) { }
    public void OnSessionListUpdated(NetworkRunner runner, List<SessionInfo> sessionList) { }
    public void OnCustomAuthenticationResponse(NetworkRunner runner, Dictionary<string, object> data) { }
    public void OnHostMigration(NetworkRunner runner, HostMigrationToken hostMigrationToken) { }
    public void OnObjectExitAOI(NetworkRunner runner, NetworkObject obj, PlayerRef player) { }
    public void OnObjectEnterAOI(NetworkRunner runner, NetworkObject obj, PlayerRef player) { }
    public void OnReliableDataReceived(NetworkRunner runner, PlayerRef player, ReliableKey key, ArraySegment<byte> data) { }
    public void OnReliableDataProgress(NetworkRunner runner, PlayerRef player, ReliableKey key, float progress) { }
}
