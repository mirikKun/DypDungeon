using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(GraphDungeonGenerator))]
[CanEditMultipleObjects]
public class EditorGraphDungeonGenerator : Editor
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
    private SerializedProperty rightAngle;
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
        rightAngle = serializedObject.FindProperty("rightAngle");
        camera = serializedObject.FindProperty("camera");

    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        GraphDungeonGenerator generator = (GraphDungeonGenerator)target;

        EditorGUILayout.PropertyField(randomSeed);
        EditorGUILayout.PropertyField(roomCount);
        EditorGUILayout.PropertyField(chances);
        EditorGUILayout.PropertyField(cyclesAllowed);

        if (GUILayout.Button("GenerateMatrix"))
        {
            generator.GenerateMatrix();
        }

        EditorGUILayout.LabelField("Matrix");
        EditorGUI.DrawRect(EditorGUILayout.GetControlRect(false, 1), Color.white);


        GUIStyle columnStyle = new GUIStyle();
        columnStyle.fixedWidth = 35;

        GUIStyle rowStyle = new GUIStyle();
        rowStyle.fixedHeight = 25;
        rowStyle.fixedWidth = 25;


        GUIStyle rowHeaderStyle = new GUIStyle();
        rowHeaderStyle.fixedWidth = 35;
        rowHeaderStyle.fixedHeight = 25f;


        GUIStyle columnHeaderStyle = new GUIStyle();
        columnHeaderStyle.fixedWidth = 25;
        columnHeaderStyle.fixedHeight = 25f;


        EditorGUILayout.BeginHorizontal();
        for (int y = -1; y < generator.graph.GetLength(0); y++)
        {
            if (y != 0)
            {
                EditorGUILayout.BeginVertical();
                for (int x = -1; x < generator.graph.GetLength(0) - 1; x++)
                {
                    if (x == -1 && y == -1)
                    {
                        EditorGUILayout.BeginVertical(rowHeaderStyle);
                        EditorGUILayout.LabelField("[X,Y]");
                        EditorGUILayout.EndHorizontal();
                    }
                    else if (x == -1)
                    {
                        EditorGUILayout.BeginVertical(columnHeaderStyle);
                        EditorGUILayout.LabelField((y + 1).ToString());
                        EditorGUILayout.EndHorizontal();
                    }
                    else if (y == -1)
                    {
                        EditorGUILayout.BeginVertical(rowHeaderStyle);
                        EditorGUILayout.LabelField((x + 1).ToString());
                        EditorGUILayout.EndHorizontal();
                    }
                    else if (x == y)
                    {
                        EditorGUILayout.BeginVertical(rowStyle);
                        EditorGUILayout.LabelField((y + 1).ToString());
                        EditorGUILayout.EndHorizontal();
                    }

                    if (x >= 0 && y >= 0)
                    {
                        EditorGUILayout.BeginHorizontal(rowStyle);
                        if (x < y)
                        {
                            generator.graph[x, y] = EditorGUILayout.IntField(generator.graph[x, y]);
                        }

                        EditorGUILayout.EndHorizontal();
                    }
                }

                EditorGUILayout.EndVertical();
            }
        }

        bool checkMatrix = generator.CheckMatrix();

        EditorGUILayout.EndHorizontal();
        if (!checkMatrix)
        {
            GUI.contentColor = Color.red;
            EditorGUI.DrawRect(EditorGUILayout.GetControlRect(false, 1), Color.red);

            EditorGUILayout.LabelField("Матриця не досяжна");
        }

        EditorGUI.DrawRect(EditorGUILayout.GetControlRect(false, 1), Color.white);
        GUI.contentColor = Color.white;

        EditorGUILayout.PropertyField(roomSize);
        EditorGUILayout.PropertyField(corridorLenght);
        EditorGUILayout.PropertyField(corridorWidth);
        EditorGUILayout.PropertyField(corridorLenghtRange);
        EditorGUILayout.PropertyField(roomSizeRange);
        EditorGUILayout.PropertyField(randomAngles);
        EditorGUILayout.PropertyField(rightAngle);
        EditorGUILayout.PropertyField(camera);
        if (checkMatrix)
        {
            if (GUILayout.Button("Build Object"))
            {
                generator.GenerateDungeon();
            }
        }
        
        if (GUILayout.Button("Clear"))
        {
            generator.ClearAll();
        }

        serializedObject.ApplyModifiedProperties();
    }
}