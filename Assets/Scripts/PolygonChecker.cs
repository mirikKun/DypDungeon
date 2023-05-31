using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class PolygonChecker 
{
    public static bool IsInsideTurnedPolygon(Vector2 point, Vector2[] polygon)
    {
        int numVertices = polygon.Length;
        int j = numVertices - 1;
        bool inside = false;

        for (int i = 0; i < numVertices; i++)
        {
            if (polygon[i].y < point.y && polygon[j].y >= point.y || polygon[j].y < point.y && polygon[i].y >= point.y)
            {
                if (polygon[i].x + (point.y - polygon[i].y) / (polygon[j].y - polygon[i].y) *
                    (polygon[j].x - polygon[i].x) < point.x)
                {
                    inside = !inside;
                }
            }

            j = i;
        }

        return inside;
    }
    
    
    public static bool ArePolygonsIntersecting(Vector2[] polygon1, Vector2[] polygon2)
    {
        for (int i = 0; i < polygon1.Length; i++)
        {
            Vector2 currentVertex = polygon1[i];
            Vector2 nextVertex = polygon1[(i + 1) % polygon1.Length];

            if (IsIntersectingWithPolygon(currentVertex, nextVertex, polygon2))
                return true;
        }

        return false;
    }

    public static bool IsIntersectingWithPolygon(Vector2 start, Vector2 end, Vector2[] polygon)
    {
        for (int i = 0; i < polygon.Length; i++)
        {
            Vector2 currentVertex = polygon[i];
            Vector2 nextVertex = polygon[(i + 1) % polygon.Length];

            if (AreSegmentsIntersecting(start, end, currentVertex, nextVertex))
                return true;
        }

        return false;
    }

    public static bool AreSegmentsIntersecting(Vector2 p1, Vector2 p2, Vector2 p3, Vector2 p4)
    {
        float denominator = ((p4.y - p3.y) * (p2.x - p1.x)) - ((p4.x - p3.x) * (p2.y - p1.y));

        if (denominator == 0)
            return false;

        float ua = (((p4.x - p3.x) * (p1.y - p3.y)) - ((p4.y - p3.y) * (p1.x - p3.x))) / denominator;
        float ub = (((p2.x - p1.x) * (p1.y - p3.y)) - ((p2.y - p1.y) * (p1.x - p3.x))) / denominator;

        return (ua >= 0 && ua <= 1) && (ub >= 0 && ub <= 1);
    }
}
