using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace Waves
{
    public class WaveGenerator
    {

        private static WaveGenerator _instance;
        public static WaveGenerator Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new WaveGenerator();
                }

                return _instance;
            }
        }

        private SineWaveData _sineWaveData;
        public SineWaveData SineWaveData
        {
            set { _sineWaveData = value; }
        }

        // **** Constants *****
        private const int DELIMETER = 10;
        // **** private variable ****
        private int _segmentation;
        
        List<float> _noiseValues;

        public void SetSegmentation(int dimension)
        {
            _segmentation = dimension;
            if (dimension > 35) _segmentation = dimension * 3 / 4;
            if (dimension > 60) _segmentation = dimension * 1 / 2;
        }

        public void CalculateWaveVertices(ref Vector3[] vertices, ref int[] triangles, Vector3[] meshVertices)
        {
            for (int row = 0; row < _segmentation; row++)
            {
                int rowIndex = row * (_segmentation + 1) * 2;

                for (int i = 0; i < (_segmentation + 1); i++)
                {
                    float progress = (float)i / (float)_segmentation;
                    float yPos = WaterWaveEffects.Instance.GetWaveYPoint(WaterWaveEffects.WaterEffects.FORWARD, progress, 0, _sineWaveData);
                    float xPos = meshVertices[i * 2 + 0 + rowIndex].x;
                    float zPos = meshVertices[i * 2 + 0 + rowIndex].z;

                    vertices[i * 2 + 0 + rowIndex] = new Vector3(xPos, yPos, zPos);

                    xPos = meshVertices[i * 2 + 1 + rowIndex].x;
                    zPos = meshVertices[i * 2 + 1 + rowIndex].z;
                    vertices[i * 2 + 1 + rowIndex] = new Vector3(xPos, yPos, zPos);
                }
            }
        }

        public void CalculateWaveVerticesOptimized(ref Vector3[] vertices, ref int[] triangles, Vector3[] meshVertices)
        {
            float progressZAxis = 0;
            float progressXAxis = 0;
            for (int row = 0; row < _segmentation; row++)
            {
                int rowIndex = row * _segmentation;
                progressZAxis = (float)row / (float)_segmentation;

                for (int i = 0; i < _segmentation; i++)
                {
                    progressXAxis = (float)i / (float)_segmentation;
                    float yPos = WaterWaveEffects.Instance.GetWaveYPoint(_sineWaveData.WaterEffectEnum, progressXAxis, progressZAxis, _sineWaveData);

                    float xPos = meshVertices[i + rowIndex].x;
                    float zPos = meshVertices[i + rowIndex].z;

                    vertices[i + rowIndex] = new Vector3(xPos, yPos, zPos);
                }
            }
        }

        //public GameObject GenerateWaveObject(int x, int y, int lenght, int depth)
        //{
        //    GameObject wave = new GameObject("wave_" + x + y, typeof(MeshRenderer), typeof(MeshFilter), typeof(MeshCollider));
        //    MeshRenderer meshRenderer = wave.GetComponent<MeshRenderer>();
        //    return null;
        //}

        public void CreateWaveMesh(int dimension, out List<Vector3> vertices1D, out List<Vector2>  uvs1D, out List<int> triangles1D)
        {
            Vector3[][] vertices = new Vector3[_segmentation][];
            Vector2[][] uvs = new Vector2[_segmentation][];
            int[][] triangles = new int[_segmentation][];

            float width = (float)dimension / (float)_segmentation;
            float height = (float)dimension / (float)_segmentation;
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
                    float yPos = WaterWaveEffects.Instance.GetWaveYPoint(WaterWaveEffects.WaterEffects.FORWARD, progress, 0, _sineWaveData);
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

            Convert2DTo1DArray(vertices, uvs, triangles, out vertices1D, out uvs1D, out triangles1D);
        }

        public void CreateWaveMeshOptimized(int dimension, ref List<Vector3> vertices, ref List<Vector2> uvs, ref List<int> triangles)
        {     
            _noiseValues = new List<float>();

            float width = (float)dimension / (float)_segmentation;
            float height = (float)dimension / (float)_segmentation;
            float progressXAxis = 0.0f;
            float progressZAxis = 0.0f;

            for (int row = 0; row < _segmentation; row++)
            {
                float zPos = (float)row * height;
                progressZAxis = (float)row / (float)_segmentation; //_segmentation;

                for (int i = 0; i < _segmentation; i++)
                {
                    progressXAxis = (float)i / (float)_segmentation;
                    float yPos = WaterWaveEffects.Instance.GetWaveYPoint(_sineWaveData.WaterEffectEnum, progressXAxis, progressZAxis, _sineWaveData);
                    float xPos = (float)i * width;

                    vertices.Add(new Vector3(xPos, yPos, zPos));
                    uvs.Add(new Vector2(0.0f, 1.0f));
                }
            }

            int[] singleTriangle = new int[6];
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
        }

        
        // Helper method to convert 2d array to 1d array
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
        //float CalculatePointHeight(float progress)
        //{
        //    float y = _sineWaveData.Amplitude * Mathf.Sin((_sineWaveData.Frequency * progress
        //        + _sineWaveData.AmplOffset + _sineWaveData.ElapsedTime) * Mathf.PI);

        //    return y;
        //}

        //float CalculatePointHeightWithNoise(float progress)
        //{
        //    float y = _sineWaveData.Amplitude * Mathf.Sin((_sineWaveData.Frequency * progress
        //        + _sineWaveData.AmplOffset + _sineWaveData.ElapsedTime) * Mathf.PI);

        //    float noise = Random.Range(-_sineWaveData.Noise, _sineWaveData.Noise);
        //    _noiseValues.Add(noise);

        //    y += noise;


        //    return y;
        //}

        //float GenerateDiagonalSineWave(float progressXAxis, float progressZAxis, bool includeNoise)
        //{
        //    float offset = progressXAxis + progressZAxis;
        //    float y = _sineWaveData.Amplitude * Mathf.Sin((_sineWaveData.Frequency * offset
        //        + _sineWaveData.AmplOffset + _sineWaveData.ElapsedTime) * Mathf.PI);

        //    float noise = 0;
        //    if (includeNoise)
        //    {
        //        noise = Random.Range(-_sineWaveData.Noise, _sineWaveData.Noise);
        //        _noiseValues.Add(noise);
        //    }

        //    y += noise;

        //    return y;
        //}

        //float GenerateRippleEffect(float progressXAxis, float progressZAxis)
        //{
        //    progressXAxis -= 0.5f;
        //    progressZAxis -= 0.5f;
        //    float offset = (progressXAxis * progressXAxis) + (progressZAxis * progressZAxis);
        //    float y = _sineWaveData.Amplitude * Mathf.Sin((_sineWaveData.ElapsedTime + offset * _sineWaveData.Frequency) * Mathf.PI);

        //    return y;
        //}
    }
}
