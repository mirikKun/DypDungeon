using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class DungeonSegment : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI text;
    [SerializeField] private Transform planeObject;

    public void SetText(int index)
    {
        if (text)
        {
            text.text = index.ToString();
        }
    }

  
    public void SetupScale(Vector3 scale)
    {

        planeObject.localScale = scale;
    }

    public void SetupRotation(float angle)
    {
        planeObject.eulerAngles = new Vector3(0, angle, 0);
    }

    public void SetParent(Transform parent)
    {
        transform.parent = parent;
    }
}