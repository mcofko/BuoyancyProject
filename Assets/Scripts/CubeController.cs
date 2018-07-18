using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(LineRenderer), typeof(Rigidbody))]
public class CubeController : MonoBehaviour {

    [SerializeField]
    private GameObject lineRendPrefab;
    [SerializeField]
    private GameObject planePrefab;
    [SerializeField]
    private GameObject spherePrefab;

    private GameObject[] linePrefabs = new GameObject[3];
    private GameObject facingVelocityPlane;

    private LineRenderer lineRend;
    private Rigidbody cubeBody;
    private BoxCollider boxCollider;

    private Vector3 boxSize;
    private Vector3 startPos;

    private float waterLevel = -100f;
    private Vector3 _buoyancyForce = new Vector3(0f, 12.0f, 0f);
    private Vector3 _viscosity = new Vector3(0.75f, 0f, 0.75f);
    private Vector3 _currentWaterForceUp;
    private float _currentViscosity;
    private Vector3 _currentViscosityVector;

    private Vector3[] _cubeVerticesLS = new Vector3[8];
    private Vector3[] _cubeVerticesWS = new Vector3[8];
    private List<Vector3> _surfacePointsPushingDown;
    private Vector3 vec1FacinfSurface;
    private Vector3 vec2FacinfSurface;

    private float duration = 10.0f;
    private float elapsedTime = 0.0f;
    private Vector3 startVelocity = new Vector3(0.0f, 0.0f, 0.0f);

    private void Awake()
    {
        lineRend = GetComponent<LineRenderer>();
        cubeBody = GetComponent<Rigidbody>();
        boxCollider = GetComponent<BoxCollider>();

        boxSize = boxCollider.size;
        startPos = transform.position;
        _surfacePointsPushingDown = new List<Vector3>();
    }

    // Use this for initialization
    void Start () {
        //add start speed to cube
        
        cubeBody.velocity = startVelocity;
        cubeBody.SetDensity(0.3f);
        float volume = CalculateObjectsVolume();
        float buoyancy = 0.4f * volume * 9.8f;
        _buoyancyForce = new Vector3(0f, buoyancy, 0f);
        Debug.Log("Wooden Cube mass: " + cubeBody.mass + ", Volume: " + volume + ", Buoyance: " + buoyancy);

        for (int i = 0; i < linePrefabs.Length; i++)
        {
            linePrefabs[i] = Instantiate(lineRendPrefab, Vector3.zero, Quaternion.identity);
            linePrefabs[i].transform.parent = gameObject.transform;
        }

        UpdateCubeVertices(true);
	}
	
	// Update is called once per frame
	void Update () {
        return;

        UpdateCubeVertices(false);
        CalculateWaterForcePointsPushingOnSurface();
        Vector3 pivotPoint = UpdateCubesPivotPointOptimized();
        ApplyWaterForce(pivotPoint);


        //DrawForceLines();
        RestartCubesPosition();
	}

    void ApplyWaterForce(Vector3 pivotPoint)
    {
        Vector3 direction = cubeBody.velocity.normalized;

        float bottomYPos = transform.position.y; // - transform.localScale.y / 2;
        float deltaY = waterLevel - bottomYPos;
        float deltaUnderWater = deltaY / transform.localScale.y;

        if (deltaUnderWater > 1.0f) deltaUnderWater = 1.0f;
        else if (deltaUnderWater < 0.0f) deltaUnderWater = 0.0f;

        _currentWaterForceUp = deltaUnderWater * _buoyancyForce;

        _currentViscosity = (deltaUnderWater * _viscosity).magnitude;
        _currentViscosityVector = cubeBody.velocity * -1.0f * _currentViscosity;

        //Debug.Log("Under sea! Cubes velocity is: " + cubeBody.velocity + ", Up Force: " + _currentWaterForceUp + ", viscosity: " + _currentViscosity);


        // buoyance force
        cubeBody.AddForceAtPosition(_currentWaterForceUp, pivotPoint);
        cubeBody.AddForceAtPosition(_currentViscosityVector, pivotPoint);

        for (int i = 0; i < _surfacePointsPushingDown.Count; i++)
        {
            cubeBody.AddForceAtPosition(-cubeBody.velocity / 10, _surfacePointsPushingDown[i]);
        }
        
    }

