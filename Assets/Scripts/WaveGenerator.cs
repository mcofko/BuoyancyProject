using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(MeshRenderer), typeof(MeshFilter), typeof(MeshCollider))]
public class WaveGenerator : MonoBehaviour {

    private static WaveGenerator _instance;
    public static WaveGenerator Instance
    {
        get
        {
            return _instance;
        }
    }

    private MeshRenderer waveMR;
    private MeshFilter waveMF;
    private Mesh waveMesh;
    private MeshCollider waveMC;

    public Material material;
    [Range(10, 50)]
    public int _dimension = 20;
    //[Range(20, 80)]
    private int _segmentation;
    [Range(0.1f, 3.0f)]
    public float _frequency = 1.0f;
    [Range(0.1f, 5.0f)]
    public float _amplitude = 0.75f;

    private const int DELIMETER = 10;

    //[Range(0.1f, 10.0f)]
    public float deltaTime = 0.0f;
    List<float> noiseValues;

    private void Awake()
    {
        if (Instance)
        {
            Destroy(this);
        }
        else
        {
            _instance = this;
            DontDestroyOnLoad(this);
        }


        waveMF = GetComponent<MeshFilter>();
        waveMR = GetComponent<MeshRenderer>();
        waveMC = GetComponent<MeshCollider>();
        waveMC.convex = true;
        waveMesh = waveMF.mesh;

        _segmentation = _dimension;
        if (_dimension > 35) _segmentation = _dimension * 3 / 4;
        if (_dimension > 60) _segmentation = _dimension * 1 / 2;
    }

    // Use this for initialization
    void Start () {
        //GenerateWaves();
        GenerateWavesOptimized();
        UpdateWavesRenderer();
	}
	
	// Update is called once per frame
	void Update () {
        deltaTime += Time.deltaTime * 0.5f;
        deltaTime %= 6;


        //UpdateWaves();
        //UpdateWavesOptimized();
        //UpdateWavesRenderer();
	}


    void UpdateWaves()
    {
        Vector3[] vertices = new Vector3[waveMesh.vertices.Length];
        //int[] triangles = waveMesh.triangles;
        int[] triangles = new int[waveMesh.triangles.Length];

        for (int row = 0; row < _segmentation; row++)
        {
            int rowIndex = row * (_segmentation + 1) * 2;

            for (int i = 0; i < (_segmentation + 1); i++)
            {
                float progress = (float)i / (float)_segmentation;
                float yPos = CalculatePointHeight(progress);
                float xPos = waveMesh.vertices[i * 2 + 0 + rowIndex].x;
                float zPos = waveMesh.vertices[i * 2 + 0 + rowIndex].z;

                vertices[i * 2 + 0 + rowIndex] = new Vector3(xPos, yPos, zPos);

                xPos = waveMesh.vertices[i * 2 + 1 + rowIndex].x;
                zPos = waveMesh.vertices[i * 2 + 1 + rowIndex].z;
                vertices[i * 2 + 1 + rowIndex] = new Vector3(xPos, yPos, zPos);
            }
        }

        waveMesh.vertices = vertices;
        waveMesh.RecalculateNormals();
        waveMesh.RecalculateBounds();
        waveMesh.RecalculateTangents();
        waveMF.mesh = waveMesh;
    }

    void UpdateWavesOptimized()
    {
        Vector3[] vertices = new Vector3[waveMesh.vertices.Length];
        //int[] triangles = waveMesh.triangles;
        int[] triangles = new int[waveMesh.triangles.Length];
        float progressZAxis = 0;

        for (int row = 0; row < _segmentation; row++)
        {
            int rowIndex = row * _segmentation;
            progressZAxis = (float)row / (float)_segmentation;

            for (int i = 0; i < _segmentation; i++)
            {
                float progress = (float)i / (float)DELIMETER;
                float yPos = CalculatePointHeightSpecial(progress, progressZAxis, false) + noiseValues[row * _segmentation + i] + Random.Range(-0.05f, 0.05f);
                
                float xPos = waveMesh.vertices[i + rowIndex].x;
                float zPos = waveMesh.vertices[i + rowIndex].z;

                vertices[i + rowIndex] = new Vector3(xPos, yPos, zPos);
            }
        }

        waveMesh.vertices = vertices;
        waveMesh.RecalculateNormals();
        waveMesh.RecalculateBounds();
        waveMesh.RecalculateTangents();
        //waveMF.mesh = waveMesh;
    }

