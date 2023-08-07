using System;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

public class GraphGenerator
{
    private float cyclicity;
    private float deviation;
    private int iterations;

    public GraphGenerator(float cyclicity,float deviation,int seed)
    {
        Random.InitState(seed);
        this.cyclicity = cyclicity;
        this.deviation = deviation;

    }


    public static void PrintSyntaxMatrix(int[,] matrix)
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

    public int[,] GenerateGraph(int roomCount)
    {
        int[,] generatedMatrix = GenerateDiagonalMatrix(roomCount);
        AddDeviation(generatedMatrix);
        AddCyclicity(generatedMatrix);
        PrintSyntaxMatrix(generatedMatrix);
        return generatedMatrix;
    }


    private int[,] GenerateDiagonalMatrix(int size)
    {
        int[,] matrix = new int[size, size];
        for (int i = 0; i < size-1; i++)
        {
            matrix[i, i+1]=1;
                matrix[i+1, i]=1;
        }

        return matrix;
    }

    private void AddDeviation( int[,] matrix)
    {
        int size=matrix.GetLength(0);
        for (int i = 0; i < size-1; i++)
        {
            float chanceToDeviate = Random.Range(0, 1f);
            if (chanceToDeviate < deviation)
            {
                matrix[i, i+1]=0;
                matrix[i+1, i]=0;
                int randomRow = Random.Range(0, size);
                if (matrix[i, randomRow] == 1||i==randomRow)
                {
                    matrix[i, i+1]=1;
                    matrix[i+1, i]=1;
                    i--;
                }
                else
                {
                    matrix[i, randomRow] = 1;
                    matrix[randomRow, i] = 1;
                    if (!GraphChecks.IsReachable(matrix, 0))
                    {
                        matrix[i, i+1]=1;
                        matrix[i+1, i]=1;
                        matrix[i, randomRow] = 0;
                        matrix[randomRow, i] = 0;
                        i--;

                    }
                }
            }
        }
    }
    private void AddCyclicity( int[,] matrix)
    {
        int size=matrix.GetLength(0);
        for (int i = 0; i < size-1; i++)
        {
            float chanceToCycle = Random.Range(0, 1f);
            if (chanceToCycle <cyclicity)
            {
                int randomRow = Random.Range(0, size);
                if ( i != randomRow)
                {
                    matrix[i, randomRow] = 1;
                    matrix[randomRow, i] = 1;
                }
            }
        }
    }

    private bool CheckOnIterations()
    {
        iterations++;
        if (iterations > 2450)
        {
            Debug.Log("Out of iterations");
            return true;
        }

        return false;
    }
}