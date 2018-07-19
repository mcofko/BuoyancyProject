using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(LineRenderer), typeof(Rigidbody))]
public class CubeController : MonoBehaviour
{

    [SerializeField]
    private GameObject lineRendPrefab;
    [SerializeField]
    private GameObject planePrefab;
    [SerializeField]
    private GameObject spherePrefab;
    [SerializeField]
    private GameObject wavesObj;


    private Mesh _waveMesh;
    private LineRenderer _lineRend;
    private Rigidbody _cubeBody;
    private BoxCollider _boxCollider;

    private float _waveWaterLevel = -100f;
    private Vector3 _buoyancyForce = new Vector3(0f, 0.0f, 0f);
    private Vector3 _currentBuoyancyForce;

    private Vector3[] _cubeVerticesLS = new Vector3[8];
    private Vector3[] _cubeVerticesWS = new Vector3[8];
    private List<Vector3> _surfacePointsPushingDown;
    private Vector3 _vec1FacinfSurface;
    private Vector3 _vec2FacinfSurface;

    // Constants
    private const float WATER_DENSITY = 0.4f;
    private const float CUBE_DENSITY = 0.3f;

    // helper variables to allow easier testing
    //helper sphere points
    private GameObject[] linePrefabs = new GameObject[3];
    private GameObject[] spheres = new GameObject[3];
    private Vector3 startPos;
    private float duration = 16.0f;
    private float elapsedTime = 0.0f;
    private Vector3 startVelocity = new Vector3(-6.0f, 0.0f, 0.0f);

    private void Awake()
    {
        _lineRend = GetComponent<LineRenderer>();
        _cubeBody = GetComponent<Rigidbody>();
        _boxCollider = GetComponent<BoxCollider>();
        _surfacePointsPushingDown = new List<Vector3>();


        // helper stuff
        startPos = transform.position;
        for (int i = 0; i < spheres.Length; i++)
        {
            spheres[i] = Instantiate(spherePrefab, Vector3.zero, Quaternion.identity);
        }
        for (int i = 0; i < linePrefabs.Length; i++)
        {
            linePrefabs[i] = Instantiate(lineRendPrefab, Vector3.zero, Quaternion.identity);
            linePrefabs[i].transform.parent = gameObject.transform;
        }
    }

    // Use this for initialization
    void Start()
    {
        //add start speed to cube
        _cubeBody.velocity = startVelocity;
        _waveMesh = wavesObj.GetComponent<MeshFilter>().sharedMesh;

        CalculateBuoyancyForce();
        UpdateCubeVertices(true);
    }

    // Update is called once per frame
    void Update()
    {
        //UpdateCubeVertices(false);
        //FindWaterLevelTriggerPoint();

        //CalculateWaterForcePointsPushingOnSurface();
        //Vector3 pivotPoint = UpdateCubesPivotPointOptimized();
        //ApplyWaterForce(pivotPoint);

        //RestartCubesPosition();
    }

    // water forces are need to constatly affect on the object in the water
    // that is the main reason of moving methods from Update() to FixedUpdate()
    private void FixedUpdate()
    {
        UpdateCubeVertices(false);
        FindWaterLevelTriggerPoint();

        CalculateWaterForcePointsPushingOnSurface();
        //Vector3 pivotPoint = UpdateCubesPivotPointOptimized();
        ApplyWaterForce(Vector3.zero);

        //DrawForceLines();
        RestartCubesPosition();
    }

