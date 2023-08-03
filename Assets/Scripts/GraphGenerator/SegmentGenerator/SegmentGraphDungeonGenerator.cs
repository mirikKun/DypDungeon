using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(SegmentDungeonPlacer))]
public class SegmentGraphDungeonGenerator : GraphDungeonGenerator
{
    private int[,] dungeonSegments;

    private Vector2Int size = new Vector2Int(15, 15);
    private SegmentRoom[] rooms;
    private SegmentDungeonPlacer dungeonPlacer;

    private enum SegmentType
    {
        None = 0,
        Room = 1,
        Door = -2,
        Hallway = -1
    }

    public override void GenerateDungeon()
    {
        base.GenerateDungeon();
        StartRoomPositionGenerating();
    }

    protected override void ResetVariables()
    {
        base.ResetVariables();
    }

    private void StartRoomPositionGenerating()
    {
        dungeonPlacer = GetComponent<SegmentDungeonPlacer>();
        dungeonSegments = new int[size.x, size.y];
        iterations = 0;
        rooms = new SegmentRoom[roomCount];
        for (int i = 0; i < roomCount; i++)
        {
            rooms[i] = new SegmentRoom(i+1,(Vector2)size/2f, VectorHelper.GetRandomSize(roomSize, roomSize + roomSizeRange));
        }

        for (int i = 0; i < roomCount; i++)
        {
            for (int j = 0; j < roomCount; j++)
            {
                if (dungeonSegments[i, j] == 1)
                {
                    rooms[i].СonnectedRooms.Add(rooms[j]);
                }
            }
        }

        int firstRoomIndex = decomposedChains[0].completeCycle[0];
        rooms[firstRoomIndex].Placed = true;
        PlaceRoom(rooms[firstRoomIndex]);
        bool success = GenerateRoomsByChain(rooms[firstRoomIndex], decomposedChains[0], 1, 0);
        Debug.Log($"Generated: {success}");
        GraphGenerator.PrintSyntaxMatrix(dungeonSegments);
        dungeonPlacer.Place(dungeonSegments);
    }

    private void PlaceRoom(SegmentRoom room)
    {
        room.Placed = true;
        FillGrid(room.ID, room);
        foreach (var hallWay in room.HallWays)
        {
            FillGridHallway((int)SegmentType.Hallway, hallWay);
        }
    }
    private void ClearRoom(SegmentRoom room)
    {
        room.Placed = false;
        room.ClearHallways();

        FillGrid(0, room);
        foreach (var hallWay in room.HallWays)
        {

            foreach (var corridor in hallWay.Corridors)
            {
                for (int i = corridor.Left; i < corridor.Right; i++)
                {
                    for (int j = corridor.Bottom; j < corridor.Top; j++)
                    {
                        
                        if (dungeonSegments[i, j] ==(int)SegmentType.Hallway)
                        {
                            dungeonSegments[i, j]=0;
                        }
                        
                    }
                }
            }
        }
    }

    private void FillGrid(int value, int xFrom, int xTo, int tFrom, int yTo)
    {
        Debug.Log(value);
        for (int i = xFrom; i < xTo; i++)
        {
            for (int j = tFrom; j < yTo; j++)
            {
                dungeonSegments[i, j] = value;
            }
        }
    }

    private void FillGrid(int value, SegmentRoom room)
    {
        FillGrid(value, room.Left, room.Right, room.Bottom, room.Top);
    }

    private void FillGridHallway(int value, HallWay hallWay)
    {
        foreach (var corridor in hallWay.Corridors)
        {
            for (int i = corridor.Left; i < corridor.Right; i++)
            {
                for (int j = corridor.Bottom; j < corridor.Top; j++)
                {
                    if (dungeonSegments[i, j] == 0)
                    {
                        dungeonSegments[i, j] = value;
                    }
                }
            }
        }
    }


    private bool GenerateRoomsByChain(SegmentRoom previousRoom, Chain chain, int roomOrder, int chainIndex)
    {
        if (CheckOnIterations())
        {
            return false;
        }
        int roomIndex = chain.completeCycle[roomOrder];

        Vector2Int lastPosition = new Vector2Int(previousRoom.Left, previousRoom.Bottom);
        bool lastInCycle = chain.cycle && roomOrder == chain.completeCycle.Count - 1;
        int randDirectionX = Random.Range(0, 4);
        int maxIterationsCount = 8;

        for (int i = 0; i < maxIterationsCount; i++)
        {
            Vector2Int offset = CalculateOffset(i, randDirectionX);
            
            rooms[roomIndex].SetPosition(offset + lastPosition);
            rooms[roomIndex].ClearHallways();


            HallWay newHallWays = ConnectRooms(previousRoom, rooms[roomIndex]);
            rooms[roomIndex].AddHallway(newHallWays);
            if (lastInCycle)
            {
                HallWay newCycleHallWays = ConnectRooms(rooms[chain.exit], rooms[roomIndex]);
                rooms[roomIndex].AddHallway(newCycleHallWays);
            }


            if (CanBePlacedOnGrid(rooms[roomIndex]))
            {
                PlaceRoom(rooms[roomIndex]);
                if (roomOrder == chain.completeCycle.Count - 1)
                {
                    if (chainIndex == decomposedChains.Count - 1 || GenerateRoomsByChain(
                            rooms[decomposedChains[chainIndex + 1].enter], decomposedChains[chainIndex + 1], 0,
                            chainIndex + 1))
                    {
                        return true;
                    }

                    continue;
                }

                if (GenerateRoomsByChain(rooms[roomIndex], chain, roomOrder + 1, chainIndex))
                {
                    return true;
                }
            }
        }
        ClearRoom(rooms[chain.completeCycle[roomOrder]]);
        return false;
    }

