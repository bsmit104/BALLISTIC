using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Fusion;
using UnityEngine.SceneManagement;
using System.Threading.Tasks;
using System.Linq;

/// <summary>
/// Use to make testing specific levels easier. Press Play while in the level's scene.
/// This will spawn a new NetworkRunner, player, and dodgeballs for you.
/// </summary>
public class LevelTester : MonoBehaviour
{
    [Tooltip("Empty game object with the NetworkRunner and NetworkPlayerManager scripts attached.")]
    [SerializeField] private NetworkRunner networkRunnerPrefab;

    void Awake()
    {
        Destroy(transform.GetChild(0).gameObject); // Delete reference player
        if (FindFirstObjectByType<NetworkRunner>() != null)
        {
            return;
        }
        string lobbyName = "Test-" + SceneManager.GetActiveScene().name;
        Debug.Log("Creating lobby: " + lobbyName);
        StartGame(GameMode.Host, lobbyName);
    }

    // Creates a new NetworkRunner, and joins a Photon lobby
    private void StartGame(GameMode mode, string lobbyName)
    {
        var networkRunner = Instantiate(networkRunnerPrefab);
        networkRunner.ProvideInput = true;
        networkRunner.name = "Network Runner";

        var clientTask = InitializeNetworkRunner(
            networkRunner,
            mode,
            SceneRef.FromIndex(SceneManager.GetActiveScene().buildIndex),
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
