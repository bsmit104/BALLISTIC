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
            ballManager.SendBallStates(player);

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

    [Header("Pause Menu")]
    [SerializeField] private GameObject PauseCanvas;
    [SerializeField] private TextMeshProUGUI lobbyCodeText;

    public bool IsPaused { get { return PauseCanvas.activeInHierarchy; } }

    public void Unpause()
    {
        // toggle cursor visibility and lock state
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;

        // enable pause menu canvas
        PauseCanvas.SetActive(false);
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape) || Input.GetKeyDown(KeyCode.U))
        {
            // toggle cursor visibility and lock state
            Cursor.visible = !Cursor.visible;
            Cursor.lockState = Cursor.visible ? CursorLockMode.None : CursorLockMode.Locked;

            // enable pause menu canvas
            PauseCanvas.SetActive(Cursor.visible);

            lobbyCodeText.text = "Lobby Code: " + runner.SessionInfo.Name;
        }

        if (Input.GetKeyDown(KeyCode.O))
        {
            NetworkPlayer.Local.SetHUDActive(!NetworkPlayer.Local.IsHUDActive);
        }
    }

    public void OnInput(NetworkRunner runner, NetworkInput input)
    {
        var data = new NetworkInputData();

        if (!IsPaused)
        {
            data.horizontal = Input.GetAxis("Horizontal");
            data.vertical = Input.GetAxis("Vertical");
            data.sprintButtonPressed = Input.GetKey(KeyCode.LeftShift);
            data.throwButtonPressed = Input.GetMouseButton(0);
            data.testButtonPressed = Input.GetKey(KeyCode.R);
            data.jumpButtonPressed = Input.GetKey(KeyCode.Space);
            data.crouchButtonPressed = Input.GetKey(KeyCode.C);
        }

        input.Set(data);
    }

    public void OnInputMissing(NetworkRunner runner, PlayerRef player, NetworkInput input) { }

    // * ==================================================

    // * On Connection & On Disconnection =================

    [Space]
    [Header("Connection Status Popups")]
    [SerializeField] ConnectionPopup popupPrefab;

    /// <summary>
    /// Leave the current lobby. If the player is the host, then the lobby will be shutdown,
    /// and all players will be disconnected.
    /// </summary>
    public void LeaveGame()
    {
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        runner.Shutdown(true, ShutdownReason.Ok);
        SceneManager.LoadScene("PlayMenu");
    }

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
        var popup = Instantiate(popupPrefab);
        popup.SetText(message);
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
                runner.Shutdown(true, ShutdownReason.ConnectionTimeout);
                break;
            case NetDisconnectReason.Requested:
                runner.Shutdown(true, ShutdownReason.Ok);
                break;
            default:
                runner.Shutdown(true, ShutdownReason.Error);
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
        runner.Shutdown(true, ShutdownReason.ConnectionRefused);
    }

    // * ==================================================

    // * Scene Loading Events =============================

    public void OnSceneLoadDone(NetworkRunner runner)
    {

    }
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
