using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Fusion;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

// ! ANY SERIALIZED FIELDS NEED TO BE ADDED TO THE EDITOR SCRIPT

/// <summary>
/// Manages scene transitions between levels.
/// Tracks how many players are alive to know when to end the game, and then move on to the next level.
/// </summary>
public class NetworkLevelManager : MonoBehaviour
{
    /// <summary>
    /// Get the global level manager instance.
    /// </summary>
    public static NetworkLevelManager Instance { get { return _instance; } }
    private static NetworkLevelManager _instance = null;

    // * Managers and Init ================================

    /// <summary>
    /// The local network runner.
    /// </summary>
    public static NetworkRunner Runner { get { return _runner; } }
    private static NetworkRunner _runner = null;

    private NetworkPlayerManager playerManager;
    private NetworkBallManager ballManager;

    private Coroutine waitForGameStart;

    /// <summary>
    /// Initializes the level manager, should only be called once when the NetworkRunnerPrefab is created.
    /// </summary>
    public void Init(NetworkRunner runner, NetworkPlayerManager players, NetworkBallManager balls)
    {
        if (_instance != null && _instance != this)
        {
            Debug.LogError("NetworkLevelManager singleton instantiated twice");
            Destroy(this);
        }
        _instance = this;
        _runner = runner;
        playerManager = players;
        ballManager = balls;

        // Init level picking values
        numLevels = lastLevelIndex - firstLevelIndex + 1;
        remainingLevels = new List<int>(numLevels);

        // If starting level isn't the lobby, then the level si being tested
        if (IsInLevel || SceneManager.GetActiveScene().name == "BallBuffTest")
        {
            ResetLevel();
            return;
        }
        
        // Display the lobby code while waiting for the game to start
        if (Runner.IsServer)
        {
            waitForGameStart = StartCoroutine(WaitForGameStart());
        }
    }

    [Header("Lobby Code")]
    [Tooltip("The canvas which will display the lobby code at the start of the game.")]
    [SerializeField] private GameObject lobbyCanvas;
    [Tooltip("The text attached to the lobbyCanvas used to display the lobby code.")]
    [SerializeField] private TextMeshProUGUI lobbyCodeText;

    IEnumerator WaitForGameStart()
    {
        lobbyCodeText.text = "Lobby Code: " + Runner.SessionInfo.Name + "\nPress P To Start";
        lobbyCanvas.SetActive(true);
        while (true)
        {
            if (Runner.IsServer && Input.GetKeyDown(KeyCode.P))
            {
                break;
            }
            yield return null;
        }
        lobbyCanvas.SetActive(false);

        StartLevelTransition(GetRandomLevel());
    }

    // * ==================================================

    [Header("Scene Indices")]
    [Tooltip("The build index of the lobby scene")]
    [SerializeField] private int lobbySceneIndex;
    [Tooltip("The next level will be picked based on a range of scene build indices. Each level should be placed in a row.")]
    [SerializeField] private int firstLevelIndex;
    [Tooltip("The next level will be picked based on a range of scene build indices. Each level should be placed in a row.")]
    [SerializeField] private int lastLevelIndex;

    /// <summary>
    /// Returns true if the current scene loaded in the lobby waiting room.
    /// </summary>
    public bool IsAtLobby { get { 
        return SceneManager.GetActiveScene().buildIndex == lobbySceneIndex;
    } }

    /// <summary>
    /// Returns true if the current scene loaded is one of the levels.
    /// </summary>
    public bool IsInLevel { get {
        return firstLevelIndex <= SceneManager.GetActiveScene().buildIndex
            && SceneManager.GetActiveScene().buildIndex <= lastLevelIndex;
    }}

    // * Level Picking ==========================================

    [Space]
    [Header("Level Picking")]
    [Tooltip("The level manager will track which levels have been played. Refreshing will reset this list.")]
    [Range(0f, 1f)]
    [SerializeField] private float refreshChance;

    // the number of levels to pick from
    private int numLevels;

    // an array of levels that haven't been played yet
    private List<int> remainingLevels;

