using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
[Serializable]
public class GraphRoom : Room
{
   public bool placed=false;
   public int index;
   public List<GraphRoom> connections=new List<GraphRoom>();

   public bool CanBePlaced(GraphRoom[] placedRooms)
   {
      for (var i = 0; i < placedRooms.Length; i++)
      {
         var room = placedRooms[i];
         if (room.placed && room.index!=index)
         {
            if (!CanBePlacedWith(room))
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

   public void CopyPosition(GraphRoom room)
   {
      this.left = room.left;
      this.right = room.right;
      this.bottom = room.bottom;
      this.top = room.top;
   }
   public GraphRoom(int index )
   {
      this.index = index;
      placed = false;
   }
}
