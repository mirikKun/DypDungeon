using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Sirenix.OdinInspector;
using UnityEngine;
using WaveFunctionCollapse;
using Random = UnityEngine.Random;

public class WFCLevelGenerator : MonoBehaviour
{
    [SerializeField] private WfcGenerationRules _generationRules;
    public List<WfcElementObject> TilePrefabs;
    public Vector2Int MapSize = new Vector2Int(10, 10);

    private WfcElementObject[,] spawnedTiles;

    private Queue<Vector2Int> recalcPossibleTilesQueue = new Queue<Vector2Int>();
    private List<WfcElementObject>[,] possibleTiles;
    [SerializeField]private WfcElementObject _edgeObject;
    public WfcElementObject first;
    public WfcElementObject second;
    public WfcRotations direction;

    private void Start()
    {
        spawnedTiles = new WfcElementObject[MapSize.x, MapSize.y];

        int countBeforeAdding = TilePrefabs.Count;
        foreach (WfcElementObject tile in TilePrefabs)
        {
            tile.GetVertexes();
        }

        Debug.Log("__");
        for (int i = 0; i < countBeforeAdding; i++)
        {
            SetupTile(i);
        }

        Generate();
    }

    private void SetupTile(int tileIndex)
    {
        WfcElementObject clone;
        Transform parent = TilePrefabs[0].transform.parent;
        switch (TilePrefabs[tileIndex].WfcPositioning)
        {
            case WfcPositioning.OneRotation:
                break;

            case WfcPositioning.HorizontalRotations:
                TilePrefabs[tileIndex].Weight /= 4;
                if (TilePrefabs[tileIndex].Weight <= 0) TilePrefabs[tileIndex].Weight = 0.5f;


                clone = Instantiate(TilePrefabs[tileIndex], TilePrefabs[tileIndex].transform.position + Vector3.forward * 1.2f,
                    Quaternion.identity, parent);
                clone.GetVertexes();
                clone.RotateX90();
                TilePrefabs.Add(clone);

                clone = Instantiate(TilePrefabs[tileIndex], TilePrefabs[tileIndex].transform.position + Vector3.forward * 2 * 1.2f,
                    Quaternion.identity, parent);
                clone.GetVertexes();
                clone.RotateX90();
                clone.RotateX90();
                TilePrefabs.Add(clone);

                clone = Instantiate(TilePrefabs[tileIndex], TilePrefabs[tileIndex].transform.position + Vector3.forward * 3 * 1.2f,
                    Quaternion.identity, parent);
                clone.GetVertexes();
                clone.RotateX90();
                clone.RotateX90();
                clone.RotateX90();
                TilePrefabs.Add(clone);
                break;
            case WfcPositioning.AllRotations:
                TilePrefabs[tileIndex].Weight /= 8;
                if (TilePrefabs[tileIndex].Weight <= 0) TilePrefabs[tileIndex].Weight = 0.5f;


                clone = Instantiate(TilePrefabs[tileIndex], TilePrefabs[tileIndex].transform.position + Vector3.forward * 1.2f,
                    Quaternion.identity, parent);
                clone.GetVertexes();
                clone.RotateX90();
                TilePrefabs.Add(clone);

                clone = Instantiate(TilePrefabs[tileIndex], TilePrefabs[tileIndex].transform.position + Vector3.forward * 2 * 1.2f,
                    Quaternion.identity, parent);
                clone.GetVertexes();
                clone.RotateX90();
                clone.RotateX90();
                TilePrefabs.Add(clone);

                clone = Instantiate(TilePrefabs[tileIndex], TilePrefabs[tileIndex].transform.position + Vector3.forward * 3 * 1.2f,
                    Quaternion.identity, parent);
                clone.GetVertexes();
                clone.RotateX90();
                clone.RotateX90();
                clone.RotateX90();
                TilePrefabs.Add(clone);

                /////////////
                clone = Instantiate(TilePrefabs[tileIndex], TilePrefabs[tileIndex].transform.position + Vector3.forward * 4 * 1.2f,
                    Quaternion.identity, parent);
                clone.GetVertexes();
                clone.RotateY180();
                TilePrefabs.Add(clone);

                clone = Instantiate(TilePrefabs[tileIndex], TilePrefabs[tileIndex].transform.position + Vector3.forward * 5 * 1.2f,
                    Quaternion.identity, parent);
                clone.GetVertexes();
                clone.RotateY180();

                clone.RotateX90();
                TilePrefabs.Add(clone);

                clone = Instantiate(TilePrefabs[tileIndex], TilePrefabs[tileIndex].transform.position + Vector3.forward * 6 * 1.2f,
                    Quaternion.identity, parent);
                clone.GetVertexes();
                clone.RotateY180();

                clone.RotateX90();
                clone.RotateX90();
                TilePrefabs.Add(clone);

                clone = Instantiate(TilePrefabs[tileIndex], TilePrefabs[tileIndex].transform.position + Vector3.forward * 7 * 1.2f,
                    Quaternion.identity, parent);
                clone.GetVertexes();
                clone.RotateY180();

                clone.RotateX90();
                clone.RotateX90();
                clone.RotateX90();
                TilePrefabs.Add(clone);
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.D))
        {
            foreach (WfcElementObject spawnedTile in spawnedTiles)
            {
                if (spawnedTile != null) Destroy(spawnedTile.gameObject);
            }

            Generate();
        }
    }

    private void Generate()
    {
        possibleTiles = new List<WfcElementObject>[MapSize.x, MapSize.y];

        int maxAttempts = 10;
        int attempts = 0;
        while (attempts++ < maxAttempts)
        {
            for (int x = 0; x < MapSize.x; x++)
            for (int y = 0; y < MapSize.y; y++)
            {
                possibleTiles[x, y] = new List<WfcElementObject>(TilePrefabs);
            }

            //WfcElementObject tileInCenter = GetRandomTile(TilePrefabs);
            WfcElementObject tileInCenter = TilePrefabs[2];
            possibleTiles[MapSize.x / 2, MapSize.y / 2] = new List<WfcElementObject> { tileInCenter };

            recalcPossibleTilesQueue.Clear();
            EnqueueNeighboursToRecalc(new Vector2Int(MapSize.x / 2, MapSize.y / 2));
            FillEdges();
            bool success = GenerateAllPossibleTiles();

            if (success) break;
        }

        PlaceAllTiles();
    }

    private void FillEdges()
    {
        _edgeObject.GetVertexes();

        for (int i = 0; i < MapSize.x; i++)
        {
            for (int j = 0; j < MapSize.y ; j++)
            {
                if (i == 0 || j == 0 ||
                    i == MapSize.x - 1 || j == MapSize.y - 1)
                {
                    possibleTiles[i, j] =
                        new List<WfcElementObject> { _edgeObject };
                }
            }
        }
            
       
    }
    private bool GenerateAllPossibleTiles()
    {
        int maxIterations = MapSize.x * MapSize.y;
        int iterations = 0;
        int backtracks = 0;

        while (iterations++ < maxIterations)
        {
            int maxInnerIterations = 500;
            int innerIterations = 0;

            while (recalcPossibleTilesQueue.Count > 0 && innerIterations++ < maxInnerIterations)
            {
                Vector2Int position = recalcPossibleTilesQueue.Dequeue();
                if (position.x == 0 || position.y == 0 ||
                    position.x == MapSize.x - 1 || position.y == MapSize.y - 1)
                {
                    continue;
                }

                List<WfcElementObject> possibleTilesHere = possibleTiles[position.x, position.y];

                int countRemoved = possibleTilesHere.RemoveAll(t => !IsTilePossible(t, position));

                if (countRemoved > 0) EnqueueNeighboursToRecalc(position);

                if (possibleTilesHere.Count == 0)
                {
                    // Зашли в тупик, в этих координатах невозможен ни один тайл. Попробуем ещё раз, разрешим все тайлы
                    // в этих и соседних координатах, и посмотрим устаканится ли всё
                    possibleTilesHere.AddRange(TilePrefabs);
                    possibleTiles[position.x + 1, position.y] = new List<WfcElementObject>(TilePrefabs);
                    possibleTiles[position.x - 1, position.y] = new List<WfcElementObject>(TilePrefabs);
                    possibleTiles[position.x, position.y + 1] = new List<WfcElementObject>(TilePrefabs);
                    possibleTiles[position.x, position.y - 1] = new List<WfcElementObject>(TilePrefabs);

                    EnqueueNeighboursToRecalc(position);

                    backtracks++;
                }
            }

            if (innerIterations == maxInnerIterations) break;

            List<WfcElementObject> maxCountTile = possibleTiles[1, 1];
            Vector2Int maxCountTilePosition = new Vector2Int(1, 1);

            for (int x = 1; x < MapSize.x - 1; x++)
            for (int y = 1; y < MapSize.y - 1; y++)
            {
                if (possibleTiles[x, y].Count > maxCountTile.Count)
                {
                    maxCountTile = possibleTiles[x, y];
                    maxCountTilePosition = new Vector2Int(x, y);
                }
            }

            if (maxCountTile.Count == 1)
            {
                Debug.Log($"Generated for {iterations} iterations, with {backtracks} backtracks");
                return true;
            }

            WfcElementObject tileToCollapse = GetRandomTile(maxCountTile);
            possibleTiles[maxCountTilePosition.x, maxCountTilePosition.y] =
                new List<WfcElementObject> { tileToCollapse };
            EnqueueNeighboursToRecalc(maxCountTilePosition);
        }

        Debug.Log($"Failed, run out of iterations with {backtracks} backtracks");
        return false;
    }

    private bool IsTilePossible(WfcElementObject tile, Vector2Int position)
    {
        bool isAllRightImpossible = possibleTiles[position.x - 1, position.y]
            .All(rightTile => !CanAppendTile(tile, rightTile, WfcRotations.Left));
        if (isAllRightImpossible) return false;

        bool isAllLeftImpossible = possibleTiles[position.x + 1, position.y]
            .All(leftTile => !CanAppendTile(tile, leftTile, WfcRotations.Right));
        if (isAllLeftImpossible) return false;

        bool isAllForwardImpossible = possibleTiles[position.x, position.y - 1]
            .All(fwdTile => !CanAppendTile(tile, fwdTile, WfcRotations.Back));
        if (isAllForwardImpossible) return false;

        bool isAllBackImpossible = possibleTiles[position.x, position.y + 1]
            .All(backTile => !CanAppendTile(tile, backTile, WfcRotations.Forward));
        if (isAllBackImpossible) return false;

        return true;
    }

    private void PlaceAllTiles()
    {
        for (int x = 1; x < MapSize.x - 1; x++)
        for (int y = 1; y < MapSize.y - 1; y++)
        {
            PlaceTile(x, y);
        }
    }

    private void EnqueueNeighboursToRecalc(Vector2Int position)
    {
        recalcPossibleTilesQueue.Enqueue(new Vector2Int(position.x + 1, position.y));
        recalcPossibleTilesQueue.Enqueue(new Vector2Int(position.x - 1, position.y));
        recalcPossibleTilesQueue.Enqueue(new Vector2Int(position.x, position.y + 1));
        recalcPossibleTilesQueue.Enqueue(new Vector2Int(position.x, position.y - 1));
    }

    private void PlaceTile(int x, int y)
    {
        if (possibleTiles[x, y].Count == 0) return;

        WfcElementObject selectedTile = GetRandomTile(possibleTiles[x, y]);
        if (selectedTile.NotGenerate) return;

        Vector3 position = _generationRules._tileSize * new Vector3(x, 1, y);
        spawnedTiles[x, y] = Instantiate(selectedTile, position, selectedTile.transform.rotation, transform);
    }

    private WfcElementObject GetRandomTile(List<WfcElementObject> availableTiles)
    {
        List<float> chances = new List<float>();
        for (int i = 0; i < availableTiles.Count; i++)
        {
            chances.Add(availableTiles[i].Weight);
        }

        float value = Random.Range(0, chances.Sum());
        float sum = 0;

        for (int i = 0; i < chances.Count; i++)
        {
            sum += chances[i];
            if (value < sum)
            {
                return availableTiles[i];
            }
        }

        return availableTiles[availableTiles.Count - 1];
    }

    [Button]
    public void TestCheck()
    {
        Debug.Log(CanAppendTile(first,second,direction));
    }
    private bool CanAppendTile(WfcElementObject existingTile, WfcElementObject tileToAppend, WfcRotations direction)
    {
        if (existingTile == null) return true;

        if (direction == WfcRotations.Right)
        {
            return existingTile.RightTileVertexes.SetEquals(tileToAppend.LeftTileVertexes);
        }
        else if (direction == WfcRotations.Left)
        {
            return existingTile.LeftTileVertexes.SetEquals(tileToAppend.RightTileVertexes);
        }
        else if (direction == WfcRotations.Forward)
        {
            return existingTile.ForwardTileVertexes.SetEquals(tileToAppend.BackTileVertexes);
        }
        else if (direction == WfcRotations.Back)
        {
            return existingTile.BackTileVertexes.SetEquals(tileToAppend.ForwardTileVertexes);
        }
        else
        {
            throw new ArgumentException("Wrong direction value, should be Vector3.left/right/back/forward",
                nameof(direction));
        }
    }
}