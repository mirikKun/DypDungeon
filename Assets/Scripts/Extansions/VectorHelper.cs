using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class VectorHelper 
{
    public static Vector2Int GenerateDirection(int seed)
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

    public static Vector2Int GetRandomSize(int min, int max)
    {
        return new Vector2Int(Random.Range(min, max), Random.Range(min, max));
    }
    public static Vector2Int GetVector2Int( this Vector2 vector2)
    {
        return new Vector2Int(Mathf.FloorToInt(vector2.x), Mathf.FloorToInt(vector2.y));
    }
}
