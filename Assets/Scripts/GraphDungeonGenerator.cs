using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

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


    private int[,] graph = new int[,]
    {
        { 0, 1, 0, 0, 1 },
        { 1, 0, 0, 1, 0 },
        { 0, 0, 0, 1, 0 },
        { 0, 1, 1, 0, 1 },
        { 1, 0, 0, 1, 0 }
    };
    // private int[,] graph = new int[,]
    // {
    //     { 0, 1, 0, 0, 0, 1, 0, 0, 0, 0, 0, 0, 0 },
    //     { 1, 0, 1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 },
    //     { 0, 1, 0, 1, 0, 0, 1, 0, 0, 0, 0, 0, 0 },
    //     { 0, 0, 1, 0, 0, 0, 0, 1, 0, 0, 0, 0, 0 },
    //     { 0, 0, 0, 0, 0, 0, 0, 0, 1, 0, 0, 0, 0 },
    //     { 1, 0, 0, 0, 0, 0, 1, 0, 0, 0, 0, 0, 0 },
    //     { 0, 0, 1, 0, 0, 1, 0, 0, 0, 0, 1, 0, 0 },
    //     { 0, 0, 0, 1, 0, 0, 0, 0, 1, 0, 0, 1, 0 },
    //     { 0, 0, 0, 0, 1, 0, 0, 1, 0, 0, 0, 0, 1 },
    //     { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1, 0, 0 },
    //     { 0, 0, 0, 0, 0, 0, 1, 0, 0, 1, 0, 1, 0 },
    //     { 0, 0, 0, 0, 0, 0, 0, 1, 0, 0, 1, 0, 0 },
    //     { 0, 0, 0, 0, 0, 0, 0, 0, 1, 0, 0, 0, 0 }
    // };

    void Start()
    {
        Random.InitState(randomSeed);
        dungeonPlacer = GetComponent<DungeonPlacer>();
        PrintMatrix(graph);
        Debug.Log("______");
        PrintMatrix(GenerateDungeon());
        graph = GenerateDungeon();
        ShortestCycleOrPath();
        GenerateRooms();
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
        Debug.Log("___________");

        Debug.Log("Shortest cycle or path: ");
        Debug.Log(string.Join(" ", cycleOrPath.completeCycle));

        //PrintMatrix(cycleOrPath.GetGraph());
        completedCycles = new List<Chain>();
        completedNoCycleChains = new List<Chain>();
    }

    public List<int> ShortestCycleOrPath()
    {
        bool[] visited = new bool[roomCount];
        int[] parent = new int[roomCount];
        List<int> cycleOrPath = new List<int>();
        int startIndex = 0;
        while (!PathIsFool())
        {
            // if (!visited[startIndex])
            // {
            {
                if (DFS(startIndex, visited, parent, cycleOrPath))
                {
                    // знайдено цикл, повертаємо його
                    return cycleOrPath;
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
        return cycleOrPath;
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


    private bool GenerateByChain(GraphRoom previousRoom, Chain chain, int roomOrder)
    {
        Vector2Int lastPosition = new Vector2Int(previousRoom.left, previousRoom.bottom);

        int randDirectionX = Random.Range(0, 4);

        for (int i = 0; i < 4; i++)
        {
            int rotations = (randDirectionX + i) % 4;


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

            Vector2Int offset = direction * corridorLenght + lastPosition;
            GraphRoom newRoom = new GraphRoom(0, roomSize, 0, roomSize, offset);
            int roomIndex = chain.completeCycle[roomOrder];
            rooms[roomIndex].CopyPosition(newRoom);
            rooms[roomIndex].placed = true;
            
            if (rooms[roomIndex].CanBePlaced(rooms) && CheckOnCycleSuccess(chain, roomOrder))
            {
                Debug.Log("__Found");
                if (roomOrder == chain.completeCycle.Count - 1)
                {
                    return true;
                }

                if (GenerateByChain(newRoom, chain, roomOrder + 1))
                {
                    return true;
                }
            }
        }

        Debug.Log("______NOOO");
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

        GenerateByChain(firstRoom, decomposedChains[0], 1);
        for (int i = 1; i < decomposedChains.Count; i++)
        {
            bool success= GenerateByChain(rooms[decomposedChains[i].enter], decomposedChains[i], 0);
            if (!success)
            {
                Debug.Log("ALARM!!!!!!!!!!!!1");

            }
        }
        //bool success = MakeConnection(firstRoom, 0);

        for (int i = 0; i < roomCount; i++)
        {
            if (rooms[i].placed)
            {
                dungeonPlacer.PlaceRoom(rooms[i], Vector3.zero, i + 1);
                for (int j = i + 1; j < roomCount; j++)
                {
                    if (graph[i, j] == 1 && rooms[j].placed)
                    {
                        dungeonPlacer.PlaceCorridor(1, rooms[i].GetPosition(), rooms[j].GetPosition(), Vector3.zero);
                    }
                }
            }
        }
        //StartCoroutine(Placing());
    }

    private IEnumerator Placing()
    {
        for (int i = 0; i < decomposedChains.Count; i++)
        {
            for (int j = 0; j < decomposedChains[i].completeCycle.Count; j++)
            {
                dungeonPlacer.PlaceRoom(rooms[decomposedChains[i].completeCycle[j]], Vector3.zero, decomposedChains[i].completeCycle[j]);
                yield return new WaitForSeconds(0.5f);
                if (j < decomposedChains[i].completeCycle.Count - 1)
                {
                    dungeonPlacer.PlaceCorridor(1, rooms[decomposedChains[i].completeCycle[j]].GetPosition(), rooms[decomposedChains[i].completeCycle[j+1]].GetPosition(), Vector3.zero);
                }
                else if(decomposedChains[i].cycle)
                {
                    dungeonPlacer.PlaceCorridor(1, rooms[decomposedChains[i].completeCycle[j]].GetPosition(), rooms[decomposedChains[i].exit].GetPosition(), Vector3.zero);
                }
                yield return new WaitForSeconds(0.5f);

            }

            if (i < decomposedChains.Count - 1)
            {
                dungeonPlacer.PlaceCorridor(1, rooms[decomposedChains[i+1].enter].GetPosition(), rooms[decomposedChains[i+1].completeCycle[0]].GetPosition(), Vector3.zero);
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

    public int[,] GenerateDungeon()
    {
        int[,] adjacencyMatrix = new int[roomCount, roomCount];

        // для каждой вершины генерируем случайное количество соседей
        for (int i = 0; i < roomCount; i++)
        {
            int numNeighbors = Random.Range(0, 4); // случайное число соседей от 0 до 4

            // выбираем случайные вершины из графа в качестве соседей
            List<int> neighbors = new List<int>();
            while (neighbors.Count < numNeighbors)
            {
                int neighbor = Random.Range(0, roomCount);
                if (neighbor != i && !neighbors.Contains(neighbor))
                {
                    neighbors.Add(neighbor);
                }
            }

            // создаем ребра в матрице смежности для выбранных соседей
            for (int j = 0; j < numNeighbors; j++)
            {
                adjacencyMatrix[i, neighbors[j]] = 1;
                adjacencyMatrix[neighbors[j], i] = 1;
            }
        }

        // Повертаємо згенеровану матрицю суміжностей
        return adjacencyMatrix;
    }
}