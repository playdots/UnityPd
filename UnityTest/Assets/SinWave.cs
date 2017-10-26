/* <copyright file="SinWave.cs" company="Playdots, Inc.">
 * Copyright (C) 2016 Playdots, Inc.
 * </copyright>
 * ----------------------------
 */
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using System;

/// <summary>
/// 
/// ...
/// </summary>
[ExecuteInEditMode]
public class SinWave : MonoBehaviour {
    // un-optimized version
    public double frequency = 440;
    public double gain = 0.05;
    private double increment;
    private double phase;
    private int sampling_frequency = 48000;

    void Start()
    {
        sampling_frequency = AudioSettings.GetConfiguration().sampleRate;
    }

    void Update()
    {
    }

    void OnAudioFilterRead(float[] data, int channels) {
        // update increment in case frequency has changed
        increment = frequency * 2 * Math.PI / sampling_frequency;

        for (var i = 0; i < data.Length; i += channels)
        {
            phase += increment;
            // this is where we copy audio data to make them “available” to Unity
            data[i] = (float)(gain * Math.Sin(phase));
            // if we have stereo, we copy the mono data to each channel
            if (channels == 2)
                data[i + 1] = data[i];
            if (phase > 2 * Mathf.PI)
                phase = 0;
        }
    }

}
