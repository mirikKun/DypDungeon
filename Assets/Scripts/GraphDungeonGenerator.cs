using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using UnityEngine;
using Random = UnityEngine.Random;

[ExecuteInEditMode]
public class GraphDungeonGenerator : MonoBehaviour
{
    [SerializeField] private int roomSize = 10;
    [SerializeField] private int randomSeed = 10;
    [SerializeField] private int roomCount = 13;
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
    private DungeonPlacer dungeonPlacer;
    private GraphRoom[] rooms;
    private int iterations;
    private List<Chain> completedCycles = new List<Chain>();
    private List<Chain> completedNoCycleChains = new List<Chain>();
    private List<Chain> decomposedChains = new List<Chain>();
    private List<int> passedRooms = new List<int>();
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


    private int[,] graph = new int[,]
    {
        { 0, 0, 0, 0, 1, 1 },
        { 0, 0, 0, 1, 0, 0 },
        { 0, 0, 0, 1, 0, 1 },
        { 0, 1, 1, 0, 0, 1 },
        { 1, 0, 0, 0, 0, 0 },
        { 1, 0, 1, 1, 0, 0 },
    };


    void Start()
    {
        minRoomDistance = (int)Mathf.Ceil((corridorLenght + corridorLenghtRange - 1) * Mathf.Sqrt(2));
    }

    public void Generate()
    {
        iterations = 0;

        ResetVariables();
        dungeonPlacer.RemoveEverything();
        Random.InitState(randomSeed);
        PrintMatrix(graph);
        int[,] generatedMatrix = GeneratePlanarGraphAdjacencyMatrix(roomCount);
        PrintSyntaxMatrix(generatedMatrix);
        graph = generatedMatrix;
        ShortestCycleOrPath();
        GenerateRooms();

        //SetDungeonGeneration(FindObjectOfType<Dungeon3DGenerator>());
        //FindObjectOfType<Dungeon3DGenerator>().GenerateMesh();
        FindObjectOfType<TextureSetter>().SetTexturesByRooms(rooms, corridors.ToArray());
    }

    public void SetDungeonGeneration(Dungeon3DGenerator generator)
    {
        generator.ClearMeshes();
        for (int i = 0; i < roomCount; i++)
        {
            if (rooms[i].placed)
            {
                generator.PlaceRoom(rooms[i], Vector3.zero, i);
            }
        }

        for (int i = 0; i < roomCount; i++)
        {
            if (rooms[i].placed)
            {
                for (int j = i + 1; j < roomCount; j++)
                {
                    if (graph[i, j] == 1 && rooms[j].placed)
                    {
                        generator.PlaceCorridor(corridorWidth, rooms[i].GetPosition(), rooms[j].GetPosition(),
                            Vector3.zero);
                    }
                }
            }
        }
    }


    private void ResetVariables()
    {
        completedCycles = new List<Chain>();
        completedNoCycleChains = new List<Chain>();
        decomposedChains = new List<Chain>();
        passedRooms = new List<int>();
        corridors = new List<GraphRoom>();
        dungeonPlacer = GetComponent<DungeonPlacer>();
    }

    //Пошук найкоротшого ланцюга
    public void ShortestChain()
    {
        int minLenght = Int32.MaxValue;

        int minIndex = -1;
        List<Chain> completedPaths;
        bool cycle = completedCycles.Count > 0;
        if (cycle)
        {
            completedPaths = new List<Chain>(completedCycles);
        }
        else
        {
            completedPaths = new List<Chain>(completedNoCycleChains);
        }

        for (int i = 0; i < completedPaths.Count; i++)
        {
            if (completedPaths[i].Count() < minLenght)
            {
                minLenght = completedPaths[i].Count();
                minIndex = i;
            }
        }

        Chain cycleOrPath = completedPaths[minIndex];
        decomposedChains.Add(cycleOrPath);
        completedCycles = new List<Chain>();
        completedNoCycleChains = new List<Chain>();
    }

    public void ShortestCycleOrPath()
    {
        bool[] visited = new bool[roomCount];
        int[] parent = new int[roomCount];
        List<int> cycleOrPath = new List<int>();
        int startIndex = 0;
        while (!PathIsFool())
        {
            if (CheckOnIterations())
            {
                return;
            }

            if (DFS(startIndex, visited, parent, cycleOrPath))
            {
                // знайдено цикл, повертаємо його
                return;
            }

            ShortestChain();
            startIndex = decomposedChains[^1].completeCycle[0];
            visited = new bool[roomCount];
            passedRooms = GetAllPassedRooms();
            parent = new int[roomCount];
            cycleOrPath = new List<int>();
        }
    }

