using System.Collections;
using System.Collections.Generic;
using Fusion;
using TMPro;
using UnityEngine;

[System.Serializable]
public struct BallBuffChance
{
    [Tooltip("The ball buff prefab that will be added as a child to the actual ball.")]
    public BallBuff ballBuffPrefab;
    [Tooltip(@"The chances of a ball having this buff. Value will be normalized. 
        The higher it is compared to other buffs, the more likely it will appear.")]
    public int chance;
}


/// <summary>
/// Singleton object manager for Dodgeballs, makes sure they are networked properly.
/// Manager is set to DontDestroyOnLoad.
/// All balls spawned will be children of this object.
/// </summary>
public class NetworkBallManager : MonoBehaviour
{
    /// <summary>
    /// Get the global ball manager instance.
    /// </summary>
    public static NetworkBallManager Instance { get { return _instance; } }
    private static NetworkBallManager _instance = null;

    /// <summary>
    /// The local network runner.
    /// </summary>
    public NetworkRunner Runner { get { return runner; } }
    private NetworkRunner runner;

    // * Init =============================================

    /// <summary>
    /// Initializes the ball manager, this should only happen once per lobby creation.
    /// </summary>
    /// <param name="networkRunner">The local NetworkRunner instance</param>
    public void Init(NetworkRunner networkRunner)
    {
        if (_instance != null && _instance != this)
        {
            Debug.LogError("NetworkBallManager singleton instantiated twice");
            Destroy(gameObject);
        }
        _instance = this;

        runner = networkRunner;

        // Init pool
        if (Runner.IsServer)
        {
            // Set up ball buff chance list
            for (int i = 0; i < ballBuffs.Length; i++) 
            {
                for (int j = 0; j < ballBuffs[i].chance; j++) 
                {
                    buffChances.Add(i);
                }
            }

            // Create initial balls
            for (int i = 0; i < defaultPoolSize; i++) 
            {
                SpawnNew();
            }

            ReleaseAllBalls();
        }
    }

    // * ==================================================

    [Tooltip("Prefab object that will be spawned on request")]
    [SerializeField] private NetworkObject ballPrefab;
    [Tooltip("Size pool queue will be initialized at")]
    [SerializeField] private int defaultPoolSize;
    [Tooltip("Max size of pool queue before dodgeballs will be recycled")]
    [SerializeField] private int maxPoolSize;

    [Space]
    [SerializeField] private BallBuffChance[] ballBuffs;
    // Contains indices to ballBuffs array, uses more of the same indices to produce weighted probability
    private List<int> buffChances = new List<int>();

    private List<NetworkDodgeball> inactive = new List<NetworkDodgeball>();
    private List<NetworkDodgeball> active = new List<NetworkDodgeball>();
    private int totalBalls;

    private NetworkDodgeball SpawnNew()
    {
        if (!Runner.IsServer) return null;

        NetworkDodgeball ball = null;
        // Recycle if at max ball count
        if (totalBalls >= maxPoolSize && active.Count > 0)
        {
            ball = active[0];
        }
        else
        {
            // Otherwise spawn a new ball
            ball = runner.Spawn(ballPrefab).GetComponent<NetworkDodgeball>();
        }

        // Add the ball the active list
        if (ball)
        {
            active.Add(ball);
            totalBalls++;
        }
        return ball;
    }

    private NetworkDodgeball GetFromPool()
    {
        // If there are no inactive balls
        if (inactive.Count == 0) return null;

        // Otherwise, pull from the front of the queue
        var ball = inactive[0];
        inactive.RemoveAt(0);

        // Set it active for all players and add it to the active list
        ball.NetworkSetActive(true);
        active.Add(ball);
        return ball;
    }

    private void MakeInactive(NetworkDodgeball ball)
    {
        // Do nothing if the ball is already inactive
        if (!active.Contains(ball)) return;
        active.Remove(ball);
        // set it inactive for all players
        ball.NetworkSetActive(false);
        inactive.Add(ball);
    }

    /// <summary>
    /// Gets a ball from the pool. If one needs to be instantiated, use the NetworkRunner to synchronize
    /// the instantiation across clients.
    /// </summary>
    /// <returns>Dodgeball, transform values are not reset.</returns>
    public NetworkDodgeball GetBall()
    {
        var ball = GetFromPool();
        // Spawn a new ball if there are no inactive balls
        if (ball == null)
        {
            ball = SpawnNew();
        }
        // Reset ball and assign it a random ball buff
        return ball.Reset(Random.Range(0, buffChances.Count));
    }

    /// <summary>
    /// Releases the Dodgeball back to the pool. synchronizes game object deactivation across clients.
    /// </summary>
    /// <param name="ball">The ball to be released.</param>
    public void ReleaseBall(NetworkDodgeball ball)
    {
        MakeInactive(ball);
    }

    /// <summary>
    /// Releases all Dodgeballs back to the pool.
    /// </summary
    public void ReleaseAllBalls() {
        int count = active.Count;
        for (int i = 0; i < count; i++)
        {
            ReleaseBall(active[0]);
        }
    }

    /// <summary>
    /// Returns a new instance of a requested ball buff.
    /// </summary>
    /// <param name="index">The index in the ball buffs array.</param>
    /// <returns>A newly instantiated ball buff.</returns>
    public BallBuff GetBuff(int index)
    {
        return Instantiate(ballBuffs[index].ballBuffPrefab);
    }

    [Space]
    [Header("Ball Buff Descriptions")]
    [Tooltip("Canvas used to display ball buff descriptions.")]
    [SerializeField] GameObject ballBuffCanvas;
    [Tooltip("Displays ball buff title.")]
    [SerializeField] TextMeshProUGUI buffTitleText;
    [Tooltip("Displays ball buff description.")]
    [SerializeField] TextMeshProUGUI buffDescText;
    [Tooltip("How long the ball buff description will be displayed for after picking up the ball.")]
    [SerializeField] float ballBuffTextDuration;

    /// <summary>
    /// Displays the given text in the ball buff UI in the bottom-right.
    /// </summary>
    /// <param name="title">The title of the ball buff.</param>
    /// <param name="desc">The description of the ball buff.</param>
    public void DisplayBuffText(string title, string desc)
    {
        if (buffTextTween != null)
        {
            StopCoroutine(buffTextTween);
        }
        buffTextTween = StartCoroutine(BuffTextTween(title, desc));
    }

    private Coroutine buffTextTween;

    IEnumerator BuffTextTween(string title, string desc)
    {
        buffTitleText.text = title;
        buffDescText.text = desc;
        ballBuffCanvas.SetActive(true);

        float timer = ballBuffTextDuration;

        // Wait for timer to end
        while (timer > 0)
        {
            timer -= Time.deltaTime;
            yield return null;
        }

        // Hide ball buff description
        ballBuffCanvas.SetActive(false);
    }

    /// <summary>
    /// Notify a newly joined player which balls are active, and their state. 
    /// Should only be called by the client.
    /// </summary>
    /// <param name="receiver">The player who will be receiving the state update.</param>
    public void SendBallStates(PlayerRef receiver)
    {
        if (!Runner.IsServer) return;
        foreach (var ball in active)
        {
            BallManagerMessages.RPC_SendBallState(Runner, receiver, ball.NetworkID, ball.gameObject.activeInHierarchy, ball.transform.position, ball.BuffID);
        }
    }

    /// <summary>
    /// Updates a ball's state. The receiving method for SendBallStates(). Should only be called
    /// by the client.
    /// </summary>
    /// <param name="id">The NetworkId of the ball.</param>
    /// <param name="enabled">Whether or not the ball is active.</param>
    /// <param name="position">The current position of the ball.</param>
    /// <param name="buffIndex">The ball buff attached to this ball.</param>
    public void FindBall(NetworkId id, bool enabled, Vector3 position, int buffIndex)
    {
        // Search for the ball because it might not have been spawned yet.
        StartCoroutine(SearchForBall(id, enabled, position, buffIndex));
    }

    IEnumerator SearchForBall(NetworkId id, bool enabled, Vector3 position, int buffIndex)
    {
        // wait at most 20 secs for ball to be found
        float timer = 20f;
        NetworkObject ballObj = null;

        while (timer > 0)
        {
            // Stop waiting if the ball is found
            if (Runner.TryFindObject(id, out ballObj))
            {
                break;
            }
            yield return null;
        }

        // Update state
        if (ballObj)
        {
            var ball = ballObj.GetComponent<NetworkDodgeball>();
            ball.gameObject.SetActive(enabled);
            ball.transform.position = position;
            ball.SetBuff(buffIndex);
        }
    }
}

/// <summary>
/// Message broker for NetworkBallManager.
/// </summary>
public class BallManagerMessages : SimulationBehaviour
{
    /// <summary>
    /// Sends a ball's state to a specific client. Should only be called by the host.
    /// </summary>
    /// <param name="receiver">The target player who is receiving the state.</param>
    /// <param name="id">The NetworkId of the ball.</param>
    /// <param name="enabled">Whether the ball is active or not.</param>
    /// <param name="position">The current position of the ball.</param>
    /// <param name="buffIndex">The ball buff attached to the ball.</param>
    [Rpc]
    public static void RPC_SendBallState(NetworkRunner runner, PlayerRef receiver, NetworkId id, bool enabled, Vector3 position, int buffIndex)
    {
        if (NetworkBallManager.Instance.Runner.LocalPlayer != receiver) return;
        NetworkBallManager.Instance.FindBall(id, enabled, position, buffIndex);
    }    
}
