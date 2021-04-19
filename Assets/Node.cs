using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Node: IComparable<Node>
{
    public bool walkable;
    public Vector3 worldPos;
    public int gridX;
    public int gridY;
    public GameObject NodeObj;
    public Node parentNode;
    public int gCost; //distance from start
    public int hCost; //distance from the end

    public Node(bool w, Vector3 wp, int gx, int gy) {
        walkable = w;
        worldPos = wp;
        gridX = gx;
        gridY = gy;
    } 

    public int fCost {
        get {
            return gCost + hCost;
        }
    }

    public int CompareTo(Node other)
    {
        // A null value means that this object is greater.
        if (other == null)
            return 1;

        else
            return this.hCost.CompareTo(other.hCost);
    }
}
