/* DotSoundConfig.cs
 * ----------------------------
 */
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

[CreateAssetMenu( menuName = "Dot Sound Config" )]
/// <summary>
/// Config for a dot sound 
/// </summary>
public class DotSoundConfig : ScriptableObject {
    [Range( 0, 5000)]
    public float attack = 60f;
    [Range( 0, 5000)]
    public float sustain = 200f;
    [Range( 0, 5000)]
    public float decay = 500f;

    [Range(0, 1)]
    public float adder1Vol = .5f;
    [Range(0, 5)]
    public float adder1FreqMultiply = 2f;
    [Range(0, 100)]
    public float adder1ModFreq = 200f;
    [Range(0, 20)]
    public float adder1ModAmount = 1f;

    [Range(0, 1)]
    public float adder2Vol = .25f;
    [Range(0, 5)]
    public float adder2FreqMultiply = 3f;
    [Range(0, 100)]
    public float adder2ModFreq = 100f;
    [Range(0, 20)]
    public float adder2ModAmount = 1.01f;

    [Range( 0, 1000)]
    public float lopCutoff = 500f;

    public void SendValues() {
        UnityPD.SendFloat( "setAttack", attack );
        UnityPD.SendFloat( "setSustain", sustain );
        UnityPD.SendFloat( "setDecay", decay );

        UnityPD.SendFloat( "adder-1-vol", adder1Vol );
        UnityPD.SendFloat( "adder-1-control-freq-multiply", adder1FreqMultiply );
        UnityPD.SendFloat( "adder-1-freq-mod-freq", adder1ModFreq );
        UnityPD.SendFloat( "adder-1-freq-mod-amount", adder1ModAmount );

        UnityPD.SendFloat( "adder-2-vol", adder2Vol );
        UnityPD.SendFloat( "adder-2-control-freq-multiply", adder2FreqMultiply );
        UnityPD.SendFloat( "adder-2-freq-mod-freq", adder2ModFreq );
        UnityPD.SendFloat( "adder-2-freq-mod-amount", adder2ModAmount );

        UnityPD.SendFloat( "lop-cutoff", lopCutoff );
    }
}
