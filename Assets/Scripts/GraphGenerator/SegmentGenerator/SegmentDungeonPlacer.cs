using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SegmentDungeonPlacer : DungeonPlacer
{
    [SerializeField] private Transform hallWay;
    [SerializeField] private Transform room;

    public void Place(int[,] grid)
    {

        for (int i = 0; i < grid.GetLength(0); i++)
        {
            for (int j = 0; j < grid.GetLength(1); j++)
            {
                if (grid[i,j] > 0)
                {
                    Instantiate(room, new Vector3(i, 0, j),Quaternion.identity);
                }
                else if (grid[i,j] == -1)
                {
                    Instantiate(hallWay, new Vector3(i, 0, j),Quaternion.identity);

                }
            }
        }
        
    }
}
