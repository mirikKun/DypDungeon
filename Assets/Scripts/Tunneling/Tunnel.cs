using UnityEngine;

public class Tunnel : Room
{
    public Direction Direction { get; protected set; }
    public int Width { get; protected set; }
    public int Lenght { get; protected set; }

    public bool Fork { get; private set; }

    public Direction GetTurnedDirection()
    {
        int directionId = (int)Direction;
        while (directionId % 2 == (int)Direction % 2)
        {
            directionId = Random.Range(0, 4);
        }

        return (Direction)directionId;
    }
    public Vector2 GetEndEdgeCenter()
    {
        if (Direction == Direction.right)
        {
            return new Vector2(Right, (Bottom + Top) / 2f);
        }
        else if (Direction == Direction.left)
        {
            return new Vector2(Left, (Bottom + Top) / 2f);
        }
        else if (Direction == Direction.top)
        {
            return new Vector2((Left + Right) / 2f, Top);
        }
        else
        {
            return new Vector2((Left + Right) / 2f, Bottom);
        }
    }

    public Vector2 GetEndSegmentCenter()
    {
        return GetEndEdgeCenter() - Direction.GetVector() * Width / 2;
    }

    public Vector2 GetSegmentCenter()
    {
        return new Vector2((Left + Right) / 2f, (Bottom + Top) / 2f);
    }

    public Tunnel(Vector2 lastEndEdgeCenter, Direction direction, int width, int lenght, bool fork = false)
    {
        Fork = fork;
        Direction = direction;
        Width = width;
        Lenght = lenght;
        if (direction == Direction.right)
        {
            Right = Mathf.FloorToInt(lastEndEdgeCenter.x + lenght);
            Left = Mathf.FloorToInt(lastEndEdgeCenter.x);
            Top = Mathf.FloorToInt(lastEndEdgeCenter.y + width / 2f);
            Bottom = Mathf.FloorToInt(lastEndEdgeCenter.y - width / 2f);
        }
        else if (direction == Direction.left)
        {
            Right = Mathf.FloorToInt(lastEndEdgeCenter.x);
            Left = Mathf.FloorToInt(lastEndEdgeCenter.x - lenght);
            Top = Mathf.FloorToInt(lastEndEdgeCenter.y + width / 2f);
            Bottom = Mathf.FloorToInt(lastEndEdgeCenter.y - width / 2f);
        }
        else if (direction == Direction.top)
        {
            Right = Mathf.FloorToInt(lastEndEdgeCenter.x + width / 2f);
            Left = Mathf.FloorToInt(lastEndEdgeCenter.x - width / 2f);
            Top = Mathf.FloorToInt(lastEndEdgeCenter.y + lenght);
            Bottom = Mathf.FloorToInt(lastEndEdgeCenter.y);
        }
        else
        {
            Right = Mathf.FloorToInt(lastEndEdgeCenter.x + width / 2f);
            Left = Mathf.FloorToInt(lastEndEdgeCenter.x - width / 2f);
            Top = Mathf.FloorToInt(lastEndEdgeCenter.y);
            Bottom = Mathf.FloorToInt(lastEndEdgeCenter.y - lenght);
        }
    }


    public bool AreInBounds(int maxRight, int maxTop)
    {
        return Left >= 0 && Right < maxRight && Bottom >= 0 && Top < maxTop;
    }
}