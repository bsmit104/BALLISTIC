# NetworkDodgeball.cs
**Found in [/Ball](../BALLISTIC/Assets/Scripts/Ball/NetworkDodgeball.cs)**

[Return to glossary](Glossary.md)


> ## `public class NetworkDodgeball : NetworkBehaviour`
> **Networked object to manage dodgeball. Spawn and release using the NetworkBallManager.**
> 
> ### **Serialized Properties:**
>> **`private float throwSpeed`**\
>> The constant speed the ball will travel at will it is deadly.
> 
>> **`private float deadlyTime`**\
>> The duration the ball will be deadly for after being thrown.
> 
>> **`private int bounceLimit`**\
>> The ball will stop being deadly after bouncing this many times.
> 
>> **`public event DodgeballEvent OnSpawned`**\
>> Invoked when the ball is activated in the level, and has been given a new ball buff.
> 
>> **`public event ThrowEvent OnThrow`**\
>> Invoked when the ball is thrown.
> 
>> **`public event BounceEvent OnBounce`**\
>> Invoked when the ball bounces on a surface, while it is deadly.
> 
>> **`public event PlayerEvent OnPlayerHit`**\
>> Invoked when the ball hits a player while it is deadly.
> 
>> **`public event PlayerEvent OnPickup`**\
>> Invoked when the ball is picked up by a player.
> 
>> **`public event PlayerEvent OnDropped`**\
>> Invoked when a player drops the ball.
> 
>> **`public event Notify OnNotDeadly`**\
>> Invoked when the ball becomes not deadly.
> 
> ### **Methods, Getters, and Setters:**
>> **`public DodgeballCollider BallCol`**\
>> The client-sided script responsible for detecting collisions.
>> 
> 
>> **`public Rigidbody Rig`**\
>> The ball's rigidbody.
>> 
> 
>> **`public Collider Col`**\
>> The ball's collider.
>> 
> 
>> **`public TrailRenderer Trail`**\
>> The ball's Trail Renderer.
>> 
> 
>> **`public MeshRenderer Rend`**\
>> The ball's mesh renderer.
>> 
> 
>> **`public NetworkId NetworkID`**\
>> Returns the NetworkId associated with the NetworkObject attached to the ball.
>> 
> 
>> **`public NetworkPosition NetPos`**\
>> The network position component responsible for synchronizing this ball's transform state.
>> 
> 
>> **`public bool IsDeadly`**\
>> Returns true if the ball kill a player on collision.
>> 
> 
>> **`public PlayerRef Owner`**\
>> The player who threw the ball, or is currently holding it.Use IsHeld to see if the ball is currently held by a player.
>> 
> 
>> **`public bool IsHeld`**\
>> If the ball is currently held by a player. Use Owner to see who iscurrently holding it.
>> 
> 
>> **`[Networked, HideInInspector] public bool isHeld`**\
>> DO NOT USE. Use IsHeld getter & setter instead.
>> 
> 
>> **`public NetworkDodgeball Reset(int newBuff)`**\
>> Reset any attributes for this NetworkDodgeball.Used by the NetworkBallManager to reset dodgeballs returned by GetBall().
>> 
> 
>> **`public float ThrowSpeed`**\
>> The speed the ball will travel at when deadly, must be greater than 0.
>> 
> 
>> **`public float DeadlyTime`**\
>> The max duration the ball will be deadly for after being thrown, must be greater than 0.
>> 
> 
>> **`public int BounceLimit`**\
>> The max number of times the ball will bounce before becoming not deadly,must be greater than or equal to 1.
>> 
> 
>> **`public Vector3 TravelDir`**\
>> The direction the ball is currently traveling. If the ball is not deadly,returns Vector3.zero.
>> 
> 
>> **`public void Throw(Vector3 dir)`**\
>> Activates the ball's throw state.
>> 
>> **Arguments:**\
>> *dir:* The initial throw direction.
> 
>> **`public void SetTrail()`**\
>> Activate or deactivate the ball's trail effect.
>> 
> 
>> **`public void SetTrailColor()`**\
>> Set the ball's trail color to match its owner.
>> 
> 
>> **`public void SetMarker()`**\
>> Update whether the player should be able to see the ball through walls.It should not be seen through walls if it is deadly, or held by a player.
>> 
> 
>> **`public int BuffIndex`**\
>> The index of this ball's buff in the NetworkBallManager.ballBuffs array.Use with GetBuff(BuffIndex) to get a new instance of this ball buff.
>> 
> 
>> **`public void NetworkSetBuff(int buffInd)`**\
>> Called by host to tell all clients to add the specified ball buff to this ball.
>> 
>> **Arguments:**\
>> *buffInd:* The ball buff's index in the NetworkBallManager.ballBuffs array.
> 
>> **`public void SetBuff(int buffInd)`**\
>> Adds the given ball buff to this ball.
>> 
>> **Arguments:**\
>> *buffInd:* The ball buff's index in the NetworkBallManager.ballBuffs array.
> 
>> **`public void SetBuff(BallBuff buff)`**\
>> Adds the given ball buff to this ball.
>> 
>> **Arguments:**\
>> *buff:* The ball buff instance to attach to this ball.
> 
>> **`public void NetworkSetActive(bool state)`**\
>> Use instead of gameObject.SetActive(). Ensures game object is in the same stateacross all clients.
>> 
>> **Arguments:**\
>> *state:* The active state the game object will be set to.
> 
>> **`public void NetworkSetOwner(PlayerRef player)`**\
>> Sets the owner of the ball across all clients.Use PlayerRef.None to signal the ball has been dropped.
>> 
>> **Arguments:**\
>> *player:* The player who owns the ball.
> 

