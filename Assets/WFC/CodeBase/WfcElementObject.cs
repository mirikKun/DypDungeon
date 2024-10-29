using System;
using System.Collections.Generic;
using System.Linq;
using Sirenix.OdinInspector;
using UnityEditor;
using UnityEngine;

namespace WaveFunctionCollapse
{
    [SelectionBase]
    public class WfcElementObject : MonoBehaviour
    {
        [Range(0, 1f)] public float Weight;
        public bool NotGenerate;
        public WfcEdgeType[] Edges;
        public VertexType[] Vertexes;
        public BoxCollider[] Colliders;
        public WfcPositioning WfcPositioning;
        public int range = 5;
        public bool _staticElement;


        public HashSet<Vector2Int> ForwardTileVertexes = new HashSet<Vector2Int>();
        public HashSet<Vector2Int> RightTileVertexes = new HashSet<Vector2Int>();

        public HashSet<Vector2Int> UpTileVertexes = new HashSet<Vector2Int>();

        public HashSet<Vector2Int> BackTileVertexes = new HashSet<Vector2Int>();
        public HashSet<Vector2Int> LeftTileVertexes = new HashSet<Vector2Int>();
        public HashSet<Vector2Int> BottomTileVertexes = new HashSet<Vector2Int>();


        [ShowInInspector] public List<Vector2Int> ForwardTileVertexesList => ForwardTileVertexes.ToList();
        [ShowInInspector] public List<Vector2Int> RightTileVertexesList => RightTileVertexes.ToList();
        [ShowInInspector] public List<Vector2Int> UpTileVertexesList => UpTileVertexes.ToList();
        [ShowInInspector] public List<Vector2Int> BackTileVertexesList => BackTileVertexes.ToList();
        [ShowInInspector] public List<Vector2Int> LeftTileVertexesList => LeftTileVertexes.ToList();
        [ShowInInspector] public List<Vector2Int> BottomTileVertexesList => BottomTileVertexes.ToList();

        [Button]
        public void GetVertexes()
        {
            ForwardTileVertexes.Clear();
            RightTileVertexes.Clear();
            LeftTileVertexes.Clear();
            BackTileVertexes.Clear();
            UpTileVertexes.Clear();
            BottomTileVertexes.Clear();
            Colliders = GetComponentsInChildren<BoxCollider>();

            foreach (var elementCollider in Colliders)
            {
                Vector3 center = elementCollider.transform.localPosition + elementCollider.center;
                Vector3 halfSize = elementCollider.transform.localScale / 2f;

                for (int x = -1; x <= 1; x += 2)
                {
                    for (int y = -1; y <= 1; y += 2)
                    {
                        for (int z = -1; z <= 1; z += 2)
                        {
                            Vector3 vertexFloat = center + new Vector3(x * halfSize.x, y * halfSize.y, z * halfSize.z);

                            Vector3Int vertex = new Vector3Int(Mathf.RoundToInt(vertexFloat.x * 10),
                                Mathf.RoundToInt(vertexFloat.y * 10), Mathf.RoundToInt(vertexFloat.z * 10));
                            if (vertex.x == range)
                            {
                                RightTileVertexes.Add(new Vector2Int(vertex.z, vertex.y));
                            }

                            if (vertex.z == range)
                            {
                                ForwardTileVertexes.Add(new Vector2Int(vertex.x, vertex.y));
                            }

                            if (vertex.y == range)
                            {
                                UpTileVertexes.Add(new Vector2Int(vertex.x, vertex.z));
                            }

                            if (vertex.x == -range)
                            {
                                LeftTileVertexes.Add(new Vector2Int(vertex.z, vertex.y));
                            }

                            if (vertex.z == -range)
                            {
                                BackTileVertexes.Add(new Vector2Int(vertex.x, vertex.y));
                            }

                            if (vertex.y == -range)
                            {
                                BottomTileVertexes.Add(new Vector2Int(vertex.x, vertex.z));
                            }
                        }
                    }
                }

                RightTileVertexes = CheckOnFull(RightTileVertexes);
                ForwardTileVertexes = CheckOnFull(ForwardTileVertexes);
                UpTileVertexes = CheckOnFull(UpTileVertexes);
                LeftTileVertexes = CheckOnFull(LeftTileVertexes);
                BackTileVertexes = CheckOnFull(BackTileVertexes);
                BottomTileVertexes = CheckOnFull(BottomTileVertexes);
            }
        }

        private HashSet<Vector2Int> CheckOnFull(HashSet<Vector2Int> hashSet)
        {
            if (hashSet.Count < 4)
                return hashSet;
            int minX = hashSet.Min(point => point.x);
            int minY = hashSet.Min(point => point.y);
            int maxX = hashSet.Max(point => point.x);
            int maxY = hashSet.Max(point => point.y);

            Vector2 center = new Vector2((minX + maxX) / 2f, (minY + maxY) / 2f);

            // Ширина и высота прямоугольника
            float width = (maxX - minX) / 2f;
            float height = (maxY - minY) / 2f;
            int corners = 0;
            foreach (Vector2Int vector2 in hashSet)
            {
                Vector2 vector = vector2 - center;
                int x = (int)(Mathf.Abs(vector.x) - width);
                int y = (int)(Mathf.Abs(vector.y) - height);
                if (x == 0 && y == 0)
                {
                    corners++;
                }
            }

            if (corners == 4)
            {
                hashSet = new HashSet<Vector2Int>()
                {
                    new Vector2Int((int)(center.x + width), (int)(center.y + height)),
                    new Vector2Int((int)(center.x + width), (int)(center.y - height)),
                    new Vector2Int((int)(center.x - width), (int)(center.y + height)),
                    new Vector2Int((int)(center.x - width), (int)(center.y- height))
                };
            }

            return hashSet;
        }

