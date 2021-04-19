
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;

public class SolverAI : MonoBehaviour
{
    //=================================== MAZE DATA =================================== 
    public MazeData mData;
    Grid grid;
    Node currentNode;
    List<Node> discoveredNodes; //nodes we know about but have not been to
    public struct NodeCost
    {
        public int hCost;
        public int gCost;
    }
    Node bestDiscoveredNode;

    //=================================== SOLVER MOVEMENT ===================================
    bool aiEnabled = false;
    public bool serriesSolver = false;
    public float movementSpeed;
    public Vector3 destinationVector;
    public Node Cost_Target;
    bool currentlyBacktracking = false;
    Queue<Node> backtrackPath;
    public int currentLayer = 0;
    //=================================== UI / UX ===================================
    public LineRenderer pathRender;
    int pathPos = 1;
    bool GotIceCream = false;
    //=================================== FUNCTIONS START ===================================
    public float startTime;
    public float endTime;
    public float estTime;
    void Start()
    {
        
    }

    public void WriteMazeDataToFile() {
        endTime = Time.time;
        string realTime = ((endTime - startTime) / 60).ToString();
        Debug.Log("Time taken: " + realTime);
        using (StreamWriter writetext = new StreamWriter("O:\\Desktop\\MazeData.csv", true))
        {
            writetext.WriteLine(mData.size.ToString() + "," + mData.numLayers.ToString() + "," + movementSpeed.ToString() + "," + estTime.ToString() + "," + realTime);
        }
    }
    public float GetTimeToSolve() {
        //est time to complete maze
        //float timeToFinish = (((Mathf.Pow(mData.size - 1, 2) * mData.roomSize) * mData.numLayers) / movementSpeed) / 60;
        float timeToFinish = (((Mathf.Pow(mData.size, 2) * 19) * mData.numLayers) / 60) / movementSpeed;
        estTime = timeToFinish;
        Debug.Log("Estimated Time to Complete Maze (Worst Case): " + timeToFinish);
        startTime = Time.time;
        return timeToFinish;

    }
    // Update is called once per frame
    void Update()
    {

        if (aiEnabled)
        {
            Vector3 pathTransform = new Vector3(transform.position.x, transform.position.y - 3, transform.position.z);
            pathRender.SetPosition(pathPos, pathTransform);
            //movement code
            if (Vector3.Distance(transform.position, destinationVector) < 0.01f)
            {
                //update lineRenderer
                pathPos += 1;
                pathRender.positionCount += 1;
                pathRender.SetPosition(pathPos, pathTransform);

                transform.position = destinationVector;

                //detecting if at end of the maze
                if (Vector3.Distance(transform.position,mData.locations[currentLayer].Item2) < 0.01f && mData.numLayers-1 > currentLayer) {
                    Debug.Log("Moving to next layer");
                    discoveredNodes.Clear();
                    //you are at the portal on the current layer, but there are more layers
                    currentLayer++;
                    mData.mazeObjs[currentLayer].GetComponent<MeshRenderer>().enabled = true;
                    Node PortalDest = grid.NodeFromWorldPos(mData.locations[currentLayer].Item1, currentLayer);
                    PortalDest.parentNode = currentNode;
                    currentNode = PortalDest;
                    transform.position = currentNode.worldPos;
                    pathPos += 1;
                    pathRender.positionCount += 1;
                    pathRender.SetPosition(pathPos, PortalDest.worldPos);
                    pathPos += 1;
                    pathRender.positionCount += 1;
                    Cost_Target = grid.NodeFromWorldPos(mData.locations[currentLayer].Item2, currentLayer);

                }
                if (Vector3.Distance(transform.position, mData.locations[currentLayer].Item2) < 0.01f && !GotIceCream)
                {
                    if (serriesSolver) {
                        FindObjectOfType<MazeController>().endRun();
                    }
                    //triggers the firts time you reach the end of the maze
                    Debug.Log("I MADE IT TO THE END, getting ice cream");
                    //WriteMazeDataToFile();
                    MazeController.script.setBannerState(2); //set the ui to state 2
                    GetTheIceCream();
                    GotIceCream = true;
                }
                else if (transform.position == mData.locations[currentLayer].Item2 && GotIceCream)
                {
                    //triggers the second time you reach the end of the maze
                    Debug.Log("I got the ice cream :)");
                    aiEnabled = false;
                    pathRender.Simplify(1);
                    
                    this.enabled = false;

                }
                if (currentlyBacktracking)
                {
                    //backtracking
                    if (backtrackPath.Count > 0)
                    {
                        destinationVector = backtrackPath.Dequeue().worldPos;
                    }
                    else
                    {
                        currentlyBacktracking = false;
                        movementSpeed /= 2;
                        UpdatePosition();
                    }
                }
                else
                {
                    //not backtracking
                    if(aiEnabled)
                        UpdatePosition();
                }
            }
            else
            {
                //not at destination yet
                transform.position = Vector3.MoveTowards(transform.position, destinationVector, movementSpeed * Time.deltaTime); //move towards destination
            }
        }
    }

