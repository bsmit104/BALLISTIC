using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Fusion;
using Fusion.Sockets;
using UnityEngine.SceneManagement;
using System.Threading.Tasks;
using System;
using System.Linq;
using TMPro;

/// <summary>
/// Spawns the NetworkRunner to start the online lobby.
/// OnHost(), and OnClient() methods are main interface for starting the game.
/// </summary>
public class NetworkRunnerHandler : MonoBehaviour
{
    [Tooltip("Empty game object with the NetworkRunner and NetworkPlayerManager scripts attached.")]
    [SerializeField] private NetworkRunner networkRunnerPrefab;

    [Tooltip("Loading popup prefab spawned when loading into a game.")]
    [SerializeField] private GameObject loadingPopupPrefab;

    [Tooltip("The number of digits that a lobby name will have")]
    [SerializeField] private int lobbyCodeDigits;

    [Tooltip("The build index of the scene to transition to on host/join. Find in build settings.")]
    [SerializeField] private int lobbyScene;

    [SerializeField] private TMP_InputField lobbyNameInputField;

    private NetworkRunner networkRunner; // the current runner instance

    private string _lobbyName = "";

    /// <summary>
    /// The lobby name/code for the currently joined game. 
    /// Will return an empty string if not in a lobby.
    /// </summary>
    public string LobbyName { get {return _lobbyName; } }
    
    /// <summary>
    /// Start a game as a host, uses a random number (turned to string) as the lobby name?
    /// </summary>
    public void OnHost()
    {
        if (FindFirstObjectByType<NetworkRunner>() != null)
        {
            return;
        }
        string lobbyName = UnityEngine.Random.Range((int)Mathf.Pow(10, (lobbyCodeDigits - 1)), (int)Mathf.Pow(10, lobbyCodeDigits)).ToString();
        Debug.Log("OnHost Clicked, creating lobby: " + lobbyName);
        Instantiate(loadingPopupPrefab);
        StartGame(GameMode.Host, lobbyName);
    }

    /// <summary>
    /// Joins an existing game as a client, fails if a fetched lobby name is not found.
    /// </summary>
    public void OnClient()
    {
        string lobbyName = lobbyNameInputField.text;
        if (lobbyName.Length != lobbyCodeDigits || FindFirstObjectByType<NetworkRunner>() != null)
        {
            return;
        }
        Instantiate(loadingPopupPrefab);
        StartGame(GameMode.Client, lobbyName);
    }

    // Creates a new NetworkRunner, and joins a Photon lobby
    private void StartGame(GameMode mode, string lobbyName)
    {
        networkRunner = Instantiate(networkRunnerPrefab);
        networkRunner.ProvideInput = true;
        networkRunner.name = "Network Runner";

        var clientTask = InitializeNetworkRunner(
            networkRunner,
            mode,
            SceneRef.FromIndex(lobbyScene),
            lobbyName
        );

        Debug.Log("Game Started");
    }

    // Fetch a NetworkSceneManager, and initialize the NetworkRunner's state
    private Task InitializeNetworkRunner(
        NetworkRunner runner,
        GameMode mode,
        SceneRef scene,
        string lobbyName
    ) {
        var sceneManager = runner.GetComponents(typeof(MonoBehaviour)).OfType<INetworkSceneManager>().FirstOrDefault();

        if (sceneManager == null)
        {
            sceneManager = runner.gameObject.AddComponent<NetworkSceneManagerDefault>();
        }

        return runner.StartGame(new StartGameArgs{
            GameMode = mode,
            Scene = scene,
            SessionName = lobbyName,
            SceneManager = sceneManager
        });
    }
}
