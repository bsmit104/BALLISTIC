using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(NetworkLevelManager))]
public class NetworkLevelManagerEditor : Editor
{
    NetworkLevelManager manager;

    public override void OnInspectorGUI()
    {
        manager = target as NetworkLevelManager;

        serializedObject.Update();

        // scene indices
        EditorGUILayout.PropertyField(serializedObject.FindProperty("lobbySceneIndex"));
        EditorGUILayout.PropertyField(serializedObject.FindProperty("firstLevelIndex"));
        EditorGUILayout.PropertyField(serializedObject.FindProperty("lastLevelIndex"));

        // level picking
        EditorGUILayout.PropertyField(serializedObject.FindProperty("refreshChance"));

        // transitions
        EditorGUILayout.PropertyField(serializedObject.FindProperty("transitionCanvas"));
        EditorGUILayout.PropertyField(serializedObject.FindProperty("transitionElement"));

        GUILayout.Label(@"
        TUTORIAL:
            Press Play to use the test transition buttons.");

        EditorGUILayout.PropertyField(serializedObject.FindProperty("enterTransitionDuration"));
        EditorGUILayout.PropertyField(serializedObject.FindProperty("enterTransitionEndPos"));
        if (GUILayout.Button("Test Enter Transition") && !manager.EnterRunning)
        {
            manager.StartEnterTransition();
        }

        EditorGUILayout.PropertyField(serializedObject.FindProperty("exitTransitionDuration"));
        EditorGUILayout.PropertyField(serializedObject.FindProperty("exitTransitionStartPos"));
        if (GUILayout.Button("Test Exit Transition") && !manager.ExitRunning)
        {
            manager.StartExitTransition();
        }

        EditorGUILayout.PropertyField(serializedObject.FindProperty("loadCompletionCheck"));

        // win state
        EditorGUILayout.PropertyField(serializedObject.FindProperty("winScreenDuration"));

        serializedObject.ApplyModifiedProperties();
    }

    void OnEnable()
    {
        manager = target as NetworkLevelManager;
    }
}