    // рекурсивна функція для пошуку циклу або ланцюга
    private bool DFS(int u, bool[] visited, int[] parent, List<int> cycleOrPath)
    {
        visited[u] = true;
        cycleOrPath.Add(u);


        for (int v = 0; v < roomCount; v++)
        {
            if (graph[u, v] != 0)
            {
                if (!visited[v] && IsBackToVisited(u, v))
                {
                    parent[v] = u + 1;
                    if (DFS(v, visited, parent, cycleOrPath))
                        return true;
                }
                else if (v != parent[u] - 1 && IsOuter(u))
                {
                    List<int> completedCycle = new List<int>(cycleOrPath);
                    int enterRoomIndex = -1;
                    int exitRoomIndex = v;
                    while ((!CoreFound() && completedCycle[0] != v) || !IsOuter(completedCycle[0]))
                    {
                        enterRoomIndex = completedCycle[0];

                        completedCycle.RemoveAt(0);
                    }


                    Chain newChain = new Chain(completedCycle, true);
                    newChain.enter = enterRoomIndex;
                    newChain.exit = exitRoomIndex;
                    completedCycles.Add(newChain);
                }
                else if (CheckIfEndRoom(u) && IsOuter(u))
                {
                    List<int> completedChain = new List<int>(cycleOrPath);
                    int enterRoomIndex = -1;

                    while (!IsOuter(completedChain[0]))
                    {
                        enterRoomIndex = completedChain[0];
                        completedChain.RemoveAt(0);
                    }


                    Chain newChain = new Chain(completedChain, false);
                    newChain.enter = enterRoomIndex;

                    completedNoCycleChains.Add(newChain);
                }
            }
        }

        cycleOrPath.RemoveAt(cycleOrPath.Count - 1);
        visited[u] = false;
        parent[u] = 0;
        return false;
    }

    private bool PathIsFool()
    {
        bool[] roomsInPath = new bool[roomCount]; // создаем массив флагов для чисел от 0 до 10

        foreach (var chain in decomposedChains)
        {
            foreach (var num in chain.completeCycle)
            {
                roomsInPath[num] = true; // устанавливаем флаг для числа в массиве флагов
            }
        }

        // проверяем, есть ли какое-то число без флага
        for (int i = 0; i < roomCount; i++)
        {
            if (!roomsInPath[i])
            {
                return false; // число не найдено, возвращаем false
            }
        }

        return true; // все числа найдены, возвращаем true
    }

    private List<int> GetAllPassedRooms()
    {
        List<int> result = new List<int>();
        HashSet<int> set = new HashSet<int>();

        foreach (var chain in decomposedChains)
        {
            foreach (int roomIndex in chain.completeCycle)
            {
                if (!set.Contains(roomIndex))
                {
                    result.Add(roomIndex);
                    set.Add(roomIndex);
                }
            }
        }

        return result;
    }

    private bool CheckIfEndRoom(int index)
    {
        int nearRoomCount = 0;
        for (int i = 0; i < roomCount; i++)
        {
            if (graph[index, i] == 1)
            {
                nearRoomCount++;
            }
        }

        return nearRoomCount == 1;
    }

    private bool CoreFound()
    {
        return decomposedChains.Count > 0;
    }

    private bool IsBackToVisited(int cur, int next)
    {
        bool backToVisited = IsOuter(cur) && !IsOuter(next);

        return !CoreFound() || !backToVisited;
    }

