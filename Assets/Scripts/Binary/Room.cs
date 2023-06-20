using UnityEngine;

public class Room
{
    public int left, right, bottom, top;
    public Room upperTreeRoom;
    public Room sideTreeRoom;
    public Room[] dawnTreeRooms;
    public DungeonSegment dungeonSegment;
    public bool leftBorder;
    public bool rightBorder;
    public bool bottomBorder;
    public bool topBorder;

    public Vector2 GetPosition()
    {
        float xPos = left + GetWidth() / 2f;
        float zPos = bottom + GetHeight() / 2f;
        return new Vector2(xPos, zPos);
    }

    public bool IsOuter()
    {
        return leftBorder || rightBorder || bottomBorder || topBorder;
    }

    public Vector3 Get3DPosition()
    {
        float xPos = left - GetWidth() / 2f;
        float zPos = bottom - GetHeight() / 2f;
        return new Vector3(xPos, 0, zPos);
    }

    public bool CanBePlacedWith(Room square)
    {
      

        // перевіряємо, чи квадрати перетинаються по горизонталі (осі X)
        if (square.left <= right && square.right >= left)
        {
            // перевіряємо, чи квадрати перетинаються по вертикалі (осі Y)
            if (square.bottom <= top && square.top >= bottom)
            {
                // якщо квадрати перетинаються, повертаємо true
                return false;
            }

            if (square.bottom >= top && square.top <= bottom)
            {
                // якщо квадрати перетинаються, повертаємо true
                return false;
            }
        }

        if (square.left >= right && square.right <= left)
        {
            // перевіряємо, чи квадрати перетинаються по вертикалі (осі Y)
            if (square.bottom <= top && square.top >= bottom)
            {
                // якщо квадрати перетинаються, повертаємо true
                return false;
            }

            if (square.bottom >= top && square.top <= bottom)
            {
                // якщо квадрати перетинаються, повертаємо true
                return false;
            }
        }

        return true;
    }

    public int GetWidth()
    {
        return right - left;
    }

    public int GetHeight()
    {
        return top - bottom;
    }

    public Room()
    {
        this.left = 0;
        this.right = 1;
        this.bottom = 0;
        this.top = 1;
    }

    public Room(int left, int right, int bottom, int top)
    {
        this.left = left;
        this.right = right;
        this.bottom = bottom;
        this.top = top;
    }
}