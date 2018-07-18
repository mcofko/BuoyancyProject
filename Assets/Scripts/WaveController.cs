using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(MeshRenderer), typeof(MeshFilter), typeof(MeshCollider))]
public class WaveController : MonoBehaviour {

    private MeshRenderer waveMR;
    private MeshFilter waveMF;
    private Mesh waveMesh;
    private MeshCollider waveMC;

    public Material material;
    [Range(10, 50)]
    public int _dimension = 20;
    [Range(0.0f, 100.0f)]
    public float _frequency = 1.0f;
    [Range(0.0f, 10.0f)]
    public float _amplitude = 0.75f;
    [Range(0.0f, 30.0f)]
    public float _amplOffset = 0.0f;
    [Range(-10.0f, 10.0f)]
    public float _speed = 1.0f;
    [Range(0.0f, 1.0f)]
    public float _noise = 0.0f;
    public WaterWaveEffects.WaterEffects _waterEffectEnum = WaterWaveEffects.WaterEffects.RIPPLE;


    private float _elapsedTime;
    private SineWaveData _sineWaveData;

    private void Awake()
    {
        waveMF = GetComponent<MeshFilter>();
        waveMR = GetComponent<MeshRenderer>();
        waveMC = GetComponent<MeshCollider>();
        waveMC.convex = true;
        waveMesh = waveMF.mesh;

        _sineWaveData = new SineWaveData(_amplitude, _frequency, _amplOffset, _elapsedTime, _noise, _waterEffectEnum);
    }

    // Use this for initialization
    void Start () {
        Waves.WaveGenerator.Instance.SetSegmentation(_dimension);
        Waves.WaveGenerator.Instance.SineWaveData = _sineWaveData;

        GenerateWavesOptimized();
        UpdateWavesRenderer();
    }

    // Update is called once per frame
    void Update()
    {
        _elapsedTime += Time.deltaTime * _speed;
        _elapsedTime %= 6;

        UpdateSineWaveData();
        UpdateWaves();
        UpdateWavesRenderer();
    }

    void UpdateSineWaveData()
    {
        _sineWaveData.Amplitude = _amplitude;
        _sineWaveData.AmplOffset = _amplOffset;
        _sineWaveData.Frequency = _frequency;
        _sineWaveData.ElapsedTime = _elapsedTime;
        _sineWaveData.Noise = _noise;
        _sineWaveData.WaterEffectEnum = _waterEffectEnum;
        Waves.WaveGenerator.Instance.SineWaveData = _sineWaveData;
    }

    void UpdateWaves()
    {
        Vector3[] vertices = new Vector3[waveMesh.vertices.Length];
        int[] triangles = new int[waveMesh.triangles.Length];

        //Waves.WaveGenerator.Instance.CalculateWaveVertices(ref vertices, ref triangles, waveMesh.vertices);
        Waves.WaveGenerator.Instance.CalculateWaveVerticesOptimized(ref vertices, ref triangles, waveMesh.vertices);

        waveMesh.vertices = vertices;
        waveMesh.RecalculateNormals();
        waveMesh.RecalculateBounds();
        waveMesh.RecalculateTangents();
        // update meshFilter with new generated mesh
        waveMF.mesh = waveMesh;
    }

    void UpdateWavesRenderer()
    {
        waveMF.sharedMesh = waveMesh;
        waveMR.material = material;
        waveMC.sharedMesh = waveMesh;
    }

    void GenerateWaves()
    {
        List<Vector3> vertices1D;
        List<Vector2> uvs1D;
        List<int> triangles1D;

        Waves.WaveGenerator.Instance.CreateWaveMesh(_dimension, out vertices1D, out uvs1D, out triangles1D);

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

        Waves.WaveGenerator.Instance.CreateWaveMeshOptimized(_dimension, ref vertices, ref uvs, ref triangles);

        waveMesh.Clear();
        waveMesh.vertices = vertices.ToArray();
        waveMesh.uv = uvs.ToArray();
        waveMesh.triangles = triangles.ToArray();
        waveMesh.RecalculateNormals();
        waveMesh.RecalculateBounds();
    }
}
