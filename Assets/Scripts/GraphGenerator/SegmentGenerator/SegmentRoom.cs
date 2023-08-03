using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SegmentRoom : Room
{
    public int ID { get; private set; }

    public Vector2 Center { get; private set; }


    public Vector2Int Size { get; private set; }
    private List<SegmentRoom> connectedRooms = new List<SegmentRoom>();
    public List<SegmentRoom> СonnectedRooms => connectedRooms; // возвращаем значение свойства

    public bool Placed { get; set; }

    private List<HallWay> hallWays = new List<HallWay>();

    public List<HallWay> HallWays => hallWays;
    public SegmentRoom(int id, Vector2 center, Vector2Int size)
    {
        ID = id;
        Center = center;
        Bottom = Mathf.FloorToInt(center.y-size.x / 2f);
        Left = Mathf.FloorToInt(center.x-size.y / 2f);
        Right = Left + size.x;
        Top = Bottom + size.y;
        Size = size;
        Placed = false;
    }

    public SegmentRoom(int id, Vector2Int size)
    {
        ID = id;
        Center = Vector2.zero;
        Bottom = Mathf.FloorToInt(size.x / 2f);
        Left = Mathf.FloorToInt(size.y / 2f);
        Right = Left + size.x;
        Top = Bottom + size.y;
        Size = size;
        Placed = false;

    }

    public void SetPosition(int left, int bottom)
    {
        Left = left;
        Bottom = bottom;
        Right = Left + Size.x;
        Top = Bottom + Size.y;
        Center = new Vector2(Left, Bottom) + (Vector2)Size / 2f;
    }

    public void SetPosition(Vector2 newCenter)
    {
        Center = newCenter;
        Bottom = Mathf.FloorToInt(newCenter.x - Size.x / 2f);
        Left = Mathf.FloorToInt(newCenter.y - Size.y / 2f);
        Right = Left + Size.x;
        Top = Bottom + Size.y;
    }

    public void AddHallway(HallWay hallWay)
    {
        hallWays.Add(hallWay);
    }

    public void ClearHallways()
    {
        hallWays.Clear();
    }
}