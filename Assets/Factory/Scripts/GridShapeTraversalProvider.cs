using Pathfinding;

public class GridShapeTraversalProvider : ITraversalProvider
{
    Int2[] shape;

    public static GridShapeTraversalProvider SquareShape(int width)
    {
        if ((width % 2) != 1) throw new System.ArgumentException("only odd widths are supported");
        var shape = new GridShapeTraversalProvider();
        shape.shape = new Int2[width * width];

        // Create an array containing all integer points within a width*width square
        int i = 0;
        for (int x = -width / 2; x <= width / 2; x++)
        {
            for (int z = -width / 2; z <= width / 2; z++)
            {
                shape.shape[i] = new Int2(x, z);
                i++;
            }
        }
        return shape;
    }

    public bool CanTraverse(Path path, GraphNode node)
    {
        GridNodeBase gridNode = node as GridNodeBase;

        // Don't do anything special for non-grid nodes
        if (gridNode == null) return DefaultITraversalProvider.CanTraverse(path, node);
        int x0 = gridNode.XCoordinateInGrid;
        int z0 = gridNode.ZCoordinateInGrid;
        var grid = gridNode.Graph as GridGraph;

        // Iterate through all the nodes in the shape around the current node
        // and check if those nodes are also traversable.
        for (int i = 0; i < shape.Length; i++)
        {
            var inShapeNode = grid.GetNode(x0 + shape[i].x, z0 + shape[i].y);
            if (inShapeNode == null || !DefaultITraversalProvider.CanTraverse(path, inShapeNode)) return false;
        }
        return true;
    }

    public uint GetTraversalCost(Path path, GraphNode node)
    {
        // Use the default traversal cost.
        // Optionally this could be modified to e.g taking the average of the costs inside the shape.
        return DefaultITraversalProvider.GetTraversalCost(path, node);
    }
}
