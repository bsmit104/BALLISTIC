# NetworkBallManager.cs
**Found in [/Managers](../BALLISTIC/Assets/Scripts/Managers/NetworkBallManager.cs)**

[Return to glossary](Glossary.md)


> ## `public struct BallBuffChance`
> **Missing summary...**
> 
> ### **Serialized Properties:**
>> **`public BallBuff ballBuffPrefab`**\
>> The ball buff prefab that will be added as a child to the actual ball.
> 
>> **`public int chance`**\
>>         The higher it is compared to other buffs, the more likely it will appear.
> 

> ## `public class NetworkBallManager : MonoBehaviour`
> **Singleton object manager for Dodgeballs, makes sure they are networked properly.Manager is set to DontDestroyOnLoad.All balls spawned will be children of this object.**
> 
> ### **Serialized Properties:**
>> **`private NetworkObject ballPrefab`**\
>> Prefab object that will be spawned on request
> 
>> **`private int defaultPoolSize`**\
>> Size pool queue will be initialized at
> 
>> **`private int maxPoolSize`**\
>> Max size of pool queue before dodgeballs will be recycled
> 
>> **`GameObject ballBuffCanvas`**\
>> Canvas used to display ball buff descriptions.
> 
>> **`TextMeshProUGUI buffTitleText`**\
>> Displays ball buff title.
> 
>> **`TextMeshProUGUI buffDescText`**\
>> Displays ball buff description.
> 
>> **`float ballBuffTextDuration`**\
>> How long the ball buff description will be displayed for after picking up the ball.
> 
> ### **Methods, Getters, and Setters:**
>> **`public static NetworkBallManager Instance`**\
>> Get the global ball manager instance.
>> 
> 
>> **`public NetworkRunner Runner`**\
>> The local network runner.
>> 
> 
>> **`public void Init(NetworkRunner networkRunner)`**\
>> Initializes the ball manager, this should only happen once per lobby creation.
>> 
>> **Arguments:**\
>> *networkRunner:* The local NetworkRunner instance
> 
>> **`public NetworkDodgeball GetBall()`**\
>> Gets a ball from the pool. If one needs to be instantiated, use the NetworkRunner to synchronizethe instantiation across clients.
>> 
>>
>>**Returns:** Dodgeball, transform values are not reset.
> 
>> **`public void ReleaseBall(NetworkDodgeball ball)`**\
>> Releases the Dodgeball back to the pool. synchronizes game object deactivation across clients.
>> 
>> **Arguments:**\
>> *ball:* The ball to be released.
> 
>> **`public void ReleaseAllBalls()`**\
>> Releases all Dodgeballs back to the pool.
>> 
> 
>> **`public BallBuff GetBuff(int index)`**\
>> Returns a new instance of a requested ball buff.
>> 
>> **Arguments:**\
>> *index:* The index in the ball buffs array.
>>
>>**Returns:** A newly instantiated ball buff.
> 
>> **`public void DisplayBuffText(string title, string desc)`**\
>> Displays the given text in the ball buff UI in the bottom-right.
>> 
>> **Arguments:**\
>> *title:* The title of the ball buff.\
>> *desc:* The description of the ball buff.
> 
>> **`public void SendBallStates(PlayerRef receiver)`**\
>> Notify a newly joined player which balls are active, and their state.Should only be called by the client.
>> 
>> **Arguments:**\
>> *receiver:* The player who will be receiving the state update.
> 
>> **`public void FindBall(NetworkId id, bool enabled, Vector3 position, int buffIndex)`**\
>> Updates a ball's state. The receiving method for SendBallStates(). Should only be calledby the client.
>> 
>> **Arguments:**\
>> *id:* The NetworkId of the ball.\
>> *enabled:* Whether or not the ball is active.\
>> *position:* The current position of the ball.\
>> *buffIndex:* The ball buff attached to this ball.
> 

> ## `public class BallManagerMessages : SimulationBehaviour`
> **Message broker for NetworkBallManager.**
> 
> ### **Methods, Getters, and Setters:**
>> **`[Rpc]
public static void RPC_SendBallState(NetworkRunner runner, PlayerRef receiver, NetworkId id, bool enabled, Vector3 position, int buffIndex)`**\
>> Sends a ball's state to a specific client. Should only be called by the host.
>> 
>> **Arguments:**\
>> *receiver:* The target player who is receiving the state.\
>> *id:* The NetworkId of the ball.\
>> *enabled:* Whether the ball is active or not.\
>> *position:* The current position of the ball.\
>> *buffIndex:* The ball buff attached to the ball.
> 

