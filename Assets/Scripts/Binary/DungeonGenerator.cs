using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Graphs;
using Unity.VisualScripting;
using UnityEngine;
using Random = UnityEngine.Random;

public class DungeonGenerator : MonoBehaviour
{
    public DungeonSegment cube;
    public int seed = 30;

    public int height = 30;
    public int width = 30;
    public int trimSize = 1;
    [Range(0, 0.5f)] public float maxRoomDecreasing = 0.15f;
    public int corridorWidth = 1;

    public int minRoomHeight = 3;
    public int minRoomWidth = 5;

    public int maxRoomHeight = 3;
    public int maxRoomWidth = 5;

    public int enterLenght = 35;
    public Queue<BinaryRoom> roomsToSplit = new Queue<BinaryRoom>();
    public List<DungeonSegment> finalRoomObjects = new List<DungeonSegment>();
    public List<BinaryRoom> finalRooms = new List<BinaryRoom>();
    public DungeonSegment enterSegment;
    private float placeMuptiplayer = 0.5f;
    private SimpleDungeonPlacer dungeonPlacer;

    private void Start()
    {
        dungeonPlacer = GetComponent<SimpleDungeonPlacer>();
        Generate();
    }

    public void Generate()
    {
        if (finalRoomObjects.Count != 0)
        {
            foreach (var finalRoomObject in finalRoomObjects)
            {
                if (finalRoomObject)
                    DestroyImmediate(finalRoomObject.gameObject);
            }

            finalRoomObjects.Clear();
            roomsToSplit.Clear();
            finalRooms.Clear();
            foreach (Transform child in transform)
            {
                DestroyImmediate(child.gameObject);
            }
        }

        Random.InitState(seed);

        BinaryRoom firstRoom = new BinaryRoom(0, width, 0, height);
        roomsToSplit.Enqueue(firstRoom);
        curIteration = 0;
        GenerateRooms();
        AddTrims();
        DecreaseRooms();
        PlaceRooms();
        //GenerateEnter();
        HashSet<Prim.Edge> edges = CreateHallways(DelaunayGraph());
        GenerateCorridorsByEdges(edges);
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.R))
        {
            foreach (var room in finalRoomObjects)
            {
                Destroy(room.gameObject);
            }

            finalRoomObjects.Clear();
            finalRooms.Clear();
            Generate();
        }
    }

    private void GenerateEnter()
    {
        BinaryRoom[] outerRooms = GetAllOuterRooms();


        BinaryRoom enterRoom = outerRooms[Random.Range(0, outerRooms.Length - 1)];
        bool horizontal = enterRoom.leftBorder || enterRoom.rightBorder;

        float zPos;
        float lenght = enterLenght;
        float xPos;
        Vector3 corridorScale;

        Vector3 dungeonOffset = new Vector3(width * placeMuptiplayer, 0, height * placeMuptiplayer);

        if (horizontal)
        {
            zPos = enterRoom.Bottom + (enterRoom.Top - enterRoom.Bottom) / 2;
            if (enterRoom.rightBorder)
            {
                xPos = enterRoom.Right + lenght / 2;
            }
            else
            {
                xPos = enterRoom.Left - lenght / 2;
            }

            corridorScale = new Vector3(lenght, 10, corridorWidth);
            if (!enterRoom.rightBorder)
            {
                PlaceEnter(new Vector3(enterRoom.Left - lenght + 4, 0, zPos) + transform.position - dungeonOffset,
                    true);
            }
            else
            {
                PlaceEnter(new Vector3(enterRoom.Right + lenght - 4, 0, zPos) + transform.position - dungeonOffset,
                    true);
            }
        }
        else
        {
            xPos = enterRoom.Left + (enterRoom.Right - enterRoom.Left) / 2;
            if (enterRoom.topBorder)
            {
                zPos = enterRoom.Top + lenght / 2;
            }
            else
            {
                zPos = enterRoom.Bottom - lenght / 2;
            }

            corridorScale = new Vector3(corridorWidth, 10, lenght);
            if (!enterRoom.topBorder)
            {
                PlaceEnter(new Vector3(xPos, 0, enterRoom.Bottom - lenght + 4) + transform.position - dungeonOffset,
                    false);
            }
            else
            {
                PlaceEnter(new Vector3(xPos, 0, enterRoom.Top + lenght - 4) + transform.position - dungeonOffset,
                    false);
            }
        }

        PlaceCorridor(xPos, zPos, corridorScale, horizontal);
    }

    private void PlaceEnter(Vector3 position, bool horizontal)
    {
        DungeonSegment enterArk = Instantiate(enterSegment);
        finalRoomObjects.Add(enterArk);
        enterArk.transform.position = position;
        enterArk.transform.SetParent(transform);
    }


    private BinaryRoom[] GetAllOuterRooms()
    {
        List<BinaryRoom> outerRooms = new List<BinaryRoom>();
        for (int i = 0; i < finalRooms.Count; i++)

        {
            bool left = true;
            bool right = true;
            bool bottom = true;
            bool top = true;
            for (int j = 0; j < finalRooms.Count; j++)
            {
                if (finalRooms[i].Right < finalRooms[j].Left)
                {
                    right = false;
                    ;
                }

                if (finalRooms[i].Left > finalRooms[j].Right)
                {
                    left = false;
                }

                if (finalRooms[i].Bottom > finalRooms[j].Top)
                {
                    bottom = false;
                }

                if (finalRooms[i].Top < finalRooms[j].Bottom)
                {
                    top = false;
                }
            }

            if (left || right || bottom || top)
            {
                if (left)
                {
                    finalRooms[i].leftBorder = true;
                }

                if (right)
                {
                    finalRooms[i].rightBorder = true;
                }

                if (bottom)
                {
                    finalRooms[i].bottomBorder = true;
                }

                if (top)
                {
                    finalRooms[i].topBorder = true;
                }

                outerRooms.Add(finalRooms[i]);
            }
        }

        return outerRooms.ToArray();
    }

    private Vector3 dungeonOffset()
    {
        return transform.position - new Vector3(width * placeMuptiplayer, 0, height * placeMuptiplayer);
    }

    public Delaunay2D DelaunayGraph()
    {
        List<Vertex> vertices = new List<Vertex>();
        foreach (var room in finalRooms)
        {
            vertices.Add(new Vertex<BinaryRoom>(room.GetPosition(), room));
        }

        Delaunay2D delaunay;

        delaunay = Delaunay2D.Triangulate(vertices);
        foreach (var edge in delaunay.Edges)
        {
            Vector3 vPos = new Vector3(edge.V.Position.x, 0, edge.V.Position.y) + dungeonOffset();
            Vector3 uPos = new Vector3(edge.U.Position.x, 0, edge.U.Position.y) + dungeonOffset();
            // Debug.DrawLine(vPos,uPos,Color.red,5);
        }

        return delaunay;
    }

    private HashSet<Prim.Edge> CreateHallways(Delaunay2D delaunay)
    {
        List<Prim.Edge> edges = new List<Prim.Edge>();

        foreach (var edge in delaunay.Edges)
        {
            edges.Add(new Prim.Edge(edge.U, edge.V));
        }

        List<Prim.Edge> mst = Prim.MinimumSpanningTree(edges, edges[0].U);

        foreach (var edge in mst)
        {
            Vector3 vPos = new Vector3(edge.V.Position.x, 6, edge.V.Position.y) + dungeonOffset();
            Vector3 uPos = new Vector3(edge.U.Position.x, 6, edge.U.Position.y) + dungeonOffset();
            Debug.DrawLine(vPos, uPos, Color.blue, 25);
        }

        HashSet<Prim.Edge> selectedEdges = new HashSet<Prim.Edge>(mst);
        var remainingEdges = new HashSet<Prim.Edge>(edges);
        remainingEdges.ExceptWith(selectedEdges);

        foreach (var edge in remainingEdges)
        {
            var random = Random.Range(0, 1f);
            if (random < 0.425 &&
                (!GetRoomByPosition(edge.U.Position).IsOuter() ||
                 !GetRoomByPosition(edge.V.Position).IsOuter()))
            {
                selectedEdges.Add(edge);

                Vector3 vPos = new Vector3(edge.V.Position.x, 6, edge.V.Position.y) + dungeonOffset();
                Vector3 uPos = new Vector3(edge.U.Position.x, 6, edge.U.Position.y) + dungeonOffset();
                Debug.DrawLine(vPos, uPos, Color.yellow, 25);
            }
        }

        return selectedEdges;
    }

    private bool CheckEdgeOnOuter(Prim.Edge edge)
    {
        Vector2 u = edge.U.Position;
        Vector2 v = edge.V.Position;
        bool horizontalBottom = u.x <= maxRoomHeight / 2f && v.x <= maxRoomHeight / 2f;
        bool horizontalTop = u.x >= height - maxRoomHeight / 2f && v.x >= height - maxRoomHeight / 2f;
        bool verticalLeft = u.y <= maxRoomWidth / 2f && v.y <= maxRoomWidth / 2f;
        bool verticalRight = u.x >= width - maxRoomWidth / 2f && v.x >= width - maxRoomWidth / 2f;
        return horizontalBottom || horizontalTop || verticalLeft || verticalRight;
    }

    private void GenerateCorridorsByEdges(HashSet<Prim.Edge> edges)
    {
        foreach (var edge in edges)
        {
            BinaryRoom room1 = GetRoomByPosition(edge.U.Position);
            BinaryRoom room2 = GetRoomByPosition(edge.V.Position);

            if (RoomAreDiagonal(room1, room2))
            {
                //AddDiagonalCorridor(room1, room2);
            }
            else
            {
                AddSimpleCorridor(room1, room2);
            }
        }
    }

    private BinaryRoom GetRoomByPosition(Vector2 position)
    {
        foreach (var room in finalRooms)
        {
            if (room.GetPosition() == position)
            {
                return room;
            }
        }

        Debug.Log("Room not found at :" + position);
        return new BinaryRoom(0, 1, 0, 1);
    }

    private void GenerateCorridorsBinary(BinaryRoom upperRoom)
    {
        if (upperRoom.dawnTreeRooms != null)
        {
            GetNearestRoom(upperRoom.dawnTreeRooms[0], upperRoom.dawnTreeRooms[1]);
            GenerateCorridorsBinary(upperRoom.dawnTreeRooms[0]);
            GenerateCorridorsBinary(upperRoom.dawnTreeRooms[1]);
        }
    }

    private void GetNearestRoom(BinaryRoom firstRoom, BinaryRoom secondRoom)
    {
        BinaryRoom[] firstNestedRooms = GetAllNestedRooms(firstRoom);
        BinaryRoom[] secondNestedRooms = GetAllNestedRooms(secondRoom);

        BinaryRoom firstNearestRoom = firstRoom;
        BinaryRoom secondNearestRoom = secondRoom;
        float distance = Single.MaxValue;
        foreach (var firstNestedRoom in firstNestedRooms)
        {
            foreach (var secondNestedRoom in secondNestedRooms)
            {
                if (Vector2.Distance(firstNestedRoom.GetPosition(), secondNestedRoom.GetPosition()) < distance)
                {
                    distance = Vector2.Distance(firstNestedRoom.GetPosition(), secondNestedRoom.GetPosition());
                    firstNearestRoom = firstNestedRoom;
                    secondNearestRoom = secondNestedRoom;
                }
            }
        }

        AddSimpleCorridor(firstNearestRoom, secondNearestRoom);
    }

    private BinaryRoom[] GetAllNestedRooms(BinaryRoom room)
    {
        if (room.dawnTreeRooms != null)
        {
            BinaryRoom[] firstNestedRooms = GetAllNestedRooms(room.dawnTreeRooms[0]);
            BinaryRoom[] secondNestedRooms = GetAllNestedRooms(room.dawnTreeRooms[1]);
            return firstNestedRooms.Concat(secondNestedRooms).ToArray();
        }
        else
        {
            return new[] { room };
        }
    }

    private RoomOrientation CheckOnCorridor(BinaryRoom firstRoom, BinaryRoom secondRoom)
    {
        RoomOrientation orientation;
        if ((firstRoom.Left > secondRoom.Right || firstRoom.Right < secondRoom.Left) &&
            GetLenghtOfCovering(firstRoom.Bottom, firstRoom.Top, secondRoom.Bottom, secondRoom.Top) > corridorWidth)
        {
            orientation = RoomOrientation.Horizontal;
        }
        else if ((firstRoom.Bottom > secondRoom.Top || firstRoom.Top < secondRoom.Bottom) &&
                 GetLenghtOfCovering(firstRoom.Left, firstRoom.Right, secondRoom.Left, secondRoom.Right) >
                 corridorWidth)
        {
            orientation = RoomOrientation.Vertical;
        }
        else
        {
            orientation = RoomOrientation.Diagonal;
        }

        return orientation;
    }

    private int GetLenghtOfCovering(int firstStart, int firstEnd, int secondStart, int secondEnd)
    {
        int lenght;
        if (firstStart < secondStart)
        {
            if (firstEnd < secondEnd)
            {
                lenght = firstEnd - secondStart;
            }
            else
            {
                lenght = secondEnd - secondStart;
            }
        }
        else
        {
            if (firstEnd < secondEnd)
            {
                lenght = firstEnd - firstStart;
            }
            else
            {
                lenght = secondEnd - firstStart;
            }
        }

        return lenght;
    }

    private float GetCenterOfCovering(int firstStart, int firstEnd, int secondStart, int secondEnd)
    {
        float center;
        if (firstStart < secondStart)
        {
            center = secondStart + GetLenghtOfCovering(firstStart, firstEnd, secondStart, secondEnd) / 2f;
        }
        else
        {
            center = firstStart + GetLenghtOfCovering(firstStart, firstEnd, secondStart, secondEnd) / 2f;
        }

        return center;
    }


    private void AddDiagonalCorridor(BinaryRoom firstRoom, BinaryRoom secondRoom)
    {
        // Координати центрів квадратів
        Vector2 firstCenter = firstRoom.GetPosition();
        Vector2 secondCenter = secondRoom.GetPosition();


        // GameObject cube1 = GameObject.CreatePrimitive(PrimitiveType.Cube);
        // cube1.transform.position = new Vector3(firstCenter.x, 0, firstCenter.y) + dungeonOffset();
        //
        // GameObject cube2 = GameObject.CreatePrimitive(PrimitiveType.Cube);
        // cube2.transform.position = new Vector3(secondCenter.x, 0, secondCenter.y) + dungeonOffset();


        float distance = Vector2.Distance(firstCenter, secondCenter);


        // Визначаємо ширину та довжину прямокутника
        float corridorLength = distance - firstRoom.GetHeight / 4f - firstRoom.GetWidth / 4f
                               - secondRoom.GetHeight / 4f - secondRoom.GetWidth / 4f;

        float firstX;
        float secondX;

        float firstY;
        float secondY;
        if (firstRoom.Right < secondRoom.Left)
        {
            firstX = firstRoom.Right;
            secondX = secondRoom.Left;
        }
        else
        {
            firstX = firstRoom.Left;
            secondX = secondRoom.Right;
        }

        if (firstRoom.Top < secondRoom.Bottom)
        {
            firstY = firstRoom.Top;
            secondY = secondRoom.Bottom;
        }
        else
        {
            firstY = firstRoom.Bottom;
            secondY = secondRoom.Top;
        }

        float cornerDistance = Vector2.Distance(Vector2.zero, new Vector2(firstX - secondX, firstY - secondY));


        Vector3 corridorPosition = new Vector3((firstX + secondX) / 2f, 0, (firstY + secondY) / 2f);


        float sqrt = corridorWidth / Mathf.Sqrt(2);


        // Визначаємо кут між діагоналями квадратів
        float angle = Vector2.Angle(new Vector2(secondX - firstX + sqrt, secondY - firstY + sqrt), Vector2.right);
        if (secondCenter.y > firstCenter.y)
        {
            angle = 360f - angle;
        }

        if (firstX < secondX && firstY < secondY)
        {
            cornerDistance = Vector2.Distance(new Vector2(firstX - sqrt, firstY), new Vector2(secondX, secondY + sqrt));
        }
        else if (firstX < secondX && firstY > secondY)
        {
            cornerDistance = Vector2.Distance(new Vector2(firstX + sqrt, firstY), new Vector2(secondX, secondY + sqrt));
        }
        else if (firstX > secondX && firstY < secondY)
        {
            cornerDistance = Vector2.Distance(new Vector2(firstX - sqrt, firstY), new Vector2(secondX, secondY - sqrt));
        }
        else
        {
            cornerDistance = Vector2.Distance(new Vector2(firstX + sqrt, firstY), new Vector2(secondX, secondY - sqrt));
        }


        DungeonSegment corridor = Instantiate(cube, corridorPosition + dungeonOffset(), Quaternion.identity);
        float wallLenght = cornerDistance;


        corridor.SetParent(transform);
        corridor.SetupScale(new Vector3(cornerDistance, 10, corridorWidth));
        corridor.SetupRotation(angle);
        Vector3 firstWallLeft;
        Vector3 firstWallRight;

        Vector3 secondWallRight;
        Vector3 secondWallLeft;

        corridor.gameObject.name = "DiagonalCorridor";
        finalRoomObjects.Add(corridor);
    }

    private void AddSimpleCorridor(BinaryRoom firstRoom, BinaryRoom secondRoom)
    {
        bool horizontal = firstRoom.Left > secondRoom.Right || firstRoom.Right < secondRoom.Left;

        float zPos;
        float lenght;
        float xPos;
        Vector3 corridorScale;
        if (horizontal)
        {
            // zPos = (firstRoom.bottom + secondRoom.bottom) / 2f +
            //        ((firstRoom.top + secondRoom.top) / 2f - (firstRoom.bottom + secondRoom.bottom) / 2f) / 2;
            zPos = GetCenterOfCovering(firstRoom.Bottom, firstRoom.Top, secondRoom.Bottom, secondRoom.Top);
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
            // xPos = (firstRoom.left + secondRoom.left) / 2f +
            //        ((firstRoom.right + secondRoom.right) / 2f - (firstRoom.left + secondRoom.left) / 2f) / 2;
            xPos = GetCenterOfCovering(firstRoom.Left, firstRoom.Right, secondRoom.Left, secondRoom.Right);
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

        PlaceCorridor(xPos, zPos, corridorScale, horizontal);
    }


    private bool RoomAreDiagonal(BinaryRoom room1, BinaryRoom room2)
    {
        bool rightTop = room1.Right < room2.Left + corridorWidth && room1.Top < room2.Bottom + corridorWidth;
        bool rightBottom = room1.Right < room2.Left + corridorWidth && room1.Bottom > room2.Top - corridorWidth;

        bool leftTop = room1.Left > room2.Right - corridorWidth && room1.Top < room2.Bottom + corridorWidth;
        bool leftBottom = room1.Left > room2.Right - corridorWidth && room1.Bottom > room2.Top - corridorWidth;

        return rightTop || rightBottom || leftTop || leftBottom;
    }

    private void AddTrims()
    {
        foreach (var finalRoom in finalRooms)
        {
            finalRoom.ChangeSize(trimSize, -trimSize, trimSize, -trimSize);
        }
    }

    private void DecreaseRooms()
    {
        foreach (var finalRoom in finalRooms)
        {
            int widthMasDecreasing = (int)(finalRoom.GetWidth * maxRoomDecreasing);
            int heightMasDecreasing = (int)(finalRoom.GetHeight * maxRoomDecreasing);
            finalRoom.ChangeSize(Random.Range(0, widthMasDecreasing), -Random.Range(0, widthMasDecreasing),
                Random.Range(0, heightMasDecreasing), -Random.Range(0, heightMasDecreasing));
        }
    }

    int curIteration = 0;

    public void GenerateRooms()
    {
        if (curIteration++ > 350)
        {
            Debug.Log("Out of vax iterations");
            return;
        }

        BinaryRoom upperTreeRoom = roomsToSplit.Dequeue();
        BinaryRoom[] newRooms = SplitRoom(upperTreeRoom);
        if (newRooms.Length == 2)
        {
            upperTreeRoom.dawnTreeRooms = newRooms;

            roomsToSplit.Enqueue(newRooms[0]);
     

            GenerateRooms();

            roomsToSplit.Enqueue(newRooms[1]);
          


            GenerateRooms();
        }
        else
        {
            finalRooms.Add(newRooms[0]);
        }
    }

    private void PlaceRooms()
    {
        foreach (var finalRoom in finalRooms)
        {
            var newRoom = PlaceRoom(finalRoom);
            finalRoomObjects.Add(newRoom);
        }
    }

    private void PlaceCorridor(float xPos, float zPos, Vector3 corridorScale, bool horizontal)
    {
        Vector3 corridorPosition = new Vector3(xPos, 0, zPos);
        Vector3 dungeonOffset = new Vector3(width * placeMuptiplayer, 0, height * placeMuptiplayer);

        DungeonSegment corridor =
            dungeonPlacer.PlaceCorridor(corridorScale, corridorPosition, dungeonOffset);
        finalRoomObjects.Add(corridor);
    }

    private DungeonSegment PlaceRoom(BinaryRoom r)
    {
        Vector3 dungeonOffset = new Vector3(width * placeMuptiplayer, 0, height * placeMuptiplayer);
        DungeonSegment room = dungeonPlacer.PlaceRoom(r, dungeonOffset);
        return room;
    }

    public BinaryRoom[] SplitRoom(BinaryRoom room)
    {
        int minHeightSplit = minRoomHeight + trimSize;
        int minWidthSplit = minRoomWidth + trimSize;
        if (room.GetHeight >= room.GetWidth && room.GetHeight > minHeightSplit * 2 &&
            room.GetHeight > maxRoomHeight)
        {
            int splitHeight = Random.Range(minHeightSplit, room.GetHeight - minHeightSplit);

            BinaryRoom firstRoom = new BinaryRoom(room.Left, room.Right, room.Bottom, room.Bottom + splitHeight);
            BinaryRoom secondRoom = new BinaryRoom(room.Left, room.Right, room.Bottom + splitHeight, room.Top);
            return new[] { firstRoom, secondRoom };
        }
        else if (room.GetHeight <= room.GetWidth && room.GetWidth > minWidthSplit * 2 &&
                 room.GetWidth > maxRoomWidth)
        {
            int splitWidth = Random.Range(minWidthSplit, room.GetWidth - minWidthSplit);

            BinaryRoom firstRoom = new BinaryRoom(room.Left, room.Left + splitWidth, room.Bottom, room.Top);
            BinaryRoom secondRoom = new BinaryRoom(room.Left + splitWidth, room.Right, room.Bottom, room.Top);
            return new[] { firstRoom, secondRoom };
        }
        else
        {
            return new[] { room };
        }
    }
}