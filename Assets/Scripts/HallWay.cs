using System.Collections.Generic;

public class HallWay
{
    public SegmentRoom[] Corridors { get;private set ; }
    public int[] RoomsIds { get; private set; }
    public List<SegmentRoom[]> alternatives=new List<SegmentRoom[]>();
    public bool AreAlternatives => alternatives.Count > 1;

    public void AddAlternative(SegmentRoom[] alternative)
    {
        alternatives.Add(alternative);
    }

    public void DiscardCurrentAlternative()
    {
        alternatives.Remove(Corridors);
        Corridors = alternatives[0];
    }
    public bool Straight => Corridors.Length == 1;

    public HallWay(SegmentRoom straightHallway,int roomId1,int roomId2)
    {
        Corridors = new[] { straightHallway };
        RoomsIds = new[] { roomId1, roomId2 };
    }

    public HallWay(SegmentRoom firstPartHallway,SegmentRoom secondPartHallway,int roomId1,int roomId2)
    {
        Corridors = new[] { firstPartHallway,secondPartHallway };
        RoomsIds = new[] { roomId1, roomId2 };

    }

}