    // reset the remaining levels
    private void Refresh()
    {
        remainingLevels.Clear();
        for (int i = 0; i < numLevels; i++) 
        {
            remainingLevels.Add(firstLevelIndex + i);
        }
    }

    /// <summary>
    /// Gets a random scene build index for a level.
    /// Avoids revisiting levels, which can be controlled with the "refreshChance" attribute.
    /// </summary>
    /// <returns>A scene build index.</returns>
    public int GetRandomLevel()
    {
        // Refresh the level queue if it's empty, or by random chance
        if (remainingLevels.Count == 0 || Random.Range(0f, 1f) < refreshChance)
        {
            Refresh();
        }

        // Pick a random level
        int index = Random.Range(0, remainingLevels.Count);
        int selection = remainingLevels[index];
        remainingLevels.RemoveAt(index);

        return selection;
    }

    // * =========================================================

    // *Transitions ==============================================

    [Space]
    [Header("Transitions")]
    [Tooltip("The canvas used for displaying transitions.")]
    [SerializeField] private GameObject transitionCanvas;
    [Tooltip("The UI element used to create the transition.")]
    [SerializeField] RectTransform transitionElement;

    /// <summary>
    /// Wrapper around GameObject.SetActive().
    /// </summary>
    public void SetTransitionCanvasActive(bool state)
    {
        transitionCanvas?.SetActive(state);
    }

    [Space]
    [Tooltip("Duration for transition into a scene.")]
    [SerializeField] float enterTransitionDuration;
    [SerializeField] Vector2 enterTransitionEndPos;

    [Space]
    [Tooltip("Duration for transition out of a scene.")]
    [SerializeField] float exitTransitionDuration;
    [SerializeField] Vector2 exitTransitionStartPos;
    [Tooltip("Hold on the last frame of the transition to prevent it from being too disorienting.")]
    [SerializeField] float waitBetweenTransitions;

    [Space]
    [Tooltip("Frequency in seconds for checking if the next level has loaded.")]
    [Range(0.05f, 2f)]
    [SerializeField] float loadCompletionCheck = 0.05f;

    // exit transition ============

    private Coroutine exitTransition;

    /// <summary>
    /// Returns true if the exit transition is playing.
    /// </summary>
    public bool ExitRunning { get { return exitRunning; } }
    private bool exitRunning = false;

    public void StartExitTransition()
    {
        if (exitTransition != null)
        {
            StopCoroutine(exitTransition);
        }
        exitRunning = true;
        exitTransition = StartCoroutine(ExitTransition());
    }

    // exit tween (probably some sort of fade to black) to hide scene unloading / game object moving
    IEnumerator ExitTransition()
    {
        SetTransitionCanvasActive(true);
        float timer = exitTransitionDuration;
        transitionElement.anchoredPosition = exitTransitionStartPos;

        float speed = exitTransitionStartPos.magnitude / timer;
        Vector2 dir = exitTransitionStartPos.normalized;

        while (timer > 0)
        {
            timer -= Time.deltaTime;

            transitionElement.anchoredPosition -= speed * Time.deltaTime * dir;

            yield return null;
        }
        transitionElement.anchoredPosition = Vector2.zero;

        // Hold on black for a bit
        timer = waitBetweenTransitions;
        while (timer > 0)
        {
            timer -= Time.deltaTime;
            yield return null;
        }

        // Only deactivate the canvas if this transition isn't being played as part of a larger transition
        if (!LevelChangeRunning && !WinSequenceRunning) SetTransitionCanvasActive(false);
        exitRunning = false;
    }

    // ============================

    // enter transition ===========

    private Coroutine enterTransition;

    /// <summary>
    /// Returns true if the enter transition is playing.
    /// </summary>
    public bool EnterRunning { get { return enterRunning; } }
    private bool enterRunning = false;

    public void StartEnterTransition()
    {
        if (enterTransition != null)
        {
            StopCoroutine(enterTransition);
        }
        enterRunning = true;
        enterTransition = StartCoroutine(EnterTransition());
    }

