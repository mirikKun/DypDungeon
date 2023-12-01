using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using WaveFunctionCollapse;

[CreateAssetMenu(fileName = "WFC Element", menuName = "WFC/Element")]
public class WfcElement : ScriptableObject
{
    public Transform Element;
    public WfcPositioning WfcPositioning;
    [Space]

    public WfcEdgeType ForwardEdge;
    public WfcEdgeType BackEdge;
    
    public WfcEdgeType RightEdge;
    public WfcEdgeType LeftEdge;
    
    public WfcEdgeType UpperEdge;
    public WfcEdgeType BottomEdge;
    [Space]
    public VertexType BLBVertex;
    public VertexType BRBVertex;
    public VertexType FLBVertex;
    public VertexType FRBVertex;
    
    public VertexType BLUVertex;
    public VertexType BRUVertex;
    public VertexType FLUVertex;
    public VertexType FRUVertex;
}
