using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Singleton which should exist on every level. Used to get valid spawn positions.
/// Provides an editor interface for level designers to define valid spawn areas.
/// </summary>
public class Spawner : MonoBehaviour
{
    public static Spawner Instance { get { return _instance; } }
    private static Spawner _instance = null;

    [SerializeField] private SpawnArea spawnAreaPrefab;

    List<SpawnArea> spawnAreas;

    public void AddSpawnArea()
    {
        spawnAreas.Add(Instantiate(spawnAreaPrefab, transform));
    }

    void Awake()
    {
        foreach (SpawnArea area in spawnAreas)
        {
            area.gameObject.SetActive(false);
        }
    }

    /// <summary>
    /// Gets a random, valid spawn position for the current level.
    /// </summary>
    /// <returns>The spawn position in global space.</returns>
    public static Vector3 GetSpawnPoint()
    {
        int selection = Random.Range(0, Instance.spawnAreas.Count);
        return Vector3.zero;
    }

    /// <summary>
    /// Gets a random, valid spawn position for the current level.
    /// Accounts for a given bounds to place objects on the floor cleanly.
    /// Assumes the object's origin is in the center of the bounds.
    /// </summary>
    /// <param name="bounds">The bounds of the object this spawn position will be used on.</param>
    /// <returns>The spawn position in global space.</returns>
    public static Vector3 GetSpawnPoint(Bounds bounds)
    {
        return GetSpawnPoint() + new Vector3(0, bounds.extents.y, 0);
    }
}
