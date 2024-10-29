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
    public Vector3Int MapSize = new Vector3Int(10, 10, 10);

    private WfcElementObject[,,] spawnedTiles;

    private Queue<Vector3Int> recalcPossibleTilesQueue = new Queue<Vector3Int>();
    private List<WfcElementObject>[,,] possibleTiles;
    [SerializeField] private WfcElementObject _edgeObject;
    [SerializeField] private WfcElementObject _centerObject;
    [SerializeField] private WfcElementObject _emptyObject;
    public WfcElementObject first;
    public WfcElementObject second;
    public WfcRotations direction;
    
    private void Start()
    {
        spawnedTiles = new WfcElementObject[MapSize.x, MapSize.y, MapSize.z];

        int countBeforeAdding = TilePrefabs.Count;
        foreach (WfcElementObject tile in TilePrefabs)
        {
            tile.GetVertexes();
        }

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
            case WfcPositioning.OneVerticalRotation:
                TilePrefabs[tileIndex].Weight /= 2;
                if (TilePrefabs[tileIndex].Weight <= 0) TilePrefabs[tileIndex].Weight = 0.5f;

                clone = Instantiate(TilePrefabs[tileIndex],
                    TilePrefabs[tileIndex].transform.position + Vector3.forward * 1.2f,
                    Quaternion.identity, parent);
                clone.GetVertexes();
                clone.RotateY180();
                TilePrefabs.Add(clone);

                break;

            case WfcPositioning.HorizontalRotations:
                TilePrefabs[tileIndex].Weight /= 4;
                if (TilePrefabs[tileIndex].Weight <= 0) TilePrefabs[tileIndex].Weight = 0.5f;


                clone = Instantiate(TilePrefabs[tileIndex],
                    TilePrefabs[tileIndex].transform.position + Vector3.forward * 1.2f,
                    Quaternion.identity, parent);
                clone.GetVertexes();
                clone.RotateX90();
                TilePrefabs.Add(clone);

                clone = Instantiate(TilePrefabs[tileIndex],
                    TilePrefabs[tileIndex].transform.position + Vector3.forward * 2 * 1.2f,
                    Quaternion.identity, parent);
                clone.GetVertexes();
                clone.RotateX90();
                clone.RotateX90();
                TilePrefabs.Add(clone);

                clone = Instantiate(TilePrefabs[tileIndex],
                    TilePrefabs[tileIndex].transform.position + Vector3.forward * 3 * 1.2f,
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


                clone = Instantiate(TilePrefabs[tileIndex],
                    TilePrefabs[tileIndex].transform.position + Vector3.forward * 1.2f,
                    Quaternion.identity, parent);
                clone.GetVertexes();
                clone.RotateX90();
                TilePrefabs.Add(clone);

                clone = Instantiate(TilePrefabs[tileIndex],
                    TilePrefabs[tileIndex].transform.position + Vector3.forward * 2 * 1.2f,
                    Quaternion.identity, parent);
                clone.GetVertexes();
                clone.RotateX90();
                clone.RotateX90();
                TilePrefabs.Add(clone);

                clone = Instantiate(TilePrefabs[tileIndex],
                    TilePrefabs[tileIndex].transform.position + Vector3.forward * 3 * 1.2f,
                    Quaternion.identity, parent);
                clone.GetVertexes();
                clone.RotateX90();
                clone.RotateX90();
                clone.RotateX90();
                TilePrefabs.Add(clone);

                /////////////
                clone = Instantiate(TilePrefabs[tileIndex],
                    TilePrefabs[tileIndex].transform.position + Vector3.forward * 4 * 1.2f,
                    Quaternion.identity, parent);
                clone.GetVertexes();
                clone.RotateY180();
                TilePrefabs.Add(clone);

                clone = Instantiate(TilePrefabs[tileIndex],
                    TilePrefabs[tileIndex].transform.position + Vector3.forward * 5 * 1.2f,
                    Quaternion.identity, parent);
                clone.GetVertexes();
                clone.RotateY180();

                clone.RotateX90();
                TilePrefabs.Add(clone);

                clone = Instantiate(TilePrefabs[tileIndex],
                    TilePrefabs[tileIndex].transform.position + Vector3.forward * 6 * 1.2f,
                    Quaternion.identity, parent);
                clone.GetVertexes();
                clone.RotateY180();

                clone.RotateX90();
                clone.RotateX90();
                TilePrefabs.Add(clone);

                clone = Instantiate(TilePrefabs[tileIndex],
                    TilePrefabs[tileIndex].transform.position + Vector3.forward * 7 * 1.2f,
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
        possibleTiles = new List<WfcElementObject>[MapSize.x, MapSize.y, MapSize.z];

        int maxAttempts = 10;
        int attempts = 0;
        while (attempts++ < maxAttempts)
        {
            for (int x = 0; x < MapSize.x; x++)
            for (int y = 0; y < MapSize.y; y++)
            for (int z = 0; z < MapSize.z; z++)
            {
                possibleTiles[x, y, z] = new List<WfcElementObject>(TilePrefabs);
            }

             //WfcElementObject tileInCenter = GetRandomTile(TilePrefabs);
            possibleTiles[MapSize.x / 2, MapSize.y / 2, MapSize.z / 2] = new List<WfcElementObject> { _centerObject };
            
            // Vector3Int position = new Vector3Int(MapSize.x / 2, MapSize.y / 2, MapSize.z / 2);
            // List<WfcElementObject> possibleTilesHere = possibleTiles[position.x, position.y, position.z];
            // possibleTilesHere.RemoveAll(t => !IsTilePossible(t, position));
            // possibleTiles[position.x, position.y, position.z] = new List<WfcElementObject>
            //     { GetRandomTile(possibleTilesHere) };
            // recalcPossibleTilesQueue.Clear();
            EnqueueNeighboursToRecalc(new Vector3Int(MapSize.x / 2, MapSize.y / 2, MapSize.z / 2));
            FillEdges();
            bool success = GenerateAllPossibleTiles();

            if (success) break;
        }

        PlaceAllTiles();
    }

    private void FillEdges()
    {
        _edgeObject.GetVertexes();

        for (int x = 0; x < MapSize.x; x++)
        for (int y = 0; y < MapSize.y-1; y++)
        for (int z = 0; z < MapSize.z; z++)
        {
            if ((x == 0 || y == 0 || z == 0 ||
                                       x == MapSize.x - 1 || z == MapSize.z - 1))
            {
                possibleTiles[x, y, z] =
                    new List<WfcElementObject> { _edgeObject };
            }
        }
    }

    private bool GenerateAllPossibleTiles()
    {
        int maxIterations = MapSize.x * MapSize.y;
        int iterations = 0;
        int backtracks = 0;
        int maxInnerIterations = 200;

        while (iterations++ < maxIterations)
        {
            int innerIterations = 0;

            while (recalcPossibleTilesQueue.Count > 0 && innerIterations++ < maxInnerIterations)
            {
                Vector3Int position = recalcPossibleTilesQueue.Dequeue();
                if (position.x == 0 || position.y == 0 || position.z == 0 ||
                    position.x == MapSize.x - 1 || position.y == MapSize.y - 1 || position.z == MapSize.z - 1)
                {
                    continue;
                }

                List<WfcElementObject> possibleTilesHere = possibleTiles[position.x, position.y, position.z];
                int countRemoved = possibleTilesHere.RemoveAll(t => !IsTilePossible(t, position));

                if (countRemoved > 0) EnqueueNeighboursToRecalc(position);

                if (possibleTilesHere.Count == 0)
                {
                    possibleTilesHere.AddRange(TilePrefabs);
                    if (position.x + 1 < MapSize.x - 1)
                        possibleTiles[position.x + 1, position.y, position.z] = new List<WfcElementObject>(TilePrefabs);
                    if (position.x - 1 > 0)
                        possibleTiles[position.x - 1, position.y, position.z] = new List<WfcElementObject>(TilePrefabs);
                    if (position.y + 1 < MapSize.y - 1)
                        possibleTiles[position.x, position.y + 1, position.z] = new List<WfcElementObject>(TilePrefabs);
                    if (position.y - 1 > 0)
                        possibleTiles[position.x, position.y - 1, position.z] = new List<WfcElementObject>(TilePrefabs);
                    if (position.z + 1 < MapSize.z - 1)
                        possibleTiles[position.x, position.y, position.z + 1] = new List<WfcElementObject>(TilePrefabs);
                    if (position.z - 1 > 0)
                        possibleTiles[position.x, position.y, position.z - 1] = new List<WfcElementObject>(TilePrefabs);

                    EnqueueNeighboursToRecalc(position);

                    backtracks++;
                }
            }

            if (innerIterations == maxInnerIterations) break;

            List<WfcElementObject> maxCountTile = possibleTiles[1, 1, 1];
            Vector3Int maxCountTilePosition = new Vector3Int(1, 1, 1);

            for (int x = 1; x < MapSize.x - 1; x++)
            for (int y = 1; y < MapSize.y - 1; y++)
            for (int z = 1; z < MapSize.z - 1; z++)
            {
                if (possibleTiles[x, y, z].Count > maxCountTile.Count)
                {
                    maxCountTile = possibleTiles[x, y, z];
                    maxCountTilePosition = new Vector3Int(x, y, z);
                }
            }

            if (maxCountTile.Count == 1)
            {
                Debug.Log($"Generated for {iterations} iterations, with {backtracks} backtracks");
                return true;
            }

            WfcElementObject tileToCollapse = GetRandomTile(maxCountTile);
            possibleTiles[maxCountTilePosition.x, maxCountTilePosition.y, maxCountTilePosition.z] =
                new List<WfcElementObject> { tileToCollapse };
            EnqueueNeighboursToRecalc(maxCountTilePosition);
        }

        Debug.Log($"Failed, run out of iterations with {backtracks} backtracks");
        return false;
    }

    private bool IsTilePossible(WfcElementObject tile, Vector3Int position)
    {
        bool isAllRightImpossible = possibleTiles[position.x - 1, position.y, position.z]
            .All(rightTile => !CanAppendTile(tile, rightTile, WfcRotations.Left));
        if (isAllRightImpossible) return false;

        bool isAllLeftImpossible = possibleTiles[position.x + 1, position.y, position.z]
            .All(leftTile => !CanAppendTile(tile, leftTile, WfcRotations.Right));
        if (isAllLeftImpossible) return false;


        bool isAllForwardImpossible = possibleTiles[position.x, position.y, position.z - 1]
            .All(fwdTile => !CanAppendTile(tile, fwdTile, WfcRotations.Back));
        if (isAllForwardImpossible) return false;

        bool isAllBackImpossible = possibleTiles[position.x, position.y, position.z + 1]
            .All(backTile => !CanAppendTile(tile, backTile, WfcRotations.Forward));
        if (isAllBackImpossible) return false;

        bool isAllUpImpossible = possibleTiles[position.x, position.y - 1, position.z]
            .All(fwdTile => !CanAppendTile(tile, fwdTile, WfcRotations.Bottom));
        if (isAllUpImpossible) return false;

        bool isAllBottomImpossible = possibleTiles[position.x, position.y + 1, position.z]
            .All(backTile => !CanAppendTile(tile, backTile, WfcRotations.Up));
        if (isAllBottomImpossible) return false;

        return true;
    }

    private void PlaceAllTiles()
    {
        // for (int x = 1; x < MapSize.x - 1; x++)
        // for (int y = 1; y < MapSize.y - 1; y++)
        // for (int z = 1; z < MapSize.z - 1; z++)
         for (int x = 0; x < MapSize.x ; x++)
         for (int y = 0; y < MapSize.y - 1; y++)
         for (int z = 0; z < MapSize.z ; z++)
        {
            PlaceTile(x, y, z);
        }
    }

    private void EnqueueNeighboursToRecalc(Vector3Int position)
    {
   

        EnqueueTile(new Vector3Int(position.x + 1, position.y, position.z));
        EnqueueTile(new Vector3Int(position.x - 1, position.y, position.z));
        EnqueueTile(new Vector3Int(position.x, position.y + 1, position.z));
        EnqueueTile(new Vector3Int(position.x, position.y - 1, position.z));
        EnqueueTile(new Vector3Int(position.x, position.y, position.z + 1));
        EnqueueTile(new Vector3Int(position.x, position.y, position.z - 1));
    }

    private void EnqueueTile(Vector3Int position)
    {
        if (possibleTiles.Length == 1 && possibleTiles[position.x, position.y, position.z][0] == _emptyObject)
            return;
        recalcPossibleTilesQueue.Enqueue(position);

    }

    private void PlaceTile(int x, int y, int z)
    {
        if (possibleTiles[x, y, z].Count == 0) return;

        WfcElementObject selectedTile = GetRandomTile(possibleTiles[x, y, z]);
        if (selectedTile.NotGenerate) return;

        Vector3 position = _generationRules._tileSize * new Vector3(x, y + 1, z);
        spawnedTiles[x, y, z] = Instantiate(selectedTile, position, selectedTile.transform.rotation, transform);
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

        return availableTiles[^1];
    }

    [Button]
    public void TestCheck()
    {
        Debug.Log(CanAppendTile(first, second, direction));
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
        else if (direction == WfcRotations.Up)
        {
            return existingTile.UpTileVertexes.SetEquals(tileToAppend.BottomTileVertexes);
        }
        else if (direction == WfcRotations.Bottom)
        {
            return existingTile.BottomTileVertexes.SetEquals(tileToAppend.UpTileVertexes);
        }
        else
        {
            throw new ArgumentException("Wrong direction value, should be Vector3.left/right/back/forward",
                nameof(direction));
        }
    }
}