    // Method applies all water forces which act on submerged object - buoyancy and viscosity forces
    // First it checks how deep the object is, and then it applies the correct ratio of forces to it
    // Object rotation gets dampered with help of Torque force acting in oppposite direction of angular velocity
    void ApplyWaterForce(Vector3 pivotPoint)
    {
        float deltaUnderWater = CalculateSubMergedRatio();
        if (deltaUnderWater > 1.0f) deltaUnderWater = 1.0f;
        else if (deltaUnderWater < 0.2f) deltaUnderWater = 0.0f;

        //spheres[0].transform.position = _cubeVerticesWS[0];
        //spheres[1].transform.position = _cubeVerticesWS[7];
        //spheres[2].transform.position = new Vector3(_cubeVerticesWS[0].x, waterLevel, _cubeVerticesWS[0].z);

        // buoyance force
        _currentBuoyancyForce = deltaUnderWater * _buoyancyForce;
        _cubeBody.AddForceAtPosition(_currentBuoyancyForce, transform.position);

        // Viscosity Force working on Cube when it's under water
        for (int i = 0; i < _surfacePointsPushingDown.Count; i++)
        {
            _cubeBody.AddForceAtPosition(-_cubeBody.velocity / 8, _surfacePointsPushingDown[i]);
        }

        // CHECK TORQUE FORCE, if it's too big damper it
        if (deltaUnderWater > 0.2f)
        {
            if (_cubeBody.angularVelocity.sqrMagnitude > 2.5f)
            {
                _cubeBody.AddTorque(_cubeBody.angularVelocity * -1.0f);
                Debug.Log("Angular Velocity: " + _cubeBody.angularVelocity + ", magnitude: " + _cubeBody.angularVelocity.sqrMagnitude);
            }
        }
    }

    // Calculates both perpendicular vectors to velocity vector and with raycasting back to the object (opposite direction of velocity)
    // it tries to define number of points acting as forces back to the buoyancy force
    void CalculateWaterForcePointsPushingOnSurface()
    {
        Vector3 velocity = _cubeBody.velocity;
        Vector3 velocityDirection = velocity.normalized;
        Vector3 tempVector = Vector3.forward;

        Vector3 perp1 = Vector3.Cross(velocityDirection, tempVector);
        Vector3 perp2 = Vector3.Cross(velocityDirection, perp1);

        Vector3 velEndPoint = transform.position + velocityDirection * 4.0f;
        _vec1FacinfSurface = velEndPoint + perp1 * 4.0f;
        _vec2FacinfSurface = velEndPoint + perp2 * 4.0f;

        //DrawVelocityVectors(velEndPoint);

        float width = Vector3.Distance(velEndPoint, _vec1FacinfSurface);
        float height = Vector3.Distance(velEndPoint, _vec1FacinfSurface);
        RaycastHit hit;
        _surfacePointsPushingDown.Clear();

        for (float x = -width; x < width; x += 0.8f)
        {
            for (float y = -height; y < height; y += 0.8f)
            {
                Vector3 start = velEndPoint + (perp1.normalized * x) + (perp2.normalized * y);

                if (Physics.Raycast(start, -velocityDirection, out hit, 40.0f))
                {
                    if (hit.transform.gameObject.Equals(gameObject) && hit.point.y < (_waveWaterLevel - 0.2f))
                    {
                        _surfacePointsPushingDown.Add(hit.point);
                        //Debug.DrawLine(start, hit.point, Color.blue, 0.1f, false);
                    }
                }
            }
        }
    }

    // Find center point of submerged part of the object
    Vector3 UpdateCubesPivotPointOptimized()
    {
        Vector3 perp1 = transform.TransformVector(Vector3.left);
        Vector3 perp2 = transform.TransformVector(Vector3.forward);

        Vector3 centerUnderWaterPoint = new Vector3(transform.position.x, -50.0f, transform.position.z);
        Vector3 centerOfMass = Vector3.zero;

        RaycastHit hit;
        List<Vector3> underWaterPoints = new List<Vector3>();

        float length = Vector3.Distance(_cubeVerticesWS[0], _cubeVerticesWS[_cubeVerticesWS.Length - 1]);
        float width = length;
        float height = length;

        for (float x = -width; x < width; x += 0.5f)
        {
            for (float y = -height; y < height; y += 0.5f)
            {
                Vector3 start = centerUnderWaterPoint + (perp1.normalized * x) + (perp2.normalized * y);


                if (Physics.Raycast(start, Vector3.up, out hit, 80.0f))
                {
                    if (hit.transform.gameObject.Equals(gameObject) && hit.point.y < _waveWaterLevel)
                    {
                        centerOfMass += hit.point;
                        underWaterPoints.Add(hit.point);
                    }
                }
            }
        }

        if (underWaterPoints.Count > 0)
        {
            centerOfMass /= underWaterPoints.Count;
        }
        else
        {
            centerOfMass = transform.position;
        }

        //Debug.Log("New Center Point: " + centerOfMass + ", position:" + center);
        return centerOfMass;
    }

