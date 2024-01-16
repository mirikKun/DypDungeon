using UnityEngine;

public static class DirectionExtension
{
    public static Vector2Int GetVector(this Direction direction)
    {
        switch (direction)
        {
            case Direction.right:
                return new Vector2Int(1, 0);
            case Direction.top:
                return new Vector2Int(0, 1);

            case Direction.left:
                return new Vector2Int(-1, 0);

            default :
                return new Vector2Int(0, -1);
        }
    }

    public static Direction GetOpposite(this Direction direction)
    {
        return (Direction)(((int)direction + 2) % 4);
    }
}

public enum Direction
{
    right,
    top,
    left,
    bottom
}