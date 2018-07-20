using UnityEngine;
using System.Collections;

public class CollisionScript : MonoBehaviour
{
    private int waveNumber;
    public float distanceX, distanceZ;
    public float[] waveAmplitude = new float[8];
    public float magnitudeDivider = 0.1f;

    public Vector2[] impactPos;
    public float[] distance = new float[8];
    public float speedWaveSpread;


    Mesh mesh;
    Renderer renderer;
    // Use this for initialization
    void Start()
    {
        mesh = GetComponent<MeshFilter>().mesh;
        renderer = GetComponent<Renderer>();

        Debug.Log("Mehs bounds z: " + mesh.bounds.size.z);
    }

    // Update is called once per frame
    void Update()
    {

        for (int i = 0; i < 8; i++)
        {
            //float test = renderer.material.GetFloat("_TestX");
            waveAmplitude[i] = renderer.material.GetFloat("_WaveAmpl" + (i + 1));
            if (waveAmplitude[i] > 0)
            {
                float offsetx = renderer.material.GetFloat("_OffsetX" + (i + 1));
                float offsetz = renderer.material.GetFloat("_OffsetZ" + (i + 1));


                distance[i] += speedWaveSpread;
                renderer.material.SetFloat("_Distance" + (i + 1), distance[i]);
                renderer.material.SetFloat("_WaveAmpl" + (i + 1), waveAmplitude[i] * 0.98f); // it will decrease amplitude value over time
            }
            if (waveAmplitude[i] < 0.05)
            {
                renderer.material.SetFloat("_WaveAmpl" + (i + 1), 0.0f);
                distance[i] = 0;
            }

        }
    }

    void OnCollisionEnter(Collision col)
    {
        if (col.rigidbody)
        {
            waveNumber++;
            if (waveNumber == 9)
            {
                waveNumber = 1;
            }

            waveAmplitude[waveNumber - 1] = 0;
            distance[waveNumber - 1] = 0;
            // center position of plane - position of sphere
            distanceX = this.transform.position.x - col.gameObject.transform.position.x;
            distanceZ = this.transform.position.z - col.gameObject.transform.position.z;
            impactPos[waveNumber - 1].x = col.transform.position.x;
            impactPos[waveNumber - 1].y = col.transform.position.z;

            renderer.material.SetFloat("_xImpact" + waveNumber, impactPos[waveNumber - 1].x);
            renderer.material.SetFloat("_zImpact" + waveNumber, impactPos[waveNumber - 1].y);

            renderer.material.SetFloat("_OffsetX" + waveNumber, distanceX / (float)mesh.bounds.size.x * 2.0f);
            renderer.material.SetFloat("_OffsetZ" + waveNumber, distanceZ / (float)mesh.bounds.size.z * 1.35f);

            renderer.material.SetFloat("_WaveAmpl" + waveNumber, col.rigidbody.velocity.magnitude * magnitudeDivider);

        }
    }
}
