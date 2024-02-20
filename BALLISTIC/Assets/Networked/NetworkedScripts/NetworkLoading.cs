using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Fusion;

/// <summary>
/// Used to notify the host when all clients have loaded into a level.
/// </summary>
public class NetworkLoading : NetworkBehaviour
{
    [Networked] int playersLoaded { get; set; }

    public override void Spawned()
    {
        if (Runner.IsServer)
        {
            playersLoaded++;
        }
    }
}
