using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Fusion;
using Fusion.Sockets;
using System;
using TMPro;
using UnityEngine.SceneManagement;

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

    [Header("Joining and Leaving Popups")]
    [SerializeField] private GameObject joinLeaveCanvas;
    [SerializeField] private TextMeshProUGUI joinLeaveText;
    [SerializeField] private float joinLeaveDisplayTime;

    private Coroutine joinLeaveTween;

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

        if (joinLeaveTween != null)
        {
            StopCoroutine(joinLeaveTween);
        }
        joinLeaveTween = StartCoroutine(DisplayJoinLeave(playerManager.GetColor(player).colorName + " Has Joined"));
    }

    public void OnPlayerLeft(NetworkRunner runner, PlayerRef player)
    {
        playerManager.DespawnPlayer(player);
        if (joinLeaveTween != null)
        {
            StopCoroutine(joinLeaveTween);
        }
        joinLeaveTween = StartCoroutine(DisplayJoinLeave(playerManager.GetColor(player).colorName + " Has Left"));
    }

    private IEnumerator DisplayJoinLeave(string message)
    {
        float timer = joinLeaveDisplayTime;

        joinLeaveText.text = message;
        joinLeaveCanvas.SetActive(true);

        while (timer > 0)
        {
            timer -= Time.deltaTime;
            yield return null;
        }

        joinLeaveCanvas.SetActive(false);
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

    [Space]
    [Header("Connection Status Popups")]
    [SerializeField] private GameObject connectionStatusCanvas;
    [SerializeField] private TextMeshProUGUI connectionStatusText;

    Coroutine shutdownPopup;
    private bool closedPopup = false;

    public void OnShutdown(NetworkRunner runner, ShutdownReason shutdownReason)
    {
        string message = "Network Connection Closed:\n";
        switch (shutdownReason)
        {
            case ShutdownReason.Ok:
                message += "Host has ended the game.";
                break;
            case ShutdownReason.ServerInRoom:
            case ShutdownReason.GameIdAlreadyExists:
                message += "Failed to host lobby.";
                break;
            case ShutdownReason.GameNotFound:
                message += "Lobby could not be found.";
                break;
            case ShutdownReason.ConnectionRefused:
            case ShutdownReason.GameIsFull:
                message += "Lobby could not be joined.";
                break;
            case ShutdownReason.ConnectionTimeout:
            case ShutdownReason.OperationTimeout:
            case ShutdownReason.PhotonCloudTimeout:
                message += "Connection timeout.";
                break;
            default:
                message += "Network Error.";
                break;
        }
        if (shutdownPopup != null)
        {
            StopCoroutine(shutdownPopup);
        }
        StartCoroutine(ShutdownPopup(message));
    }


    private IEnumerator ShutdownPopup(string message)
    {
        connectionStatusText.text = message;
        connectionStatusCanvas.SetActive(true);

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        while (!closedPopup)
        {
            yield return null;
        }

        SceneManager.LoadScene("PlayMenu");
        Destroy(gameObject);
    }

    public void ClosePopup()
    {
        closedPopup = true;
    }

    public void OnConnectedToServer(NetworkRunner runner)
    {
        InitManagers();
        notInitialized = true;
    }

    public void OnDisconnectedFromServer(NetworkRunner runner, NetDisconnectReason reason)
    {
        switch (reason)
        {
            case NetDisconnectReason.Timeout:
                runner.Shutdown(false, ShutdownReason.ConnectionTimeout);
                break;
            case NetDisconnectReason.Requested:
                runner.Shutdown(false, ShutdownReason.Ok);
                break;
            default:
                runner.Shutdown(false, ShutdownReason.Error);
                break;
        }
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
        runner.Shutdown(false, ShutdownReason.ConnectionRefused);
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
