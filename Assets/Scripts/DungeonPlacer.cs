using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DungeonPlacer : MonoBehaviour
{    public DungeonSegment cube;

    public DungeonSegment PlaceCorridor( Vector3 corridorScale,Vector3 corridorPosition,Vector3 dungeonOffset)
    {
        DungeonSegment corridor =
            Instantiate(cube, transform.position + corridorPosition - dungeonOffset, Quaternion.identity);
        corridor.SetParent(transform);
        corridor.SetupScale(corridorScale);
        return corridor;
    }
    public DungeonSegment PlaceCorridor(float width, Vector2 firstRoom, Vector2 secondRoom,Vector3 dungeonOffset)
    {
        Vector3 positionBetween = new Vector3((firstRoom.x + secondRoom.x)/2, 0, (firstRoom.y + secondRoom.y)/2);
        DungeonSegment corridor =
            Instantiate(cube, transform.position + positionBetween - dungeonOffset, Quaternion.identity);
        corridor.SetParent(transform);
        corridor.SetupScale(new Vector3(width,1,Vector3.Distance(firstRoom,secondRoom)));
        Vector2 direction = firstRoom - secondRoom;
        float angle = Mathf.Atan2(direction.x, direction.y) * Mathf.Rad2Deg;
        corridor.SetupRotation(angle);
        corridor.gameObject.name = "corridor";

        return corridor;
    }
    public DungeonSegment PlaceRoom(Room r,Vector3 dungeonOffset,int index=0)
    {
        
        Vector3 roomPosition = new Vector3(r.left + (float)r.GetWidth() / 2, 0, r.bottom + (float)r.GetHeight() / 2);

        DungeonSegment room = Instantiate(cube, transform.position + roomPosition - dungeonOffset, Quaternion.identity);
        room.SetParent(transform);

        room.SetupScale(new Vector3(r.GetWidth(), 10, r.GetHeight()));
        r.dungeonSegment = room;
        room.gameObject.name = "room "+index;
        return room;
    }
}