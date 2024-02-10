using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sebastian.Geometry;

/// <summary>
/// Interface for the rest of the Geometry library created by Sebastian Lague.
/// Provide MeshMaker.MakeMesh() an array of LOCAL SPACE Vector3s, and it will return a Mesh which can 
/// be provided to a mesh renderer.
/// </summary>
public static class MeshMaker {
    // returns a new mesh based on the given paths
    public static Mesh MakeMesh(Vector3[] points) {
        List<Shape> shapes = new List<Shape>();
        Shape shape = new Shape();
        for (int i = 0; i < points.Length; i++) {
            shape.points.Add(points[i]);
        }
        shapes.Add(shape);

        CompositeShape compShape = new CompositeShape(shapes);
        return compShape.GetMesh();
    }
}
