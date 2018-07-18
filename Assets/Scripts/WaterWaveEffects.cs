using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WaterWaveEffects {

    private static WaterWaveEffects _instance;
    public static WaterWaveEffects Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = new WaterWaveEffects();
            }

            return _instance;
        }
    }

    public enum WaterEffects
    {
        FORWARD = 0,
        DIAGONAL,
        RIPPLE,
        FORWARD_NOISE,
        DIAGONAL_NOISE,
        RIPPLE_NOISE
    }

    public float GetWaveYPoint(WaterEffects waterEffect, float progressXAxis, float progressYAxis, SineWaveData waveData)
    {
        switch (waterEffect)
        {
            case WaterEffects.FORWARD:
                return GenerateForwardSineWave(progressXAxis, progressYAxis, false, waveData);
            case WaterEffects.DIAGONAL:
                return GenerateDiagonalSineWave(progressXAxis, progressYAxis, false, waveData);
            case WaterEffects.RIPPLE:
                return GenerateRippleEffect(progressXAxis, progressYAxis, false, waveData);
            case WaterEffects.FORWARD_NOISE:
                return GenerateForwardSineWave(progressXAxis, progressYAxis, true, waveData);
            case WaterEffects.DIAGONAL_NOISE:
                return GenerateDiagonalSineWave(progressXAxis, progressYAxis, true, waveData);
            case WaterEffects.RIPPLE_NOISE:
                return GenerateRippleEffect(progressXAxis, progressYAxis, true, waveData);
            default:
                return 0;
        }
    }

    //************** SINE FUNCTION ******************************************
    //********* y = sin (x) ********** Amplitude => y = a * sin (x) *********
    //******************************** Frequency => y = sin (a * x) *********
    //******************************** Move curve => y = sin (a + x) ********
    //***********************************************************************
    float GenerateForwardSineWave(float progressXAxis, float progressZAxis, bool includeNoise, SineWaveData waveData)
    {
        float y = waveData.Amplitude * Mathf.Sin((waveData.Frequency * progressXAxis
            + waveData.AmplOffset + waveData.ElapsedTime) * Mathf.PI);

        y += AddNoise(includeNoise, waveData);


        return y;
    }

    float GenerateDiagonalSineWave(float progressXAxis, float progressZAxis, bool includeNoise, SineWaveData waveData)
    {
        float offset = progressXAxis + progressZAxis;
        float y = waveData.Amplitude * Mathf.Sin((waveData.Frequency * offset
            + waveData.AmplOffset + waveData.ElapsedTime) * Mathf.PI);

        
        y += AddNoise(includeNoise, waveData);

        y += AddPerlinNoise(progressXAxis, progressZAxis);

        return y;
    }

    float GenerateRippleEffect(float progressXAxis, float progressZAxis, bool includeNoise, SineWaveData waveData)
    {
        progressXAxis -= 0.5f;
        progressZAxis -= 0.5f;

        float offset = (progressXAxis * progressXAxis) + (progressZAxis * progressZAxis);
        float y = waveData.Amplitude * Mathf.Sin((waveData.ElapsedTime + offset * waveData.Frequency) * Mathf.PI);

        y += AddNoise(includeNoise, waveData);


        y += AddPerlinNoise(progressXAxis, progressZAxis);

        return y;
    }

    float AddNoise(bool includeNoise, SineWaveData waveData)
    {
        float noise = 0;
        if (includeNoise)
        {
            noise = Random.Range(-waveData.Noise, waveData.Noise);
        }

        return noise;
    }

    float AddPerlinNoise(float progressXAxis, float progressZAxis)
    {
        float noiseWalk = 10.0f;
        float noiseStrength = 1.0f;
        return Mathf.PerlinNoise(progressXAxis + noiseWalk, progressZAxis + Mathf.Sin(Time.time* 0.1f)) * noiseStrength;
    }
}
