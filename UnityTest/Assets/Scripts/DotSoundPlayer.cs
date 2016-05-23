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
    private const float _DOT_START_FREQ = 220;
    private const int _MAX_DOT_SOUND_NOTE = 15;
    private float[] dotProgressionScale = {
        1, 
        5f / 4f, // major 3rd
        4f / 3f, // major 4th
        3f / 2f, // major 5th
        15f / 8f // major 7th
    };

    private readonly DotSoundConfig _dotSoundConfig;
    private int _dotSoundPatch = -1;

    public DotSoundPlayer( DotSoundConfig dotSoundConfig ) {
        // TODO make this variable per world
        _dotSoundConfig = dotSoundConfig;
        _dotSoundPatch = UnityPD.OpenPatch( "simpleSynth.pd" );
        _dotSoundConfig.SendValues();
    }

    /// <summary>
    /// Plays the dot connect sound of the 1-index based number of dots in connection
    /// </summary>
    /// <param name="dotsInConnection">Dots in connection.</param>
    public void PlayDotConnectedSound( int dotsInConnection ) {
        #if UNITY_EDITOR
        _dotSoundConfig.SendValues();
        #endif

        dotsInConnection = PingPongInt( dotsInConnection, _MAX_DOT_SOUND_NOTE );
        int octaves = ( dotsInConnection - 1 ) / dotProgressionScale.Length;
        float octaveStart = _DOT_START_FREQ * Mathf.Pow( 2, octaves );
        int scaleNote = ( dotsInConnection - 1 ) % dotProgressionScale.Length;
        Debug.Log (octaveStart + " " + scaleNote); 
        float freq = octaveStart * dotProgressionScale[scaleNote];
        UnityPD.SendFloat( "playNote", freq );
    }

    public void PlaySquareSound( int dotsInConnection ) {
        for ( int i = 0; i < 3; i++ ) {
            PlayDotConnectedSound( dotsInConnection + 2 * i ); // play a triad (this note, note + 2, note + 4)
        }
    }

    public void SwitchToPatch( string patch ) {
        if ( _dotSoundPatch >= 0 )
            UnityPD.ClosePatch( _dotSoundPatch );
        _dotSoundPatch = UnityPD.OpenPatch( patch );
        _dotSoundConfig.SendValues();
    }

    /// <summary>
    /// Ping pong an int value between 0 and length (0 inclusive, length exclusive)
    /// </summary>
    /// <returns>The value to ping pong </returns>
    /// <param name="value">Value.</param>
    /// <param name="length">Length.</param>
    public static int PingPongInt( int value, int length ) {
        if ( length == 1 )
            return 0;

        //ping pong
        int exclusiveLength = length - 1;
        int ponged = value % exclusiveLength;
        if ( ( value / exclusiveLength ) % 2 == 1 ) // if bouncing back, count down from length
            ponged = exclusiveLength - ponged;
        return ponged;
    }
}
