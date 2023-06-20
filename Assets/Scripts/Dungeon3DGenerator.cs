using System.Collections;
using System.Collections.Generic;
using Palmmedia.ReportGenerator.Core;
using UnityEngine;

public class Dungeon3DGenerator : MonoBehaviour
{
    [SerializeField] private MeshCombiner meshCombiner;
    
    [SerializeField] private float height=1.4f;
    private List<MeshFilter> dungeonSegments=new List<MeshFilter>();
    private List<MeshFilter> rooms=new List<MeshFilter>();
    private List<MeshFilter> corridors=new List<MeshFilter>();
    public DungeonSegment cube;

 
    public void ClearMeshes()
    {
        dungeonSegments=new List<MeshFilter>();
        rooms=new List<MeshFilter>();
        corridors=new List<MeshFilter>();
        int childCount = transform.childCount;
        for (int i = 0; i < childCount; i++)
        {
            DestroyImmediate(transform.GetChild(0).gameObject);
        }
    }
    public void GenerateMesh()
    {
        StartCoroutine( meshCombiner.Combining3DMesh(dungeonSegments.ToArray()));
    }
    public DungeonSegment PlaceCorridor(GraphRoom room,Vector3 dungeonOffset)
    {
        Vector3 positionBetween = new Vector3(room.GetPosition().x, 0, room.GetPosition().y);
        DungeonSegment corridor =
            Instantiate(cube, transform.position + positionBetween - dungeonOffset, Quaternion.identity);
        corridor.SetParent(transform);
        corridor.SetupScale( new Vector3(room.GetWidth(), height-0.05f, room.GetHeight()));
        float angle=room.angle+90;
        corridor.SetupRotation(angle);
        corridor.gameObject.name = "corridor";
        dungeonSegments.Add(corridor.GetComponent<MeshFilter>());
        corridors.Add(corridor.GetComponent<MeshFilter>());
        
        return corridor;
    }
    public DungeonSegment PlaceRoom(Room r,Vector3 dungeonOffset,int index=0)
    {
        Vector3 roomPosition = new Vector3(r.left + (float)r.GetWidth() / 2, 0, r.bottom + (float)r.GetHeight() / 2);

        DungeonSegment room = Instantiate(cube, transform.position + roomPosition - dungeonOffset, Quaternion.identity);
        room.SetParent(transform);

        room.SetupScale(new Vector3(r.GetWidth(), height, r.GetHeight()));
        r.dungeonSegment = room;
        room.gameObject.name = "room "+index;
        dungeonSegments.Add(room.GetComponent<MeshFilter>());
        rooms.Add(room.GetComponent<MeshFilter>());

        return room;
    }
}
