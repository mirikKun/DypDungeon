
using UnityEngine;

public class DungeonPlacer : MonoBehaviour
{   

    public void RemoveEverything()
    {
        int childCount = transform.childCount;
        for (int i = 0; i < childCount; i++)
        {
            DestroyImmediate(transform.GetChild(0).gameObject);
        }

    }
}