    // Updates cubes local space vertex positions to world space
    void UpdateCubeVertices(bool initialize)
    {
        if (initialize)
        {
            float width = 0.5f; //transform.localScale.x / 2 / 3
            float height = 0.5f; //transform.localScale.y / 2 / 0.66f
            float depth = 0.5f;

            _cubeVerticesLS[0] = new Vector3(-width, -height, -depth);
            _cubeVerticesLS[1] = new Vector3(+width, -height, -depth);
            _cubeVerticesLS[2] = new Vector3(-width, +height, -depth);
            _cubeVerticesLS[3] = new Vector3(+width, +height, -depth);
            _cubeVerticesLS[4] = new Vector3(-width, -height, +depth);
            _cubeVerticesLS[5] = new Vector3(+width, -height, +depth);
            _cubeVerticesLS[6] = new Vector3(-width, +height, +depth);
            _cubeVerticesLS[7] = new Vector3(+width, +height, +depth);
        }

        for (int i = 0; i < _cubeVerticesLS.Length; i++)
        {
            _cubeVerticesWS[i] = transform.TransformPoint(_cubeVerticesLS[i]);

            //Instantiate(spherePrefab, _cubeVerticesWS[i], Quaternion.identity);
        }
    }

    void FindWaterLevelTriggerPoint()
    {
        // sort cube vertices from lowest to highest one
        Array.Sort(_cubeVerticesWS, delegate (Vector3 pt1, Vector3 pt2) { return pt1.y.CompareTo(pt2.y); });

        //find closest water vertex to one of cubes lowest vertices
        float minDist = 9999;
        int cubeVertexIndex = -1;
        int waterVertexIndex = -1;
        int waterTriangleIndex = -1;
        int[] triangles = _waveMesh.triangles;
        Vector3[] vertices = _waveMesh.vertices;
        Vector3 waveVertex = Vector3.zero;
        Vector3 dist = Vector3.zero;

        for (int i = 0; i < _cubeVerticesWS.Length / 2; i++)
        {
            Vector3 cubeVertex = _cubeVerticesWS[i];
            for (int t = 0; t < triangles.Length; t++)
            {
                waveVertex = vertices[triangles[t]];
                dist = cubeVertex - waveVertex;

                // check current magnitude against current shortest one
                if (dist.sqrMagnitude < minDist)
                {
                    minDist = dist.sqrMagnitude;
                    cubeVertexIndex = i;
                    waterTriangleIndex = t;
                    waterVertexIndex = triangles[t];
                }
            }
        }

        waveVertex = vertices[waterVertexIndex];

        if (_cubeVerticesWS[cubeVertexIndex].y <= waveVertex.y)
        {
            _waveWaterLevel = Mathf.Round(waveVertex.y * 10) / 10;
            //GameObject sphere = Instantiate(spherePrefab, _cubeVerticesWS[cubeVertexIndex], Quaternion.identity);
            //DestroyObject(sphere, 0.3f);
        }
    }

    //  float volume = mesh.bounds.size.x * mesh.bounds.size.y * mesh.bounds.size.z;
    private float CalculateObjectsVolume()
    {
        Mesh mesh = GetComponent<MeshFilter>().mesh;
        float volume = VolumeOfMesh(mesh) * transform.localScale.x * transform.localScale.y * transform.localScale.z;
        return volume;
    }

