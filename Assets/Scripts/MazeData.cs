using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MazeData : MonoBehaviour
{
    public Node[,,] grid; //x,y,layer
    public List<Vector3> passages = new List<Vector3>();
    public float roomSize = 10;
    public int size = 151;
    public int numLayers = 3;
    public List<Tuple<Vector3, Vector3>> locations = new List<Tuple<Vector3, Vector3>>();
    public List<GameObject> mazeObjs = new List<GameObject>();
}