        public void CopyValues(WfcElementObject wfcElementObject)
        {
            ForwardTileVertexes = wfcElementObject.ForwardTileVertexes;
            RightTileVertexes = wfcElementObject.RightTileVertexes;

            UpTileVertexes = wfcElementObject.UpTileVertexes;

            BackTileVertexes = wfcElementObject.BackTileVertexes;
            LeftTileVertexes = wfcElementObject.LeftTileVertexes;
            BottomTileVertexes = wfcElementObject.BottomTileVertexes;
        }

        private void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.green;
            Gizmos.DrawWireCube(transform.position, Vector3.one);

            Gizmos.color = Color.yellow;

            // foreach (Vector3 vertex in ForwardTileVertexes)
            // {
            //     Vector3 vertex3=new Vector3()
            //     Vector3 worldPosition = transform.TransformPoint(vertex/10f);
            //     Gizmos.DrawWireSphere(worldPosition, 0.03f);
            // }
            //
            // Gizmos.color = Color.red;
            //
            // foreach (Vector3 vertex in RightTileVertexes)
            // {
            //     Vector3 worldPosition = transform.TransformPoint(vertex/10f);
            //     Gizmos.DrawWireSphere(worldPosition, 0.03f);
            // }
            //
            // Gizmos.color = Color.blue;
            //
            // foreach (Vector3 vertex in LeftTileVertexes)
            // {
            //     Vector3 worldPosition = transform.TransformPoint(vertex/10f);
            //     Gizmos.DrawWireSphere(worldPosition, 0.03f);
            // }

            // Gizmos.color = Color.magenta;
            //
            // foreach (Vector3 vertex in BackTileVertexes)
            // {
            //     Vector3 worldPosition = transform.TransformPoint(vertex/10f);
            //     Gizmos.DrawWireSphere(worldPosition, 0.03f);
            // }
        }


        public void RotateX90()
        {
            HashSet<Vector2Int> forwardTileVertexes = new HashSet<Vector2Int>(LeftTileVertexes);
            HashSet<Vector2Int> leftTileVertexes = RevertX(BackTileVertexes);
            HashSet<Vector2Int> backTileVertexes = new HashSet<Vector2Int>(RightTileVertexes);
            HashSet<Vector2Int> rightTileVertexes = RevertX(ForwardTileVertexes);
            HashSet<Vector2Int> upTileVertexes = RotateVectors(UpTileVertexes);
            HashSet<Vector2Int> bottomTileVertexes = RotateVectors(BottomTileVertexes);

            LeftTileVertexes = leftTileVertexes;
            BackTileVertexes = backTileVertexes;
            RightTileVertexes = rightTileVertexes;
            ForwardTileVertexes = forwardTileVertexes;

            UpTileVertexes = upTileVertexes;
            BottomTileVertexes = bottomTileVertexes;
            //TODO make for Y
            transform.eulerAngles += (new Vector3(0, 90, 0));
        }


        private HashSet<Vector2Int> RevertX(HashSet<Vector2Int> oldSet)
        {
            HashSet<Vector2Int> vertexes = new HashSet<Vector2Int>();
            foreach (Vector2Int vector2Int in oldSet)
            {
                Vector2Int newVector = vector2Int;
                newVector.x = -newVector.x;
                vertexes.Add(newVector);
            }

            return vertexes;
        }

        private HashSet<Vector2Int> RevertY(HashSet<Vector2Int> oldSet)
        {
            HashSet<Vector2Int> vertexes = new HashSet<Vector2Int>();
            foreach (Vector2Int vector2Int in oldSet)
            {
                Vector2Int newVector = vector2Int;
                newVector.y = -newVector.y;
                vertexes.Add(newVector);
            }

            return vertexes;
        }

        private HashSet<Vector2Int> RotateVectors(HashSet<Vector2Int> oldSet, int count = 1)
        {
            HashSet<Vector2Int> vertexes = new HashSet<Vector2Int>();
            foreach (Vector2Int vector2Int in oldSet)
            {
                Vector2Int newVector = vector2Int;
                for (int i = 0; i < count; i++)
                {
                    newVector = new Vector2Int(newVector.y, -newVector.x);
                }

                vertexes.Add(newVector);
            }

            return vertexes;
        }

        public void RotateY180()
        {
            HashSet<Vector2Int> forwardTileVertexes = RevertY(BackTileVertexes);
            HashSet<Vector2Int> leftTileVertexes = RotateVectors(LeftTileVertexes, 2);
            HashSet<Vector2Int> backTileVertexes = RevertY(ForwardTileVertexes);
            HashSet<Vector2Int> rightTileVertexes = RotateVectors(RightTileVertexes, 2);
            HashSet<Vector2Int> upTileVertexes = RevertY(BottomTileVertexes);
            HashSet<Vector2Int> bottomTileVertexes = RevertY(UpTileVertexes);

            LeftTileVertexes = leftTileVertexes;
            BackTileVertexes = backTileVertexes;
            RightTileVertexes = rightTileVertexes;
            ForwardTileVertexes = forwardTileVertexes;

            UpTileVertexes = upTileVertexes;
            BottomTileVertexes = bottomTileVertexes;


            transform.Rotate(new Vector3(180, 0, 0));
        }


    }
}