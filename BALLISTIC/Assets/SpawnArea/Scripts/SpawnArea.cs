using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// A 2D mesh that represents a valid spawn area.
/// Drawn using in-editor tool.
/// </summary>
[RequireComponent(typeof(MeshFilter))]
[RequireComponent(typeof(MeshRenderer))]
public class SpawnArea : MonoBehaviour
{
    [Header("Editor Attributes")]
    [Tooltip("The radius of point handles.")]
    [SerializeField] public float handleRadius = 0.15f;
    [Tooltip("Width of lines connecting points.")]
    [SerializeField] public float lineWidth = 4f;
    [Tooltip("How close the mouse needs to be to a line to click it.")]
    [SerializeField] public float lineSensitivity = 0.1f;

    // stores points in local space
    [SerializeField] private List<Vector3> points = new List<Vector3>{
        new Vector3(1, 0, 1),
        new Vector3(1, 0, -1),
        new Vector3(-1, 0, -1),
        new Vector3(-1, 0, 1)
    };

    public int PointCount { get { return points.Count; } }

    private bool pointsChanged = true;

    /// <summary>
    /// Returns a bounds encapsulating the shape defined by points.
    /// </summary>
    public Bounds GetBounds 
    {
        get 
        {
            Bounds bounds = new Bounds();
            for (int i = 0; i < points.Count; i++) 
            {
                bounds.Encapsulate(points[i]);
            }
            return bounds;
        }
    }

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
        pointsChanged = true;
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
        pointsChanged = true;
    }

    /// <summary>
    /// Removes a point from the given index.
    /// </summary>
    public void RemovePoint(int index)
    {
        points.RemoveAt(index);
        pointsChanged = true;
    }

    /// <summary>
    /// The current Y position of the plane.
    /// </summary>
    public float Height { get { return transform.position.y; } }

    private MeshFilter filter = null;
    public MeshFilter Filter { 
        get 
        {
            if (filter == null)
            {
                filter = GetComponent<MeshFilter>();
            }
            return filter;
        }
    }

    private bool isValid = true;

    public bool IsValid { get { return ValidateShape(false); } } 

    private bool ValidateShape(bool debug)
    {
        if (!pointsChanged)
        {
            return isValid;
        }

        bool result = true;
        if (points.Count < 3)
        {
            if (debug) Debug.LogError("Shapes must have at least 3 points to valid areas.");
            result = false;
        }

        if (result)
        {
            for (int i = 0; i < points.Count; i++) {
                Vector2 A1 = new Vector2(points[i].x, points[i].z);
                Vector2 A2 = new Vector2(points[(i + 1) % points.Count].x, points[(i + 1) % points.Count].z);
                for (int j = 0; j < points.Count; j++) {
                    if (i == j) continue;
                    Vector2 B1 = new Vector2(points[j].x, points[j].z);
                    Vector2 B2 = new Vector2(points[(j + 1) % points.Count].x, points[(j + 1) % points.Count].z);
                    if (LineChecker.Intersecting(A1, A2, B1, B2, out Vector2 point))
                    {
                        if (point != A1 && point != A2 && point != B1 && point != B2)
                        {
                            if (debug) Debug.LogError("Shapes cannot intersect themselves.");
                            result = false;
                            break;
                        }
                    }
                }
                if (!result) break;
            }
        }

        if (!result && debug)
        {
            Debug.LogWarning("SpawnAreas with invalid shapes will not be used for spawning.");
        }
        isValid = result;
        pointsChanged = false;
        return result;
    }

    /// <summary>
    /// Updates the spawn area's mesh to match the current shape defined by points.
    /// </summary>
    public void GenerateMesh()
    {
        if (!pointsChanged) return;
        ValidateShape(true);
        Mesh mesh = MeshMaker.MakeMesh(points.ToArray());
        Filter.mesh = mesh;
    }

    public void SetRenderActive(bool state)
    {
        GetComponent<MeshRenderer>().enabled = state;
    }

    /// <summary>
    /// Returns a position inside of the area defined by points.
    /// If the area is not valid, then it will return Vector3.zero.
    /// </summary>
    /// <returns>The random position within this area.</returns>
    public Vector3 GetRandomPosition()
    {
        if (!ValidateShape(false))
        {
            return Vector3.zero;
        }
        Bounds bounds = GetBounds;

        Vector3 pos = Vector3.zero;
        for (int i = 0; i < 50; i++) 
        {
            Vector2 temp = new Vector2(
                transform.position.x + Random.Range(bounds.min.x, bounds.max.x),
                transform.position.z + Random.Range(bounds.min.z, bounds.max.z)
            );

            Vector2 edge = temp + new Vector2(0, bounds.size.z);

            int intersectCount = 0;
            for (int j = 0; j < points.Count; j++) 
            {
                Vector2 A1 = new Vector2(GetPoint(j).x, GetPoint(j).z);
                Vector2 A2 = new Vector2(GetPoint((j + 1) % points.Count).x, GetPoint((j + 1) % points.Count).z);
                if (LineChecker.Intersecting(A1, A2, temp, edge, out Vector2 point))
                {
                    intersectCount++;
                }
            }

            if (intersectCount % 2 == 1)
            {
                pos = new Vector3(temp.x, Height, temp.y);
                break;
            }
        }

        if (Physics.Raycast(pos, Vector3.down, out RaycastHit data, Mathf.Infinity, LayerMask.GetMask("Default")))
        {
            pos.y = data.point.y;
        }

        return pos;
    }
}

public static class LineChecker
{
    const float EPS = 0.001f;
    // test if 2 lines, A and B, intersect based on their start and end points
    public static bool Intersecting(Vector2 A1, Vector2 A2, Vector2 B1, Vector2 B2, out Vector2 point) {
        point = Vector2.zero;

        Vector2 dirA = A2 - A1;
        Vector2 dirB = B2 - B1;

        // cross product
        float denom = dirA.x * dirB.y - dirA.y * dirB.x;
        
        // lines are parallel, so no intersection
        if (denom == 0f) return false;

        // position of point along both lines, represented as fractions of the line
        float fracOfA = ((B1.x - A1.x) * dirB.y - (B1.y - A1.y) * dirB.x) / denom;
        float fracOfB = ((B1.x - A1.x) * dirA.y - (B1.y - A1.y) * dirA.x) / denom;

        // intersection only exists if point is in between start and end of both lines
        if (0 <= fracOfA && fracOfA <= 1f && 0 <= fracOfB && fracOfB <= 1f) {
            if (EPS <= fracOfA && fracOfA <= (1f - EPS) && EPS <= fracOfB && fracOfB <= (1f - EPS)) {
                point = A1 + fracOfA * dirA;

            } else if (0 <= fracOfA && fracOfA < EPS) {
                point = A1;
            } else if ((1f - EPS) < fracOfA && fracOfA <= 1f) {
                point = A2;

            } else if (0 <= fracOfB && fracOfB < EPS) {
                point = B1;
            } else if ((1f - EPS) < fracOfB && fracOfB <= 1f) {
                point = B2;
            }

            return true;
        }

        return false;
    }

}
