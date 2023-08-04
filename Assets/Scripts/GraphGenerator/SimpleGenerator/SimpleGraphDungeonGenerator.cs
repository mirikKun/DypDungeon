using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(DungeonPlacer))]
public  class SimpleGraphDungeonGenerator:GraphDungeonGenerator
{
    private GraphRoom[] rooms;
    private GraphRoom[] corridorSpaces;
    private List<GraphRoom> corridors;
    private SimpleDungeonPlacer simpleDungeonPlacer;
    public override void GenerateDungeon()
    {
        base.GenerateDungeon();
        bool success = GenerateRooms();
        if (!success)
        {
            return;
        }
        //FindObjectOfType<TextureSetter>().SetTexturesByRooms(rooms, corridors.ToArray(),dungeonTextureColor,backgroundTextureColor);

    }

    protected override void ResetVariables()
    {
        base.ResetVariables();
        corridors = new List<GraphRoom>();

    }

    private bool GenerateRooms()
    {
        simpleDungeonPlacer = GetComponent<SimpleDungeonPlacer>();
        GraphRoom firstRoom = new GraphRoom(0, GetRoomSize(), 0, GetRoomSize());

        rooms = new GraphRoom[roomCount];
        for (int i = 0; i < roomCount; i++)
        {
            rooms[i] = new GraphRoom(i);
        }

        int edgeCount = 0;
        for (int i = 0; i < roomCount; i++)
        {
            for (int j = 0; j < roomCount; j++)
            {
                if (graph[i, j] == 1)
                {
                    rooms[i].connections.Add(rooms[j]);
                    rooms[i].placed = false;
                    if (j > i)
                    {
                        edgeCount++;
                    }
                }
            }
        }

        corridorSpaces = new GraphRoom[edgeCount];

        rooms[decomposedChains[0].completeCycle[0]] = firstRoom;
        firstRoom.index = decomposedChains[0].completeCycle[0];
        firstRoom.placed = true;
        iterations = 0;

        bool success = GenerateByChain(firstRoom, decomposedChains[0], 1, 0);
        if (!success)
        {
            Debug.Log("Can`t be placed");
            return false;
        }

        for (int i = 0; i < roomCount; i++)
        {
            if (rooms[i].placed)
            {
                simpleDungeonPlacer.PlaceRoom(rooms[i], Vector3.zero, i);
                for (int j = i + 1; j < roomCount; j++)
                {
                    if (graph[i, j] == 1 && rooms[j].placed)
                    {
                        GraphRoom newRoom = GenerateCorridor(rooms[i], rooms[j]);
                        corridors.Add(newRoom);

                        simpleDungeonPlacer.PlaceCorridor(newRoom,
                            Vector3.zero);
                    }
                }
            }
        }

        return true;
    }
    private int GetCorridorIndex(int roomOrder, int chainIndex)
    {
        int corridorIndex = 0;

        for (int i = 0; i < chainIndex; i++)
        {
            corridorIndex += decomposedChains[i].completeCycle.Count - 1;
            if (i == 0 && decomposedChains[i].cycle)
            {
                corridorIndex++;
            }

            if (i > 0)
            {
                corridorIndex++;
                if (decomposedChains[i].cycle)
                {
                    corridorIndex++;
                }
            }
        }

        corridorIndex += roomOrder;
        if (chainIndex > 0)
        {
            corridorIndex++;
        }

        return corridorIndex;
    }
     private bool GenerateByChain(GraphRoom previousRoom, Chain chain, int roomOrder, int chainIndex)
    {
        if (CheckOnIterations())
        {
            return false;
        }

        int corridorIndex = GetCorridorIndex(roomOrder, chainIndex);


        int roomIndex = chain.completeCycle[roomOrder];

        Vector2Int lastPosition = new Vector2Int(previousRoom.Left, previousRoom.Bottom);

        bool lastInCycle = chain.cycle && roomOrder == chain.completeCycle.Count - 1;

        int randDirectionX = Random.Range(0, 4);

        int maxIterationsCount = 8;
        if (randomAngles)
        {
            maxIterationsCount = 20;
        }

        for (int i = 0; i < maxIterationsCount; i++)
        {
            int rotation;
            if (i < 4)
            {
                rotation = (randDirectionX + i) % 4;
            }
            else
            {
                rotation = (randDirectionX + i) % 4 + 4;
            }

            Vector2Int offset = new Vector2Int();
            if (randomAngles)
            {
                Vector2 newDirection = GenerateDirection();
                int newCorridorLenght = GetCorridorLenght();
                offset = new Vector2Int((int)(newDirection.x * newCorridorLenght),
                    (int)(newDirection.y * newCorridorLenght));
            }
            else
            {
                Vector2Int direction = VectorHelper.GenerateDirection(rotation);
                offset = direction * GetCorridorLenght();
            }

            int width = GetRoomSize() - roomSize;
            int lenght = GetRoomSize() - roomSize;
            GraphRoom newRoom = new GraphRoom(-width / 2, roomSize + width / 2, -lenght / 2, roomSize + width / 2,
                offset + lastPosition);
            rooms[roomIndex].CopyPosition(newRoom);
            rooms[roomIndex].placed = true;
            List<GraphRoom> curCorridors = new List<GraphRoom>();
            corridorSpaces[corridorIndex - 1] = GenerateCorridor(newRoom, previousRoom);
            corridorSpaces[corridorIndex - 1].placed = true;
            corridorSpaces[corridorIndex - 1].index = roomCount + corridorIndex - 1;
            curCorridors.Add(corridorSpaces[corridorIndex - 1]);
            if (lastInCycle)
            {
                corridorSpaces[corridorIndex] = GenerateCorridor(newRoom, rooms[chain.exit]);
                corridorSpaces[corridorIndex].placed = true;
                corridorSpaces[corridorIndex].index = roomCount + corridorIndex;
                curCorridors.Add(corridorSpaces[corridorIndex]);
            }


            if (rooms[roomIndex].CanBePlacedWithRooms(rooms) &&
                rooms[roomIndex].CanBePlacedWithCorridors(corridorSpaces, curCorridors) &&
                CheckOnCycleSuccess(chain, roomOrder) &&
                corridorSpaces[corridorIndex - 1].CanBePlacedWithCorridors(corridorSpaces, new List<GraphRoom>()) &&
                corridorSpaces[corridorIndex - 1]
                    .CanBePlacedWithCorridors(rooms, new List<GraphRoom>() { rooms[roomIndex], previousRoom }) &&
                (!lastInCycle || corridorSpaces[corridorIndex]
                        .CanBePlacedWithCorridors(corridorSpaces, new List<GraphRoom>())
                    && corridorSpaces[corridorIndex].CanBePlacedWithCorridors(rooms,
                        new List<GraphRoom>() { rooms[roomIndex], rooms[chain.exit] })
                ))
            {
                if (roomOrder == chain.completeCycle.Count - 1)
                {
                    if (chainIndex == decomposedChains.Count - 1 || GenerateByChain(
                            rooms[decomposedChains[chainIndex + 1].enter], decomposedChains[chainIndex + 1], 0,
                            chainIndex + 1))
                    {
                        return true;
                    }

                    continue;
                }

                if (GenerateByChain(newRoom, chain, roomOrder + 1, chainIndex))
                {
                    return true;
                }
            }
        }

        rooms[chain.completeCycle[roomOrder]].placed = false;
        corridorSpaces[corridorIndex - 1] = null;
        if (lastInCycle)
        {
            corridorSpaces[corridorIndex] = null;
        }

        return false;
    }


