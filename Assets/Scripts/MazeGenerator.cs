using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MazeGenerator : MonoBehaviour
{
    public MeshRenderer mr;
    public MeshFilter mf;
    public MeshCollider mc;
    public Material mat1;
    public Material mat2;
    public MazeData data;
    public bool debug = false;
    public int seed = 0;
    public bool generationFinished = false;
    public GameObject iceCreamObj;
    public GameObject taterObj;
    public GameObject portalIn;
    public GameObject portalOut;
    bool oddLayer = true;
    // Start is called before the first frame update
    void Start()
    {
        //CreateMaze(); //this will be moved to a button on the ui
        
    }

    public void StartCreateMaze() {
        StartCoroutine(CreateMaze());
    }
    public IEnumerator CreateMaze() {
        if (seed == 0)
        {
            seed = System.DateTime.Now.Millisecond;
        }
        data = GetComponent<MazeData>();
        CreateGrid(1);
        Random.InitState(seed);
        for (int i = 0; i < data.numLayers; i++) {
            StartCoroutine(GenerateMaze(i));
        }
        yield return null;
    }


    public void setSeed(string _seed) {
        try
        {
            seed = int.Parse(_seed);
        }
        catch {
            seed = 0;
        }
    }

    public void setSize(string _size)
    {
        try
        {
            data.size = int.Parse(_size);
        }
        catch
        {
            return;
        }
    }

    public void setLayers(string _size)
    {
        try
        {
            data.numLayers = int.Parse(_size);
        }
        catch
        {
            return;
        }
    }
    public void CreateGrid(int y_hieght)
    {
        float gWorldSize = data.size * data.roomSize;
        data.grid = new Node[data.size, data.size, data.numLayers];
        //Vector3 bottomLeft = center - Vector3.right * gWorldSize / 2 - Vector3.forward * gWorldSize / 2;
        
        //Debug.Log(bottomLeft);
        for (int l = 0; l < data.numLayers; l++) {
            float layerHieght = l * 10;
            layerHieght += (layerHieght * 0.1f);
            Vector3 bottomLeft = new Vector3(-5, l, -5);
            for (int x = 0; x < data.size; x++)
            {
                for (int y = 0; y < data.size; y++)
                {
                    Vector3 worldPosition = bottomLeft + Vector3.right * (x * data.roomSize + data.roomSize / 2) + Vector3.forward * (y * data.roomSize + data.roomSize / 2);
                    worldPosition.y = layerHieght;
                    //bool walkable = false; //we assume empty maze by default
                    data.grid[x, y, l] = new Node(false, worldPosition, x, y);
                    if (debug)
                    {
                        GameObject n = GameObject.CreatePrimitive(PrimitiveType.Cube);
                        n.transform.position = new Vector3(worldPosition.x, worldPosition.y, worldPosition.z);
                        n.SetActive(debug);
                        data.grid[x, y, l].NodeObj = n;
                    }
                }
            }
        }
      
    }
    private Vector2 RandomDirection() {
        Vector2 newDirection = Vector2.down;
        int d = Mathf.FloorToInt(Random.value * 3.99f);
        switch (d) {
            case 0:
                newDirection = Vector2.down;
                break;
            case 1:
                newDirection = Vector2.left;
                break;
            case 2:
                newDirection = Vector2.up;
                break;
            case 3:
                newDirection = Vector2.right;
                break;
        }
        return newDirection;
    }
    IEnumerator GenerateMaze(int layer) {
        int rMax = data.grid.GetUpperBound(0); //highest row
        int cMax = data.grid.GetUpperBound(1); //highest column
        //logic
        //RecurseHelper(1,1,data.size-2, data.size-2, ChooseDirection(data.size-2, data.size-2));
        yield return StartCoroutine(PrimsMaze(layer));
        DefineStartandEnd(layer);
        if (debug) {
            for (int l = 0; l < data.numLayers; l++) {
                for (int i = 0; i < data.grid.GetLength(0); i++)
                {
                    for (int j = 0; j < data.grid.GetLength(1); j++)
                    {
                        if (data.grid[i, j, l].walkable)
                            data.grid[i, j, l].NodeObj.GetComponent<MeshRenderer>().material.color = new Color(0, 255, 0);
                        else
                            data.grid[i, j, l].NodeObj.GetComponent<MeshRenderer>().material.color = new Color(255, 0, 0);
                    }
                }
            }
            
        }
        yield return StartCoroutine(MeshFromData(layer)); 
        yield return null;
    }


    private void DefineStartandEnd(int layer)
    {
        Vector2 min = new Vector2(int.MaxValue, int.MaxValue);
        Vector2 max = new Vector2(int.MinValue, int.MinValue);
        for (int i = 0; i < data.grid.GetLength(0); i++)
        {
            for (int a = 0; a < data.grid.GetLength(1); a++)
            {
                if (data.grid[i, a, 0].walkable) {
                    if (i <= min.x && a <= min.y) {
                        min = new Vector2(i, a);
                    }
                    if (i >= max.x && a >= max.y) {
                        max = new Vector2(i, a);
                    }
                }
            }
        }
        float y_hieght = layer * 10;
        y_hieght += (y_hieght * 0.1f);
        Vector3 start = new Vector3(min.x * data.roomSize, y_hieght, min.y * data.roomSize);
        Vector3 finish = new Vector3(max.x * data.roomSize, y_hieght, max.y * data.roomSize);
        GameObject inObj = portalIn;
        GameObject outObj = portalOut;
        if (layer == 0) {
            outObj = taterObj;
        }
        if (layer == data.numLayers - 1) {
            inObj = iceCreamObj;
        }
        if (oddLayer)
        {
            data.locations.Add(new System.Tuple<Vector3, Vector3>(start, finish));
            Debug.Log("Layer: " + layer + " Start: " + start + " Finish: " + finish);
            GameObject tmp = Instantiate(inObj, finish, portalIn.transform.rotation);
            tmp.transform.parent = gameObject.transform;
            tmp = Instantiate(outObj, start, portalOut.transform.rotation);
            tmp.transform.parent = gameObject.transform;
        }
        else {
            data.locations.Add(new System.Tuple<Vector3, Vector3>(finish, start));
            Debug.Log("Layer: " + layer + " Start: " + finish + " Finish: " + start);
            GameObject tmp = Instantiate(inObj, start, portalIn.transform.rotation);
            tmp.transform.parent = gameObject.transform;
            tmp = Instantiate(outObj, finish, portalOut.transform.rotation);
            tmp.transform.parent = gameObject.transform;
        }
        oddLayer = !oddLayer;
    }
    //prims
    IEnumerator PrimsMaze(int layer) {
        List<Vector2> maze = new List<Vector2>();
        List<Vector2> openSet = new List<Vector2>();
        maze.Add(new Vector2(1, 1)); //start of the maze
        data.grid[1, 1, layer].walkable = true;
        openSet.AddRange(getCells(new Vector2(1, 1), layer));
        //main loop
        while (openSet.Count > 0 && maze.Count > 0 ) {
            Vector2 cell = maze[Random.Range(0, maze.Count)]; //pick random cell from the list
            List<Vector2> neighbors = getCells(cell, layer);
            if (neighbors.Count > 0)
            {
                //connect to a random neighbor
                Vector2 newCell = neighbors[Random.Range(0, neighbors.Count)];
                data.grid[(int)newCell.x, (int)newCell.y, layer].walkable = true;
                Vector2 middle = new Vector2((cell.x + newCell.x) / 2, (cell.y + newCell.y) / 2);
                data.grid[(int)middle.x, (int)middle.y, layer].walkable = true; //remove connecting wall from the cells
                neighbors.Remove(newCell);
                maze.Add(newCell);
                openSet.AddRange(neighbors);
                openSet.Remove(cell);
            }
            else {
                //Debug.Log("Neighbor count of this cell is 0");
                maze.Remove(cell);
            }
        }
        Debug.Log(openSet.Count);
        yield return new WaitForSeconds(0);
    }

    private List<Vector2> getCells(Vector2 current, int layer)
    {
        List<Vector2> neighbors = new List<Vector2>();
        if (current.x - 2 > 0 && current.x < data.size-1)
            if(data.grid[(int)current.x - 2, (int)current.y, layer].walkable == false)
                neighbors.Add(new Vector2(current.x - 2, current.y));

        if (current.x + 2 < data.size-1 && current.x > 0)
            if (data.grid[(int)current.x + 2, (int)current.y, layer].walkable == false) 
                neighbors.Add(new Vector2(current.x + 2, current.y));

        if (current.y - 2 > 0 && current.y < data.size-1)
            if (data.grid[(int)current.x, (int)current.y - 2, layer].walkable == false) 
                neighbors.Add(new Vector2(current.x, current.y - 2));

        if (current.y + 2 < data.size-1 && current.y > 0)
            if (data.grid[(int)current.x, (int)current.y + 2, layer].walkable == false) 
                neighbors.Add(new Vector2(current.x, current.y + 2));
        return neighbors;
    }
 
    public IEnumerator MeshFromData(int layer) {
        float y_height = layer;
        y_height *= 10;
        y_height += (y_height * 0.1f);
        Mesh maze = new Mesh();

        maze.indexFormat = UnityEngine.Rendering.IndexFormat.UInt32;

        //3
        List<Vector3> newVertices = new List<Vector3>();
        List<Vector2> newUVs = new List<Vector2>();

        maze.subMeshCount = 2;
        List<int> floorTriangles = new List<int>();
        List<int> wallTriangles = new List<int>();

        int rMax = data.grid.GetLength(0);
        int cMax = data.grid.GetLength(1);
        float halfH = data.roomSize * .5f;

        //4
        for (int i = 0; i < rMax; i++)
        {
            for (int j = 0; j < cMax; j++)
            {
                if (data.grid[i, j, layer].walkable)
                {

                 
                        // floor
                        AddQuad(Matrix4x4.TRS(
                            new Vector3(j * data.roomSize, y_height, i * data.roomSize),
                            Quaternion.LookRotation(Vector3.up),
                            new Vector3(data.roomSize, data.roomSize, 1)
                        ), ref newVertices, ref newUVs, ref floorTriangles);
                    
                    




                    // walls on sides next to blocked grid cells

                    if (i - 1 < 0 || data.grid[i - 1, j, layer].walkable == false)
                    {
                        AddQuad(Matrix4x4.TRS(
                            new Vector3(j * data.roomSize, halfH + y_height, (i - .5f) * data.roomSize),
                            Quaternion.LookRotation(Vector3.forward),
                            new Vector3(data.roomSize, data.roomSize, 1)
                        ), ref newVertices, ref newUVs, ref wallTriangles);
                    }

                    if (j + 1 > cMax || data.grid[i, j + 1, layer].walkable == false)
                    {
                        AddQuad(Matrix4x4.TRS(
                            new Vector3((j + .5f) * data.roomSize, halfH + y_height, i * data.roomSize),
                            Quaternion.LookRotation(Vector3.left),
                            new Vector3(data.roomSize, data.roomSize, 1)
                        ), ref newVertices, ref newUVs, ref wallTriangles);
                    }

                    if (j - 1 < 0 || data.grid[i, j - 1, layer].walkable == false)
                    {
                        AddQuad(Matrix4x4.TRS(
                            new Vector3((j - .5f) * data.roomSize, halfH + y_height, i * data.roomSize),
                            Quaternion.LookRotation(Vector3.right),
                            new Vector3(data.roomSize, data.roomSize, 1)
                        ), ref newVertices, ref newUVs, ref wallTriangles);
                    }

                    if (i + 1 > rMax || data.grid[i + 1, j, layer].walkable == false)
                    {
                        AddQuad(Matrix4x4.TRS(
                            new Vector3(j * data.roomSize, halfH + y_height, (i + .5f) * data.roomSize),
                            Quaternion.LookRotation(Vector3.back),
                            new Vector3(data.roomSize, data.roomSize, 1)
                        ), ref newVertices, ref newUVs, ref wallTriangles);
                    }
                }
                else
                {
                    //roof on walls
                    if (data.grid[i, j, layer].walkable == false)
                    {
                        AddQuad(Matrix4x4.TRS(new Vector3(j * data.roomSize, (halfH * 2) + y_height, i * data.roomSize),
                            Quaternion.LookRotation(Vector3.up),
                            new Vector3(data.roomSize, data.roomSize, 1)), ref newVertices, ref newUVs, ref wallTriangles);

                        //if on edge
                        if (i == 0) {
                            AddQuad(Matrix4x4.TRS(new Vector3(j * data.roomSize, halfH + y_height, (i - .5f) * data.roomSize),
                            Quaternion.LookRotation(Vector3.back),
                            new Vector3(data.roomSize, data.roomSize, 1)), ref newVertices, ref newUVs, ref wallTriangles);

                        }
                        if (j == 0)
                        {
                            AddQuad(Matrix4x4.TRS(
                            new Vector3((j - .5f) * data.roomSize, halfH + y_height, i * data.roomSize),
                            Quaternion.LookRotation(Vector3.left),
                            new Vector3(data.roomSize, data.roomSize, 1)
                            ), ref newVertices, ref newUVs, ref wallTriangles);
                        }
                        if (i + 1 >= rMax) {
                            AddQuad(Matrix4x4.TRS(new Vector3(j * data.roomSize, halfH + y_height, (i + .5f) * data.roomSize),
                                Quaternion.LookRotation(Vector3.forward),
                                new Vector3(data.roomSize, data.roomSize, 1)), ref newVertices, ref newUVs, ref wallTriangles);
                        }
                        if (j + 1 >= cMax) {
                            AddQuad(Matrix4x4.TRS(
                            new Vector3((j + .5f) * data.roomSize, halfH + y_height, i * data.roomSize),
                            Quaternion.LookRotation(Vector3.right),
                            new Vector3(data.roomSize, data.roomSize, 1)
                            ), ref newVertices, ref newUVs, ref wallTriangles);
                        }
                    }
                }
            }
        }

        maze.vertices = newVertices.ToArray();
        maze.uv = newUVs.ToArray();

        maze.SetTriangles(floorTriangles.ToArray(), 0);
        maze.SetTriangles(wallTriangles.ToArray(), 1);

        //5
        maze.RecalculateNormals();
        GameObject mazeObj = new GameObject();
        mazeObj.transform.parent = gameObject.transform;
        mazeObj.transform.position = new Vector3(0, -5, 0);
        mazeObj.transform.rotation = Quaternion.Euler(0, -90, 0);
        mazeObj.transform.localScale = new Vector3(1, 1, 1);
        mazeObj.name = "MazeLayer_" + y_height;
        mf = mazeObj.AddComponent<MeshFilter>();
        mr = mazeObj.AddComponent<MeshRenderer>();
        mf.mesh = maze;
        Material randomMat = new Material(mat2);
        randomMat.color = Random.ColorHSV(0f, 1f, 1f, 1f, 0.5f, 1f);
        mr.materials = new Material[2] { mat1, randomMat };
        mf.mesh.OptimizeIndexBuffers();
        mf.mesh.Optimize();
        if (layer != 0) {
            mazeObj.GetComponent<MeshRenderer>().enabled = false;
        }
        data.mazeObjs.Add(mazeObj);
        yield return null;
    }

     private void AddQuad(Matrix4x4 matrix, ref List<Vector3> newVertices,
        ref List<Vector2> newUVs, ref List<int> newTriangles)
    {
        int index = newVertices.Count;

        // corners before transforming
        Vector3 vert1 = new Vector3(-.5f, -.5f, 0);
        Vector3 vert2 = new Vector3(-.5f, .5f, 0);
        Vector3 vert3 = new Vector3(.5f, .5f, 0);
        Vector3 vert4 = new Vector3(.5f, -.5f, 0);

        newVertices.Add(matrix.MultiplyPoint3x4(vert1));
        newVertices.Add(matrix.MultiplyPoint3x4(vert2));
        newVertices.Add(matrix.MultiplyPoint3x4(vert3));
        newVertices.Add(matrix.MultiplyPoint3x4(vert4));

        newUVs.Add(new Vector2(1, 0));
        newUVs.Add(new Vector2(1, 1));
        newUVs.Add(new Vector2(0, 1));
        newUVs.Add(new Vector2(0, 0));

        newTriangles.Add(index + 2);
        newTriangles.Add(index + 1);
        newTriangles.Add(index);

        newTriangles.Add(index + 3);
        newTriangles.Add(index + 2);
        newTriangles.Add(index);
    }
}
