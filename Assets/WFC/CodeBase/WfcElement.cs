using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using WaveFunctionCollapse;

[CreateAssetMenu(fileName = "WFC Element", menuName = "WFC/Element")]
public class WfcElement : ScriptableObject
{
    public WfcPositioning WfcPositioning;
    [Space]

    public WfcEdgeType ForwardEdge;

    public WfcEdgeType RightEdge;
    public WfcEdgeType BackEdge;

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

    public WfcEdgeType[] Edges => new WfcEdgeType[]
    {
        ForwardEdge,RightEdge,BackEdge,LeftEdge,UpperEdge,BottomEdge
    };
    public VertexType[] Vertexes => new VertexType[]
    {
        BLBVertex,BRBVertex,FLBVertex,FRBVertex,BLUVertex,BRUVertex,FLUVertex,FRUVertex
    };
}
