using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Fusion;

/// <summary>
/// Replacement for NetworkTransform, but only synchronizes position and rotation.
/// </summary>
public class NetworkPosition : NetworkBehaviour
{
    private ChangeDetector detector;

    [Networked, HideInInspector] public Vector3 position { get; set; }
    [Networked, HideInInspector] public Vector3 rotation { get; set; }

    private Coroutine trackPosition;
    IEnumerator TrackPosition()
    {
        while (true)
        {
            position = transform.position;
            rotation = transform.eulerAngles;
            yield return new WaitForFixedUpdate();
        }
    }

    public override void Spawned()
    {
        detector = GetChangeDetector(ChangeDetector.Source.SimulationState);
        if (Runner.IsServer)
        {
            trackPosition = StartCoroutine(TrackPosition());
        }
    }

    public override void Render()
    {
        foreach (var attrName in detector.DetectChanges(this))
        {
            switch (attrName)
            {
                case nameof(position):
                    transform.position = position;
                    break;
                case nameof(rotation):
                    transform.eulerAngles = rotation;
                    break;
            }
        }
    }
}
