using UnityEngine;

public class Room
{
    public int Left { get; protected set; }
    public int Right { get; protected set; }
    public int Bottom { get; protected set; }
    public int Top { get; protected set; }

    public Vector2 GetPosition()
    {
        float xPos = Left + GetWidth / 2f;
        float zPos = Bottom + GetHeight / 2f;
        return new Vector2(xPos, zPos);
    }

    public int GetWidth => Right - Left;

    public int GetHeight => Top - Bottom;

    public Room()
    {
        this.Left = 0;
        this.Right = 1;
        this.Bottom = 0;
        this.Top = 1;
    }
    public Room(int left, int right, int bottom, int top)
    {
        this.Left = left;
        this.Right = right;
        this.Bottom = bottom;
        this.Top = top;
    }
    
    public static bool RoomAreDiagonal(Room room1, Room room2,int offset)
    {
        bool rightTop = room1.Right < room2.Left + offset && room1.Top < room2.Bottom + offset;
        bool rightBottom = room1.Right < room2.Left + offset && room1.Bottom > room2.Top - offset;

        bool leftTop = room1.Left > room2.Right - offset && room1.Top < room2.Bottom + offset;
        bool leftBottom = room1.Left > room2.Right - offset && room1.Bottom > room2.Top - offset;

        return rightTop || rightBottom || leftTop || leftBottom;
    }
}