    // enter tween (probably a fade in from black) to cleanly transition from a loading state into the gameplay
    IEnumerator EnterTransition()
    {
        SetTransitionCanvasActive(true);
        float timer = enterTransitionDuration;
        transitionElement.anchoredPosition = Vector2.zero;

        float speed = enterTransitionEndPos.magnitude / timer;
        Vector2 dir = enterTransitionEndPos.normalized;


        while (timer > 0)
        {
            timer -= Time.deltaTime;

            transitionElement.anchoredPosition += speed * Time.deltaTime * dir;

            yield return null;
        }
        transitionElement.anchoredPosition = enterTransitionEndPos;

        // Only deactivate the canvas if this transition isn't being played as part of a larger transition
        if (!LevelChangeRunning && !WinSequenceRunning) SetTransitionCanvasActive(false);
        enterRunning = false;
    }

    // ============================

    // full transition ============

    private Coroutine fullTransition;

    /// <summary>
    /// Returns true if the level is currently being switched.
    /// </summary>
    public bool LevelChangeRunning { get { return levelChangeRunning; } }
    private bool levelChangeRunning = false;

    /// <summary>
    /// Notify the level manager that a player has finished loading into the next level.
    /// </summary>
    public void ClientLoaded()
    {
        clientsLoaded++;
    }
    private int clientsLoaded = 0;

    /// <summary>
    /// Returns true if all players have fished loading into the next level.
    /// </summary>
    public bool AllClientsLoaded 
    { 
        get 
        { 
            if (Runner.IsServer)
            {
                return clientsLoaded >= NetworkPlayerManager.Instance.PlayerCount; 
            }
            else
            {
                return _allClientsLoaded;
            }
        }
        set
        {
            _allClientsLoaded = value;
            if (!value) clientsLoaded = 0;
        }
    }
    private bool _allClientsLoaded = false;

    /// <summary>
    /// Returns true if the local player has finished loading into the next level.
    /// </summary>
    public bool LocalLevelLoaded
    {
        get { return _localLevelLoaded; }
        set
        {
            _localLevelLoaded = value;
            LevelManagerMessages.RPC_ClientHasLoaded(Runner);
        }
    }
    private bool _localLevelLoaded = false;

    private void StartLevelTransition(int buildIndex)
    {
        if (fullTransition != null)
        {
            StopCoroutine(fullTransition);
        }
        levelChangeRunning = true;
        fullTransition = StartCoroutine(LevelTransition(buildIndex));

        // Host tells clients to start their own level transition tweens
        if (Runner.IsServer)
        {
            LevelManagerMessages.RPC_GoToLevel(Runner, buildIndex);
        }
    }

    // exit transition, load next level, enter transition
    IEnumerator LevelTransition(int buildIndex)
    {
        SetTransitionCanvasActive(true);

        StartExitTransition();
        while (ExitRunning)
        {
            yield return null;
        }

        if (Runner.IsServer) 
        {
            // Remove the previous levels spawner so that Spawner.Instance doesn't refer to it
            Spawner.Instance.Destroy();

            // Load the next scene for all players
            Runner.LoadScene(SceneRef.FromIndex(buildIndex));

            // Wait for the level to load locally
            while (!LocalLevelLoaded)
            {
                yield return new WaitForSeconds(loadCompletionCheck);
            }

            // Prep level for gameplay
            ResetLevel();

            // Wait for all clients to finish loading and have the level ready for play
            while (!AllClientsLoaded || IsResetting)
            {
                yield return new WaitForSeconds(loadCompletionCheck);
            }

            // Tell all players to start their enter level transitions
            LevelManagerMessages.RPC_EnterLevel(Runner);
        }
        else
        {
            // Wait for host to tell them they can enter the level
            while (!AllClientsLoaded)
            {
                yield return new WaitForSeconds(loadCompletionCheck);
            }
            playerManager.ResetPlayers();
        }

        // Reset dirty bools for next level transition
        AllClientsLoaded = false;
        _localLevelLoaded = false;

        StartEnterTransition();
        while (EnterRunning)
        {
            yield return null;
        }

        SetTransitionCanvasActive(false);
        levelChangeRunning = false;
    }

