using System.Collections.Generic;
using UnityEngine;

public class Bresenham : MonoBehaviour
{
    // Referencia al grid para poder consultar los nodos por coordenadas
    private Grid grid;

    void Awake()
    {
        grid = GetComponent<Grid>();
    }

    public void SmoothPath(List<NodePathfinding> path)
    {
        if (path == null || path.Count < 3) return;

        for (int i = 0; i < path.Count - 2; i++)
        {
            NodePathfinding start = path[i];
            NodePathfinding end = path[i + 2];

            if (BresenhamWalkable(start, end))
            {
                path.RemoveAt(i + 1);
                i--;
            }
        }
    }

    public bool BresenhamWalkable(NodePathfinding node1, NodePathfinding node2)
    {
        int x = node1.mGridX;
        int y = node1.mGridY;

        int w = node2.mGridX - x;
        int h = node2.mGridY - y;

        int dx1 = 0, dy1 = 0, dx2 = 0, dy2 = 0;

        if (w < 0)
        {
            dx1 = -1;
        }
        else if (w > 0)
        {
            dx1 = 1;
        }

        if (h < 0)
        {
            dy1 = -1;
        }
        else if (h > 0)
        {
            dy1 = 1;
        }

        if (w < 0)
        {
            dx2 = -1;
        }
        else if (w > 0)
        {
            dx2 = 1;
        }

        int longest = Mathf.Abs(w);
        int shortest = Mathf.Abs(h);

        if (!(longest > shortest))
        {
            longest = Mathf.Abs(h);
            shortest = Mathf.Abs(w);
            if (h < 0)
            {
                dy2 = -1;
            }
            else if (h > 0)
            {
                dy2 = 1;
            }

            dx2 = 0;
        }

        int numerator = longest >> 1;

        for (int i = 0; i <= longest; i++)
        {
            NodePathfinding currentNode = grid.GetNode(x, y);

            if (!currentNode.mWalkable)
            {
                return false;
            }

            numerator += shortest;
            if (!(numerator < longest))
            {
                numerator -= longest;
                x += dx1;
                y += dy1;
            }
            else
            {
                x += dx2;
                y += dy2;
            }
        }

        return true;
    }
}