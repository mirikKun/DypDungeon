using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using UnityEngine;
using UnityEngine.UIElements;
using Random = UnityEngine.Random;

[ExecuteInEditMode]
public class GraphDungeonGenerator : MonoBehaviour
{
    [SerializeField] private int roomSize = 10;
    [SerializeField] private int randomSeed = 10;
    public int roomCount = 13;
    [SerializeField] private float[] chances = { 0.45f, 0.84f, 1f, 1 };

    [SerializeField] private int corridorLenght = 10;
    [SerializeField] private float corridorWidth = 1;

    private int minRoomDistance = 18;
    [SerializeField] private bool cyclesAllowed = true;
    [SerializeField] private bool randomAngles = true;
    [SerializeField] private bool rightAngle = true;

    [SerializeField] private int corridorLenghtRange = 5;
    [SerializeField] private int roomSizeRange = 5;
    [SerializeField] private Transform camera;
    [SerializeField] private Color dungeonTextureColor = Color.black;
    [SerializeField] private Color backgroundTextureColor = Color.white;
    private DungeonPlacer dungeonPlacer;
    private GraphRoom[] rooms;

    private int iterations;

    private List<Chain> decomposedChains = new List<Chain>();
    private GraphRoom[] corridorSpaces;
    private List<GraphRoom> corridors;

    private bool CheckOnIterations()
    {
        iterations++;
        if (iterations > 550)
        {
            Debug.Log("Out of iterations");
            return true;
        }

        return false;
    }

    public int[,] graph = new int[,]
    {
        { 0, 1, 0, 0, 0, 0 },
        { 1, 0, 1, 0, 0, 0 },
        { 0, 1, 0, 1, 0, 0 },
        { 0, 0, 1, 0, 1, 0 },
        { 0, 0, 0, 1, 0, 1 },
        { 0, 0, 0, 0, 1, 0 },
    };


    void Start()
    {
        minRoomDistance = (int)Mathf.Ceil((corridorLenght + corridorLenghtRange - 1) * Mathf.Sqrt(2));
    }

    public void GenerateMatrix()
    {
        GraphGenerator graphGenerator = new GraphGenerator(chances, cyclesAllowed, randomSeed);
        graph = graphGenerator.GenerateGraph(roomCount);
    }

    public void GenerateDungeon()
    {
        roomCount = graph.GetLength(0);

        iterations = 0;

        ResetVariables();
        dungeonPlacer.RemoveEverything();
        var graphHelper = new GraphChainDecomposer(roomCount, graph);
        decomposedChains = graphHelper.GetChainsOfGraph();
        bool success = GenerateRooms();
        if (!success)
        {
            return;
        }
        //FindObjectOfType<TextureSetter>().SetTexturesByRooms(rooms, corridors.ToArray(),dungeonTextureColor,backgroundTextureColor);
    }

    public void SaveTexture()
    {
        TextureSetter.SavePng(rooms, corridors.ToArray(), dungeonTextureColor, backgroundTextureColor);
    }

    public bool CheckMatrix()
    {
        //check if matrix with right size
        if (roomCount != graph.GetLength(0))
        {
            return true;
        }

        FillMatrixBottom();
        //check if all rooms are reachable

        if (!GraphChecks.IsReachable(graph, 0))
        {
            return false;
        }

        return true;
    }

    private void FillMatrixBottom()
    {
        for (int y = 0; y < roomCount - 1; y++)
        {
            for (int x = y + 1; x < roomCount; x++)
            {
                graph[x, y] = graph[y, x];
            }
        }
    }

    private void ResetVariables()
    {
        decomposedChains = new List<Chain>();
        corridors = new List<GraphRoom>();
        dungeonPlacer = GetComponent<DungeonPlacer>();
    }

    private int GetCorridorIndex(int roomOrder, int chainIndex)
    {
        int corridorIndex = 0;

        for (int i = 0; i < chainIndex; i++)
        {
            corridorIndex += decomposedChains[i].completeCycle.Count - 1;
            if (i == 0 && decomposedChains[i].cycle)
            {
                corridorIndex++;
            }

            if (i > 0)
            {
                corridorIndex++;
                if (decomposedChains[i].cycle)
                {
                    corridorIndex++;
                }
            }
        }

        corridorIndex += roomOrder;
        if (chainIndex > 0)
        {
            corridorIndex++;
        }

        return corridorIndex;
    }

