using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SegmentDungeonPlacer : DungeonPlacer
{
    [SerializeField] private Transform hallWay;
    [SerializeField] private Transform room;
    [SerializeField] private Transform wall;

    public void Place(int[,] grid)
    {
        for (int i = 0; i < grid.GetLength(0); i++)
        {
            for (int j = 0; j < grid.GetLength(1); j++)
            {
                Transform segment;

                if (grid[i, j] > 0)
                {
                    segment = room;
                }
                else if (grid[i, j] == -1)
                {
                    segment = hallWay;
                }
                else
                {
                    segment = wall;
                }

                Instantiate(segment, new Vector3(j, 0, i), Quaternion.identity, transform);
            }
        }
    }
}