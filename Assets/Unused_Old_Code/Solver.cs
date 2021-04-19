using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
public class Solver : MonoBehaviour
{
    //visual aids
    LineRenderer lr;
    public Text txt;
    public float speed;
    public bool enabled = false;
    //algorithm vars
    int[,] mazeData;
    bool[,] pastPositions;
    int[,] floodData;
    public Vector3 target;
    public Vector3 end;
    public int CR, CC, TR, TC;
    public bool canMove = true;
    Tuple<int, int> currentPosition;
    Tuple<int, int> startPosition;
    Tuple<int, int> endPosition;
    Maze mg;
    //sensors
    public float sensorMaxDistance = 10;
    float[] sensors;
    Vector3[] directions = new Vector3[4];
    int layerMask;
    // Start is called before the first frame update
    void Start()
    {
        sensors = new float[4];
        layerMask = 1 << 8;
        layerMask = ~layerMask;
        mg = FindObjectOfType<Maze>();
        mazeData = new int[mg.size, mg.size];
    }

    private void FixedUpdate()
    {
        UpdateSensors();
        if (enabled) {
            UpdateMazeData();
            if (target != null && canMove)
            {
                if (Vector3.Distance(transform.position, target) < 0.001f)
                {
                    transform.position = target;
                    Debug.Log("Reached Destination");
                    canMove = false;
                    if (Vector3.Distance(target, end) < 1f)
                    {
                        Debug.Log("Maze Solved");
                    }
                    else {
                        Logic();
                    }
                    
                }
                else
                {
                    //Debug.Log("Moving");
                    transform.position = Vector3.MoveTowards(transform.position, target, speed * Time.deltaTime);
                }
            }
        } //end if enabled
    } //end fixed update

    public void MoveToStartPosition() {
        startPosition = new Tuple<int, int>(mg.startR, mg.startC);
        endPosition = new Tuple<int, int>(mg.endR, mg.endC);
        transform.position = new Vector3(startPosition.Item1 * mg.roomSize, 0.5f, startPosition.Item2 * mg.roomSize);
        currentPosition = new Tuple<int, int>(startPosition.Item1, startPosition.Item2);
        target = transform.position;
        end = new Vector3(endPosition.Item1 * mg.roomSize, 0.5f, endPosition.Item2 * mg.roomSize);
        floodData = new int[mazeData.GetLength(0), mazeData.GetLength(1)];
    }
    void Logic() {
        bool[,] visited = new bool[mazeData.GetLength(0), mazeData.GetLength(1)];
        
        FloodFill(mazeData, floodData, visited, endPosition.Item1, endPosition.Item2, mazeData.GetLength(0), mazeData.GetLength(1), 0);
        int min = int.MaxValue;
        int targetX = 0;
        int targetY = 0;

        if (floodData[currentPosition.Item1+1, currentPosition.Item2] < min)
        {
            min = floodData[currentPosition.Item1, currentPosition.Item2];
            targetX = currentPosition.Item1+1;
            targetY = currentPosition.Item2;
        }
        if (floodData[currentPosition.Item1, currentPosition.Item2+1] < min)
        {
            min = floodData[currentPosition.Item1, currentPosition.Item2];
            targetX = currentPosition.Item1;
            targetY = currentPosition.Item2+1;
        }
        if (floodData[currentPosition.Item1, currentPosition.Item2-1] < min)
        {
            min = floodData[currentPosition.Item1, currentPosition.Item2];
            targetX = currentPosition.Item1;
            targetY = currentPosition.Item2-1;
        }
        if (floodData[currentPosition.Item1-1, currentPosition.Item2] < min)
        {
            min = floodData[currentPosition.Item1, currentPosition.Item2];
            targetX = currentPosition.Item1-1;
            targetY = currentPosition.Item2;
        }

        target = new Vector3(targetX * mg.roomSize, 0.5f, targetY * mg.roomSize);
        currentPosition = new Tuple<int, int>(targetX, targetY);
        canMove = true;

        
        
    }
    void UpdateMazeData()
    {
        //check for walls around us
        bool[] walls = new bool[4];
        for (int i = 0; i < sensors.Length; i++)
        {
            walls[i] = (sensors[i] >= 0) ? true : false;
        }
        //update past positions
        //pastPositions[currentPosition.Item1, currentPosition.Item2] = true;

        //update mazeData with the walls we see
        mazeData[currentPosition.Item1 + 1, currentPosition.Item2] = (walls[1]) ? 1 : 0; //front
        mazeData[currentPosition.Item1, currentPosition.Item2 - 1] = (walls[0]) ? 1 : 0; //left
        mazeData[currentPosition.Item1, currentPosition.Item2 + 1] = (walls[3]) ? 1 : 0; //right
        mazeData[currentPosition.Item1 - 1, currentPosition.Item2] = (walls[2]) ? 1 : 0; //back

        txt.text = "";
        for (int i = 0; i < mazeData.GetLength(0); i++)
        {
            for (int a = 0; a < mazeData.GetLength(1); a++)
            {
                txt.text += " " + mazeData[i, a];
            }
            txt.text += "\n";
        }
    }

    static void FloodFill(int[,] mData, int[,] data, bool[,] visited, int x, int y, int width, int height, int distance) {
        if (x < 0 || x > width-1 || y < 0 || y > height-1) return;
        //quit if been there before
        if (visited[x, y]) return;
        visited[x, y] = true;
        if (data[x, y] == 1) return;

        //visit places that are open
        if (mData[x, y] == 0) data[x, y] = distance;

        //recurse
        FloodFill(mData, data, visited, x + 1, y, width, height, distance+1);
        FloodFill(mData, data, visited, x - 1, y, width, height, distance+1);
        FloodFill(mData, data, visited, x, y + 1, width, height, distance+1);
        FloodFill(mData, data, visited, x, y - 1, width, height, distance+1);

    }



    void UpdateSensors()
    {
        directions[0] = transform.forward;//forward
        directions[1] = -transform.right; //left
        directions[2] = transform.right; //right
        directions[3] = -transform.forward;//back
        for (int i = 0; i < sensors.Length; i++)
        {
            RaycastHit hit;
            if (Physics.Raycast(transform.position, directions[i], out hit, sensorMaxDistance, layerMask))
            {
                sensors[i] = hit.distance;
                Color rayColor = Color.red;
                if (i == 0)
                {
                    rayColor = Color.blue;
                }
                Debug.DrawRay(transform.position, directions[i] * hit.distance, rayColor);
            }
            else
            {
                sensors[i] = -1;
                Color rayColor = Color.green;
                if (i == 0)
                {
                    rayColor = Color.magenta;
                }
                Debug.DrawRay(transform.position, directions[i] * sensorMaxDistance, rayColor);
            }
        }
    }


}
