using System.Collections;
using System.Collections.Generic;
using Palmmedia.ReportGenerator.Core;
using UnityEngine;

public class Dungeon3DGenerator : MonoBehaviour
{
    [SerializeField] private GraphDungeonGenerator graphDungeonGenerator;
    [SerializeField] private MeshCombiner meshCombiner;
    
    [SerializeField]private MeshFilter[]defSegments;
    [SerializeField] private float height=1.4f;
    private List<MeshFilter> dungeonSegments=new List<MeshFilter>();
    public DungeonSegment cube;
    // Start is called before the first frame update
    void Start()
    {
        // graphDungeonGenerator.SetDungeonGeneration(this);
         StartCoroutine( meshCombiner.Combining3DMesh(defSegments));

    }

    public void ClearMeshes()
    {
        dungeonSegments=new List<MeshFilter>();
        int childCount = transform.childCount;
        for (int i = 0; i < childCount; i++)
        {
            DestroyImmediate(transform.GetChild(0).gameObject);
        }
    }
    public void GenerateMesh()
    {
        Debug.Log(dungeonSegments.ToArray());
        Debug.Log(dungeonSegments.Count);
        
        StartCoroutine( meshCombiner.Combining3DMesh(dungeonSegments.ToArray()));
    }
    public DungeonSegment PlaceCorridor(float width, Vector2 firstRoom, Vector2 secondRoom,Vector3 dungeonOffset)
    {
        Vector3 positionBetween = new Vector3((firstRoom.x + secondRoom.x)/2, 0, (firstRoom.y + secondRoom.y)/2);
        DungeonSegment corridor =
            Instantiate(cube, transform.position + positionBetween - dungeonOffset, Quaternion.identity);
        corridor.SetParent(transform);
        corridor.SetupScale(new Vector3(width,height-0.02f,Vector3.Distance(firstRoom,secondRoom)*0.8f));
        Vector2 direction = firstRoom - secondRoom;
        float angle = Mathf.Atan2(direction.x, direction.y) * Mathf.Rad2Deg;
        corridor.SetupRotation(angle);
        corridor.gameObject.name = "corridor";
        dungeonSegments.Add(corridor.GetComponent<MeshFilter>());
        Debug.Log(2);

        return corridor;
    }
    public DungeonSegment PlaceRoom(Room r,Vector3 dungeonOffset,int index=0)
    {
        Debug.Log(1);
        Vector3 roomPosition = new Vector3(r.left + (float)r.GetWidth() / 2, 0, r.bottom + (float)r.GetHeight() / 2);

        DungeonSegment room = Instantiate(cube, transform.position + roomPosition - dungeonOffset, Quaternion.identity);
        room.SetParent(transform);

        room.SetupScale(new Vector3(r.GetWidth(), height, r.GetHeight()));
        r.dungeonSegment = room;
        room.gameObject.name = "room "+index;
        dungeonSegments.Add(room.GetComponent<MeshFilter>());

        return room;
    }
}
