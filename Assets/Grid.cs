using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Grid : MonoBehaviour
{

    public int nodeSize = 10;
    public int gridSize;
    public Node[,] grid;
    public List<Node> OPEN = new List<Node>();
    public List<Node> CLOSED = new List<Node>();
    public MazeData mg;
    Vector3 center;
    // Start is called before the first frame update
    private void Start()
    {
        
    }
    public void Setup() {
        mg = FindObjectOfType<MazeData>();
        gridSize = mg.size;
        center = new Vector3(mg.gameObject.transform.position.x + (mg.size / 2) * mg.roomSize, 1, mg.gameObject.transform.position.z + (mg.size / 2) * mg.roomSize);
    }

    public List<Node> Neighbors(Node node, int currentLayer) {
        List<Node> neighbors = new List<Node>();
        for (int x = -1; x <= 1; x++) {
            for (int y = -1; y <= 1; y++) {
                if (x == 0 && y == 0) {
                    continue;
                }
                if (x == -1 && y == -1)
                {
                    continue;
                }
                if (x == -1 && y == 1)
                {
                    continue;
                }
                if (x == 1 && y == -1)
                {
                    continue;
                }
                if (x == 1 && y == 1)
                {
                    continue;
                }
                int cX = node.gridX + x;
                int cY = node.gridY + y;

                if (cX >= 0 && cX < gridSize && cY >= 0 && cY < gridSize) { //if in bounds, add to neighbors list
                    //check if node is 1 or 0
                    if (mg.grid[cX, cY, currentLayer].walkable && !CLOSED.Contains(mg.grid[cX,cY, currentLayer]))
                    {
                        neighbors.Add(mg.grid[cX, cY, currentLayer]);
                    }
                    else { 
                    //do nothing
                    }
                    
                }
            }
        }
        return neighbors;
    }

    public Node NodeFromWorldPos(Vector3 wp, int layer) { //take a wordspace transform and convert to grid coordinate
        float gX = wp.x / nodeSize;
        float gY = wp.z / nodeSize;
        int x = Mathf.RoundToInt(gX);
        int y = Mathf.RoundToInt(gY);
        try
        {
            return mg.grid[x, y, layer];
        }
        catch (System.IndexOutOfRangeException e) {
            Debug.Log("Shits Fucked");
            return mg.grid[0, 0, 0];
        }
        
    }

    //Manhattan Distance
    public int MD(Node node1, Node node2)
    {   
        int dstX = Mathf.Abs(node1.gridX - node2.gridX);
        int dstY = Mathf.Abs(node1.gridY - node2.gridY);
     
        
        if (dstX > dstY)
        {
            return 20 * dstY + 10 * (dstX - dstY);
        }
        else {
            return 20 * dstX + 10 * (dstY - dstX);
        }
        
    }

}