    /// <summary>
    /// Synchronizes scene transitions between clients.
    /// Activates transition animations for unloading and loading the scene.
    /// Scene change only happens on host instance, and is then synchronized.
    /// </summary>
    /// <param name="buildIndex">The build index for the level.</param>
    public void GoToLevel(int buildIndex)
    {
        if (buildIndex != lobbySceneIndex && (buildIndex < firstLevelIndex || lastLevelIndex < buildIndex))
        {
            Debug.LogError("Invalid scene index used in NetworkLevelManager.GoToLevel(): " + buildIndex.ToString());
            return;
        }

        if (LevelChangeRunning) return;

        StartLevelTransition(buildIndex);
    }

    // ============================

    // * =========================================================

    // * Level Prep ==============================================

    [Tooltip("Fixed number of balls to spawn per level.")]
    [SerializeField] private int ballsPerLevel; // TEMP: Hardcoded

    /// <summary>
    /// Returns true if the level is currently being reset/prepared for play.
    /// </summary>
    public bool IsResetting { get { return isResetting; } }
    private bool isResetting = false;

    /// <summary>
    /// Resets the current level to be prepared for play.
    /// Places players and balls into scene.
    /// </summary>
    public void ResetLevel()
    {
        isResetting = true;
        StartCoroutine(WaitForSpawner());
    }

    IEnumerator WaitForSpawner()
    {
        // Wait for spawner to load
        while (Spawner.Instance == null)
        {
            yield return null;
        }
        Debug.Log("resetting");

        // Reset all balls in pool
        ballManager.ReleaseAllBalls();
        for (int i = 0; i < ballsPerLevel; i++)
        {
            var ball = ballManager.GetBall();
            ball.transform.position = Spawner.GetSpawnPoint(ball.Col.bounds);
        }

        // Reset all players
        playerManager.ResetPlayers();

        // Give players random spawn points
        foreach (var player in playerManager.Players)
        {
            player.Value.NetworkSetPosition(Spawner.GetSpawnPoint());
        }

        isResetting = false;
    }

    // * =========================================================

    // * Win State ===============================================

    [Space]
    [Header("Win Screen")]
    [Tooltip("Pause after last player dies before going to the win screen.")]
    [SerializeField] float waitBeforeWinScreen;
    [Tooltip("How long the winner will be displayed before going to the next level.")]
    [SerializeField] float winScreenDuration;
    [Tooltip("What screen is displayed when the round is over.")]
    [SerializeField] private GameObject winScreen;
    [Tooltip("Displays the name of the winner.")]
    [SerializeField] private TextMeshProUGUI winText;
    [Tooltip("Displays the remote player's name that won.")]
    [SerializeField] private GameObject remoteWinText;
    [Tooltip("Displays 'YOURE THE #1 BALLER'")]
    [SerializeField] private GameObject localWinText;

    // The winner of that round
    private PlayerRef winner;

    private Coroutine winnerSequence;

    /// <summary>
    /// Returns true if the winner is currently being displayed.
    /// </summary>
    public bool WinSequenceRunning { get { return winSequenceRunning; } }
    private bool winSequenceRunning = false;

    /// <summary>
    /// Set a winner player, and transition to the winning screen.
    /// Should be called by player manager when only 1 player is left alive.
    /// </summary>
    /// <param name="player">The winning player</param>
    public void DeclareWinner(PlayerRef player)
    {
        if (winSequenceRunning) return;

        winner = player;

        if (winnerSequence != null)
        {
            StopCoroutine(winnerSequence);
        }
        winSequenceRunning = true;
        StartCoroutine(WinnerSequence());

        // Host tells other clients who the winner is
        if (Runner.IsServer)
        {
            LevelManagerMessages.RPC_DeclareWinner(Runner, player);
        }
    }

