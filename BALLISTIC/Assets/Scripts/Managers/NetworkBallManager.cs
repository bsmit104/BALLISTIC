using System.Collections;
using System.Collections.Generic;
using Fusion;
using UnityEngine;
using UnityEngine.Pool;

/// <summary>
/// Singleton object manager for Dodgeballs, makes sure they are networked properly.
/// Manager is set to DontDestroyOnLoad.
/// All balls spawned will be children of this object.
/// </summary>
public class NetworkBallManager : MonoBehaviour
{
    public static NetworkBallManager Instance { get { return _instance; } }
    private static NetworkBallManager _instance = null;

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

        // seed pool
        if (Runner.IsServer)
        {
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


    private List<NetworkDodgeball> pool = new List<NetworkDodgeball>();
    private List<NetworkDodgeball> active = new List<NetworkDodgeball>();
    private int totalBalls;

    private NetworkDodgeball SpawnNew()
    {
        NetworkDodgeball ball = null;
        if (totalBalls >= maxPoolSize && active.Count > 0)
        {
            ball = active[0];
        }
        else if (runner.IsServer)
        {
            ball = runner.Spawn(ballPrefab).GetComponent<NetworkDodgeball>();
        }
        if (ball)
        {
            active.Add(ball);
            totalBalls++;
        }
        return ball;
    }

    private NetworkDodgeball GetFromPool()
    {
        if (pool.Count == 0) return null;
        var ball = pool[0];
        pool.RemoveAt(0);
        ball.NetworkSetActive(true);
        active.Add(ball);
        return ball;
    }

    private void MakeInactive(NetworkDodgeball ball)
    {
        if (!active.Contains(ball)) return;
        active.Remove(ball);
        ball.NetworkSetActive(false);
        pool.Add(ball);
    }

    /// <summary>
    /// Gets a ball from the pool. If one needs to be instantiated, use the NetworkRunner to synchronize
    /// the instantiation across clients.
    /// </summary>
    /// <returns>Dodgeball, transform values are not reset.</returns>
    public NetworkDodgeball GetBall()
    {
        var ball = GetFromPool();
        if (ball == null)
        {
            ball = SpawnNew();
        }
        return ball.Reset();
    }

    /// <summary>
    /// Releases the Dodgeball back to the pool. synchronizes game object deactivation across clients.
    /// </summary>
    /// <param name="ball">The ball to be released.</param>
    public void ReleaseBall(NetworkDodgeball ball)
    {
        ball.Reset();
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
}
