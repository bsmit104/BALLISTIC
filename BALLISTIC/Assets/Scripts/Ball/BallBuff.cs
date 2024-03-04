using System.Collections;
using System.Collections.Generic;
using Fusion;
using UnityEngine;

public abstract class BallBuff : MonoBehaviour
{
    // * Getters ================================

    [Header("Required Parameters")]
    [Tooltip("A unique material to identify this ball buff.")]
    [SerializeField] private Material material;
    [Tooltip("The name of the ball buff.")]
    [SerializeField] private string title;
    [Tooltip("A description of the ball buff that can be displayed to players.")]
    [SerializeField] private string description;

    /// <summary>
    /// Returns the name of the ball buff.
    /// </summary>
    public string Title { get { return title; } set { title = value; } }

    /// <summary>
    /// Returns a helpful blurb that explains what this buff does.
    /// </summary>
    public string Description { get { return description; } set { description = value; } }

    /// <summary>
    /// The NetworkDodgeball this buff is attached to.
    /// </summary>
    public NetworkDodgeball Ball { 
        get {
            if (_ball == null)
            {
                _ball = transform.parent.GetComponent<NetworkDodgeball>();
            }
            return _ball;
        }
    }
    private NetworkDodgeball _ball = null;

    /// <summary>
    /// This ball's rigidbody.
    /// </summary>
    public Rigidbody Rig { get { return Ball.Rig; } }

    /// <summary>
    /// This ball's collider.
    /// </summary>
    public Collider Col { get { return Ball.Col; } }

    /// <summary>
    /// This ball's trail renderer.
    /// </summary>
    public TrailRenderer Trail { get { return Ball.Trail; } }

    /// <summary>
    /// The ball's current velocity.
    /// </summary>
    public Vector3 Velocity { get { return Ball.Rig.velocity; } }

    /// <summary>
    /// The player who is holding, or just threw the ball.
    /// </summary>
    public PlayerRef Owner { get { return Ball.Owner; } }

    /// <summary>
    /// Returns true if the ball will currently kill a player on collision.
    /// </summary>
    public bool IsDeadly { get { return Ball.IsDeadly; } }

    // * ========================================

    // * Events =================================

    /// <summary>
    /// Called when buff is first attached to the ball.
    /// </summary>
    /// <param name="ball">The ball this buff is attached to.</param>
    public void OnSpawn(NetworkDodgeball ball)
    {
        _ball = ball;
        if (material) ball.SetMaterial(material);
        OnSpawnBuff(ball);
    }

    /// <summary>
    /// Called when buff is first attached to the ball.
    /// </summary>
    /// <param name="ball">The ball this buff is attached to.</param>
    protected virtual void OnSpawnBuff(NetworkDodgeball ball) {}

    /// <summary>
    /// Called when a player throws the ball.
    /// </summary>
    /// <param name="thrower">The player who threw the ball.</param>
    /// /// <param name="throwDirection">The direction the ball was thrown in (normalized).</param>
    public virtual void OnThrow(NetworkPlayer thrower, Vector3 throwDirection) {}

    /// <summary>
    /// Called when the ball bounces off of a surface. Synced to FixedUpdate.
    /// </summary>
    /// <param name="normal">The normal of the surface the ball bounced off of.</param>
    /// <param name="newDirection">The new direction the ball is traveling in (normalized).</param>
    /// <param name="bounceCount">The number of bounces since the ball was thrown.</param>
    /// <param name="hitSurface">True if the what the ball hit was a level surface.</param>
    public virtual void OnBounce(Vector3 normal, Vector3 newDirection, int bounceCount, bool hitSurface) {}

    /// <summary>
    /// Called every FixedUpdate while the ball is deadly.
    /// </summary>
    /// <param name="curDirection">The current direction the ball is heading in (normalized).</param>
    public virtual void WhileDeadly(Vector3 curDirection) {}

    /// <summary>
    /// Called when the ball hits and kills a player.
    /// </summary>
    /// <param name="player">The player who was hit.</param>
    public virtual void OnPlayerHit(NetworkPlayer player) {}

    /// <summary>
    /// Called when the ball is picked up by a player.
    /// </summary>
    /// <param name="player">The player who picked up the ball.</param>
    public virtual void OnPickup(NetworkPlayer player) {}

    /// <summary>
    /// Called when the ball is dropped by a player. This is a separate event from OnThrow.
    /// For example, this is called when a player dies while holding a ball.
    /// </summary>
    /// <param name="player">The player who dropped the ball.</param>
    public virtual void OnDropped(NetworkPlayer player) {}

    /// <summary>
    /// Called when the ball becomes no longer deadly, and can be picked up by players again.
    /// </summary>
    public virtual void OnNotDeadly() {}

    /// <summary>
    /// Called every FixedUpdate while the ball is not deadly.
    /// </summary>
    public virtual void WhileNotDeadly() {}

    /// <summary>
    /// Called every FixedUpdate while the ball is being held by a player.
    /// </summary>
    /// <param name="player">The player holding the ball.</param>
    public virtual void WhileHeld(NetworkPlayer player) {}

    // * ========================================
}
