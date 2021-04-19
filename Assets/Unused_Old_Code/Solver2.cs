using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Solver2 : MonoBehaviour
{
    int[,] maze; //1= wall 0=open
    Queue<int> X, Y;
    bool[,] visited;
    int[] dl, dc;
    // Start is called before the first frame update
    void Start()
    {
        //X/Y direction arrays
        dl = new int[4] {-1,0,1,0};
        dc = new int[4] {0,1,0,-1};

    }

    // Update is called once per frame
    void Update()
    {
        
    }

    //before calling this make sure to push current position to the stack
    void Lee() {
        int x, y, xx, yy;
        while (X.Count != 0) {
            x = X.Peek();
            y = Y.Peek();
            for (int i = 0; i < 4; i++) {
                xx = dl[i];
                yy = dc[i];
                if (maze[xx,yy] != 1) {
                    visited[xx, yy] = true;
                    X.Enqueue(xx);
                    Y.Enqueue(yy);
                }
            }
            X.Dequeue();
            Y.Dequeue();
        }
    }

    float MD(Tuple<int, int> p1, Tuple<int, int> p2) {
        return Mathf.Abs(p1.Item1 - p2.Item1) + Mathf.Abs(p1.Item2 - p2.Item2);
    }
}
