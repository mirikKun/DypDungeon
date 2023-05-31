using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TextureSetter : MonoBehaviour
{
    public void SetTexturesByRooms(Room[] rooms,GraphRoom[] corridors)
    {
        int minLeft = Int32.MaxValue,
            minBottom = Int32.MaxValue,
            maxRight = Int32.MinValue,
            maxTop = Int32.MinValue;
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

        int size = 900;
        Texture2D texture = new Texture2D(size, size);

// Заповнюємо текстуру білим кольором
        Color[] colors = new Color[size * size];
        for (int i = 0; i < colors.Length; i++)
        {
            colors[i] = Color.white;
        }

        texture.SetPixels(colors);

// Обчислюємо координати прямокутника


        List<Vector2[]> cornersSets = new List<Vector2[]>();
        float roomsMaxSize;
        float horizontalOffset;
        float verticalOffset;
        if ((maxRight - minLeft) > (maxTop - minBottom))
        {
            roomsMaxSize = (maxRight - minLeft);
            horizontalOffset = 0;
            verticalOffset = -(float)(maxTop - minBottom-maxRight + minLeft) / (maxRight - minLeft) * size / 2;
        }
        else
        {
            roomsMaxSize = (maxTop - minBottom);
            horizontalOffset = -(float)(maxRight - minLeft-maxTop + minBottom)/(maxTop - minBottom)   * size / 2;
            verticalOffset = 0;
        }
        
        foreach (var room in rooms)
        {
            float texLeft = (float)(room.left - minLeft) / (roomsMaxSize) * size+horizontalOffset;
            float texRight = (float)(room.right - minLeft) / (roomsMaxSize) * size+horizontalOffset;
            float texBottom = (float)(room.bottom - minBottom) / (roomsMaxSize) * size+verticalOffset;
            float texTop = (float)(room.top - minBottom) / (roomsMaxSize) * size+verticalOffset;
            
            cornersSets.Add(GetSquareCorners(texLeft, texRight, texBottom, texTop, 0));
        }        
        foreach (var room in corridors)
        {
            float texLeft = (float)(room.left - minLeft) / (roomsMaxSize) * size+horizontalOffset;
            float texRight = (float)(room.right - minLeft) / (roomsMaxSize) * size+horizontalOffset;
            float texBottom = (float)(room.bottom - minBottom) / (roomsMaxSize) * size+verticalOffset;
            float texTop = (float)(room.top - minBottom) / (roomsMaxSize) * size+verticalOffset;
            
            cornersSets.Add(GetSquareCorners(texLeft, texRight, texBottom, texTop, 90-room.angle));
            Debug.Log(room.angle);
        }

// Малюємо прямокутник на текстурі
        Color blue = Color.blue;
        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                foreach (var corners in cornersSets)
                {

                    if (PolygonChecker.IsInsideTurnedPolygon(new Vector2(x, y), corners))
                    {
                        texture.SetPixel(x, y, blue);
                    }
                }
            }
        }

// Оновлюємо текстуру
        texture.Apply();

        // присвоєння текстури площині
        GetComponent<Renderer>().sharedMaterial.mainTexture = texture;
    }

    private Vector2[] GetSquareCorners(float left, float right, float bottom, float up, float angle)
    {
        Vector2 center = new Vector2((left + right) / 2, (bottom + up) / 2);
        float width = right - left;
        float height = up - bottom;

        Vector2[] corners = new Vector2[4];
        corners[1] = center + (Vector2)(Quaternion.Euler(0f, 0f, angle) * new Vector2(-width / 2f, -height / 2f));
        corners[0] = center + (Vector2)(Quaternion.Euler(0f, 0f, angle) * new Vector2(width / 2f, -height / 2f));
        corners[2] = center + (Vector2)(Quaternion.Euler(0f, 0f, angle) * new Vector2(-width / 2f, height / 2f));
        corners[3] = center + (Vector2)(Quaternion.Euler(0f, 0f, angle) * new Vector2(width / 2f, height / 2f));
        return corners;
    }


}