using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WavePerlinNoise : MonoBehaviour {

    public float scale = 2.0f;
    public float speed = 1.0f;
    public float resolution = 1.0f;
    public float offset = 1.0f;
    public float waveSpeed = 1.0f;
    public float waveHeigth = 1.0f;

    Mesh mesh;
    MeshCollider mc;

    Vector3[] vertices;
    Vector3[] newVertices;

    float randomizer = 1.0f;

	// Use this for initialization
	void Start () {
        mc = GetComponent<MeshCollider>();
        mesh = GetComponent<MeshFilter>().mesh;
	}
	
	// Update is called once per frame
	void Update () {
        randomizer = -Time.time / 2;
        vertices = mesh.vertices;
        newVertices = new Vector3[vertices.Length];

        if (resolution < 0)
        {
            resolution = 0;
        }

        for (int i = 0; i < vertices.Length; i++)
        {
            Vector3 vert = vertices[i];
            vert.y = Mathf.PerlinNoise ( (speed * randomizer) + (vertices[i].x + transform.position.x) / resolution, 
                                        -(speed * randomizer) + (vertices[i].z + transform.position.z) / resolution ) * scale;
            //vert.y = 0;
            newVertices[i] = vert;
        }

        mesh.vertices = newVertices;
        mesh.RecalculateBounds();
        mesh.RecalculateNormals();

        //mc.sharedMesh = null;
        //mc.sharedMesh = mesh;
	}

    float GeneratePerlinNoiseOnY(float vertX, float vertZ)
    {
        float y = 0;

        float pX = (vertX * scale) + (Time.timeSinceLevelLoad * waveSpeed) + offset;
        float pZ = (vertZ * scale) + (Time.timeSinceLevelLoad * waveSpeed) + offset;
        y = Mathf.PerlinNoise(pX, pZ) * waveHeigth;

        return y;
    }
}
