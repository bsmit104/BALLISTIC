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

    [Tooltip("Prefab object that will be spawned on request")]
    [SerializeField] private NetworkObject ballPrefab;
    [Tooltip("Size pool queue will be initialized at")]
    [SerializeField] private int defaultPoolSize;
    [Tooltip("Max size of pool queue before dodgeballs will be recycled")]
    [SerializeField] private int maxPoolSize;

    private ObjectPool<NetworkDodgeball> pool;

    private NetworkRunner runner;
    public NetworkRunner Runner { get { return runner; } }

    /// <summary>
    /// Initializes the ball manager, this should only happen once per lobby creation.
    /// </summary>
    /// <param name="networkRunner">The local NetworkRunner instance</param>
    public void Init(NetworkRunner networkRunner)
    {
        if (_instance != null && _instance != this)
        {
            Debug.LogError("NetworkBallManager singleton instantiated twice");
            Destroy(this);
        }
        _instance = this;

        runner = networkRunner;
        pool = new ObjectPool<NetworkDodgeball>(
            () => {
                NetworkDodgeball ball = runner.IsServer ? runner.Spawn(ballPrefab).GetComponent<NetworkDodgeball>() : null;
                return ball;
            },
            ball => {
                ball.networkEnabled = true;
            },
            ball => {
                ball.networkEnabled = false;
            },
            ball => {
                if (runner.IsServer) runner.Despawn(ball.GetComponent<NetworkObject>());
            },
            true, defaultPoolSize, maxPoolSize
        );
    }

    /// <summary>
    /// Gets a ball from the pool. If one needs to be instantiated, use the NetworkRunner to synchronize
    /// the instantiation across clients.
    /// </summary>
    /// <returns>Dodgeball, transform values are not reset.</returns>
    public NetworkDodgeball GetBall()
    {
        return pool.Get().Reset();
    }

    /// <summary>
    /// Releases the Dodgeball back to the pool. synchronizes game object deactivation across clients.
    /// </summary>
    /// <param name="ball">The ball to be released.</param>
    public void ReleaseBall(NetworkDodgeball ball)
    {
        pool.Release(ball);
    }
}
