# BallBuff.cs
**Found in [/Ball](../BALLISTIC/Assets/Scripts/Ball/BallBuff.cs)**

[Return to glossary](glossary.md)

> ## `public abstract class BallBuff : MonoBehaviour`
> **The super class used to create all ball buffs. All ball buffs need to inherit from this class.**
> 
> ### **Serialized Properties:**
>> **`private Material material`**\
>> A unique material to identify this ball buff.
> 
>> **`private string title`**\
>> The name of the ball buff.
> 
>> **`private string description`**\
>> A description of the ball buff that can be displayed to players.
> 
> ### **Methods, Getters, and Setters:**
>> **`public string Title`**\
>> Returns the name of the ball buff.
>> 
> 
>> **`public string Description`**\
>> Returns a helpful blurb that explains what this buff does.
>> 
> 
>> **`public NetworkDodgeball Ball`**\
>> The NetworkDodgeball this buff is attached to.
>> 
> 
>> **`public Rigidbody Rig`**\
>> This ball's rigidbody.
>> 
> 
>> **`public Collider Col`**\
>> This ball's collider.
>> 
> 
>> **`public TrailRenderer Trail`**\
>> This ball's trail renderer.
>> 
> 
>> **`public Vector3 Velocity`**\
>> The ball's current velocity.
>> 
> 
>> **`public PlayerRef Owner`**\
>> The player who is holding, or just threw the ball.
>> 
> 
>> **`public bool IsDeadly`**\
>> Returns true if the ball will currently kill a player on collision.
>> 
> 
>> **`public Material PickupMat`**\
>> Returns the pickup material for this specific ball buff.Assign to the ball with Ball.SetPickupMaterial().
>> 
> 
>> **`public void OnSpawn(NetworkDodgeball ball)`**\
>> Called when buff is first attached to the ball.
>> 
>> **Arguments:**\
>> *ball:* The ball this buff is attached to.
> 
>> **`protected virtual void OnSpawnBuff(NetworkDodgeball ball)`**\
>> Called when buff is first attached to the ball.
>> 
>> **Arguments:**\
>> *ball:* The ball this buff is attached to.
> 
>> **`public virtual void OnThrow(NetworkPlayer thrower, Vector3 throwDirection)`**\
>> Called when a player throws the ball.
>> 
>> **Arguments:**\
>> *thrower:* The player who threw the ball.\
>> *throwDirection:* The direction the ball was thrown in (normalized).
> 
>> **`public virtual void OnBounce(Vector3 normal, Vector3 newDirection, int bounceCount, bool hitSurface)`**\
>> Called when the ball bounces off of a surface. Synced to FixedUpdate.
>> 
>> **Arguments:**\
>> *normal:* The normal of the surface the ball bounced off of.\
>> *newDirection:* The new direction the ball is traveling in (normalized).\
>> *bounceCount:* The number of bounces since the ball was thrown.\
>> *hitSurface:* True if the what the ball hit was a level surface.
> 
>> **`public virtual void WhileDeadly(Vector3 curDirection)`**\
>> Called every FixedUpdate while the ball is deadly.
>> 
>> **Arguments:**\
>> *curDirection:* The current direction the ball is heading in (normalized).
> 
>> **`public virtual void OnPlayerHit(NetworkPlayer player)`**\
>> Called when the ball hits and kills a player.
>> 
>> **Arguments:**\
>> *player:* The player who was hit.
> 
>> **`public virtual void OnPickup(NetworkPlayer player)`**\
>> Called when the ball is picked up by a player.
>> 
>> **Arguments:**\
>> *player:* The player who picked up the ball.
> 
>> **`public virtual void OnDropped(NetworkPlayer player)`**\
>> Called when the ball is dropped by a player. This is a separate event from OnThrow.For example, this is called when a player dies while holding a ball.
>> 
>> **Arguments:**\
>> *player:* The player who dropped the ball.
> 
>> **`public virtual void OnNotDeadly()`**\
>> Called when the ball becomes no longer deadly, and can be picked up by players again.
>> 
> 
>> **`public virtual void WhileNotDeadly()`**\
>> Called every FixedUpdate while the ball is not deadly.
>> 
> 
>> **`public virtual void WhileHeld(NetworkPlayer player)`**\
>> Called every FixedUpdate while the ball is being held by a player.
>> 
>> **Arguments:**\
>> *player:* The player holding the ball.
> 
