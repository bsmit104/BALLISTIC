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

    [Tooltip("When enabled, spawn areas will be displayed in the game.")]
    [SerializeField] private bool displayAreaOnPlay = false;

    [SerializeField] private List<SpawnArea> spawnAreas = new List<SpawnArea>();

    public void SetRenderersActive(bool state)
    {
        for (int i = 0; i < spawnAreas.Count; i++)
        {
            spawnAreas[i]?.SetRenderActive(state);
        }
    }

    public void AddSpawnArea()
    {
        spawnAreas.Add(Instantiate(spawnAreaPrefab, transform));
    }

    void Awake()
    {
        if (_instance != null && _instance != this)
        {
            Debug.LogError("Spawner singleton instantiated twice");
            Destroy(this);
        }
        _instance = this;

        if (GetComponent<MeshRenderer>()) GetComponent<MeshRenderer>().enabled = false;

        foreach (SpawnArea area in spawnAreas)
        {
            area.gameObject.SetActive(displayAreaOnPlay);
        }
        SetRenderersActive(displayAreaOnPlay);
    }

    /// <summary>
    /// Gets a random, valid spawn position for the current level.
    /// </summary>
    /// <returns>The spawn position in global space.</returns>
    public static Vector3 GetSpawnPoint()
    {
        if (Instance == null && Instance.spawnAreas.Count > 0) return Vector3.zero;

        int selection = Random.Range(0, Instance.spawnAreas.Count);
        int iters = 0;
        while (!Instance.spawnAreas[selection].IsValid && iters < 20)
        {
            selection = Random.Range(0, Instance.spawnAreas.Count);
            iters++;
        }
        return Instance.spawnAreas[selection].GetRandomPosition();
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
        if (Instance == null) return Vector3.zero;
        return GetSpawnPoint() + new Vector3(0, bounds.extents.y, 0);
    }
}