    public GameObject GenerateWaveObject(int x, int y, int lenght, int depth)
    {
        GameObject wave = new GameObject("wave_" + x + y, typeof(MeshRenderer), typeof(MeshFilter), typeof(MeshCollider));
        MeshRenderer meshRenderer = wave.GetComponent<MeshRenderer>();
        return null;
    }

    void GenerateWaves()
    {
        Vector3[][] vertices = new Vector3[_segmentation][];
        Vector2[][] uvs = new Vector2[_segmentation][];
        int[][] triangles = new int[_segmentation][];

        float width = (float)_dimension / (float)_segmentation;
        float height = (float)_dimension / (float)_segmentation;
        float progress = 0.0f;

        for (int row = 0; row < _segmentation; row++) 
        {
            float zPos = (float)row * height;

            // initialize each row of 2d array
            vertices[row] = new Vector3[(_segmentation + 1) * 2];
            uvs[row] = new Vector2[(_segmentation + 1) * 2];
            triangles[row] = new int[_segmentation * 6];

            for (int i = 0; i < (_segmentation + 1); i++)
            {
                progress = (float)i / (float)_segmentation + 1.0f;
                float yPos = CalculatePointHeight(progress);
                float xPos = (float)i * width;

                vertices[row][i * 2 + 0] = new Vector3(xPos, yPos, zPos);
                vertices[row][i * 2 + 1] = new Vector3(xPos, yPos, zPos + height);

                uvs[row][i * 2 + 0] = new Vector2(0.0f, 1.0f);
                uvs[row][i * 2 + 1] = new Vector2(0.0f, 1.0f);

                if (i < _segmentation)
                {
                    triangles[row][i * 6 + 0] = i * 2 + (row * vertices[0].Length); //0
                    triangles[row][i * 6 + 1] = triangles[row][i * 6 + 4] = i * 2 + 1 + (row * vertices[0].Length);  //1
                    triangles[row][i * 6 + 2] = triangles[row][i * 6 + 3] = (i + 1) * 2 + (row * vertices[0].Length); //2
                    triangles[row][i * 6 + 5] = (i + 1) * 2 + 1 + (row * vertices[0].Length); //3
                }
            }
        }

        List<Vector3> vertices1D;
        List<Vector2> uvs1D;
        List <int> triangles1D;
        Convert2DTo1DArray(vertices, uvs, triangles, out vertices1D, out uvs1D, out triangles1D);

        waveMesh.Clear();
        waveMesh.vertices = vertices1D.ToArray();
        waveMesh.uv = uvs1D.ToArray();
        waveMesh.triangles = triangles1D.ToArray();
        waveMesh.RecalculateNormals();
        waveMesh.RecalculateBounds();
    }

