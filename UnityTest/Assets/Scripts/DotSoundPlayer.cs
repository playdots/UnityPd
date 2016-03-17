/* DotSoundPlayer.cs
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
public class DotSoundPlayer {
    private const float DOT_START_FREQ = 220;
    private const float DOT_PROGRESSION_MULTIPLIER = 9f/8f;

    private readonly DotSoundConfig _dotSoundConfig;
    private int _dotSoundPatch;

    public DotSoundPlayer( DotSoundConfig dotSoundConfig ) {
        // TODO make this variable per world
        Debug.Log( dotSoundConfig, dotSoundConfig );
        _dotSoundConfig = dotSoundConfig;
        _dotSoundPatch = UnityPD.OpenPatch( "control.pd" );
    }

    public void PlayDotConnectedSound( int dotsInConnection ) {
        _dotSoundConfig.SendValues();

        float freq = DOT_START_FREQ * Mathf.Pow( DOT_PROGRESSION_MULTIPLIER, dotsInConnection );
        UnityPD.SendFloat( "noteOn", freq );
    }
}
