using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SegmentDungeonPlacer : DungeonPlacer
{
    [SerializeField] private Transform roomPlane;
    [SerializeField] private Transform doorPlane;
    [SerializeField] private Transform nonePlane;
    [SerializeField] private Transform hallwayPlane;


    [SerializeField] private Transform roomNothing;
    [SerializeField] private Transform roomWall;
    [SerializeField] private Transform roomCorner;

    [SerializeField] private Transform door;

    [SerializeField] private Transform hallWayStraight;
    [SerializeField] private Transform hallWayCorner;
    [SerializeField] private Transform hallWayFork;
    private int Any => (int)SegmentType.Anything;
    private int[,] extendedGrid;


    public void Place(int[,] grid)
    {
        Debug.Log(grid.GetLength(0));
        extendedGrid = ExtendArray(grid);
        for (int i = 1; i < extendedGrid.GetLength(0) - 1; i++)
        {
            for (int j = 1; j < extendedGrid.GetLength(1) - 1; j++)
            {
                // Transform segment;
                //
                // if (extendedGrid[i, j] > 0)
                // {
                //     segment = roomPlane;
                // }
                // else if (extendedGrid[i, j] == -1)
                // {
                //     segment = hallwayPlane;
                // }
                // else if (extendedGrid[i, j] == -2)
                // {
                //     segment = doorPlane;
                // }
                // else
                // {
                //     segment = nonePlane;
                // }
                //
                // Instantiate(segment, new Vector3(j, -2, i), Quaternion.identity, transform);


                if (extendedGrid[i, j] > 0)
                {
                    PlaceRoom(i, j, extendedGrid[i, j]);
                }
                else if (extendedGrid[i, j] == -1)
                {
                    PlaceHallway(i, j);
                }
                else if (extendedGrid[i, j] == -2)
                {
                    PlaceDoor(i, j);
                }
            

            }
        }
    }

    private int[,] ExtendArray(int[,] originalArray)
    {
        int originalRows = originalArray.GetLength(0);
        int originalColumns = originalArray.GetLength(1);

        int newRows = originalRows + 2;
        int newColumns = originalColumns + 2;

        int[,] newArray = new int[newRows, newColumns];

        for (int i = 0; i < originalRows; i++)
        {
            for (int j = 0; j < originalColumns; j++)
            {
                newArray[i + 1, j + 1] = originalArray[i, j];
            }
        }

        for (int i = 0; i < newRows; i++)
        {
            newArray[i, 0] = 0;
            newArray[i, newColumns - 1] = 0;
        }

        for (int j = 0; j < newColumns; j++)
        {
            newArray[0, j] = 0;
            newArray[newRows - 1, j] = 0;
        }

        return newArray;
    }

    private void PlaceRoom(int y, int x, int id)
    {
        if (CheckOnPlace(new int[] { id, id, id, id, id, id, id, id, id }, y, x, true))
        {
            Instantiate(roomNothing, new Vector3(x, 0, y), Quaternion.identity, transform);
        }
        else
            //walls
        if (CheckOnPlace(new int[] { Any, Any, Any, id, id, id, id, id, id }, y, x, true))
        {
            Instantiate(roomWall, new Vector3(x, 0, y), Quaternion.identity, transform);
        }
        else if (CheckOnPlace(new int[] { id, id, id, id, id, id, Any, Any, Any }, y, x, true))
        {
            Instantiate(roomWall, new Vector3(x, 0, y), Quaternion.Euler(0, 180, 0), transform);
        }
        else if (CheckOnPlace(new int[] { Any, id, id, Any, id, id, Any, id, id }, y, x, true))
        {
            Instantiate(roomWall, new Vector3(x, 0, y), Quaternion.Euler(0, 90, 0), transform);
        }
        else if (CheckOnPlace(new int[] { id, id, Any, id, id, Any, id, id, Any }, y, x, true))
        {
            Instantiate(roomWall, new Vector3(x, 0, y), Quaternion.Euler(0, -90, 0), transform);
        }
        else
            //corners
        if (CheckOnPlace(new int[] { Any, Any, Any, Any, id, id, Any, id, id }, y, x, true))
        {
            Instantiate(roomCorner, new Vector3(x, 0, y), Quaternion.identity, transform);
        }
        else if (CheckOnPlace(new int[] { Any, Any, Any, id, id, Any, id, id, Any }, y, x, true))
        {
            Instantiate(roomCorner, new Vector3(x, 0, y), Quaternion.Euler(0, -90, 0), transform);
        }
        else if (CheckOnPlace(new int[] { id, id, Any, id, id, Any, Any, Any, Any }, y, x, true))
        {
            Instantiate(roomCorner, new Vector3(x, 0, y), Quaternion.Euler(0, 180, 0), transform);
        }
        else if (CheckOnPlace(new int[] { Any, id, id, Any, id, id, Any, Any, Any }, y, x, true))
        {
            Instantiate(roomCorner, new Vector3(x, 0, y), Quaternion.Euler(0, 90, 0), transform);
        }
    }

    private void PlaceDoor(int y, int x)
    {
        int door = (int)SegmentType.Door;
        if (CheckOnPlace(new[] { Any, Any, Any, 1, door, 1, 1, 1, 1 }, y, x))
        {
            Instantiate(this.door, new Vector3(x, 0, y), Quaternion.identity, transform);
        }
        else if (CheckOnPlace(new[] { Any, 1, 1, Any, door, 1, Any, 1, 1 }, y, x))
        {
            Instantiate(this.door, new Vector3(x, 0, y), Quaternion.Euler(0, 90, 0), transform);
        }
        else if (CheckOnPlace(new[] { 1, 1, Any, 1, door, Any, 1, 1, Any }, y, x))
        {
            Instantiate(this.door, new Vector3(x, 0, y), Quaternion.Euler(0, -90, 0), transform);
        }
        else if (CheckOnPlace(new[] { 1, 1, 1, 1, door, 1, Any, Any, Any }, y, x))
        {
            Instantiate(this.door, new Vector3(x, 0, y), Quaternion.Euler(0, 180, 0), transform);
        }
    }


    private void PlaceHallway(int y, int x)
    {
        int hallWay = (int)SegmentType.Hallway;

        //forks
        if (CheckOnPlace(new[] { Any, Any, Any, hallWay, hallWay, hallWay, Any, hallWay, Any }, y, x))
        {
            Instantiate(hallWayFork, new Vector3(x, 0, y), Quaternion.identity, transform);
        }
        else if (CheckOnPlace(new[] { Any, hallWay, Any, hallWay, hallWay, hallWay, Any, Any, Any }, y, x))
        {
            Instantiate(hallWayFork, new Vector3(x, 0, y), Quaternion.Euler(0, 180, 0), transform);
        }
        else if (CheckOnPlace(new[] { Any, hallWay, Any, Any, hallWay, hallWay, Any, hallWay, Any }, y, x))
        {
            Instantiate(hallWayFork, new Vector3(x, 0, y), Quaternion.Euler(0, 90, 0), transform);
        }
        else if (CheckOnPlace(new[] { Any, hallWay, Any, hallWay, hallWay, Any, Any, hallWay, Any }, y, x))
        {
            Instantiate(hallWayFork, new Vector3(x, 0, y), Quaternion.Euler(0, -90, 0), transform);
        }
        else
        //turns
        if (CheckOnPlace(new[] { Any, hallWay, Any, Any, hallWay, hallWay, Any, Any, Any }, y, x))
        {
            Instantiate(hallWayCorner, new Vector3(x, 0, y), Quaternion.identity, transform);
        }
        else if (CheckOnPlace(new[] { Any, hallWay, Any, hallWay, hallWay, Any, Any, Any, Any }, y, x))
        {
            Instantiate(hallWayCorner, new Vector3(x, 0, y), Quaternion.Euler(0, 90, 0), transform);
        }
        else if (CheckOnPlace(new[] { Any, Any, Any, hallWay, hallWay, Any, Any, hallWay, Any }, y, x))
        {
            Instantiate(hallWayCorner, new Vector3(x, 0, y), Quaternion.Euler(0, 180, 0), transform);
        }
        else if (CheckOnPlace(new[] { Any, Any, Any, Any, hallWay, hallWay, Any, hallWay, Any }, y, x))
        {
            Instantiate(hallWayCorner, new Vector3(x, 0, y), Quaternion.Euler(0, -90, 0), transform);
        }
        else
      
        //straigthaa
        if (CheckOnPlace(new[] { Any, 0, Any, Any, hallWay, Any, Any, 0, Any }, y, x))
        {
            Instantiate(hallWayStraight, new Vector3(x, 0, y), Quaternion.identity, transform);
        }
        else if (CheckOnPlace(new[] { Any, Any, Any, 0, hallWay, 0, Any, Any, Any }, y, x))
        {
            Instantiate(hallWayStraight, new Vector3(x, 0, y), Quaternion.Euler(0, 90, 0), transform);
        }
    }

    private bool CheckOnPlace(int[] pattern, int y, int x, bool room = false)
    {
        int count = 0;
        int pos = 0;
        for (int i = -1; i < 2; i++)
        {
            for (int j = -1; j < 2; j++)
            {
                if (pattern[pos] == extendedGrid[y + i, x + j] || pattern[pos] == Any || (!room && pattern[pos] > 0 &&
                        extendedGrid[y + i, x + j] > 0) || (room && pattern[pos] > 0 &&
                                                            extendedGrid[y + i, x + j] == (int)SegmentType.Door))
                {
                    count++;
                }

                pos++;
            }
        }

        return count == 9;
    }
}