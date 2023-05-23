using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TextureSetter : MonoBehaviour
{

    public void SetTexturesByRooms(Room[] rooms)
    {
        int minLeft=Int32.MaxValue, minBottom=Int32.MaxValue,
            maxRight=Int32.MinValue, maxTop=Int32.MinValue;
        foreach (var room in rooms)
        {
            if (room.left < minLeft)
            {
                minLeft = room.left;
            }
            if (room.right > maxRight)
            {
                maxRight = room.right;
            }
            if (room.bottom < minBottom)
            {
                minBottom = room.bottom;
            }
            if (room.top > maxTop)
            {
                maxTop = room.top;
            }
        }
        Texture2D texture = new Texture2D(500, 500);
        
        for (int i = 0; i < rooms.Length; i++)
        {
                // dungeonPlacer.PlaceRoom(rooms[i], Vector3.zero, i);
                // for (int j = i + 1; j < roomCount; j++)
                // {
                //     if (graph[i, j] == 1 && rooms[j].placed)
                //     {
                //         
                //         dungeonPlacer.PlaceCorridor(1, rooms[i].GetPosition(), rooms[j].GetPosition(), Vector3.zero);
                //     }
                // }
        }
        for (int x = 0; x < texture.width; x++)
        {
            for (int y = 0; y < texture.width ; y++)
            {
                texture.SetPixel(x, y, Color.black);
            }
        }
        
    }
    void Start()
    {
        Texture2D texture = new Texture2D(500, 500);

// Заповнюємо текстуру білим кольором
        Color[] colors = new Color[500 * 500];
        for (int i = 0; i < colors.Length; i++)
        {
            colors[i] = Color.white;
        }
        texture.SetPixels(colors);

// Обчислюємо координати прямокутника
        Vector2 center = new Vector2(250, 250);
        float width = 100f;
        float height = 300f;
        float angle = 90;

        Vector2[] corners = new Vector2[4];
        corners[1] = center + (Vector2)(Quaternion.Euler(0f, 0f, angle) * new Vector2(-width / 2, -height / 2));
        corners[0] = center + (Vector2)(Quaternion.Euler(0f, 0f, angle) * new Vector2(width / 2, -height / 2));
        corners[2] = center + (Vector2)(Quaternion.Euler(0f, 0f, angle) * new Vector2(-width / 2, height / 2));
        corners[3] = center + (Vector2)(Quaternion.Euler(0f, 0f, angle) * new Vector2(width / 2, height / 2));

// Малюємо прямокутник на текстурі
        Color blue = Color.blue;
        for (int y = 0; y < 500; y++)
        {
            for (int x = 0; x < 500; x++)
            {
                if (IsInsideTurnedPolygon(new Vector2(x, y), corners))
                {
                    texture.SetPixel(x, y, blue);
                }
            }
        }

// Оновлюємо текстуру
        texture.Apply();

        // присвоєння текстури площині
        GetComponent<Renderer>().material.mainTexture = texture;
    }

    // private bool IsInsidePolygon(Vector2 point,Room room)
    // {
    //     
    // }
    private bool CheckOnCorridor(Vector2 room1,Vector2 room2, int width)
    {
        Vector2 positionBetween =  (room1 + room2)/2;
        return true;
        ////////////////////////////////////////////////////////////
    }
    private bool IsInsideTurnedPolygon(Vector2 point, Vector2[] polygon)
    {
        int numVertices = polygon.Length;
        int j = numVertices - 1;
        bool inside = false;

        for (int i = 0; i < numVertices; i++)
        {
            if (polygon[i].y < point.y && polygon[j].y >= point.y || polygon[j].y < point.y && polygon[i].y >= point.y)
            {
                if (polygon[i].x + (point.y - polygon[i].y) / (polygon[j].y - polygon[i].y) * (polygon[j].x - polygon[i].x) < point.x)
                {
                    inside = !inside;
                }
            }
            j = i;
        }

        return inside;
    }
}
