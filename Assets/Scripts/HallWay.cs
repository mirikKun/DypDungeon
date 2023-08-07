using System.Collections.Generic;

public class HallWay
{
    public SegmentCorridor[] Corridors { get;private set ; }
    public int[] RoomsIds { get; private set; }
    public List<SegmentCorridor[]> alternatives=new List<SegmentCorridor[]>();
    public bool HaveAlternatives => alternatives.Count > 1;

    public void AddAlternative(SegmentCorridor[] alternative)
    {
        alternatives.Add(alternative);
    }

    public void DiscardCurrentAlternative()
    {
        alternatives.Remove(Corridors);
        Corridors = alternatives[0];
    }
    public bool Straight => Corridors.Length == 1;

    public HallWay(SegmentCorridor straightHallway,int roomId1,int roomId2)
    {
        Corridors = new[] { straightHallway };
        RoomsIds = new[] { roomId1, roomId2 };
    }

    public HallWay(SegmentCorridor firstPartHallway,SegmentCorridor secondPartHallway,int roomId1,int roomId2)
    {
        Corridors = new[] { firstPartHallway,secondPartHallway };
        RoomsIds = new[] { roomId1, roomId2 };

    }

}