using Pathfinding;
using UnityEngine;

public class Collider2DTraversalProvider : ITraversalProvider
{
    private Int2[] shape;

    public Collider2DTraversalProvider(int width, int height)
    {
        shape = new Int2[(width+1) * (height+1)];

        int i = 0;
        for (int x = -width / 2; x <= width / 2; x++)
        {
            for (int y = -height / 2; y <= height / 2; y++)
            {
                shape[i] = new Int2(x, y);
                i++;
            }
        }
    }

    public bool CanTraverse(Path path, GraphNode node)
    {
        var gridNode = node as GridNodeBase;
        int x0 = gridNode.XCoordinateInGrid;
        int z0 = gridNode.ZCoordinateInGrid;

        var grid = gridNode.Graph as GridGraph;

        for (int i = 0; i < shape.Length; i++)
        {
            var inShapeNode = grid.GetNode(x0 + shape[i].x, z0 + shape[i].y);
            if (inShapeNode == null || !DefaultITraversalProvider.CanTraverse(path, inShapeNode))
            {
                return false;
            }
        }

        return true;
    }

    public uint GetTraversalCost(Path path, GraphNode node)
    {
        return DefaultITraversalProvider.GetTraversalCost(path, node);
    }
}