    public float SignedVolumeOfTriangle(Vector3 p1, Vector3 p2, Vector3 p3)
    {
        float v321 = p3.x * p2.y * p1.z;
        float v231 = p2.x * p3.y * p1.z;
        float v312 = p3.x * p1.y * p2.z;
        float v132 = p1.x * p3.y * p2.z;
        float v213 = p2.x * p1.y * p3.z;
        float v123 = p1.x * p2.y * p3.z;
        return (1.0f / 6.0f) * (-v321 + v231 + v312 - v132 - v213 + v123);
    }

    public float VolumeOfMesh(Mesh mesh)
    {
        float volume = 0;
        Vector3[] vertices = mesh.vertices;
        int[] triangles = mesh.triangles;
        for (int i = 0; i < mesh.triangles.Length; i += 3)
        {
            Vector3 p1 = vertices[triangles[i + 0]];
            Vector3 p2 = vertices[triangles[i + 1]];
            Vector3 p3 = vertices[triangles[i + 2]];
            volume += SignedVolumeOfTriangle(p1, p2, p3);
        }
        return Mathf.Abs(volume);
    }

    float CalculateSubMergedRatio()
    {
        float bottomYPos = _cubeVerticesWS[0].y;
        float deltaY = _waveWaterLevel - bottomYPos;
        float height = (_cubeVerticesWS[_cubeVerticesWS.Length - 1].y - _cubeVerticesWS[0].y);
        return deltaY / height;
    }

    void CalculateBuoyancyForce()
    {
        _cubeBody.SetDensity(CUBE_DENSITY);

        float volume = CalculateObjectsVolume();
        float buoyancy = WATER_DENSITY * volume * Physics.gravity.y;
        _buoyancyForce = new Vector3(0f, -buoyancy, 0f);

        //Debug.Log("Wooden Cube mass: " + _cubeBody.mass + ", Volume: " + volume + ", Buoyance: " + buoyancy);
    }

    // ********* Helper Methods *****************************************************
    // ******************************************************************************
    void RestartCubesPosition()
    {
        elapsedTime += Time.deltaTime;

        if (transform.position.y < -60.0f || elapsedTime >= duration)
        {
            elapsedTime = 0;
            transform.position = startPos;
            transform.rotation = Quaternion.identity;
            _cubeBody.velocity = startVelocity;
        }
    }

    void DrawVelocityVectors(Vector3 velEndPoint)
    {
        LineRenderer lr1 = linePrefabs[0].GetComponent<LineRenderer>();
        lr1.startColor = lr1.endColor = Color.green;
        lr1.positionCount = 2;
        lr1.SetPositions(new Vector3[] { velEndPoint, _vec1FacinfSurface });

        LineRenderer lr2 = linePrefabs[1].GetComponent<LineRenderer>();
        lr2.startColor = lr2.endColor = Color.green;
        lr2.positionCount = 2;
        lr2.SetPositions(new Vector3[] { velEndPoint, _vec2FacinfSurface });

        // Velocity vector
        LineRenderer lr3 = linePrefabs[2].GetComponent<LineRenderer>();
        lr3.startColor = lr3.endColor = Color.white;
        lr3.positionCount = 2;
        lr3.SetPositions(new Vector3[] { transform.position, velEndPoint });
    }

    void DrawForceLines()
    {
        if (_cubeBody.velocity.magnitude > 0 && transform.position.y > -50.0f)
        {
            _lineRend.positionCount = 4;
            _lineRend.SetPosition(0, transform.position);
            _lineRend.SetPosition(1, transform.position + (_currentBuoyancyForce / 5.0f));

            _lineRend.SetPosition(2, transform.position);
            _lineRend.SetPosition(3, transform.position + (_cubeBody.velocity / 5.0f));
        }
    }
}
