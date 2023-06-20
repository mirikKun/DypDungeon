using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
[Serializable]
public class GraphRoom : Room
{
   public float angle;
   public bool placed=false;
   public int index;
   public List<GraphRoom> connections=new List<GraphRoom>();

   public bool CanBePlacedWithRooms(GraphRoom[] placedRooms)
   {
      for (var i = 0; i < placedRooms.Length; i++)
      {
         var room = placedRooms[i];

         if (room!=null && room.placed && room.index!=index)
         {
        
            if (!CanBePlacedWith(room))
            {
               return false;
            }
         }
      }
      return true;
   } 
   public bool CanBePlacedWithCorridors(GraphRoom[] placedCorridors,List<GraphRoom> coonects)
   {
      for (var i = 0; i < placedCorridors.Length; i++)
      {
         var room = placedCorridors[i];

         if (room!=null && room.placed && room.index!=index&&!coonects.Contains(room))
         {
            Vector2[] set1 =
               PolygonChecker.GetSquareCorners(left, right, bottom, top, angle);
            Vector2[] set2=PolygonChecker.GetSquareCorners(room.left, room.right, room.bottom, room.top, room.angle);
            if (PolygonChecker.ArePolygonsIntersecting(set1,set2))
            {
               return false;
            }
         }
      }
      return true;
   }
   public GraphRoom(int left, int right, int bottom, int top)
      : base(left,right,bottom,top)
   {
      placed = false;
   }
   public GraphRoom(int left, int right, int bottom, int top,Vector2Int offset)
      : base(left+offset.x,right+offset.x,bottom+offset.y,top+offset.y)
   {
      placed = false;
   }

   public GraphRoom GetRoomInBetween(int roomSize, GraphRoom anotherRoom)
   {
      Vector2 position = (GetPosition() + anotherRoom.GetPosition()) / 2f;
      int newLeft = (int)(position.x - roomSize / 2f);
      int newRight = (int)(position.x + roomSize / 2f);
      int newBottom = (int)(position.y - roomSize / 2f);
      int newUp = (int)(position.y + roomSize / 2f);
      GraphRoom corridorPosition = new GraphRoom(newLeft, newRight, newBottom, newUp);
      return corridorPosition;
   }

   public void CopyPosition(GraphRoom room)
   {
      this.left = room.left;
      this.right = room.right;
      this.bottom = room.bottom;
      this.top = room.top;
   }
   public  GraphRoom(Vector2 scale,Vector2 position,float angle)
   {
      this.left = Mathf.CeilToInt(position.x - scale.x / 2f);
      this.right = Mathf.CeilToInt(position.x + scale.x / 2f);
      this.bottom = Mathf.CeilToInt(position.y - scale.y / 2f);
      this.top = Mathf.CeilToInt(position.y + scale.y / 2f);
   
      this.angle =90-angle;
   }
   public GraphRoom(int index )
   {
      this.index = index;
      placed = false;
   }
   public GraphRoom( )
   {
      placed = false;
   }
}