    private Vector2Int CalculateOffset(int iteration, int startNumber)
    {
        int rotation;
        if (iteration < 4)
        {
            rotation = (startNumber + iteration) % 4;
        }
        else
        {
            rotation = (startNumber + iteration) % 4 + 4;
        }
        Vector2Int direction = VectorHelper.GenerateDirection(rotation);
        return  direction * GetCorridorLenght();

    }
    private bool CanBePlacedOnGrid(SegmentRoom room)
    {
        if (room.Left < 0 || room.Bottom < 0 || room.Right >= size.x || room.Top >= size.y)
        {
            return false;
        }
        for (int i = room.Left; i < room.Right; i++)
        {
            for (int j = room.Bottom; j < room.Top; j++)
            {
                if (dungeonSegments[i, j] != 0)
                {
                    return false;
                }
            }
        }

        foreach (var hallWay in room.HallWays)
        {

            foreach (var corridor in hallWay.Corridors)
            {
                for (int i = corridor.Left; i < corridor.Right; i++)
                {
                    for (int j = corridor.Bottom; j < corridor.Top; j++)
                    {
                        
                        if (dungeonSegments[i, j] != 0&&dungeonSegments[i, j]!=hallWay.roomsIds[0]&&dungeonSegments[i, j]!=hallWay.roomsIds[1])
                        {
                            return false;
                        }
                        
                    }
                }
            }
        }

        return true;
    }
    private HallWay ConnectRooms(SegmentRoom firstRoom, SegmentRoom secondRoom)
    {
        if (Room.RoomAreDiagonal(firstRoom, secondRoom, corridorWidth))
        {
            return ConnectDiagonalRooms(firstRoom, secondRoom);
        }
        else
        {
            return ConnectStraightRooms(firstRoom, secondRoom);
        }
    }

    private HallWay ConnectDiagonalRooms(SegmentRoom firstRoom, SegmentRoom secondRoom)
    {
        Vector2 p1 = firstRoom.Center;
        Vector2 p2 = secondRoom.Center;

        Vector2 p3 = new Vector2(p1.x, p2.y);

        SegmentRoom corridor1 = new SegmentRoom((int)SegmentType.Hallway, (p1 + p3) / 2f,
            new Vector2Int(corridorWidth, Mathf.FloorToInt(Mathf.Abs(p1.y - p3.y))));
        SegmentRoom corridor2 = new SegmentRoom((int)SegmentType.Hallway, (p2 + p3) / 2f,
            new Vector2Int(Mathf.FloorToInt(Mathf.Abs(p2.x - p3.x)), corridorWidth));

        HallWay newHallWay = new HallWay(corridor1, corridor2,firstRoom.ID,secondRoom.ID);

        return newHallWay;
    }

    private HallWay ConnectStraightRooms(SegmentRoom firstRoom, SegmentRoom secondRoom)
    {
        HallWay newHallWay;
        bool horizontal = firstRoom.Left > secondRoom.Right || firstRoom.Right < secondRoom.Left;
        int lenght;
        float yPos;
        float xPos;
        Vector2Int hallwaySize;
        if (horizontal)
        {
            yPos = RectHelper.GetCenterOfCovering(firstRoom.Bottom, firstRoom.Top, secondRoom.Bottom, secondRoom.Top);
            if (firstRoom.Left > secondRoom.Right)
            {
                lenght = firstRoom.Left - secondRoom.Right;
                xPos = secondRoom.Right + lenght / 2f;
            }
            else
            {
                lenght = secondRoom.Left - firstRoom.Right;
                xPos = firstRoom.Right + lenght / 2f;
            }

            hallwaySize = new Vector2Int(lenght, corridorWidth);
           
        }
        else
        {
            xPos = Mathf.FloorToInt(RectHelper.GetCenterOfCovering(firstRoom.Left, firstRoom.Right, secondRoom.Left,
                secondRoom.Right));
            if (firstRoom.Bottom > secondRoom.Top)
            {
                lenght = firstRoom.Bottom - secondRoom.Top;
                yPos = secondRoom.Top + lenght / 2f;
            }
            else
            {
                lenght = secondRoom.Bottom - firstRoom.Top;
                yPos = firstRoom.Top + lenght / 2f;
            }
            hallwaySize = new Vector2Int(corridorWidth, lenght);
         
        }
        newHallWay =
            new HallWay(new SegmentRoom((int)SegmentType.Hallway, new Vector2(xPos, yPos),hallwaySize),firstRoom.ID,secondRoom.ID);
        return newHallWay;

    }
}