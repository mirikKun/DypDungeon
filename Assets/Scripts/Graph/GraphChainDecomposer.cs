using System;
using System.Collections.Generic;
using UnityEngine;
public  class GraphChainDecomposer
{
    private int graphSize;
    private int[,] graph;
    private int iterations;

    private List<Chain> completedCycles = new List<Chain>();
    private List<Chain> completedNoCycleChains = new List<Chain>();
    private List<Chain> decomposedChains = new List<Chain>();
    private List<int> passedRooms = new List<int>();

    public GraphChainDecomposer(int newGraphSize,int[,] newGraph)
    {
        graphSize = newGraphSize;
        graph = newGraph;
    }
    
    //Get chains of graph starting with minimal cycles
    public List<Chain> GetChainsOfGraph()
    {
        bool[] visited = new bool[graphSize];
        int[] parent = new int[graphSize];
        List<int> cycleOrPath = new List<int>();
        int startIndex = 0;
        //Doing until all rooms will be in chains
        while (!PathIsFool())
        {
            if (DFS(startIndex, visited, parent, cycleOrPath))
            {
                return decomposedChains;            
            }
            ShortestChain();
            startIndex = decomposedChains[^1].completeCycle[0];
            visited = new bool[graphSize];
            passedRooms = GetAllPassedRooms();
            parent = new int[graphSize];
            cycleOrPath = new List<int>();
        }

        return  decomposedChains;
    }
    
    //Search for smallest cycle or chain
    private void ShortestChain()
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


    
     // рекурсивна функція для пошуку  циклу або ланцюга
    private bool DFS(int u, bool[] visited, int[] parent, List<int> cycleOrPath)
    {
        visited[u] = true;
        cycleOrPath.Add(u);


        for (int v = 0; v < graphSize; v++)
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
        bool[] roomsInPath = new bool[graphSize]; // создаем массив флагов для чисел от 0 до 10

        foreach (var chain in decomposedChains)
        {
            foreach (var num in chain.completeCycle)
            {
                roomsInPath[num] = true; // устанавливаем флаг для числа в массиве флагов
            }
        }

        // проверяем, есть ли какое-то число без флага
        for (int i = 0; i < graphSize; i++)
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
    private bool CheckIfEndRoom(int index)
    {
        int nearRoomCount = 0;
        for (int i = 0; i < graphSize; i++)
        {
            if (graph[index, i] == 1)
            {
                nearRoomCount++;
            }
        }

        return nearRoomCount == 1;
    }

    private List<Chain> DefaultGraph()
    {
        Debug.LogWarning("Can`t generate chains");

        List<int> defaultGraph = new List<int>();
        for (int i = 0; i < graphSize; i++)
        {
            defaultGraph.Add(i);
        }

        Chain defaultChain = new Chain(defaultGraph, false);
        return new List<Chain>{defaultChain};
    }
}