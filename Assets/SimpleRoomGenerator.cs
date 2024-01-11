using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;
using WaveFunctionCollapse;
using Random = UnityEngine.Random;

public class SimpleRoomGenerator : MonoBehaviour
{
    public List<WfcElementObject> TilePrefabs;
    public Vector2Int MapSize = new Vector2Int(10, 10);

    private WfcElementObject[,] spawnedTiles;

    private void Start()
    {
        spawnedTiles = new WfcElementObject[MapSize.x, MapSize.y];

        int countBeforeAdding = TilePrefabs.Count;
        foreach (WfcElementObject tile in TilePrefabs)
        {
            tile.GetVertexes();
        }
        for (int i = 0; i < countBeforeAdding; i++)
        {
            WfcElementObject clone;
            Transform parent = TilePrefabs[0].transform.parent;
            switch (TilePrefabs[i].WfcPositioning)
            {
                case WfcPositioning.OneRotation:
                    break;

                case WfcPositioning.HorizontalRotations:
                    TilePrefabs[i].Weight /= 4;
                    if (TilePrefabs[i].Weight <= 0) TilePrefabs[i].Weight = 1;


                    clone = Instantiate(TilePrefabs[i], TilePrefabs[i].transform.position + Vector3.forward * 1.2f,
                        Quaternion.identity, parent);
                    clone.GetVertexes();

                    clone.RotateX90();
                    TilePrefabs.Add(clone);

                    clone = Instantiate(TilePrefabs[i], TilePrefabs[i].transform.position + Vector3.forward * 2 * 1.2f,
                        Quaternion.identity, parent);
                    clone.GetVertexes();

                    clone.RotateX90();
                    clone.RotateX90();

                    TilePrefabs.Add(clone);

                    clone = Instantiate(TilePrefabs[i], TilePrefabs[i].transform.position + Vector3.forward * 3 * 1.2f,
                        Quaternion.identity, parent);
                    clone.GetVertexes();

                    clone.RotateX90();
                    clone.RotateX90();
                    clone.RotateX90();
                    TilePrefabs.Add(clone);
                    break;
                case WfcPositioning.AllRotations:

                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        StartCoroutine(Generate());
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.D))
        {
            StopAllCoroutines();

            foreach (WfcElementObject spawnedTile in spawnedTiles)
            {
                if (spawnedTile != null) Destroy(spawnedTile.gameObject);
            }

            StartCoroutine(Generate());
        }
    }

    public IEnumerator Generate()
    {
        for (int x = 1; x < MapSize.x - 1; x++)
        {
            for (int y = 1; y < MapSize.y - 1; y++)
            {
                yield return new WaitForSeconds(0.12f);

                PlaceTile(x, y);
            }
        }

        yield return new WaitForSeconds(0.8f);
        foreach (WfcElementObject spawnedTile in spawnedTiles)
        {
            if (spawnedTile != null) Destroy(spawnedTile.gameObject);
        }

        StartCoroutine(Generate());
    }

    private void PlaceTile(int x, int y)
    {
        List<WfcElementObject> availableTiles = new List<WfcElementObject>();
        if(spawnedTiles[x, y - 1])
        {
            Debug.Log("________");
            Debug.Log(string.Join(" ,", spawnedTiles[x, y - 1].ForwardTileVertexes));
            Debug.Log("____");
        }
        foreach (WfcElementObject tilePrefab in TilePrefabs)
        {
            if (CanAppendTile(spawnedTiles[x - 1, y], tilePrefab, WfcRotations.Left) &&
                CanAppendTile(spawnedTiles[x + 1, y], tilePrefab, WfcRotations.Right) &&
                CanAppendTile(spawnedTiles[x, y - 1], tilePrefab, WfcRotations.Back) &&
                CanAppendTile(spawnedTiles[x, y + 1], tilePrefab, WfcRotations.Forward))
            {
                availableTiles.Add(tilePrefab);
            }
        }

        if (availableTiles.Count == 0) return;
        WfcElementObject selectedTile = GetRandomTile(availableTiles);
        Vector3 position =  new Vector3(x, 1, y);
        spawnedTiles[x, y] = Instantiate(selectedTile, position, selectedTile.transform.rotation);
        spawnedTiles[x, y].CopyValues(selectedTile);

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

    private bool CanAppendTile(WfcElementObject existingTile, WfcElementObject tileToAppend, WfcRotations direction)
    {
        if (existingTile == null) return true;

        if (direction == WfcRotations.Right)
        {
            return tileToAppend.RightTileVertexes.SetEquals(existingTile.LeftTileVertexes);
        }
        else if (direction == WfcRotations.Left)
        {
            return tileToAppend.LeftTileVertexes.SetEquals(existingTile.RightTileVertexes);
        }
        else if (direction == WfcRotations.Forward)
        {
            return tileToAppend.ForwardTileVertexes.SetEquals(existingTile.BackTileVertexes);
        }
        else if (direction == WfcRotations.Back)
        {
            return tileToAppend.BackTileVertexes.SetEquals(existingTile.ForwardTileVertexes);
        }
        else
        {
            throw new ArgumentException("Wrong direction value, should be Vector3.left/right/back/forward",
                nameof(direction));
        }
    }
}