        private Vector2 GenerateDirection()
    {
        Vector2 direction = new Vector2(Random.Range(-1f, 1f), Random.Range(-1f, 1f));

        direction = direction.normalized;
        return direction;
    }

    private bool CheckOnCycleSuccess(Chain chain, int roomIndex)
    {
        if (roomIndex != chain.completeCycle.Count - 1)
        {
            return true;
        }

        if (!chain.cycle)
        {
            return true;
        }

        GraphRoom room1;

        room1 = rooms[chain.completeCycle[^1]];
        GraphRoom room2 = rooms[chain.exit];

        Vector2 room1Position = room1.GetPosition();
        Vector2 room2Position = room2.GetPosition();
        float distance = Vector2.Distance(room1Position, room2Position);

        bool closeEnough = distance < minRoomDistance;
        return closeEnough;
    }

  

    private GraphRoom GenerateCorridor(GraphRoom room1, GraphRoom room2)
    {
        GraphRoom newRoom;
        if (Room.RoomAreDiagonal(room1, room2,corridorWidth*2) || !rightAngle)
        {
            Vector2 scale = new Vector2(Vector2.Distance(room1.GetPosition(), room2.GetPosition()) - corridorWidth * 4,
                corridorWidth);
            Vector2 position = (room2.GetPosition() + room1.GetPosition()) / 2f;
            Vector2 direction = room1.GetPosition() - room2.GetPosition();
            float angle = Mathf.Atan2(direction.x, direction.y) * Mathf.Rad2Deg;
            newRoom = new GraphRoom(scale, position, 90 - angle);
        }
        else
        {
            newRoom = GenerateSimpleCorridor(room1, room2);
        }

        return newRoom;
    }


    private GraphRoom GenerateSimpleCorridor(Room firstRoom, Room secondRoom)
    {
        bool horizontal = firstRoom.Left > secondRoom.Right || firstRoom.Right < secondRoom.Left;

        float zPos;
        float lenght;
        float xPos;
        Vector3 corridorScale;
        if (horizontal)
        {
            zPos = RectHelper.GetCenterOfCovering(firstRoom.Bottom, firstRoom.Top, secondRoom.Bottom, secondRoom.Top);
            if (firstRoom.Left > secondRoom.Right)
            {
                lenght = firstRoom.Left - secondRoom.Right;
                xPos = secondRoom.Right + lenght / 2;
            }
            else
            {
                lenght = secondRoom.Left - firstRoom.Right;
                xPos = firstRoom.Right + lenght / 2;
            }

            corridorScale = new Vector3(lenght, 10, corridorWidth);
        }
        else
        {
            xPos = RectHelper.GetCenterOfCovering(firstRoom.Left, firstRoom.Right, secondRoom.Left, secondRoom.Right);
            if (firstRoom.Bottom > secondRoom.Top)
            {
                lenght = firstRoom.Bottom - secondRoom.Top;
                zPos = secondRoom.Top + lenght / 2;
            }
            else
            {
                lenght = secondRoom.Bottom - firstRoom.Top;
                zPos = firstRoom.Top + lenght / 2;
            }

            corridorScale = new Vector3(corridorWidth, 10, lenght);
        }

        return new GraphRoom(new Vector2(corridorScale.x, corridorScale.z), new Vector2(xPos, zPos), 0);
    }



 
}
