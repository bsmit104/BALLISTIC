using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(Spawner))]
public class SpawnerEditor : Editor
{
    Spawner spawner;

    public override void OnInspectorGUI()
    {
        spawner = target as Spawner;

        serializedObject.Update();

        EditorGUILayout.PropertyField(serializedObject.FindProperty("spawnAreaPrefab"));
        EditorGUILayout.PropertyField(serializedObject.FindProperty("displayAreaOnPlay"));
        GUILayout.Space(20f);

        GUILayout.Label(@"TUTORIAL:
            Click the 'Add Spawn Area' button to create a new spawn area.
            Select that spawn area in this object's children list to edit.
            Manage the spawn areas using the list below.
        ");

        if (GUILayout.Button("Add Spawn Area"))
        {
            spawner.AddSpawnArea();
            EditorUtility.SetDirty(spawner);
        }

        GUILayout.Space(20f);
        EditorGUILayout.PropertyField(serializedObject.FindProperty("spawnAreas"));

        serializedObject.ApplyModifiedProperties();
    }

    void OnEnable()
    {
        spawner = target as Spawner;
        spawner.SetRenderersActive(true);
    }

    void OnDisable()
    {
        spawner.SetRenderersActive(false);
    }
}
