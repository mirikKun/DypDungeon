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
    [SerializeField] private int roomHallwayLenght = 2;
    [SerializeField] private int roomHallwayWidth = 1;
    [SerializeField] private int minLenght = 30;
    [SerializeField] private int maxLenght = 80;
    [SerializeField] private int roomMinSize = 3;
    [SerializeField] private int roomMaxSize = 9;

    [SerializeField] private Vector2Int size = new(100, 100);
    [SerializeField] private int seed = 0;

    private int[,] grid;
    private List<Tunnel> tunnels = new List<Tunnel>();
    private int lenght;
    private SegmentDungeonPlacer segmentDungeonPlacer;

    private int maxRestarts = 11;
    private int curRestarts = 0;

    [ContextMenu("Generate")]
    void Generate()
    {
        curRestarts = 0;
        lenght = 0;
        tunnels.Clear();
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

        PlaceRooms();

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
            tunnels.Clear();
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
                
                tunnels.Add(curTunnel);
                Direction newDirection = curTunnel.GetTurnedDirection();

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
            }
        }
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
        Tunnel newRoom = new Tunnel(from + direction.GetVector()*roomHallwayLenght, direction, roomWidth, roomLenght);
       //roomHallway
       Tunnel newHallway = new Tunnel(from , direction, roomHallwayWidth, roomHallwayLenght);

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