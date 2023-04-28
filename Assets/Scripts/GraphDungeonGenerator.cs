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
    [SerializeField] private int corridorLenght = 10;
    [SerializeField] private int minRoomDistance = 18;
    private DungeonPlacer dungeonPlacer;
    private GraphRoom[] rooms;
    private int iterations = 0;
    private List<Chain> completedCycles = new List<Chain>();
    private List<Chain> completedNoCycleChains = new List<Chain>();
    private List<Chain> decomposedChains = new List<Chain>();
    private List<int> passedRooms = new List<int>();
    private List<Room> corridors=new List<Room>();

    private bool CheckOnIterations()
    {
        iterations++;
        if (iterations > 150)
        {
            Debug.Log("_________Out of iterations__________-");
            return true;
        }

        return false;
    }

    // private int[,] graph = new int[,]
    // {
    //     { 0, 1, 0, 0, 1 },
    //     { 1, 0, 0, 1, 0 },
    //     { 0, 0, 0, 1, 0 },
    //     { 0, 1, 1, 0, 1 },
    //     { 1, 0, 0, 1, 0 }
    // };    
    // private int[,] graph = new int[,]
    // {
    //     { 0, 1, 0, 1, 0 },
    //     { 1, 0, 0, 1, 1 },
    //     { 0, 0, 0, 0, 1 },
    //     { 1, 1, 0, 0, 1 },
    //     { 0, 1, 1, 1, 0 }
    // };
    private int[,] graph = new int[,]
    {
        { 0, 1, 0, 0, 0, 1, 0, 0, 0, 0, 0, 0, 0 },
        { 1, 0, 1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 },
        { 0, 1, 0, 1, 0, 0, 1, 0, 0, 0, 0, 0, 0 },
        { 0, 0, 1, 0, 0, 0, 0, 1, 0, 0, 0, 0, 0 },
        { 0, 0, 0, 0, 0, 0, 0, 0, 1, 0, 0, 0, 0 },
        { 1, 0, 0, 0, 0, 0, 1, 0, 0, 0, 0, 0, 0 },
        { 0, 0, 1, 0, 0, 1, 0, 0, 0, 0, 1, 0, 0 },
        { 0, 0, 0, 1, 0, 0, 0, 0, 1, 0, 0, 1, 0 },
        { 0, 0, 0, 0, 1, 0, 0, 1, 0, 0, 0, 0, 1 },
        { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1, 0, 0 },
        { 0, 0, 0, 0, 0, 0, 1, 0, 0, 1, 0, 1, 0 },
        { 0, 0, 0, 0, 0, 0, 0, 1, 0, 0, 1, 0, 0 },
        { 0, 0, 0, 0, 0, 0, 0, 0, 1, 0, 0, 0, 0 }
    };

    void Start()
    {
       // Generate();
    }

    public void Generate()
    {
        ResetVariables();
        dungeonPlacer.RemoveEverything();
        Random.InitState(randomSeed);
        PrintMatrix(graph);
        int[,] generatedMatrix = GeneratePlanarGraphAdjacencyMatrix(roomCount);
        PrintMatrix(generatedMatrix);
        graph = generatedMatrix;
        ShortestCycleOrPath();
        GenerateRooms();
    }

    private void ResetVariables()
    {
        completedCycles = new List<Chain>();
        completedNoCycleChains = new List<Chain>();
        decomposedChains = new List<Chain>();
        passedRooms = new List<int>();
        corridors = new List<Room>();
        dungeonPlacer = GetComponent<DungeonPlacer>(); 
    }
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

        Debug.Log("Shortest cycle or path: ");

        //PrintMatrix(cycleOrPath.GetGraph());
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

            // if (!visited[startIndex])
            // {
            {
                if (DFS(startIndex, visited, parent, cycleOrPath))
                {
                    // знайдено цикл, повертаємо його
                    return;
                }
            }
            //  }

            ShortestChain();
            startIndex = decomposedChains[^1].completeCycle[0];
            visited = new bool[roomCount];
            passedRooms = GetAllPassedRooms();
            parent = new int[roomCount];
            cycleOrPath = new List<int>();
        }

        // якщо не знайдено цикл, повертаємо найкоротший ланцюг
        return;
    }

    public bool[] GetVisited(Chain chain)
    {
        bool[] visited = new bool[roomCount];

        for (int i = 0; i < chain.completeCycle.Count; i++)
        {
            visited[chain.completeCycle[i]] = true;
        }

        return visited;
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
                bool second = CoreFound();


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

                    //Debug.Log(string.Join(", ", completedCycle));

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

                    //Debug.Log(string.Join(", ", completedChain));

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


    private bool GenerateByChain(GraphRoom previousRoom, Chain chain, int roomOrder, int chainIndex)
    {
        int roomIndex = chain.completeCycle[roomOrder];
        Debug.Log("__Start " + roomOrder + " _ " + roomIndex);

        Vector2Int lastPosition = new Vector2Int(previousRoom.left, previousRoom.bottom);

        //int randDirectionX = Random.Range(0, 4);
        int randDirectionX = Random.Range(0, 8);

        for (int i = 0; i < 8; i++)
        {
            int rotations = (randDirectionX + i) % 8;


            Vector2Int direction = Vector2Int.left;
            if (rotations == 0)
            {
                direction = Vector2Int.up;
            }
            else if (rotations == 1)
            {
                direction = Vector2Int.right;
            }
            else if (rotations == 2)
            {
                direction = Vector2Int.down;
            }
            else if (rotations == 3)
            {
                direction = new Vector2Int(-1, 1);
            }
            else if (rotations == 5)
            {
                direction = new Vector2Int(1, 1);
            }
            else if (rotations == 6)
            {
                direction = new Vector2Int(1, -1);
            }
            else if (rotations == 7)
            {
                direction = new Vector2Int(-1, -1);
            }

            Vector2Int offset = direction * corridorLenght + lastPosition;
            GraphRoom newRoom = new GraphRoom(0, roomSize, 0, roomSize, offset);
            rooms[roomIndex].CopyPosition(newRoom);
            rooms[roomIndex].placed = true;
            if (rooms[roomIndex].CanBePlaced(rooms) && CheckOnCycleSuccess(chain, roomOrder))
            {
                Debug.Log("__Found " + roomOrder + " _ " + roomIndex);
                if (roomOrder == chain.completeCycle.Count - 1)
                {
                    if (chainIndex == decomposedChains.Count - 1 || GenerateByChain(
                            rooms[decomposedChains[chainIndex + 1].enter], decomposedChains[chainIndex + 1], 0,
                            chainIndex + 1))
                    {
                        return true;
                    }
                    else
                    {
                        continue;
                    }
                }

                if (GenerateByChain(newRoom, chain, roomOrder + 1, chainIndex))
                {
                    return true;
                }
            }
        }

        Debug.Log("______NOOO " + roomOrder + " _ " + roomIndex);
        rooms[chain.completeCycle[roomOrder]].placed = false;
        return false;
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
        GraphRoom firstRoom = new GraphRoom(0, roomSize, 0, roomSize);
        //dungeonPlacer.PlaceRoom(firstRoom, Vector3.zero);

        rooms = new GraphRoom[roomCount];
        for (int i = 0; i < roomCount; i++)
        {
            rooms[i] = new GraphRoom(i);
        }

        for (int i = 0; i < roomCount; i++)
        {
            for (int j = 0; j < roomCount; j++)
            {
                if (graph[i, j] == 1)
                {
                    rooms[i].connections.Add(rooms[j]);
                    rooms[i].placed = false;
                }
            }
        }


        rooms[decomposedChains[0].completeCycle[0]] = firstRoom;
        firstRoom.placed = true;
        iterations = 0;

        bool success = GenerateByChain(firstRoom, decomposedChains[0], 1, 0);
        if (!success)
        {
            Debug.Log("ALARM!!!!!!!!!!!!1");
        }

        // for (int i = 1; i < decomposedChains.Count; i++)
        // {
        //     success = GenerateByChain(rooms[decomposedChains[i].enter], decomposedChains[i], 0);
        //     if (!success)
        //     {
        //         Debug.Log("ALARM!!!!!!!!!!!!1");
        //     }
        // }
        //bool success = MakeConnection(firstRoom, 0);

        for (int i = 0; i < roomCount; i++)
        {
            if (rooms[i].placed)
            {
                dungeonPlacer.PlaceRoom(rooms[i], Vector3.zero, i);
                for (int j = i + 1; j < roomCount; j++)
                {
                    if (graph[i, j] == 1 && rooms[j].placed)
                    {
                        dungeonPlacer.PlaceCorridor(1, rooms[i].GetPosition(), rooms[j].GetPosition(), Vector3.zero);
                    }
                }
            }
        }
        // StartCoroutine(Placing());
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
                    dungeonPlacer.PlaceCorridor(1, rooms[decomposedChains[i].completeCycle[j]].GetPosition(),
                        rooms[decomposedChains[i].completeCycle[j + 1]].GetPosition(), Vector3.zero);
                }
                else if (decomposedChains[i].cycle)
                {
                    dungeonPlacer.PlaceCorridor(1, rooms[decomposedChains[i].completeCycle[j]].GetPosition(),
                        rooms[decomposedChains[i].exit].GetPosition(), Vector3.zero);
                }

                yield return new WaitForSeconds(0.5f);
            }

            if (i < decomposedChains.Count - 1)
            {
                dungeonPlacer.PlaceCorridor(1, rooms[decomposedChains[i + 1].enter].GetPosition(),
                    rooms[decomposedChains[i + 1].completeCycle[0]].GetPosition(), Vector3.zero);
            }

            yield return new WaitForSeconds(1.5f);
        }
    }

    // void OnDrawGizmos()
    // {
    //     Gizmos.color = Color.red;
    //     foreach (var r in rooms)
    //     {
    //         if (r.placed)
    //         {
    //             Vector3 roomPosition = new Vector3(r.left + (float)r.GetWidth() / 2, 0, r.bottom + (float)r.GetHeight() / 2);
    //             Gizmos.DrawWireSphere(this.transform.position+roomPosition,  minRoomDistance);
    //
    //         }
    //     }
    // }
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


    private int GenerateRandomVertices()
    {
        float[] chances = new[] { 0.35f, 0.74f, 0.93f, 1 };
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

        Debug.Log(finalValue);
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
                Debug.Log("????");
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
        } while (!IsReachable(matrix, 0));

        Debug.Log(" reach =  " + IsReachable(matrix, 0));
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

    bool IsReachable(int[,] matrix, int startVertex)
    {
        int n = matrix.GetLength(0);
        bool[] visited = new bool[n];

        // Запустимо алгоритм DFS
        DFS(matrix, visited, startVertex);

        // Перевіримо, чи були відвідані всі вершини
        for (int i = 0; i < n; i++)
        {
            if (!visited[i])
            {
                return false;
            }
        }

        return true;
    }

    void DFS(int[,] matrix, bool[] visited, int vertex)
    {
        visited[vertex] = true;
        int n = matrix.GetLength(0);

        for (int i = 0; i < n; i++)
        {
            if (matrix[vertex, i] == 1 && !visited[i])
            {
                DFS(matrix, visited, i);
            }
        }
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