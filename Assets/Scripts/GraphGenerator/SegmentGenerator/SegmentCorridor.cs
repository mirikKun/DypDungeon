using UnityEngine;

public class SegmentCorridor : SegmentRoom
{
    public bool Horizontal { get; private set; }
    public SegmentCorridor(int id, Vector2 center, Vector2Int size,bool horizontal):base(id,center,size)
    {
        Horizontal = horizontal;
    }
}