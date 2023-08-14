using UnityEngine;

public static class DirectionExtension
{
    public static Vector2Int GetVector(this Direction direction)
    {
        switch (direction)
        {
            case Direction.right:
                return new Vector2Int(1, 0);
                break;
            case Direction.top:
                return new Vector2Int(0, 1);

                break;
            case Direction.left:
                return new Vector2Int(-1, 0);

                break;
            default :
                return new Vector2Int(0, -1);

                break;
        }
    }
    public static Vector2Int GetStartOffsetVector(this Direction direction)
    {
        switch (direction)
        {
            case Direction.right:
                return new Vector2Int(0, 0);
                break;
            case Direction.top:
                return new Vector2Int(0, 0);

                break;
            case Direction.left:
                return new Vector2Int(-1, 0);

                break;
            default :
                return new Vector2Int(0, -1);

                break;
        }
    }
}

public enum Direction
{
    right,
    top,
    left,
    bottom
}