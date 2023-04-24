using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DungeonSegment : MonoBehaviour
{
    private Transform _transform;

    public void SetupScale(Vector3 scale)
    {
        _transform = transform;

        _transform.localScale = scale;
    }

    public void SetupRotation(float angle)
    {
        _transform.eulerAngles = new Vector3(0, angle, 0);
    }

    public void SetParent(Transform parent)
    {
        transform.parent = parent;
    }


}