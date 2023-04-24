using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Chain
{
    public List<int> completeCycle = new List<int>();
    public int[,] graph = new int[,] { };
    public bool cycle;
    public int enter=-1;
    public int exit=-1;
    public Chain(List<int> completeCycle,bool cycle)
    {
        this.completeCycle = new List<int>(completeCycle);
        this.cycle = cycle;
    }

    public int Count()
    {
        return completeCycle.Count;
    }

    public bool CheckOnSameChain(List<Chain> chains)
    {
        foreach (var chain in chains)
        {
        }

        return false;
    }

    public int[,] GetGraph()
    {

        graph = new int[completeCycle.Count, completeCycle.Count];
        int[] order = new int[completeCycle.Count];
        int prevMin = -1;

        for (int i = 0; i < completeCycle.Count; i++)
        {
            int min = Int32.MaxValue;
            int minIndex = 0;
            for (int j = 0; j < completeCycle.Count; j++)
            {
                if (completeCycle[j] < min && completeCycle[j] > prevMin)
                {
                    min = completeCycle[j];
                    minIndex = j;
                }
            }

            prevMin = min;
            order[minIndex] = i;
        }
        for (int i = 0; i < order.Length; i++)
        {
            int leftIndex = -1;
            if (i < 1)
            {
                leftIndex = order[^1];
            }
            else
            {
                leftIndex = order[i - 1];
            }

            int rightIndex = -1;
            if (i > order.Length - 2)
            {
                rightIndex = order[0];
            }
            else
            {
                rightIndex = order[i + 1];
            }
            graph[order[i], rightIndex] = 1;
            graph[order[i], leftIndex] = 1;
            graph[rightIndex, order[i]] = 1;
            graph[leftIndex, order[i]] = 1;
        }

        return graph;
    }
}