    void CalculateWaterForcePointsPushingOnSurface()
    {
        Vector3 velocity = cubeBody.velocity;
        Vector3 velocityDirection = velocity.normalized;
        Vector3 tempVector = Vector3.forward;

        Vector3 perp1 = Vector3.Cross(velocityDirection, tempVector);
        Vector3 perp2 = Vector3.Cross(velocityDirection, perp1);

        Vector3 velEndPoint = transform.position + velocityDirection * 4.0f;
        vec1FacinfSurface = velEndPoint + perp1 * 4.0f;
        vec2FacinfSurface = velEndPoint + perp2 * 4.0f;

        //DrawVelocityVectors(velEndPoint);
        //facingVelocityPlane.transform.position = velEndPoint;
        //facingVelocityPlane.transform.LookAt(transform.position, Vector3.up);

        float width = Vector3.Distance(velEndPoint, vec1FacinfSurface);
        float height = Vector3.Distance(velEndPoint, vec1FacinfSurface);
        RaycastHit hit;
        _surfacePointsPushingDown.Clear();
        
        for (float x = -width; x < width; x+=0.8f)
        {
            for (float y = -height; y < height; y+= 0.8f)
            {
                Vector3 start = velEndPoint + (perp1.normalized * x) + (perp2.normalized * y);

                
                if (Physics.Raycast(start, -velocityDirection, out hit, 40.0f))
                {
                    if (hit.transform.gameObject.Equals(gameObject) && hit.point.y < (waterLevel - 0.2f))
                    {
                        _surfacePointsPushingDown.Add(hit.point);
                        //Debug.DrawLine(start, hit.point, Color.blue, 0.1f, false);
                    }
                }
            }
        }
    }

    Vector3 UpdateCubesPivotPointOptimized()
    {
        Vector3 velocity = cubeBody.velocity;
        Vector3 velocityDirection = velocity.normalized;
        Vector3 tempVector = Vector3.forward;

        Vector3 perp1 = Vector3.Cross(velocityDirection, tempVector);
        Vector3 perp2 = Vector3.Cross(velocityDirection, perp1);

        Vector3 centerUnderWaterPoint = new Vector3(transform.position.x, -30.0f, transform.position.z);
        Vector3 centerOfMass = Vector3.zero;

        RaycastHit hit;
        List<Vector3> underWaterPoints = new List<Vector3>();

        float width = 5.0f;
        float height = 5.0f;

        for (float x = -width; x < width; x += 0.5f)
        {
            for (float y = -height; y < height; y += 0.5f)
            {
                Vector3 start = centerUnderWaterPoint + (perp1.normalized * x) + (perp2.normalized * y);


                if (Physics.Raycast(start, Vector3.up, out hit, 80.0f))
                {
                    if (hit.transform.gameObject.Equals(gameObject) && hit.point.y < (waterLevel - 0.2f))
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

            Instantiate(spherePrefab, _cubeVerticesWS[i], Quaternion.identity);
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

    private void OnTriggerEnter(Collider other)
    {
        //Debug.Log("Felt into sea! Cubes velocity is: " + cubeBody.velocity);

        if (waterLevel == -100.0f)
        {
            waterLevel = transform.position.y - gameObject.transform.localScale.y / 2;
        }
    }

    void RestartCubesPosition()
    {
        elapsedTime += Time.deltaTime;

        if (transform.position.y < -60.0f || elapsedTime >= duration) 
        {
            elapsedTime = 0;
            transform.position = startPos;
            transform.rotation = Quaternion.identity;
            cubeBody.velocity = startVelocity;
        }
    }

    void DrawVelocityVectors(Vector3 velEndPoint)
    {
        LineRenderer lr1 = linePrefabs[0].GetComponent<LineRenderer>();
        lr1.startColor = lr1.endColor = Color.green;
        lr1.positionCount = 2;
        lr1.SetPositions(new Vector3[] { velEndPoint, vec1FacinfSurface });

        LineRenderer lr2 = linePrefabs[1].GetComponent<LineRenderer>();
        lr2.startColor = lr2.endColor = Color.green;
        lr2.positionCount = 2;
        lr2.SetPositions(new Vector3[] { velEndPoint, vec2FacinfSurface });

        // Velocity vector
        LineRenderer lr3 = linePrefabs[2].GetComponent<LineRenderer>();
        lr3.startColor = lr3.endColor = Color.white;
        lr3.positionCount = 2;
        lr3.SetPositions(new Vector3[] { transform.position, velEndPoint });
    }

    void DrawForceLines()
    {
        if (cubeBody.velocity.magnitude > 0 && transform.position.y > -50.0f)
        {
            lineRend.positionCount = 4;
            lineRend.SetPosition(0, transform.position);
            lineRend.SetPosition(1, transform.position + (_currentWaterForceUp / 5.0f));

            lineRend.SetPosition(2, transform.position);
            lineRend.SetPosition(3, transform.position + (_currentViscosityVector / 2.0f));
        }
    }
}