    private IEnumerator WinnerSequence()
    {
        // Wait before exiting to win screen so winner can run around
        float timer = waitBeforeWinScreen;
        while (timer > 0)
        {
            timer -= Time.deltaTime;
            yield return null;
        }

        SetTransitionCanvasActive(true);
        winSequenceRunning = true;

        StartExitTransition();
        while (ExitRunning)
        {
            yield return null;
        }
        
        // SET WIN SCREEN ACTIVE HERE
        
        StartCoroutine(AnimateWinScreen());

        StartEnterTransition();
        while (EnterRunning)
        {
            yield return null;
        }

        SetTransitionCanvasActive(false);

        timer = winScreenDuration;
        while (timer > 0)
        {
            timer -= Time.deltaTime;

            // something can go here or not...

            yield return null;
        }

        winSequenceRunning = false;

        // go to the next level
        GoToLevel(GetRandomLevel());
    }

    IEnumerator AnimateWinScreen()
    {
        // Display "YOU'RE THE #1 BALLER" if the local player won
        if (NetworkPlayer.Local?.GetRef == winner)
        {
            localWinText.SetActive(true);
            remoteWinText.SetActive(false);
        }
        else
        {
            // Otherwise display the winner's name
            localWinText.SetActive(false);
            remoteWinText.SetActive(true);
            winText.text = playerManager.GetColor(winner).colorName + " IS THE";
        }
        winScreen.SetActive(true);

        while (WinSequenceRunning)
        {
            yield return null;
        }

        float timer = exitTransitionDuration;
        while (timer > 0)
        {
            timer -= Time.deltaTime;
            yield return null;
        }
        winScreen.SetActive(false);
    }

    // * =========================================================

    // * Testing =================================================

    private Coroutine testWinSequence;

    public void TestWinScreen()
    {
        if (testWinSequence != null)
        {
            StopCoroutine(testWinSequence);
        }
        winner = PlayerRef.FromEncoded(4);
        testWinSequence = StartCoroutine(TestWinSequence());
    }

    IEnumerator TestWinSequence()
    {
        SetTransitionCanvasActive(true);
        winSequenceRunning = true;

        StartExitTransition();
        while (ExitRunning)
        {
            yield return null;
        }

        // SET WIN SCREEN ACTIVE HERE
        StartCoroutine(AnimateWinScreen());

        StartEnterTransition();
        while (EnterRunning)
        {
            yield return null;
        }

        SetTransitionCanvasActive(false);

        float timer = winScreenDuration;
        while (timer > 0)
        {
            timer -= Time.deltaTime;
            yield return null;
        }

        winSequenceRunning = false;

        SetTransitionCanvasActive(true);

        StartExitTransition();
        while (ExitRunning)
        {
            yield return null;
        }

        StartEnterTransition();
        while (EnterRunning)
        {
            yield return null;
        }

        SetTransitionCanvasActive(false);
    }
}

/// <summary>
/// Message broker for NetworkLevelManager.
/// </summary>
public class LevelManagerMessages : SimulationBehaviour
{
    /// <summary>
    /// Called by the host to notify clients who the winner is, and trigger their winner sequence.
    /// </summary>
    [Rpc]
    public static void RPC_DeclareWinner(NetworkRunner runner, PlayerRef player)
    {
        if (runner.IsServer) return;
        NetworkLevelManager.Instance.DeclareWinner(player);
    }

    /// <summary>
    /// Called by host to tell clients to transition to the next level.
    /// </summary>
    [Rpc]
    public static void RPC_GoToLevel(NetworkRunner runner, int buildIndex)
    {
        if (runner.IsServer) return;
        if (NetworkLevelManager.Instance.LevelChangeRunning) return;
        NetworkLevelManager.Instance.GoToLevel(buildIndex);
    }

    /// <summary>
    /// Called by clients to tell the host that they've finished loading into the next level.
    /// </summary>
    [Rpc]
    public static void RPC_ClientHasLoaded(NetworkRunner runner)
    {
        if (runner.IsClient) return;
        NetworkLevelManager.Instance.ClientLoaded();
    }

    /// <summary>
    /// Called by the host to tell clients to enter into the next level to resume play.
    /// </summary>
    [Rpc]
    public static void RPC_EnterLevel(NetworkRunner runner)
    {
        if (runner.IsServer) return;
        NetworkLevelManager.Instance.AllClientsLoaded = true;
    }
}