    private int GetCorridorLenght()
    {
        return corridorLenght + Random.Range(0, corridorLenghtRange);
    }

    private int GetRoomSize()
    {
        return roomSize + Random.Range(0, roomSizeRange);
    }

    private bool GenerateByChain(GraphRoom previousRoom, Chain chain, int roomOrder, int chainIndex)
    {
        if (CheckOnIterations())
        {
            return false;
        }

        int corridorIndex = GetCorridorIndex(roomOrder, chainIndex);


        int roomIndex = chain.completeCycle[roomOrder];

        Vector2Int lastPosition = new Vector2Int(previousRoom.left, previousRoom.bottom);

        bool lastInCycle = chain.cycle && roomOrder == chain.completeCycle.Count - 1;

        int randDirectionX = Random.Range(0, 4);

        int maxIterationsCount = 8;
        if (randomAngles)
        {
            maxIterationsCount = 20;
        }

        for (int i = 0; i < maxIterationsCount; i++)
        {
            int rotation;
            if (i < 4)
            {
                rotation = (randDirectionX + i) % 4;
            }
            else
            {
                rotation = (randDirectionX + i) % 4 + 4;
            }

            Vector2Int offset = new Vector2Int();
            if (randomAngles)
            {
                Vector2 newDirection = GenerateDirection();
                int newCorridorLenght = GetCorridorLenght();
                offset = new Vector2Int((int)(newDirection.x * newCorridorLenght),
                    (int)(newDirection.y * newCorridorLenght));
            }
            else
            {
                Vector2Int direction = GenerateDirection(rotation);
                offset = direction * GetCorridorLenght();
            }

            int width = GetRoomSize() - roomSize;
            int lenght = GetRoomSize() - roomSize;
            GraphRoom newRoom = new GraphRoom(-width / 2, roomSize + width / 2, -lenght / 2, roomSize + width / 2,
                offset + lastPosition);
            rooms[roomIndex].CopyPosition(newRoom);
            rooms[roomIndex].placed = true;
            List<GraphRoom> curCorridors = new List<GraphRoom>();
            corridorSpaces[corridorIndex - 1] = GenerateCorridor(newRoom, previousRoom);
            corridorSpaces[corridorIndex - 1].placed = true;
            corridorSpaces[corridorIndex - 1].index = roomCount + corridorIndex - 1;
            curCorridors.Add(corridorSpaces[corridorIndex - 1]);
            if (lastInCycle)
            {
                corridorSpaces[corridorIndex] = GenerateCorridor(newRoom, rooms[chain.exit]);
                corridorSpaces[corridorIndex].placed = true;
                corridorSpaces[corridorIndex].index = roomCount + corridorIndex;
                curCorridors.Add(corridorSpaces[corridorIndex]);
            }


            if (rooms[roomIndex].CanBePlacedWithRooms(rooms) &&
                rooms[roomIndex].CanBePlacedWithCorridors(corridorSpaces, curCorridors) &&
                CheckOnCycleSuccess(chain, roomOrder) &&
                corridorSpaces[corridorIndex - 1].CanBePlacedWithCorridors(corridorSpaces, new List<GraphRoom>()) &&
                corridorSpaces[corridorIndex - 1]
                    .CanBePlacedWithCorridors(rooms, new List<GraphRoom>() { rooms[roomIndex], previousRoom }) &&
                (!lastInCycle || corridorSpaces[corridorIndex]
                        .CanBePlacedWithCorridors(corridorSpaces, new List<GraphRoom>())
                    && corridorSpaces[corridorIndex].CanBePlacedWithCorridors(rooms,
                        new List<GraphRoom>() { rooms[roomIndex], rooms[chain.exit] })
                ))
            {
                if (roomOrder == chain.completeCycle.Count - 1)
                {
                    if (chainIndex == decomposedChains.Count - 1 || GenerateByChain(
                            rooms[decomposedChains[chainIndex + 1].enter], decomposedChains[chainIndex + 1], 0,
                            chainIndex + 1))
                    {
                        return true;
                    }

                    continue;
                }

                if (GenerateByChain(newRoom, chain, roomOrder + 1, chainIndex))
                {
                    return true;
                }
            }
        }

        rooms[chain.completeCycle[roomOrder]].placed = false;
        corridorSpaces[corridorIndex - 1] = null;
        if (lastInCycle)
        {
            corridorSpaces[corridorIndex] = null;
        }

        return false;
    }