    public void MoveToStartPosition()
    {
        //copied from start
        mData = FindObjectOfType<MazeData>();
        grid = FindObjectOfType<Grid>();
        grid.Setup();
        discoveredNodes = new List<Node>();
        //other code
        Debug.Log("Solver Moved to Start Position");
        transform.position = mData.locations[0].Item1;
        pathRender.positionCount = 2;
        pathRender.SetPosition(0, transform.position);
        pathRender.SetPosition(1, transform.position);
        aiEnabled = true;
        destinationVector = mData.locations[0].Item1;
        currentNode = grid.NodeFromWorldPos(mData.locations[0].Item1, currentLayer);
        Cost_Target = grid.NodeFromWorldPos(mData.locations[currentLayer].Item2, currentLayer);
        Debug.Log("Starting Position: " + currentNode.gridX + " / " + currentNode.gridY);

    }

    public void GetTheIceCream()
    {
        pathRender.positionCount = 0; //clear it out
        backtrackPath = FindIceCream();
        transform.position = mData.locations[0].Item1;
        pathRender.positionCount = 0; //clear it out
        pathRender.positionCount = 2;
        pathRender.SetPosition(0, mData.locations[0].Item1);
        pathRender.SetPosition(1, mData.locations[0].Item1);
        pathPos = 1;


        currentlyBacktracking = true;
        movementSpeed *= 2;
        destinationVector = backtrackPath.Dequeue().worldPos;




    }
    private void MoveToStart()
    {
        transform.position = mData.locations[0].Item1;

    }
    private void UpdatePosition()
    {
        currentNode = grid.NodeFromWorldPos(transform.position, currentLayer); //convert current position from World-Space to Grid-Space
        grid.CLOSED.Add(currentNode);
        if (discoveredNodes.Contains(currentNode))
        {
            discoveredNodes.Remove(currentNode);
        }
        //we are on this node so it is clearly walkable
        if (mData.grid[currentNode.gridX, currentNode.gridY, currentLayer].walkable)
        {
            currentNode.walkable = true;
        }

        //get neighbors and calculate walkable for each

        List<Node> neighboringNodes = grid.Neighbors(currentNode, currentLayer);
        foreach (Node node in neighboringNodes)
        {
            //Debug.Log("NODE: [" + node.gridX + "," + node.gridY + "] = " + mData.grid[node.gridX, node.gridY]);
            node.parentNode = currentNode;
            if (mData.grid[node.gridX, node.gridY, currentLayer].walkable)
            {
                node.walkable = true;
            }
            else
            {
                node.walkable = false;
            }
            if (node.walkable)
            {
                discoveredNodes.Add(node); //add this open node to our list of discovered nodes so we can calculate its cost
                discoveredNodes.Sort();
            }
        }
        if (neighboringNodes.Count == 0)
        {
            backtrackPath = BackTrackV2(discoveredNodes[0]);
            discoveredNodes.RemoveAt(0);
            currentlyBacktracking = true;
            movementSpeed *= 2;
            destinationVector = backtrackPath.Dequeue().worldPos;
        }
        else if (neighboringNodes.Count > 0)
        {
            calculateBestNode(neighboringNodes);
        }

        //calculate next destination

    }

    private void calculateBestNode(List<Node> neighbors)
    {
        Node targetNode = currentNode;

        //calculate best node out of directly attached neighbors
        //Step 1. Calculate cost for each neighbor and track lowest
        int lowestCost = int.MaxValue;
        foreach (Node neighbor in neighbors)
        {
            NodeCost nCost = GetNodeCost(currentNode, neighbor);
            neighbor.gCost = nCost.gCost;
            neighbor.hCost = nCost.hCost;
            if (neighbor.fCost < lowestCost)
            {
                targetNode = neighbor;
                lowestCost = neighbor.fCost;
            }
            else if (neighbor.fCost == lowestCost)
            {
                //pick closer to end than start
                if (neighbor.hCost < targetNode.hCost)
                {
                    targetNode = neighbor;
                    lowestCost = neighbor.fCost;
                }
            }
        }

        //compare this node to non-visited discovered nodes
        if (bestDiscoveredNode == null || targetNode.fCost < bestDiscoveredNode.fCost)
        {
            //set destination to targetNode
            destinationVector = targetNode.worldPos;
        }
        else
        {
            //set destination to bestDiscoveredNode
            //we use [0] because the list should be sorted with lowest fCost first
            backtrackPath = BackTrackV2(discoveredNodes[0]);
            discoveredNodes.RemoveAt(0);
            currentlyBacktracking = true;
            movementSpeed *= 2;
            destinationVector = backtrackPath.Dequeue().worldPos;
        }

        //keep track of node in list with lowest cost?
        //update the list when we actually go to that node, then calculate new best node, saves on loops
        //compare best neighbor with best previous discovered nodes


        //if best node is neighbor, set node as target destination
        //if best node is non-visited node, set target to backtrack queue to target node
    }

