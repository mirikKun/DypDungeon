using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class ArrayHelper 
{
    public static int[,] GetCopy(this int[,] array)
    {
        int[,] copy = new int[array.GetLength(0), array.GetLength(1)];
        for (int i = 0; i < array.GetLength(0); i++)
        {
            for (int j = 0; j < array.GetLength(1); j++)
            {
                copy[i, j] = array[i, j];
            }
        }

        return copy;
    }
}
