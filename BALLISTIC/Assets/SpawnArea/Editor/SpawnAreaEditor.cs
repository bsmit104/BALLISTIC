using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using PlasticGui.WorkspaceWindow.BrowseRepository;

// created using Sebastian Lague's Unity Shape Editor Tool tutorial series

[CustomEditor(typeof(SpawnArea))]
public class SpawnAreaEditor : Editor
{
    SpawnArea area;

    // * Inspector GUI ============================================

    bool editing;

    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        EditorGUILayout.PropertyField(serializedObject.FindProperty("handleRadius"));
        EditorGUILayout.PropertyField(serializedObject.FindProperty("lineWidth"));
        EditorGUILayout.PropertyField(serializedObject.FindProperty("lineSensitivity"));
        GUILayout.Space(10f);

        if (GUILayout.Button("Edit Spawn Area") && !editing)
        {
            editing = true;
            Draw();
        }
        if (GUILayout.Button("Exit Spawn Area Editing") && editing)
        {
            editing = false;
            Draw();
        }

        GUILayout.Label(@"TUTORIAL:
            Right click on edges to add new points.
            Right click in empty space to create a new path.
            Click and drag points to move them around.
            Left click on points to delete them.
        ");

        serializedObject.ApplyModifiedProperties();
    }

    // * ===========================================================

    // * Scene View GUI ============================================

    SelectionInfo selection;
    bool needsRepaint;

    void OnSceneGUI()
    {
        // get editor input events
        Event guiEvent = Event.current;

        if (guiEvent.type == EventType.Repaint)
        {
            Draw();
        }

        // something something idk it was in the tutorial
        if (guiEvent.type == EventType.Layout)
        {
            HandleUtility.AddDefaultControl(GUIUtility.GetControlID(FocusType.Passive));
        }
        else if (editing)
        {
            HandleInput(guiEvent);
            if (needsRepaint)
            {
                HandleUtility.Repaint();
                needsRepaint = false;
            }
        }
    }

    void HandleInput(Event guiEvent)
    {
        // calc current mouse position on the spawn area plane
        Ray mouseRay = HandleUtility.GUIPointToWorldRay(guiEvent.mousePosition);
        float planeHeight = area.Height;
        float distToPlane = (planeHeight - mouseRay.origin.y) / mouseRay.direction.y;
        Vector3 mousePos = mouseRay.GetPoint(distToPlane);

        // add a point to the spawn area shape on mouse click
        if (guiEvent.type == EventType.MouseDown && guiEvent.button == 0 && guiEvent.modifiers == EventModifiers.None)
        {
            OnLeftMouseDown(mousePos);
        }

        if (guiEvent.type == EventType.MouseUp && guiEvent.button == 0 && guiEvent.modifiers == EventModifiers.None)
        {
            OnLeftMouseUp(mousePos);
        }

        if (guiEvent.type == EventType.MouseDrag && guiEvent.button == 0 && guiEvent.modifiers == EventModifiers.None)
        {
            OnLeftMouseDrag(mousePos);
        }

        if (guiEvent.type == EventType.MouseDown && guiEvent.button == 1 && guiEvent.modifiers == EventModifiers.None)
        {
            OnRightMouseDown(mousePos);
        }

        if (!selection.pointSelected)
        {
            UpdateSelection(mousePos);
        }
    }

    // * ==============================================================

    // * Handle Left Mouse Events =====================================

    void OnLeftMouseDown(Vector3 mousePos)
    {
        if (!selection.mouseOverPoint)
        {
            int newPointIndex = selection.mouseOverLine ? selection.lineIndex + 1 : area.PointCount;
            Undo.RecordObject(area, "Add Point");
            area.InsertPoint(newPointIndex, mousePos);
            selection.pointIndex = newPointIndex;
        }

        selection.pointSelected = true;
        selection.startPos = mousePos;
        needsRepaint = true;
    }

    void OnLeftMouseUp(Vector3 mousePos)
    {
        if (selection.pointSelected)
        {
            area.SetPoint(selection.pointIndex, selection.startPos);
            Undo.RecordObject(area, "Move Point");
            area.SetPoint(selection.pointIndex, mousePos);
            selection.pointSelected = false;
            selection.pointIndex = -1;
            needsRepaint = true;
        }
    }

    void OnLeftMouseDrag(Vector3 mousePos)
    {
        if (selection.pointSelected)
        {
            area.SetPoint(selection.pointIndex, mousePos);
            needsRepaint = true;
        }
    }

    // * =====================================================

    // * Handle Right Mouse Events ===========================

    void OnRightMouseDown(Vector3 mousePos)
    {
        if (selection.mouseOverPoint)
        {
            Undo.RecordObject(area, "Remove Point");
            area.RemovePoint(selection.pointIndex);
            needsRepaint = true;
        }
    }

    // * =====================================================

    // * Detect Hover Over ===================================

    void UpdateSelection(Vector3 mousePos)
    {
        int pointIndex = -1;
        for (int i = 0; i < area.PointCount; i++) 
        {
            if (Vector3.Distance(mousePos, area.GetPoint(i)) < area.handleRadius)
            {
                pointIndex = i;
                break;
            }
        }

        if (pointIndex != selection.pointIndex)
        {
            selection.pointIndex = pointIndex;
            selection.mouseOverPoint = pointIndex != -1;
            needsRepaint = true;
        }

        if (selection.mouseOverPoint)
        {
            selection.mouseOverLine = false;
            selection.lineIndex = -1;
        }
        else
        {
            int lineIndex = -1;
            float closest = area.lineSensitivity;
            for (int i = 0; i < area.PointCount; i++) 
            {
                Vector3 next = area.GetPoint((i + 1) % area.PointCount);
                float distToLine = HandleUtility.DistancePointToLineSegment(
                    new Vector2(mousePos.x, mousePos.z), 
                    new Vector2(area.GetPoint(i).x, area.GetPoint(i).z), 
                    new Vector2(next.x, next.z)
                );
                if (distToLine < closest)
                {
                    lineIndex = i;
                    closest = distToLine;
                }
            }

            if (lineIndex != selection.lineIndex)
            {
                selection.lineIndex = lineIndex;
                selection.mouseOverLine = lineIndex != -1;
                needsRepaint = true;
            }
        }
    }

    // * =====================================================

    // * Rendering ===========================================

    // draw handles and outline for each point
    void Draw()
    {
        for (int i = 0; i < area.PointCount; i++) 
        {


            Vector3 next = area.GetPoint((i + 1) % area.PointCount);
            if (!editing)
            {
                Handles.color = Color.grey;
            }
            else if (i == selection.lineIndex)
            {
                Handles.color = Color.red;
            }
            else
            {
                Handles.color = Color.gray;
            }
            Handles.DrawLine(area.GetPoint(i), next, area.lineWidth);

            if (!editing)
            {
                Handles.color = Color.grey;
            }
            else if (i == selection.pointIndex)
            {
                if (selection.pointSelected)
                {
                    Handles.color = Color.green;
                }
                else
                {
                    Handles.color = Color.red;
                }
            }
            else
            {
                Handles.color = Color.yellow;
            }
            Handles.DrawSolidDisc(area.GetPoint(i), Vector3.up, area.handleRadius);
        }
    }

    // * =====================================================

    void OnEnable()
    {
        area = target as SpawnArea;
        selection = new SelectionInfo
        {
            pointIndex = -1,
            lineIndex = -1
        };
    }

    public struct SelectionInfo
    {
        public int pointIndex;
        public bool mouseOverPoint;
        public int lineIndex;
        public bool mouseOverLine;
        public bool pointSelected;
        public Vector3 startPos;
    }
}
