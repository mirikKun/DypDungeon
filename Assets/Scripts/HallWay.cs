using System.Collections.Generic;

public class HallWay
{
    public SegmentRoom[] Corridors { get;private set ; }
    public int[] roomsIds;
    public bool Straight => Corridors.Length == 1;

    public HallWay(SegmentRoom straightHallway,int roomId1,int roomId2)
    {
        Corridors = new[] { straightHallway };
        roomsIds = new[] { roomId1, roomId2 };
    }

    public HallWay(SegmentRoom firstPartHallway,SegmentRoom secondPartHallway,int roomId1,int roomId2)
    {
        Corridors = new[] { firstPartHallway,secondPartHallway };
        roomsIds = new[] { roomId1, roomId2 };

    }

}