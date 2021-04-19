using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class Maze : MonoBehaviour
{
    public MeshFilter mf;
    public MeshRenderer mr;
    public MeshCollider mc;
    public Material mat1;
    public Material mat2;
    public Text mazeOutputStr;
    public float placementThreshold = 0.1f;
    public bool mazeDone = false;
    public int size = 11;
    public float roomSize = 10;
    int seed = 0;
    int[,] grid;

    int N = 1;
    int S = 2;
    int E = 4;
    int W = 8;
   
    Dictionary<int, int> DX = new Dictionary<int, int>();
    Dictionary<int, int> DY = new Dictionary<int, int>();
    Dictionary<int, int> OPP = new Dictionary<int, int>();

    public Vector3 startPos;
    public Vector3 finalPos;
    public int startR, startC, endR, endC;
    // Start is called before the first frame update
    void Start()
    {
        grid = new int[size, size];
        DX.Add(E, 1);
        DX.Add(W, -1);
        DX.Add(N,0);
        DX.Add(S,0);

        DY.Add(E,0);
        DY.Add(W,0);
        DY.Add(N,-1);
        DY.Add(S,1);

        OPP.Add(E,W);
        OPP.Add(W,E);
        OPP.Add(N,S);
        OPP.Add(S,N);
        GenerateMaze();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void GenerateMaze() {
        grid = FromDimensions();
        Mesh mazeMesh = FromData(grid);
        mf.mesh = mazeMesh;
        mr.materials = new Material[2] { mat1, mat2 };
        mc.sharedMesh = mf.mesh;
        SetStartandEndPos();

    }

    public int[,] FromDimensions()    // 2
    {
        int[,] maze = new int[size, size];
        int rMax = maze.GetUpperBound(0);
        int cMax = maze.GetUpperBound(1);

        for (int i = 0; i <= rMax; i++)
        {
            for (int j = 0; j <= cMax; j++)
            {
                //1
                if (i == 0 || j == 0 || i == rMax || j == cMax)
                {
                    maze[i, j] = 1;
                }

                //2
                else if (i % 2 == 0 && j % 2 == 0)
                {
                    if (Random.value > placementThreshold)
                    {
                        //3
                        maze[i, j] = 1;

                        int a = Random.value < .5 ? 0 : (Random.value < .5 ? -1 : 1);
                        int b = a != 0 ? 0 : (Random.value < .5 ? -1 : 1);
                        maze[i + a, j + b] = 1;
                    }
                }
            }
        }
        return maze;
    }

    public Mesh FromData(int[,] data)
    {
        Mesh maze = new Mesh();
        maze.indexFormat = UnityEngine.Rendering.IndexFormat.UInt32;

        //3
        List<Vector3> newVertices = new List<Vector3>();
        List<Vector2> newUVs = new List<Vector2>();

        maze.subMeshCount = 2;
        List<int> floorTriangles = new List<int>();
        List<int> wallTriangles = new List<int>();

        int rMax = data.GetUpperBound(0);
        int cMax = data.GetUpperBound(1);
        float halfH = roomSize * .5f;

        //4
        for (int i = 0; i <= rMax; i++)
        {
            for (int j = 0; j <= cMax; j++)
            {
                if (data[i, j] != 1)
                {
                    // floor
                    AddQuad(Matrix4x4.TRS(
                        new Vector3(j * roomSize, 0, i * roomSize),
                        Quaternion.LookRotation(Vector3.up),
                        new Vector3(roomSize, roomSize, 1)
                    ), ref newVertices, ref newUVs, ref floorTriangles);




                    // walls on sides next to blocked grid cells

                    if (i - 1 < 0 || data[i - 1, j] == 1)
                    {
                        AddQuad(Matrix4x4.TRS(
                            new Vector3(j * roomSize, halfH, (i - .5f) * roomSize),
                            Quaternion.LookRotation(Vector3.forward),
                            new Vector3(roomSize, roomSize, 1)
                        ), ref newVertices, ref newUVs, ref wallTriangles);
                    }

                    if (j + 1 > cMax || data[i, j + 1] == 1)
                    {
                        AddQuad(Matrix4x4.TRS(
                            new Vector3((j + .5f) * roomSize, halfH, i * roomSize),
                            Quaternion.LookRotation(Vector3.left),
                            new Vector3(roomSize, roomSize, 1)
                        ), ref newVertices, ref newUVs, ref wallTriangles);
                    }

                    if (j - 1 < 0 || data[i, j - 1] == 1)
                    {
                        AddQuad(Matrix4x4.TRS(
                            new Vector3((j - .5f) * roomSize, halfH, i * roomSize),
                            Quaternion.LookRotation(Vector3.right),
                            new Vector3(roomSize, roomSize, 1)
                        ), ref newVertices, ref newUVs, ref wallTriangles);
                    }

                    if (i + 1 > rMax || data[i + 1, j] == 1)
                    {
                        AddQuad(Matrix4x4.TRS(
                            new Vector3(j * roomSize, halfH, (i + .5f) * roomSize),
                            Quaternion.LookRotation(Vector3.back),
                            new Vector3(roomSize, roomSize, 1)
                        ), ref newVertices, ref newUVs, ref wallTriangles);
                    }
                }
                else {
                    //roof on walls
                    if (data[i, j] == 1)
                    {
                        AddQuad(Matrix4x4.TRS(new Vector3(j * roomSize, halfH*2, i * roomSize),
                            Quaternion.LookRotation(Vector3.up),
                            new Vector3(roomSize, roomSize, 1)), ref newVertices, ref newUVs, ref wallTriangles);
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

        return maze;
    }

    //1, 2
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

    void SetStartandEndPos() {
        int[,] maze = grid;
        int rMax = maze.GetUpperBound(0);
        int cMax = maze.GetUpperBound(1);

        //start position
        for (int i = 0; i < rMax; i++) {
            for (int a = 0; a < cMax; a++) {
                if (maze[i, a] == 0) {
                    startR = i;
                    startC = a;
                    break;
                }
            }
        }

        //end position
        for (int i = rMax; i >= 0; i--) {
            for (int a = cMax; a >= 0; a--) {
                if (maze[i, a] == 0) {
                    endR = i;
                    endC = a;
                    break;
                }
            }
        }

        //transform into worldspace
        GameObject finishLine = GameObject.CreatePrimitive(PrimitiveType.Cube);
        finishLine.transform.position = new Vector3(endR * roomSize, 1f, endC * roomSize);
        finishLine.tag = "Finish";
        finishLine.name = "MazeEnd";
        finalPos = finishLine.transform.position;
        finishLine.GetComponent<BoxCollider>().isTrigger = true;
        finishLine.GetComponent<BoxCollider>().enabled = false;//needs to be removed
        startPos = new Vector3(startC * roomSize, 1f, startR * roomSize);
        //SmartSolver s = FindObjectOfType<SmartSolver>();
        //s.MoveToStartPosition(new Vector3(startR * roomSize, 1f, startC * roomSize));
        //s.endPos = finishLine.transform.position;
    }

}