    void GenerateWavesOptimized()
    {
        List<Vector3> vertices = new List<Vector3>();
        List<Vector2> uvs = new List<Vector2>();
        List<int> triangles = new List<int>();
        noiseValues = new List<float>();

        float width = (float)_dimension / (float)_segmentation;
        float height = (float)_dimension / (float)_segmentation;
        float progressXAxis = 0.0f;
        float progressZAxis = 0.0f;

        for (int row = 0; row < _segmentation; row++)
        {
            float zPos = (float)row * height;
            progressZAxis = (float)row / (float)_segmentation;

            for (int i = 0; i < _segmentation; i++)
            {
                progressXAxis = (float)i / (float)DELIMETER;
                //float yPos = CalculatePointHeightWithNoise(progress);
                float yPos = CalculatePointHeightSpecial(progressXAxis, progressZAxis, true);
                float xPos = (float)i * width;

                vertices.Add( new Vector3(xPos, yPos, zPos) );
                //vertices[row][i * 2 + 1] = new Vector3(xPos, yPos, zPos + height);
                uvs.Add( new Vector2(0.0f, 1.0f) );
            }
        }

        int numOfQuads = _segmentation * _segmentation;
        int numOfTriangles = _segmentation * _segmentation * 2;
        int[] singleTriangle = new int[6];
        //int row = 0;
        //segmentation = 5, i = 1, j = 3
        for (int i = 0; i < _segmentation - 1; i++)
        {
            for (int j = 0; j < _segmentation - 1; j++)
            {
                singleTriangle[0] = 0 + i * _segmentation + j; // 1 * 5 + 3 = 8
                singleTriangle[1] = j + (i + 1) * _segmentation; // 3 + 2 * 5 = 13
                singleTriangle[2] = (j + 1) + i * _segmentation; // 4 + 5 = 9

                singleTriangle[3] = (j + 1) + i * _segmentation; // 9
                singleTriangle[4] = j + (i + 1) * _segmentation; //13
                singleTriangle[5] = (j + 1) + (i + 1) * _segmentation; //14

                triangles.AddRange(singleTriangle);
            }

            
        }


        waveMesh.Clear();
        waveMesh.vertices = vertices.ToArray();
        waveMesh.uv = uvs.ToArray();
        waveMesh.triangles = triangles.ToArray();
        waveMesh.RecalculateNormals();
        waveMesh.RecalculateBounds();
    }

    void UpdateWavesRenderer()
    {
        waveMF.sharedMesh = waveMesh;
        waveMR.material = material;
        waveMC.sharedMesh = waveMesh;
    }

    void Convert2DTo1DArray(Vector3[][] vertices, Vector2[][] uvs, int[][] triangles, out List<Vector3> vertices1D, out List<Vector2> uvs1D, out List<int> triangles1D)
    {
        vertices1D = new List<Vector3>();
        uvs1D = new List<Vector2>();
        triangles1D = new List<int>();

        int rows = vertices.Length;
        for (int i = 0; i < rows; i++)
        {
            vertices1D.AddRange(vertices[i]);
            uvs1D.AddRange(uvs[i]);
            triangles1D.AddRange(triangles[i]);
        }
    }


    //************** SINE FUNCTION ******************************************
    //********* y = sin (x) ********** Amplitude => y = a * sin (x) *********
    //******************************** Frequency => y = sin (a * x) *********
    //******************************** Move curve => y = sin (a + x) ********
    //***********************************************************************
    float CalculatePointHeight(float progress)
    {
        float y = _amplitude * Mathf.Sin(_frequency * progress * Mathf.PI + deltaTime * Mathf.PI);

        return y;
    }

    float CalculatePointHeightWithNoise(float progress)
    {
        float y = _amplitude * Mathf.Sin(_frequency * progress * Mathf.PI + deltaTime * Mathf.PI);

        float noise = Random.Range(-0.1f, 0.1f);
        noiseValues.Add(noise);

        y += noise;
        

        return y;
    }

    float CalculatePointHeightSpecial(float progressXAxis, float progressZAxis, bool includeNoise)
    {


        float y = _amplitude * Mathf.Sin(_frequency * progressXAxis * Mathf.PI + deltaTime * Mathf.PI + progressZAxis * Mathf.PI);
        float noise = 0;
        if (includeNoise)
        {
            noise = Random.Range(-0.1f, 0.1f);
            noiseValues.Add(noise);
        }
        
        y += noise;

        return y;
    }
}
