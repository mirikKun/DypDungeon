using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

[ExecuteInEditMode]
[RequireComponent(typeof(SegmentDungeonPlacer))]
public class TunnelingGenerator : MonoBehaviour
{
    [SerializeField] private int hallWayWidth = 4;
    [SerializeField] private int forkWidth = 6;
    [SerializeField] private int hallWayStepLength = 8;
    [SerializeField] private float hallWayChanceToTurn = 0.15f;
    [SerializeField] private float changeToFork = 0.1f;
    [SerializeField] private float changeToPlaceRoom = 0.1f;
    [SerializeField] private int minLenght = 30;
    [SerializeField] private int maxLenght = 80;
    [SerializeField] private int roomHallWayWidth = 1;
    [SerializeField] private int roomMinSize = 3;
    [SerializeField] private int roomMaxSize = 9;

    [SerializeField] private Vector2Int size = new(100, 100);
    [SerializeField] private int seed = 0;

    private int[,] grid;
    private int lenght;
    private SegmentDungeonPlacer segmentDungeonPlacer;

    private int maxRestarts = 11;
    private int curRestarts = 0;

    [ContextMenu("Generate")]
    void Generate()
    {
        curRestarts = 0;
        lenght = 0;

        Random.InitState(seed);

        segmentDungeonPlacer = GetComponent<SegmentDungeonPlacer>();
        grid = new int[size.y, size.x];
        Tunnel curTunnel = GetStartTunnel();
        while (curRestarts < maxRestarts && lenght < minLenght)
        {
            Tunneling(curTunnel);

            if (lenght < minLenght)
            {
                Restart();
                curTunnel = GetStartTunnel();
            }
        }

        segmentDungeonPlacer.RemoveEverything();
        segmentDungeonPlacer.Place(grid);
    }

    private Tunnel GetStartTunnel()
    {
        Vector2 curPoint = new Vector2(0, (size.y / 2f - hallWayWidth / 2f));
        Direction newDirection = Direction.right;
        Tunnel curTunnel = new Tunnel(curPoint, newDirection, hallWayWidth, hallWayStepLength * 3);
        return curTunnel;
    }

    private void Restart()
    {
        Debug.Log("Restart");
        curRestarts++;

        if (curRestarts >= maxRestarts)
        {
            Debug.LogError("Max out");
        }
        else
        {
            grid = new int[size.y, size.x];
        }
    }

    private void Tunneling(Tunnel curTunnel)
    {
        bool tunneling = true;
        while (tunneling && lenght < maxLenght)
        {
            //FillTunnel
            tunneling = FillSpace(curTunnel);
            if (tunneling)
            {
                Direction newDirection = ChangeDirection(curTunnel.Direction);

                if (Random.Range(0, 1f) < hallWayChanceToTurn)
                {
                    curTunnel = PlaceFork(curTunnel);
                    //FillFork
                    tunneling = FillSpace(curTunnel);
                    if (!tunneling)
                    {
                        return;
                    }

                    Vector2 newStartPoint = curTunnel.GetEndSegmentCenter() +
                                            (Vector2)newDirection.GetVector() * forkWidth / 2f;
                    curTunnel = new Tunnel(newStartPoint, newDirection, hallWayWidth, hallWayStepLength);
                    lenght += forkWidth;
                }
                else
                {
                    if (Random.Range(0, 1f) < changeToFork)
                    {
                        curTunnel = PlaceFork(curTunnel);
                        //FillFork
                        tunneling = FillSpace(curTunnel);
                        if (!tunneling)
                        {
                            return;
                        }

                        Vector2 newStartPoint = curTunnel.GetEndSegmentCenter() +
                                                (Vector2)newDirection.GetVector() * forkWidth / 2f;
                        Tunnel newTunnel = new Tunnel(newStartPoint, newDirection, hallWayWidth, hallWayStepLength);
                        lenght += forkWidth;
                        Tunneling(newTunnel);
                    }

                    curTunnel = new Tunnel(curTunnel.GetEndEdgeCenter(), curTunnel.Direction, hallWayWidth,
                        hallWayStepLength);
                    lenght += hallWayStepLength;
                }

                // if (Random.Range(0, 1f) < changeToPlaceRoom)
                // {
                //     Vector2Int newStartPoint = (curTunnel.GetSegmentCenter() +
                //                                 (Vector2)newDirection.GetVector() * hallWayWidth / 2f).GetVector2Int();
                //     TryPlaceRoom(newStartPoint, newDirection);
                // }
            }
        }
    }

    private void TryPlaceRoom(Vector2Int from, Direction direction)
    {
        int roomWidth = Random.Range(roomMinSize, roomMaxSize);
        int roomLenght = Random.Range(roomMinSize, roomMaxSize);
        Tunnel newTunnel = new Tunnel(from, direction, roomHallWayWidth, roomLenght);
        if (grid[from.x, from.y] == 0 &&
            !CheckOnIntersections(newTunnel.Left, newTunnel.Right, newTunnel.Bottom, newTunnel.Top))
        {
            grid[from.x, from.y] = 1;

            //FillRoom
            FillSpace(newTunnel);
        }
    }

    private Tunnel PlaceFork(Tunnel curTunnel)
    {
        curTunnel = new Tunnel(curTunnel.GetEndEdgeCenter(), curTunnel.Direction, forkWidth,
            forkWidth);
        lenght += hallWayStepLength;
        return curTunnel;
    }

    private Direction ChangeDirection(Direction oldDirection)
    {
        int directionId = (int)oldDirection;
        while (directionId % 2 == (int)oldDirection % 2)
        {
            directionId = Random.Range(0, 4);
        }

        return (Direction)directionId;
    }

    private bool FillSpace(Tunnel tunnel)
    {
        if (!tunnel.AreInBounds(grid.GetLength(0), grid.GetLength(1)))
        {
            Debug.Log("Out");
            return false;
        }

        if (CheckOnIntersections(tunnel.Left, tunnel.Right, tunnel.Bottom, tunnel.Top))
        {
            return false;
        }

        for (int i = tunnel.Left; i < tunnel.Right; i++)
        {
            for (int j = tunnel.Bottom; j < tunnel.Top; j++)
            {
                grid[i, j] = 1;
            }
        }

        return true;
    }

    private bool CheckOnIntersections(int xFrom, int xTo, int yFrom, int yTo)
    {
        for (int i = xFrom; i < xTo; i++)
        {
            for (int j = yFrom; j < yTo; j++)
            {
                if (grid[i, j] != 0)
                {
                    return true;
                }
            }
        }

        return false;
    }
}