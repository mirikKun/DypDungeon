using System.Collections;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Serialization;
using Random = UnityEngine.Random;

[ExecuteInEditMode]
[RequireComponent(typeof(SegmentDungeonPlacer))]
public class TunnelingGenerator : MonoBehaviour
{
    [SerializeField] private bool _randomSeed;
    [SerializeField,HideIf(nameof(_randomSeed))] 
    private int _seed = 4;
    [SerializeField] private  THallway[] _hallways;
    private  THallway currentHallway;
    [SerializeField] private float _changeToPlaceRoom = 0.53f;

    [SerializeField] private int _roomHallwayLenght = 2;
    [SerializeField] private int _roomHallwayWidth = 1;

    [SerializeField] private int _roomMinSize = 8;
    [SerializeField] private int _roomMaxSize = 16;

    [SerializeField] private int _minLenght = 110;
    [SerializeField] private int _maxLenght = 160;

    [SerializeField] private Vector2Int _size = new(100, 100);
    [SerializeField] private TStartPoint[] _startPoints;
    private int[,] grid;
    private List<Tunnel> tunnels = new List<Tunnel>();
    private List<Tunnel> tunnelBranches = new List<Tunnel>();
    private int lenght;
    private SegmentDungeonPlacer segmentDungeonPlacer;

    private int maxRestarts = 11;
    private int curRestarts = 0;


    [Button]
    void Generate()
    {
        ClearAll();

        if(!_randomSeed)
            Random.InitState(_seed);

        segmentDungeonPlacer = GetComponent<SegmentDungeonPlacer>();
        SetStartTunnels();
        while (curRestarts < maxRestarts && lenght < _minLenght)
        {
            Tunneling();
            if (lenght < _minLenght)
            {
                Restart();
                SetStartTunnels();
            }
        }

        //GenerateRooms();

        CreateDungeon();
    }

    private void CreateDungeon()
    {
        segmentDungeonPlacer.RemoveEverything();
        segmentDungeonPlacer.Place(grid);
    }

    private void SetStartTunnels()
    {
        foreach (var startPoint in _startPoints)
        {
            Vector2 curPoint = startPoint.startPoint;
            Direction newDirection = startPoint.startDirection;
            Tunnel curTunnel = new Tunnel(curPoint, newDirection, currentHallway.hallWayWidth, startPoint.startLenght);
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
        grid = new int[_size.y, _size.x];

        currentHallway = _hallways[0];
    }
    private void Tunneling()
    {
        bool tunneling = true;


        //FillTunnel
        while (tunneling && lenght < _maxLenght)
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

        if (Random.Range(0, 1f) < currentHallway.hallWayChanceToTurn)
        {
            curTunnel = PlaceFork(curTunnel);
            //FillFork
            bool tunneling = FillSpace(curTunnel);
            if (!tunneling)
            {
                return null;
            }

            Vector2 newStartPoint = curTunnel.GetEndSegmentCenter() +
                                    (Vector2)newDirection.GetVector() * currentHallway.forkWidth / 2f;
            curTunnel = new Tunnel(newStartPoint, newDirection, currentHallway.hallWayWidth, currentHallway.hallWayStepLength);
            lenght += currentHallway.forkWidth;
        }
        else
        {
            if (Random.Range(0, 1f) < currentHallway.changeToFork)
            {
                curTunnel = PlaceFork(curTunnel);
                //FillFork
                bool tunneling = FillSpace(curTunnel);
                if (!tunneling)
                {
                    return null;
                }

                Vector2 newStartPoint = curTunnel.GetEndSegmentCenter() +
                                        (Vector2)newDirection.GetVector() * currentHallway.forkWidth / 2f;
                Tunnel newTunnel = new Tunnel(newStartPoint, newDirection, currentHallway.hallWayWidth, currentHallway.hallWayStepLength);
                lenght += currentHallway.forkWidth;
                tunnelBranches.Add(newTunnel);
            }

            curTunnel = new Tunnel(curTunnel.GetEndEdgeCenter(), curTunnel.Direction, currentHallway.hallWayWidth, currentHallway.hallWayStepLength);
            lenght += currentHallway.hallWayStepLength;
        }

        return curTunnel;
    }

    private void GenerateRooms()
    {
        foreach (var tunnel in tunnels)
        {
            Direction newDirection = tunnel.GetTurnedDirection();

            if (Random.Range(0, 1f) < _changeToPlaceRoom)
            {
                Vector2Int newStartPoint = (tunnel.GetSegmentCenter() +
                                            (Vector2)newDirection.GetVector() * currentHallway.hallWayWidth / 2f).GetVector2Int();
                TryPlaceRoom(newStartPoint, newDirection);
            }

            newDirection = newDirection.GetOpposite();
            if (Random.Range(0, 1f) < _changeToPlaceRoom)
            {
                Vector2Int newStartPoint = (tunnel.GetSegmentCenter() +
                                            (Vector2)newDirection.GetVector() * currentHallway.hallWayWidth / 2f).GetVector2Int();
                TryPlaceRoom(newStartPoint, newDirection);
            }
        }
    }

    private void TryPlaceRoom(Vector2Int from, Direction direction)
    {
        int roomWidth = Random.Range(_roomMinSize, _roomMaxSize);
        int roomLenght = Random.Range(_roomMinSize, _roomMaxSize);
        //room
        Tunnel newRoom = new Tunnel(from + direction.GetVector() * _roomHallwayLenght, direction, roomWidth, roomLenght);
        //roomHallway
        Tunnel newHallway = new Tunnel(from, direction, _roomHallwayWidth, _roomHallwayLenght);

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
        curTunnel = new Tunnel(curTunnel.GetEndEdgeCenter(), curTunnel.Direction, currentHallway.forkWidth, currentHallway.forkWidth);
        lenght += currentHallway.hallWayStepLength;
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