using System.Collections.Generic;
using UnityEngine;

namespace WaveFunctionCollapse
{
    [CreateAssetMenu(fileName = "WFC Rules", menuName = "WFC/Rules")]

    public class WfcGenerationRules:ScriptableObject
    {
        public Vector2Int _size;
        public List<WfcElement> _wfcElements;
        public float _tileSize;

    }
}