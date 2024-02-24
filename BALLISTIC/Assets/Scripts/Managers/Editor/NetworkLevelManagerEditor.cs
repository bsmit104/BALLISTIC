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

        // lobby canvas
        EditorGUILayout.PropertyField(serializedObject.FindProperty("lobbyCanvas"));
        EditorGUILayout.PropertyField(serializedObject.FindProperty("lobbyCodeText"));

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
            Open the prefab and press Play to use the test transition buttons.");

        EditorGUILayout.PropertyField(serializedObject.FindProperty("enterTransitionDuration"));
        EditorGUILayout.PropertyField(serializedObject.FindProperty("enterTransitionEndPos"));
        if (GUILayout.Button("Test Enter Transition") && !manager.EnterRunning)
        {
            manager.StartEnterTransition();
        }

        EditorGUILayout.PropertyField(serializedObject.FindProperty("exitTransitionDuration"));
        EditorGUILayout.PropertyField(serializedObject.FindProperty("exitTransitionStartPos"));
        EditorGUILayout.PropertyField(serializedObject.FindProperty("waitBetweenTransitions"));
        if (GUILayout.Button("Test Exit Transition") && !manager.ExitRunning)
        {
            manager.StartExitTransition();
        }

        EditorGUILayout.PropertyField(serializedObject.FindProperty("loadCompletionCheck"));

        // win state
        EditorGUILayout.PropertyField(serializedObject.FindProperty("waitBeforeWinScreen"));
        EditorGUILayout.PropertyField(serializedObject.FindProperty("winScreenDuration"));
        EditorGUILayout.PropertyField(serializedObject.FindProperty("winScreen"));
        EditorGUILayout.PropertyField(serializedObject.FindProperty("winText"));

        serializedObject.ApplyModifiedProperties();
    }

    void OnEnable()
    {
        manager = target as NetworkLevelManager;
    }
}
