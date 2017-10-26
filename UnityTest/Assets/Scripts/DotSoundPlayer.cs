/* <copyright file="DotSoundPlayer.cs" company="Playdots, Inc.">
 * Copyright (C) 2017 Playdots, Inc.
 * </copyright>
 * ----------------------------
 */
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// Class for setting up Dot sound patches and playing the right notes
/// </summary>
public class DotSoundPlayer : MonoBehaviour
{
    public string patchName;
    public int[] noteFrequencies;

    IEnumerator Start() {
        yield return 2f;

        UnityPD.OpenPatch(patchName);
    }

    /// <summary>
    /// Plays the dot connect sound of the 1-index based number of dots in connection
    /// </summary>
    /// <param name="dotsInConnection">Dots in connection.</param>
    public void PlayDotConnectedSound(int dotsInConnection)
    {
        int soundIdx = PingPongInt(dotsInConnection, noteFrequencies.Length);

        // Play file
        //PlayFile(soundIdx);

        // Play synth
        PlaySynth(soundIdx);

        // Play Pd
        //UnityPD.SendFloat( "playNote", noteFrequencies[soundIdx]);
    }

    /// <summary>
    /// Ping pong an int value between 0 and length (0 inclusive, length exclusive)
    /// </summary>
    /// <returns>The value to ping pong </returns>
    /// <param name="value">Value.</param>
    /// <param name="length">Length.</param>
    public static int PingPongInt(int value, int length)
    {
        if (length == 1)
            return 0;

        //ping pong
        int exclusiveLength = length - 1;
        int ponged = value % exclusiveLength;
        if ((value / exclusiveLength) % 2 == 1) // if bouncing back, count down from length
            ponged = exclusiveLength - ponged;
        return ponged;
    }

    #region Files
    public AudioClip[] soundFiles;
    void PlayFile(int note)
    {
        AudioSource.PlayClipAtPoint(soundFiles[note], Vector3.zero);
    }
    #endregion

    #region Synth
    public SinWave synthPrefab;
    void PlaySynth(int note ) {
        float freq = noteFrequencies[note];

        var newSynth = Instantiate(synthPrefab);
        newSynth.frequency = freq;

        Destroy(newSynth.gameObject, 1f);
    }

    #endregion
}
