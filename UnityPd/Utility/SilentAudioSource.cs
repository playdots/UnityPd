﻿/* SilentAudioSource.cs
 * ----------------------------
 */
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

[RequireComponent(typeof(AudioSource))]
/// <summary>
/// Force an audio source to start processing (for generating audio)
/// </summary>
public class SilentAudioSource : MonoBehaviour {
    void OnAudioFilterRead(float[] data, int channels)
    {
    }
}
