using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(GraphDungeonGenerator))]
[CanEditMultipleObjects]

public class GraphDungeonGeneratorEditor : Editor
{
    private SerializedProperty roomSize;
    private SerializedProperty randomSeed;
    private SerializedProperty roomCount;
    private SerializedProperty corridorLenght;
    private SerializedProperty minRoomDistance;

    private void OnEnable()
    {
        roomSize = serializedObject.FindProperty("roomSize");
        randomSeed = serializedObject.FindProperty("randomSeed");
        roomCount = serializedObject.FindProperty("roomCount");
        corridorLenght = serializedObject.FindProperty("corridorLenght");
        minRoomDistance = serializedObject.FindProperty("minRoomDistance");
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        GraphDungeonGenerator generator = (GraphDungeonGenerator)target;

        EditorGUILayout.PropertyField(roomSize);
        EditorGUILayout.PropertyField(randomSeed);
        EditorGUILayout.PropertyField(roomCount);
        EditorGUILayout.PropertyField(corridorLenght);
        EditorGUILayout.PropertyField(minRoomDistance);
        if(GUILayout.Button("Build Object"))
        {
            generator.Generate();
        }  if(GUILayout.Button("Clear"))
        {
            generator.ClearAll();
        }
        serializedObject.ApplyModifiedProperties();

    }
}