    private Vector2Int GenerateDirection(int seed)
    {
        Vector2Int direction;
        if (seed == 0)
        {
            direction = Vector2Int.left;
        }
        else if (seed == 1)
        {
            direction = Vector2Int.up;
        }
        else if (seed == 2)
        {
            direction = Vector2Int.right;
        }
        else if (seed == 3)
        {
            direction = Vector2Int.down;
        }
        else if (seed == 4)
        {
            direction = new Vector2Int(-1, 1);
        }
        else if (seed == 5)
        {
            direction = new Vector2Int(1, 1);
        }
        else if (seed == 6)
        {
            direction = new Vector2Int(1, -1);
        }
        else
        {
            direction = new Vector2Int(-1, -1);
        }

        return direction;
    }

    private Vector2 GenerateDirection()
    {
        Vector2 direction = new Vector2(Random.Range(-1f, 1f), Random.Range(-1f, 1f));

        direction = direction.normalized;
        return direction;
    }

    private bool CheckOnCycleSuccess(Chain chain, int roomIndex)
    {
        if (roomIndex != chain.completeCycle.Count - 1)
        {
            return true;
        }

        if (!chain.cycle)
        {
            return true;
        }

        GraphRoom room1;

        room1 = rooms[chain.completeCycle[^1]];
        GraphRoom room2 = rooms[chain.exit];

        Vector2 room1Position = room1.GetPosition();
        Vector2 room2Position = room2.GetPosition();
        float distance = Vector2.Distance(room1Position, room2Position);

        bool closeEnough = distance < minRoomDistance;
        return closeEnough;
    }

    private bool GenerateRooms()
    {
        GraphRoom firstRoom = new GraphRoom(0, GetRoomSize(), 0, GetRoomSize());

        rooms = new GraphRoom[roomCount];
        for (int i = 0; i < roomCount; i++)
        {
            rooms[i] = new GraphRoom(i);
        }

        int edgeCount = 0;
        for (int i = 0; i < roomCount; i++)
        {
            for (int j = 0; j < roomCount; j++)
            {
                if (graph[i, j] == 1)
                {
                    rooms[i].connections.Add(rooms[j]);
                    rooms[i].placed = false;
                    if (j > i)
                    {
                        edgeCount++;
                    }
                }
            }
        }

        corridorSpaces = new GraphRoom[edgeCount];

        rooms[decomposedChains[0].completeCycle[0]] = firstRoom;
        firstRoom.index = decomposedChains[0].completeCycle[0];
        firstRoom.placed = true;
        iterations = 0;

        bool success = GenerateByChain(firstRoom, decomposedChains[0], 1, 0);
        if (!success)
        {
            Debug.Log("Can`t be placed");
            return false;
        }

        PlaceCamera();
        for (int i = 0; i < roomCount; i++)
        {
            if (rooms[i].placed)
            {
                dungeonPlacer.PlaceRoom(rooms[i], Vector3.zero, i);
                for (int j = i + 1; j < roomCount; j++)
                {
                    if (graph[i, j] == 1 && rooms[j].placed)
                    {
                        GraphRoom newRoom = GenerateCorridor(rooms[i], rooms[j]);
                        corridors.Add(newRoom);

                        dungeonPlacer.PlaceCorridor(newRoom,
                            Vector3.zero);
                    }
                }
            }
        }

        return true;
    }

    private GraphRoom GenerateCorridor(GraphRoom room1, GraphRoom room2)
    {
        GraphRoom newRoom;
        if (RoomAreDiagonal(room1, room2) || !rightAngle)
        {
            Vector2 scale = new Vector2(Vector2.Distance(room1.GetPosition(), room2.GetPosition()) - corridorWidth * 4,
                corridorWidth);
            Vector2 position = (room2.GetPosition() + room1.GetPosition()) / 2f;
            Vector2 direction = room1.GetPosition() - room2.GetPosition();
            float angle = Mathf.Atan2(direction.x, direction.y) * Mathf.Rad2Deg;
            newRoom = new GraphRoom(scale, position, 90 - angle);
        }
        else
        {
            newRoom = GenerateSimpleCorridor(room1, room2);
        }

        return newRoom;
    }


