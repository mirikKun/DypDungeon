
using UnityEngine;
    public  class SimpleDungeonPlacer: DungeonPlacer
    {
        public DungeonSegment cube;

        public DungeonSegment PlaceCorridor( Vector3 corridorScale,Vector3 corridorPosition,Vector3 dungeonOffset)
        {
            DungeonSegment corridor =
                Instantiate(cube, transform.position + corridorPosition - dungeonOffset, Quaternion.identity);
            corridor.SetParent(transform);
            corridor.SetupScale(corridorScale);
            return corridor;
        }

        public DungeonSegment PlaceCorridor(GraphRoom room,Vector3 dungeonOffset)
        {
            Vector3 positionBetween = new Vector3(room.GetPosition().x, 0, room.GetPosition().y);
            DungeonSegment corridor =
                Instantiate(cube, transform.position + positionBetween - dungeonOffset, Quaternion.identity);
            corridor.SetParent(transform);
            corridor.SetupScale( new Vector3(room.GetWidth, 10, room.GetHeight));
     
            float angle=room.angle+90;
        
            corridor.SetupRotation(angle);
            corridor.gameObject.name = "corridor";

            return corridor;
        }
        public DungeonSegment PlaceRoom(Room r,Vector3 dungeonOffset,int index=0)
        {
        
            Vector3 roomPosition = new Vector3(r.Left + (float)r.GetWidth / 2, 0, r.Bottom + (float)r.GetHeight / 2);

            DungeonSegment room = Instantiate(cube, transform.position + roomPosition - dungeonOffset, Quaternion.identity);
            room.SetParent(transform);
            room.SetText(index+1);
            room.SetupScale(new Vector3(r.GetWidth, 10, r.GetHeight));
            room.gameObject.name = "room "+index;
            return room;
        }
        public void RemoveEverything()
        {
            int childCount = transform.childCount;
            for (int i = 0; i < childCount; i++)
            {
                DestroyImmediate(transform.GetChild(0).gameObject);
            }

        }
    }
