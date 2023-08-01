using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GraphChecks
{
    static int lastVisitedNode, lastParentNode;
    
    public static bool CheckMatrix(int roomCount,int[,] matrix)
    {
        //check if matrix with right size
        if (roomCount != matrix.GetLength(0))
        {
            return true;
        }
        //check if all rooms are reachable

        if (!GraphChecks.IsReachable(matrix, 0))
        {
            return false;
        }

        return true;
    }
    public static bool IsReachable(int[,] matrix, int startVertex)
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
    static void DFS(int[,] matrix, bool[] visited, int vertex)
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
    public static  int[,] RemoveCycles(int[,] adjacencyMatrix)
    {
        int n = adjacencyMatrix.GetLength(0);
        bool[] visited = new bool[n];
        DFSSearchCycle(adjacencyMatrix, visited, 0, -1);
        // for (int i = 0; i < n; i++)
        // {
        //     if (!visited[i])
        //     {
        //         if ()
        //         {
        //             // Ми знайшли цикл, видаляємо останнє знайдене ребро
        //             int u = lastVisitedNode;
        //             int v = lastParentNode;
        //             adjacencyMatrix[u, v] = 0;
        //             adjacencyMatrix[v, u] = 0;
        //         }
        //     }
        // }

        return adjacencyMatrix;
    }
    static private bool DFSSearchCycle(int[,] adjacencyMatrix, bool[] visited, int node, int parent)
    {
        visited[node] = true;
    
        for (int i = 0; i < adjacencyMatrix.GetLength(1); i++)
        {
            if (adjacencyMatrix[node, i] == 1)
            {
                if (!visited[i])
                {
                    if (DFSSearchCycle(adjacencyMatrix, visited, i, node))
                        return true;
                }
                else if (i != parent)
                {
                    // Ми знайшли цикл, зберігаємо останнє знайдене ребро
                    lastVisitedNode = node;
                    lastParentNode = i;
                    adjacencyMatrix[lastVisitedNode, lastParentNode] = 0;
                    adjacencyMatrix[lastParentNode, lastVisitedNode] = 0;
                    //return true;
                }
            }
        }
    
        return false;
    }
}
