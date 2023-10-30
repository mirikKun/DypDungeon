using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;
using Random = UnityEngine.Random;

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
    [SerializeField] private int roomHallwayLenght = 2;
    [SerializeField] private int roomHallwayWidth = 1;
    [SerializeField] private int minLenght = 30;
    [SerializeField] private int maxLenght = 80;
    [SerializeField] private int roomMinSize = 3;
    [SerializeField] private int roomMaxSize = 9;

    [SerializeField] private Vector2Int size = new(100, 100);
    [SerializeField] private int seed = 0;
    [SerializeField] private StartPoint[] startPoints;
    private int[,] grid;
    private List<Tunnel> tunnels = new List<Tunnel>();
    private List<Tunnel> tunnelBranches = new List<Tunnel>();
    private int lenght;
    private SegmentDungeonPlacer segmentDungeonPlacer;

    private int maxRestarts = 11;
    private int curRestarts = 0;

    [Serializable]
    private struct StartPoint
    {
        public Vector2 startPoint;
        public Direction startDirection;
        public int startLenght;
    }

    [ContextMenu("Generate")]
    void Generate()
    {
        ClearAll();

        Random.InitState(seed);

        segmentDungeonPlacer = GetComponent<SegmentDungeonPlacer>();
        SetStartTunnels();
        while (curRestarts < maxRestarts && lenght < minLenght)
        {
            Tunneling();
            if (lenght < minLenght)
            {
                Restart();
                SetStartTunnels();
            }
        }

        PlaceRooms();

        segmentDungeonPlacer.RemoveEverything();
        segmentDungeonPlacer.Place(grid);
    }

    private void SetStartTunnels()
    {
        foreach (var startPoint in startPoints)
        {
            Vector2 curPoint = startPoint.startPoint;
            Direction newDirection = startPoint.startDirection;
            Tunnel curTunnel = new Tunnel(curPoint, newDirection, hallWayWidth, startPoint.startLenght);
            tunnelBranches.Add(curTunnel);
        }
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
            ClearAll();
        }
    }

    private void ClearAll()
    {
        lenght = 0;

        tunnelBranches.Clear();
        tunnels.Clear();
        grid = new int[size.y, size.x];
    }
    private void Tunneling()
    {
        bool tunneling = true;


        //FillTunnel
        while (tunneling && lenght < maxLenght)
        {
            for (var i = 0; i < tunnelBranches.Count; i++)
            {
                var tunnelBranch = tunnelBranches[i];
                tunneling = FillSpace(tunnelBranch);
                if (tunneling)
                {
                    tunnelBranches[i] = TunnelingMakeStep(tunnelBranch);
                    if (tunnelBranches[i] == null)
                    {
                        tunnelBranches.RemoveAt(i);
                    }
                }
                else
                {
                    tunnelBranches.RemoveAt(i);
                }
            }
        }
    }


    private Tunnel TunnelingMakeStep(Tunnel curTunnel)
    {
        tunnels.Add(curTunnel);
        Direction newDirection = curTunnel.GetTurnedDirection();

        if (Random.Range(0, 1f) < hallWayChanceToTurn)
        {
            curTunnel = PlaceFork(curTunnel);
            //FillFork
            bool tunneling = FillSpace(curTunnel);
            if (!tunneling)
            {
                return null;
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
                bool tunneling = FillSpace(curTunnel);
                if (!tunneling)
                {
                    return null;
                }

                Vector2 newStartPoint = curTunnel.GetEndSegmentCenter() +
                                        (Vector2)newDirection.GetVector() * forkWidth / 2f;
                Tunnel newTunnel = new Tunnel(newStartPoint, newDirection, hallWayWidth, hallWayStepLength);
                lenght += forkWidth;
                tunnelBranches.Add(newTunnel);
            }

            curTunnel = new Tunnel(curTunnel.GetEndEdgeCenter(), curTunnel.Direction, hallWayWidth,
                hallWayStepLength);
            lenght += hallWayStepLength;
        }

        return curTunnel;
    }

    private void PlaceRooms()
    {
        foreach (var tunnel in tunnels)
        {
            Direction newDirection = tunnel.GetTurnedDirection();

            if (Random.Range(0, 1f) < changeToPlaceRoom)
            {
                Vector2Int newStartPoint = (tunnel.GetSegmentCenter() +
                                            (Vector2)newDirection.GetVector() * hallWayWidth / 2f).GetVector2Int();
                TryPlaceRoom(newStartPoint, newDirection);
            }

            newDirection = newDirection.GetOpposite();
            if (Random.Range(0, 1f) < changeToPlaceRoom)
            {
                Vector2Int newStartPoint = (tunnel.GetSegmentCenter() +
                                            (Vector2)newDirection.GetVector() * hallWayWidth / 2f).GetVector2Int();
                TryPlaceRoom(newStartPoint, newDirection);
            }
        }
    }

    private void TryPlaceRoom(Vector2Int from, Direction direction)
    {
        int roomWidth = Random.Range(roomMinSize, roomMaxSize);
        int roomLenght = Random.Range(roomMinSize, roomMaxSize);
        //room
        Tunnel newRoom = new Tunnel(from + direction.GetVector() * roomHallwayLenght, direction, roomWidth, roomLenght);
        //roomHallway
        Tunnel newHallway = new Tunnel(from, direction, roomHallwayWidth, roomHallwayLenght);

        if (CanBePlaced(newHallway) &&
            CanBePlaced(newRoom))
        {
            //Fill Room
            FillSpace(newRoom);

            //Fill hallway to Room 
            FillSpace(newHallway);
        }
    }

    private Tunnel PlaceFork(Tunnel curTunnel)
    {
        curTunnel = new Tunnel(curTunnel.GetEndEdgeCenter(), curTunnel.Direction, forkWidth,
            forkWidth);
        lenght += hallWayStepLength;
        return curTunnel;
    }


    private bool FillSpace(Tunnel tunnel)
    {
        if (!CanBePlaced(tunnel))
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

    private bool CanBePlaced(Tunnel tunnel)
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