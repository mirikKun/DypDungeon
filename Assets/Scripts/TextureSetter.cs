using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TextureSetter : MonoBehaviour
{
    public void SetTexturesByRooms(Room[] rooms,GraphRoom[] corridors,Color dungeon,Color background)
    {
        Texture2D texture =GetTexture(rooms, corridors, dungeon, background);
        GetComponent<Renderer>().sharedMaterial.mainTexture = texture;
        
    }
    public static void  SavePng(Room[] rooms,GraphRoom[] corridors,Color dungeon,Color background)
    {
        Texture2D texture =GetTexture(rooms, corridors, dungeon, background);
        SaveTextureAsPNG(texture,"C:\\Users\\Intel\\Desktop\\pHOTOS\\dTex.png" );
        
    }
    private static Texture2D GetTexture(Room[] rooms,GraphRoom[] corridors,Color dungeon,Color background)
    {
            int minLeft = Int32.MaxValue,
            minBottom = Int32.MaxValue,
            maxRight = Int32.MinValue,
            maxTop = Int32.MinValue;
        foreach (var room in rooms)
        {
            if (room.Left < minLeft)
            {
                minLeft = room.Left;
            }

            if (room.Right > maxRight)
            {
                maxRight = room.Right;
            }

            if (room.Bottom < minBottom)
            {
                minBottom = room.Bottom;
            }

            if (room.Top > maxTop)
            {
                maxTop = room.Top;
            }
        }

        int size = 900;
        Texture2D texture = new Texture2D(size, size);

// Заповнюємо текстуру білим кольором
        Color[] colors = new Color[size * size];
        for (int i = 0; i < colors.Length; i++)
        {
            colors[i] = background;
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
            float texLeft = (float)(room.Left - minLeft) / (roomsMaxSize) * size+horizontalOffset;
            float texRight = (float)(room.Right - minLeft) / (roomsMaxSize) * size+horizontalOffset;
            float texBottom = (float)(room.Bottom - minBottom) / (roomsMaxSize) * size+verticalOffset;
            float texTop = (float)(room.Top - minBottom) / (roomsMaxSize) * size+verticalOffset;
            
            cornersSets.Add(PolygonChecker.GetSquareCorners(texLeft, texRight, texBottom, texTop, 0));
        }        
        foreach (var room in corridors)
        {
            float texLeft = (float)(room.Left - minLeft) / (roomsMaxSize) * size+horizontalOffset;
            float texRight = (float)(room.Right - minLeft) / (roomsMaxSize) * size+horizontalOffset;
            float texBottom = (float)(room.Bottom - minBottom) / (roomsMaxSize) * size+verticalOffset;
            float texTop = (float)(room.Top - minBottom) / (roomsMaxSize) * size+verticalOffset;
            
            cornersSets.Add(PolygonChecker.GetSquareCorners(texLeft, texRight, texBottom, texTop, 90-room.angle));
        }

// Малюємо прямокутник на текстурі
        Color blue = dungeon;
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
        return texture;
    }
    public static void SaveTextureAsPNG(Texture2D texture, string fullPath)
    {
        byte[] _bytes =texture.EncodeToPNG();
        System.IO.File.WriteAllBytes(fullPath, _bytes);
        Debug.Log(_bytes.Length/1024  + "Kb was saved as: " + fullPath);
    }


}