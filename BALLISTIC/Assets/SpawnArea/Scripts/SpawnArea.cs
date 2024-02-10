using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// A 2D mesh that represents a valid spawn area.
/// Drawn using in-editor tool.
/// </summary>
public class SpawnArea : MonoBehaviour
{
    // stores points in local space
    [HideInInspector] private List<Vector3> points = new List<Vector3>{
        new Vector3(1, 0, 1),
        new Vector3(1, 0, -1),
        new Vector3(-1, 0, -1),
        new Vector3(-1, 0, 1)
    };

    public int PointCount { get { return points.Count; } }

    /// <summary>
    /// Returns the global space coords for the given point.
    /// </summary>
    /// <param name="index">The index of the point.</param>
    public Vector3 GetPoint(int index)
    {
        return points[index] + transform.position;
    }

    /// <summary>
    /// Sets the point at the given index to the given global position. The position will be translated to 
    /// a local position.
    /// </summary>
    /// <param name="index">index number for the points list.</param>
    /// <param name="position">The global space position.</param>
    public void SetPoint(int index, Vector3 position)
    {
        points[index] = position - transform.position;
    }

    /// <summary>
    /// Inserts the point at the given index to the given global position. The position will be translated to 
    /// a local position.
    /// </summary>
    /// <param name="index">index number for the points list.</param>
    /// <param name="position">The global space position.</param>
    public void InsertPoint(int index, Vector3 position)
    {
        points.Insert(index, position - transform.position);
    }

    /// <summary>
    /// Removes a point from the given index.
    /// </summary>
    public void RemovePoint(int index)
    {
        points.RemoveAt(index);
    }

    [Header("Editor Attributes")]
    [Tooltip("The radius of point handles.")]
    [SerializeField] public float handleRadius = 0.15f;
    [Tooltip("Width of lines connecting points.")]
    [SerializeField] public float lineWidth = 4f;
    [Tooltip("How close the mouse needs to be to a line to click it.")]
    [SerializeField] public float lineSensitivity = 0.1f;

    public float Height { get { return transform.position.y; } }
}
