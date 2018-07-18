using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class SineWaveData : System.Object {

    private float _amplitude;
    public float Amplitude
    {
        get
        {
            return _amplitude;
        }
        set
        {
            _amplitude = value;
        }
    }

    private float _frequency;
    public float Frequency
    {
        get
        {
            return _frequency;
        }
        set
        {
            _frequency = value;
        }
    }

    private float _amplOffset;
    public float AmplOffset
    {
        get
        {
            return _amplOffset;
        }
        set
        {
            _amplOffset = value;
        }
    }

    private float _noise;
    public float Noise
    {
        get
        {
            return _noise;
        }
        set
        {
            _noise = value;
        }
    }

    private float _elapsedTime;
    public float ElapsedTime
    {
        get
        {
            return _elapsedTime;
        }
        set
        {
            _elapsedTime = value;
        }
    }

    public SineWaveData(float amplitude, float frequency, float amplitudeOffset, float elapsedTime, float noise)
    {
        this.Amplitude = amplitude;
        this.Frequency = frequency;
        this.AmplOffset = amplitudeOffset;
        this.ElapsedTime = elapsedTime;
        this.Noise = noise;
    }
}
