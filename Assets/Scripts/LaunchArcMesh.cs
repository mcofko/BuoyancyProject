using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(MeshFilter))]
public class LaunchArcMesh : MonoBehaviour
{
    private MeshCollider meshCollider;
    private Mesh mesh;
    public float meshWidth;

    public float velocity;
    public float angle;
    public int resolution = 10;

    private float g; //force of gravity on the y axis
    private float radianAngle;

    private void Awake()
    {
        mesh = GetComponent<MeshFilter>().mesh;
        meshCollider = GetComponent<MeshCollider>();
        g = Mathf.Abs(Physics2D.gravity.y);
    }

    // Use this for initialization
    void Start()
    {
        MakeArcMesh(CalculateArcArray());
    }

    private void OnValidate()
    {
        if (mesh != null && Application.isPlaying)
        {
            MakeArcMesh(CalculateArcArray());
        }
    }

    // Update is called once per frame
    void Update()
    {

    }

    // 
    void MakeArcMesh(Vector3[] arcVerts)
    {
        mesh.Clear();

        Vector3[] vertices = new Vector3[(resolution + 1) * 2];
        Vector2[] uvs = new Vector2[(resolution + 1) * 2];
        // all quads must be double sided (clock-wise rotation), every quad is two triangles, 
        // and each traingle has 3 vertices
        int[] triangles = new int[resolution * 6 * 2]; 

        for (int i = 0; i < (resolution + 1); i++)
        {
            // set vertices
            vertices[i * 2] = new Vector3(meshWidth * 0.5f, arcVerts[i].y, arcVerts[i].x);
            vertices[(i * 2) + 1] = new Vector3(meshWidth * -0.5f, arcVerts[i].y, arcVerts[i].x);

            // set triangles for each quad
            if (i < resolution)
            {
                triangles[i * 12] = i * 2;
                triangles[i * 12 + 1] = triangles[i * 12 + 4] = i * 2 + 1;
                triangles[i * 12 + 2] = triangles[i * 12 + 3] = (i + 1) * 2; 
                triangles[i * 12 + 5] = (i + 1) * 2 + 1;

                triangles[i * 12 + 6] = i * 2;
                triangles[i * 12 + 7] = triangles[i * 12 + 10] = (i + 1) * 2;
                triangles[i * 12 + 8] = triangles[i * 12 + 9] = i * 2 + 1;
                triangles[i * 12 + 11] = (i + 1) * 2 + 1;
            }
        }

            // How to do the UV mapping
            uvs[0] = new Vector2(0, 0);
            uvs[1] = new Vector2(1, 0);
            uvs[2] = new Vector2(0, 1);
            uvs[3] = new Vector2(1, 1);
        

        mesh.vertices = vertices;
        mesh.triangles = triangles;
        mesh.uv = uvs;
        mesh.RecalculateNormals();
        mesh.RecalculateTangents();

        meshCollider.sharedMesh = mesh;
    }

    // create an array of Vector3 position for arc
    Vector3[] CalculateArcArray()
    {
        Vector3[] arcArray = new Vector3[resolution + 1];

        radianAngle = Mathf.Deg2Rad * angle;
        float maxDist = (velocity * velocity * Mathf.Sin(2 * radianAngle)) / g;

        for (int i = 0; i < (resolution + 1); i++)
        {
            float t = (float)i / (float)(resolution);
            arcArray[i] = CalculateArcPoint(t, maxDist);
        }

        return arcArray;
    }

    // calcualte height and distance of each vertex
    Vector3 CalculateArcPoint(float t, float distance)
    {
        float x = t * distance;
        float y = x * Mathf.Tan(radianAngle) - ((g * x * x) / (2 * velocity * velocity * Mathf.Cos(radianAngle) * Mathf.Cos(radianAngle)));

        return new Vector3(x, y);
    }
}
