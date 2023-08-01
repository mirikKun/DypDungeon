using System;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

public class GraphGenerator
{
    private float[] chances;
    private bool cyclesAllowed;
    private int iterations;

    public GraphGenerator(float[] chances,bool cyclesAllowed,int seed)
    {
        this.chances = chances;
        this.cyclesAllowed = cyclesAllowed;
        Random.InitState(seed);

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

    public int[,] GenerateGraph(int roomCount)
    {
        int[,] generatedMatrix = GeneratePlanarGraphAdjacencyMatrix(roomCount);
        PrintSyntaxMatrix(generatedMatrix);
        return generatedMatrix;
    }
    private int[,] GeneratePlanarGraphAdjacencyMatrix(int n)
    {
        int[,] matrix = new int[n, n];

        int[] verticesInRows = new int[n];


        // Генеруємо ребра

        int maxIterations = n * 4;

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
}