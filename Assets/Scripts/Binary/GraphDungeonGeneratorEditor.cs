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
    private SerializedProperty chances;
    private SerializedProperty corridorLenght;
    private SerializedProperty corridorWidth;
    private SerializedProperty cyclesAllowed;    
    private SerializedProperty corridorLenghtRange;
    private SerializedProperty roomSizeRange;
    private SerializedProperty randomAngles;
    private SerializedProperty camera;

    private void OnEnable()
    {
        roomSize = serializedObject.FindProperty("roomSize");
        randomSeed = serializedObject.FindProperty("randomSeed");
        roomCount = serializedObject.FindProperty("roomCount");
        chances = serializedObject.FindProperty("chances");
        corridorLenght = serializedObject.FindProperty("corridorLenght");
        corridorWidth = serializedObject.FindProperty("corridorWidth");
        cyclesAllowed = serializedObject.FindProperty("cyclesAllowed");        
        corridorLenghtRange = serializedObject.FindProperty("corridorLenghtRange");
        roomSizeRange = serializedObject.FindProperty("roomSizeRange");
        randomAngles = serializedObject.FindProperty("randomAngles");
        camera = serializedObject.FindProperty("camera");
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        GraphDungeonGenerator generator = (GraphDungeonGenerator)target;

        EditorGUILayout.PropertyField(roomSize);
        EditorGUILayout.PropertyField(randomSeed);
        EditorGUILayout.PropertyField(roomCount);
        EditorGUILayout.PropertyField(chances);
        EditorGUILayout.PropertyField(corridorLenght);
        EditorGUILayout.PropertyField(corridorWidth);
        EditorGUILayout.PropertyField(cyclesAllowed);
        EditorGUILayout.PropertyField(corridorLenghtRange);
        EditorGUILayout.PropertyField(roomSizeRange);
        EditorGUILayout.PropertyField(randomAngles);
        EditorGUILayout.PropertyField(camera);
        if(GUILayout.Button("Build Object"))
        {
            generator.Generate();
        }  
        if(GUILayout.Button("Clear"))
        {
            generator.ClearAll();
        }
        serializedObject.ApplyModifiedProperties();

    }
}