    private GraphRoom GenerateSimpleCorridor(Room firstRoom, Room secondRoom)
    {
        bool horizontal = firstRoom.left > secondRoom.right || firstRoom.right < secondRoom.left;

        float zPos;
        float lenght;
        float xPos;
        Vector3 corridorScale;
        if (horizontal)
        {
            zPos = GetCenterOfCovering(firstRoom.bottom, firstRoom.top, secondRoom.bottom, secondRoom.top);
            if (firstRoom.left > secondRoom.right)
            {
                lenght = firstRoom.left - secondRoom.right;
                xPos = secondRoom.right + lenght / 2;
            }
            else
            {
                lenght = secondRoom.left - firstRoom.right;
                xPos = firstRoom.right + lenght / 2;
            }

            corridorScale = new Vector3(lenght, 10, corridorWidth);
        }
        else
        {
            xPos = GetCenterOfCovering(firstRoom.left, firstRoom.right, secondRoom.left, secondRoom.right);
            if (firstRoom.bottom > secondRoom.top)
            {
                lenght = firstRoom.bottom - secondRoom.top;
                zPos = secondRoom.top + lenght / 2;
            }
            else
            {
                lenght = secondRoom.bottom - firstRoom.top;
                zPos = firstRoom.top + lenght / 2;
            }

            corridorScale = new Vector3(corridorWidth, 10, lenght);
        }

        return new GraphRoom(new Vector2(corridorScale.x, corridorScale.z), new Vector2(xPos, zPos), 0);
    }

    private float GetCenterOfCovering(int firstStart, int firstEnd, int secondStart, int secondEnd)
    {
        float center;
        if (firstStart < secondStart)
        {
            center = secondStart + GetLenghtOfCovering(firstStart, firstEnd, secondStart, secondEnd) / 2f;
        }
        else
        {
            center = firstStart + GetLenghtOfCovering(firstStart, firstEnd, secondStart, secondEnd) / 2f;
        }

        return center;
    }

    private int GetLenghtOfCovering(int firstStart, int firstEnd, int secondStart, int secondEnd)
    {
        int lenght;
        if (firstStart < secondStart)
        {
            if (firstEnd < secondEnd)
            {
                lenght = firstEnd - secondStart;
            }
            else
            {
                lenght = secondEnd - secondStart;
            }
        }
        else
        {
            if (firstEnd < secondEnd)
            {
                lenght = firstEnd - firstStart;
            }
            else
            {
                lenght = secondEnd - firstStart;
            }
        }

        return lenght;
    }

    private bool RoomAreDiagonal(Room room1, Room room2)
    {
        float offset = corridorWidth * 2f;
        bool rightTop = room1.right < room2.left + offset && room1.top < room2.bottom + offset;
        bool rightBottom = room1.right < room2.left + offset && room1.bottom > room2.top - offset;

        bool leftTop = room1.left > room2.right - offset && room1.top < room2.bottom + offset;
        bool leftBottom = room1.left > room2.right - offset && room1.bottom > room2.top - offset;

        return rightTop || rightBottom || leftTop || leftBottom;
    }

    private void PlaceCamera()
    {
        int minLeft = Int32.MaxValue,
            minBottom = Int32.MaxValue,
            maxRight = Int32.MinValue,
            maxTop = Int32.MinValue;
        foreach (var room in rooms)
        {
            if (room.left < minLeft)
            {
                minLeft = room.left;
            }

            if (room.right > maxRight)
            {
                maxRight = room.right;
            }

            if (room.bottom < minBottom)
            {
                minBottom = room.bottom;
            }

            if (room.top > maxTop)
            {
                maxTop = room.top;
            }
        }

        camera.position = new Vector3((minLeft + maxRight) / 2f, camera.position.y, (minBottom + maxTop) / 2f);
    }

    public void ClearAll()
    {
        if (!dungeonPlacer)
        {
            dungeonPlacer = GetComponent<DungeonPlacer>();
        }

        dungeonPlacer.RemoveEverything();
    }
}