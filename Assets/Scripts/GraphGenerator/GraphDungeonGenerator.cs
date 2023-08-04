using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using UnityEngine;
using UnityEngine.UIElements;
using Random = UnityEngine.Random;


[ExecuteInEditMode]
public class GraphDungeonGenerator : MonoBehaviour
{
    [SerializeField] protected int roomSize = 10;
    [SerializeField] protected int randomSeed = 10;
    [SerializeField] protected int roomCount = 13;
    [SerializeField] protected float[] chances = { 0.45f, 0.84f, 1f, 1 };

    [SerializeField] protected int corridorLenght = 10;
    [SerializeField] protected int corridorWidth = 1;

    [SerializeField] protected bool cyclesAllowed = true;
    [SerializeField] protected bool randomAngles = true;
    [SerializeField] protected bool rightAngle = true;

    [SerializeField] protected int corridorLenghtRange = 5;
    [SerializeField] protected int roomSizeRange = 5;

    protected int iterations;
    protected int minRoomDistance = 18;
    protected List<Chain> decomposedChains = new List<Chain>();

    private DungeonPlacer dungeonPlacer;


    public int[,] graph = new int[,]
    {
        { 0, 1, 0, 0, 0, 0 },
        { 1, 0, 1, 0, 0, 0 },
        { 0, 1, 0, 1, 0, 0 },
        { 0, 0, 1, 0, 1, 0 },
        { 0, 0, 0, 1, 0, 1 },
        { 0, 0, 0, 0, 1, 0 },
    };


    void Start()
    {
        minRoomDistance = (int)Mathf.Ceil((corridorLenght + corridorLenghtRange - 1) * Mathf.Sqrt(2));
    }

    public void GenerateMatrix()
    {
        GraphGenerator graphGenerator = new GraphGenerator(chances, cyclesAllowed, randomSeed);
        graph = graphGenerator.GenerateGraph(roomCount);
    }

    public bool CheckMatrix()
    {
        return GraphChecks.CheckMatrix(roomCount, graph);
    }

    public virtual void GenerateDungeon()
    {
        roomCount = graph.GetLength(0);
        iterations = 0;
        ResetVariables();
        dungeonPlacer.RemoveEverything();
        var graphHelper = new GraphChainDecomposer(roomCount, graph);
        decomposedChains = graphHelper.GetChainsOfGraph();
        GraphGenerator.PrintSyntaxMatrix(graph);
    }

    protected virtual void ResetVariables()
    {
        decomposedChains = new List<Chain>();
        dungeonPlacer = GetComponent<DungeonPlacer>();
    }

    public void ClearAll()
    {
        if (!dungeonPlacer)
        {
            dungeonPlacer = GetComponent<DungeonPlacer>();
        }

        dungeonPlacer.RemoveEverything();
    }

    protected bool CheckOnIterations()
    {
        iterations++;
        if (iterations > 550)
        {
            Debug.Log("Out of iterations");
            return true;
        }

        return false;
    }
    protected int GetCorridorLenght()
    {
        return corridorLenght + Random.Range(0, corridorLenghtRange);
    }

    protected int GetRoomSize()
    {
        return roomSize + Random.Range(0, roomSizeRange);
    }
}