    Queue<Node> FindIceCream() //reverse the backTrack queue for going forwards
    {
        currentNode = grid.NodeFromWorldPos(mData.locations[currentLayer].Item2, currentLayer);
        Queue<Node> path = BackTrack(grid.NodeFromWorldPos(mData.locations[0].Item1, 0));
        Stack<Node> tmp = new Stack<Node>();
        while (path.Count > 0)
        {
           
            
            tmp.Push(path.Dequeue());
        }

        while (tmp.Count > 0)
        {
            Node tmpNode = tmp.Pop();
            Debug.Log("X: " + tmpNode.gridX + " Y: " + tmpNode.gridY);
            path.Enqueue(tmpNode);
        }
        return path;
    }

    Queue<Node> BackTrack(Node target)
    {
        Queue<Node> path = new Queue<Node>();
        Node tmp = currentNode;
        while (tmp != target && tmp.parentNode != null)
        {
            path.Enqueue(tmp.parentNode);
            tmp = tmp.parentNode;
        }
        path.Enqueue(target);
        return path;
    }

    //find path from current Node to targetNode
    Queue<Node> BackTrackV2(Node targetNode)
    {
        Node startingNode = grid.NodeFromWorldPos(mData.locations[currentLayer].Item1, currentLayer);
        //GameObject n = GameObject.CreatePrimitive(PrimitiveType.Cube);
        //n.transform.position = new Vector3(targetNode.worldPos.x, targetNode.worldPos.y + 12, targetNode.worldPos.z);
        Queue<Node> path = new Queue<Node>();
        List<Node> targetPath = new List<Node>();
        List<Node> currentPath = new List<Node>();
        Node tmp = targetNode;
        //find path from target to start
        while (tmp != startingNode && tmp.parentNode != null) 
        {
            targetPath.Add(tmp.parentNode);
            tmp = tmp.parentNode;
        }
        targetPath.Add(mData.grid[1, 1, currentLayer]);

        //find path from current to start
        tmp = currentNode;
        while (tmp != startingNode && tmp.parentNode != null)
        {
            currentPath.Add(tmp.parentNode);
            tmp = tmp.parentNode;
        }
        currentPath.Add(startingNode);

        //find point where each intsercects
        Node lastIntersect = startingNode; //the start, since both lists will make it back to the start
        int currentIndex = 0;
        int targetIndex = 0;
        for (int i = 0; i < currentPath.Count; i++)
        {
            if (targetPath.Contains(currentPath[i]))
            {
                lastIntersect = currentPath[i];
                currentIndex = i;
                targetIndex = targetPath.IndexOf(currentPath[i]);
                break;
            }
        }
        //combine the two lists
        for (int i = 0; i <= currentIndex; i++)
        {
            path.Enqueue(currentPath[i]);
        }
        path.Enqueue(lastIntersect);

        for (int i = targetIndex; i >= 0; i--)
        {
            path.Enqueue(targetPath[i]);
        }

        return path;
    }
    public Node PromisingNode()
    {
        Node bestNode = currentNode; //this maaaaaay cause some problems, idk
        int lowestCost = int.MaxValue;
        foreach (Node node in discoveredNodes)
        {
            if (node.fCost < lowestCost)
            {
                bestNode = node;
                lowestCost = node.fCost;
            }
        }
        return bestNode;
    }
    public NodeCost GetNodeCost(Node start, Node end)
    {
        NodeCost cost = new NodeCost();
        cost.gCost = start.gCost + 10;
        cost.hCost = grid.MD(end, Cost_Target);
        return cost;
    }
    public bool NodeInRange(int x, int y) {
        if (x < mData.grid.GetLength(0) && x > -1 && y < mData.grid.GetLength(1) && y > -1)
        {
            return true;
        }
        else {
            return false;
        }
    }
}
