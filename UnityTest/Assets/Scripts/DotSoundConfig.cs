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
    [Range( 0, 1000)]
    public float attack = 60f;
    [Range( 0, 1000)]
    public float sustain = 200f;
    [Range( 0, 1000)]
    public float decay = 500f;

    [Range(0, 1)]
    public float adder1Vol = .5f;
    [Range(0, 10)]
    public float adder1Ratio = 2f;
    [Range(0, 1000)]
    public float adder1Strength = 200f;
    [Range(0, 2)]
    public float adder1ControlRatio = 1f;

    [Range(0, 1)]
    public float adder2Vol = .25f;
    [Range(0, 10)]
    public float adder2Ratio = 3f;
    [Range(0, 1000)]
    public float adder2Strength = 100f;
    [Range(0, 2)]
    public float adder2ControlRatio = 1.01f;

    public void SendValues() {
        UnityPD.SendFloat( "setAttack", attack );
        UnityPD.SendFloat( "setSustain", sustain );
        UnityPD.SendFloat( "setDecay", decay );

        UnityPD.SendFloat( "adder-1-vol", adder1Vol );
        UnityPD.SendFloat( "adder-1-ratio", adder1Ratio );
        UnityPD.SendFloat( "adder-1-strength", adder1Strength );
        UnityPD.SendFloat( "adder-1-controlRatio", adder1ControlRatio );

        UnityPD.SendFloat( "adder-2-vol", adder2Vol );
        UnityPD.SendFloat( "adder-2-ratio", adder2Ratio );
        UnityPD.SendFloat( "adder-2-strength", adder2Strength );
        UnityPD.SendFloat( "adder-2-controlRatio", adder2ControlRatio );
    }
}