    private bool IsOuter(int roomIndex)
    {
        if (CoreFound())
        {
            foreach (var index in passedRooms)
            {
                if (roomIndex == index)
                {
                    return false;
                }
            }
        }

        return true;
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

        //int randDirectionX = Random.Range(0, 4);
        int randDirectionX = Random.Range(0, 4);

        for (int i = 0; i < 8; i++)
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
            corridorSpaces[corridorIndex - 1] = GenerateCorridor(newRoom,previousRoom);
            corridorSpaces[corridorIndex - 1].placed = true;
            corridorSpaces[corridorIndex - 1].index =roomCount+ corridorIndex - 1;
            curCorridors.Add(corridorSpaces[corridorIndex - 1]);
            if (lastInCycle)
            {
                corridorSpaces[corridorIndex] = GenerateCorridor(newRoom, rooms[chain.exit]);
                corridorSpaces[corridorIndex].placed = true;
                corridorSpaces[corridorIndex].index =roomCount+ corridorIndex;
                curCorridors.Add(corridorSpaces[corridorIndex]);

            }

        
            if (rooms[roomIndex].CanBePlacedWithRooms(rooms)&&rooms[roomIndex].CanBePlacedWithCorridors(corridorSpaces,curCorridors) && CheckOnCycleSuccess(chain, roomOrder) &&
                corridorSpaces[corridorIndex - 1].CanBePlacedWithCorridors(corridorSpaces,new List<GraphRoom>()) &&
                corridorSpaces[corridorIndex - 1].CanBePlacedWithCorridors(rooms,new List<GraphRoom>(){ rooms[roomIndex],previousRoom}) &&
                (!lastInCycle || corridorSpaces[corridorIndex].CanBePlacedWithCorridors(corridorSpaces,new List<GraphRoom>())
                    && corridorSpaces[corridorIndex].CanBePlacedWithCorridors(rooms,new List<GraphRoom>(){ rooms[roomIndex], rooms[chain.exit]})
                    ))
               // ddd
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

    private void GenerateRooms()
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
                        GraphRoom newRoom = GenerateCorridor(rooms[i],rooms[j]);
                        corridors.Add(newRoom);
             
                        dungeonPlacer.PlaceCorridor(newRoom,
                                 Vector3.zero);
                   
                    }
                }
            }
        }
        // StartCoroutine(Placing());
    }

    private GraphRoom GenerateCorridor(GraphRoom room1, GraphRoom room2)
    {
        GraphRoom newRoom;
        if (RoomAreDiagonal(room1, room2) || !rightAngle)
        {
            Vector2 scale = new Vector2(Vector2.Distance(room1.GetPosition(),room2.GetPosition())-corridorWidth*2, corridorWidth);               
            Vector2 position = (room2.GetPosition() + room1.GetPosition())/2f;     
            Vector2 direction = room1.GetPosition() - room2.GetPosition();
            float angle = Mathf.Atan2(direction.x, direction.y) * Mathf.Rad2Deg;
            newRoom = new GraphRoom(scale, position, 90-angle);

            Debug.Log(scale);
        }
        else
        {
            newRoom =GenerateSimpleCorridor(room1, room2);
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

        //dungeonPlacer.PlaceCorridor(corridorScale, xPos, zPos, Vector3.zero);
        //corridors.Add(new GraphRoom(new Vector2(corridorScale.x,corridorScale.z),new Vector2(xPos,zPos),90));
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
        bool rightTop = room1.right < room2.left + corridorWidth && room1.top < room2.bottom + corridorWidth;
        bool rightBottom = room1.right < room2.left + corridorWidth && room1.bottom > room2.top - corridorWidth;

        bool leftTop = room1.left > room2.right - corridorWidth && room1.top < room2.bottom + corridorWidth;
        bool leftBottom = room1.left > room2.right - corridorWidth && room1.bottom > room2.top - corridorWidth;

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

    private IEnumerator Placing()
    {
        for (int i = 0; i < decomposedChains.Count; i++)
        {
            for (int j = 0; j < decomposedChains[i].completeCycle.Count; j++)
            {
                if (rooms[decomposedChains[i].completeCycle[j]].placed)
                {
                    dungeonPlacer.PlaceRoom(rooms[decomposedChains[i].completeCycle[j]], Vector3.zero,
                        decomposedChains[i].completeCycle[j]);
                }
                else
                {
                    dungeonPlacer.PlaceRoom(rooms[decomposedChains[i].completeCycle[j]], Vector3.down * 10,
                        decomposedChains[i].completeCycle[j]);
                }

                yield return new WaitForSeconds(0.5f);
                if (j < decomposedChains[i].completeCycle.Count - 1)
                {
                    dungeonPlacer.PlaceCorridor(corridorWidth,
                        rooms[decomposedChains[i].completeCycle[j]].GetPosition(),
                        rooms[decomposedChains[i].completeCycle[j + 1]].GetPosition(), Vector3.zero);
                }
                else if (decomposedChains[i].cycle)
                {
                    dungeonPlacer.PlaceCorridor(corridorWidth,
                        rooms[decomposedChains[i].completeCycle[j]].GetPosition(),
                        rooms[decomposedChains[i].exit].GetPosition(), Vector3.zero);
                }

                yield return new WaitForSeconds(0.5f);
            }

            if (i < decomposedChains.Count - 1)
            {
                dungeonPlacer.PlaceCorridor(corridorWidth, rooms[decomposedChains[i + 1].enter].GetPosition(),
                    rooms[decomposedChains[i + 1].completeCycle[0]].GetPosition(), Vector3.zero);
            }

            yield return new WaitForSeconds(1.5f);
        }
    }

    private void PrintMatrix(int[,] matrix)
    {
        string strMatrix = "";
        for (int i = 0; i < matrix.GetLength(0); i++)
        {
            for (int j = 0; j < matrix.GetLength(1); j++)
            {
                strMatrix += (matrix[i, j] + " ");
            }

            strMatrix += '\n';
        }

        Debug.Log(strMatrix);
    }

    private void PrintSyntaxMatrix(int[,] matrix)
    {
        string output = "{\n";
        for (int i = 0; i < matrix.GetLength(0); i++)
        {
            output += "    { ";
            for (int j = 0; j < matrix.GetLength(1); j++)
            {
                output += matrix[i, j];
                if (j < matrix.GetLength(1) - 1)
                    output += ", ";
            }

            output += " },\n";
        }

        output += "};";

        Debug.Log(output);
    }


    private int GenerateRandomVertices()
    {
        int[] vertices = new[] { 1, 2, 3, 4 };
        float rand = Random.Range(0, 1f);
        int finalValue = 0;
        if (rand < chances[0])
        {
            finalValue = vertices[0];
        }
        else if (rand < chances[1])
        {
            finalValue = vertices[1];
        }
        else if (rand < chances[2])
        {
            finalValue = vertices[2];
        }
        else
        {
            finalValue = vertices[3];
        }

        return finalValue;
    }

    private int[,] GeneratePlanarGraphAdjacencyMatrix(int n)
    {
        int[,] matrix = new int[n, n];

        int[] verticesInRows = new int[n];


        // Генеруємо ребра

        int maxIterations = n * 2;

        do
        {
            // Ініціалізуємо матрицю суміжності нулями
            for (int i = 0; i < n; i++)
            {
                for (int j = 0; j < n; j++)
                {
                    matrix[i, j] = 0;
                }

                verticesInRows[i] = GenerateRandomVertices();
            }

            if (CheckOnIterations())
            {
                Debug.Log("Out of iterations");
                return null;
            }

            for (int i = 0; i < n; i++)
            {
                // Випадково обираємо кількість ребер для поточної вершини
                int curEdgesCount = CheckCountInRow(matrix, i);
                // Генеруємо випадкові вершини, з якими буде з'єднана поточна вершина
                List<int> connectedVertices = new List<int>();
                int curIteration = 0;
                while (connectedVertices.Count < verticesInRows[i] - curEdgesCount)
                {
                    int vertex = Random.Range(0, n);
                    if (vertex != i && !connectedVertices.Contains(vertex) &&
                        (CheckCountInRow(matrix, vertex) < verticesInRows[vertex] || curIteration > maxIterations))
                    {
                        connectedVertices.Add(vertex);
                    }

                    curIteration++;
                }

                // Записуємо з'єднання у матрицю суміжності
                foreach (int j in connectedVertices)
                {
                    matrix[i, j] = 1;
                    matrix[j, i] = 1;
                }
            }
        } while (!GraphChecks.IsReachable(matrix, 0));

        Debug.Log(" reach =  " + GraphChecks.IsReachable(matrix, 0));
        PrintMatrix(matrix);
        if (!cyclesAllowed)
        {
            matrix = GraphChecks.RemoveCycles(matrix);
        }

        return matrix;
    }

    private int CheckCountInRow(int[,] matrix, int row)
    {
        int count = 0;
        for (int col = 0; col < matrix.GetLength(1); col++)
        {
            if (matrix[row, col] == 1)
            {
                count++;
            }
        }

        return count;
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