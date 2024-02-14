using System.Collections;
using System.Collections.Generic;
using Fusion;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Container for the
/// </summary>
public struct TransitionAssets
{

}

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

    private NetworkPlayerManager playerManager;
    // private NetworkBallManager ballManager;

    /// <summary>
    /// Get the local network runner instance.
    /// </summary>
    public NetworkRunner Runner { get { return _runner; } }
    private NetworkRunner _runner = null;

    [Header("Scene Indices")]
    [Tooltip("The build index of the lobby scene")]
    [SerializeField] private int lobbySceneIndex;
    [Tooltip("The next level will be picked based on a range of scene build indices. Each level should be placed in a row.")]
    [SerializeField] private int firstLevelIndex;
    [Tooltip("The next level will be picked based on a range of scene build indices. Each level should be placed in a row.")]
    [SerializeField] private int lastLevelIndex;

    /// <summary>
    /// Initializes the level manager, should only be called once when the NetworkRunnerPrefab is created.
    /// </summary>
    /// <param name="runner">The local NetworkRunner.</param>
    public void Init(NetworkRunner runner, NetworkPlayerManager players) // TODO: add in ball manager
    {
        if (_instance != null && _instance != this)
        {
            Debug.LogError("NetworkLevelManager singleton instantiated twice");
            Destroy(this);
        }
        _instance = this;
        _runner = runner;
        playerManager = players;
        // ballManager = balls;

        numLevels = lastLevelIndex - firstLevelIndex;
        remainingLevels = new List<int>(numLevels);
    }

    /// <summary>
    /// Returns true if the current scene loaded in the lobby waiting room.
    /// </summary>
    public bool IsAtLobby { get { 
        return SceneManager.GetActiveScene().buildIndex == lobbySceneIndex;
    } }

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
        if (remainingLevels.Count == 0 || Random.Range(0f, 1f) < refreshChance)
        {
            Refresh();
        }

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
    public void SetCanvasActive(bool state)
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

    [Space]
    [Tooltip("Frequency in seconds for checking if the next level has loaded.")]
    [Range(0.05f, 2f)]
    [SerializeField] float loadCompletionCheck = 0.05f;

    // exit transition ============

    private Coroutine exitTransition;
    private bool exitRunning = false;
    public bool ExitRunning { get { return exitRunning; } }

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
        SetCanvasActive(true);
        float timer = exitTransitionDuration;
        transitionElement.anchoredPosition = exitTransitionStartPos;

        float speed = exitTransitionStartPos.magnitude / timer;
        Vector2 dir = exitTransitionStartPos.normalized;

        while (timer > 0)
        {
            timer -= Time.deltaTime;

            // TODO: create exit transition animation
            transitionElement.anchoredPosition -= speed * Time.deltaTime * dir;

            yield return null;
        }
        transitionElement.anchoredPosition = Vector2.zero;

        if (!LevelChangeRunning && !WinSequenceRunning) SetCanvasActive(false);
        exitRunning = false;
    }

    // ============================

    // enter transition ===========

    private Coroutine enterTransition;
    private bool enterRunning = false;
    public bool EnterRunning { get { return enterRunning; } }

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
        SetCanvasActive(true);
        float timer = enterTransitionDuration;
        transitionElement.anchoredPosition = Vector2.zero;

        float speed = enterTransitionEndPos.magnitude / timer;
        Vector2 dir = enterTransitionEndPos.normalized;


        while (timer > 0)
        {
            timer -= Time.deltaTime;

            // TODO: create enter transition animation
            transitionElement.anchoredPosition += speed * Time.deltaTime * dir;

            yield return null;
        }
        transitionElement.anchoredPosition = enterTransitionEndPos;

        if (!LevelChangeRunning && !WinSequenceRunning) SetCanvasActive(false);
        enterRunning = false;
    }

    // ============================

    // full transition ============

    private Coroutine fullTransition;
    private bool levelChangeRunning = false;
    public bool LevelChangeRunning { get { return levelChangeRunning; } }

    private void StartLevelTransition(int buildIndex)
    {
        if (fullTransition != null)
        {
            StopCoroutine(fullTransition);
        }
        levelChangeRunning = true;
        fullTransition = StartCoroutine(LevelTransition(buildIndex));
    }

    // exit transition, load next level, enter transition
    IEnumerator LevelTransition(int buildIndex)
    {
        SetCanvasActive(true);

        StartExitTransition();
        while (ExitRunning)
        {
            yield return null;
        }

        if (Runner.IsServer) 
        {
            Runner.LoadScene(SceneRef.FromIndex(buildIndex));
        }
        while (SceneManager.GetActiveScene().buildIndex != buildIndex)
        {
            yield return new WaitForSeconds(loadCompletionCheck);
        }
        ResetLevel();

        StartEnterTransition();
        while (EnterRunning)
        {
            yield return null;
        }

        SetCanvasActive(false);
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

        StartLevelTransition(buildIndex);
    }

    // ============================

    // * =========================================================

    // * Level Prep ==============================================

    /// <summary>
    /// Resets the current level to be prepared for play.
    /// Places players and balls into scene.
    /// </summary>
    public void ResetLevel()
    {
        // TODO: implement placements using Spawner.GetSpawnPosition()
    }

    // * =========================================================

    // * Win State ===============================================

    [Space]
    [Header("Win Screen")]
    [Tooltip("How long the winner will be displayed before going to the next level.")]
    [SerializeField] float winScreenDuration;

    private PlayerRef winner;

    private Coroutine winnerSequence;
    private bool winSequenceRunning = false;
    public bool WinSequenceRunning { get { return winSequenceRunning; } }

    /// <summary>
    /// Set a winner player, and transition to the winning screen.
    /// Should be called by player manager when only 1 player is left alive.
    /// </summary>
    /// <param name="player">The winning player</param>
    public void DeclareWinner(PlayerRef player)
    {
        winner = player;

        if (winnerSequence != null)
        {
            StopCoroutine(winnerSequence);
        }
        winSequenceRunning = true;
        StartCoroutine(WinnerSequence());
    }

    private IEnumerator WinnerSequence()
    {
        SetCanvasActive(true);
        winSequenceRunning = true;

        StartExitTransition();
        while (ExitRunning)
        {
            yield return null;
        }

        // TODO: Set up winner screen

        StartEnterTransition();
        while (EnterRunning)
        {
            yield return null;
        }

        SetCanvasActive(false);

        float timer = winScreenDuration;
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

    // * =========================================================
}
