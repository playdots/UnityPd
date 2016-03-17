/* PDTest.cs
 * ----------------------------
 */
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using System;

/// <summary>
/// ...
/// </summary>
public class PDTest : MonoBehaviour {

    public string patchName;
    public DotSoundConfig dotSoundConfig;

    DotSoundPlayer dotSoundPlayer;

    IEnumerator Start() {
        UnityPD.Init ();
        yield return new WaitForSeconds (5f);

        dotSoundPlayer = new DotSoundPlayer (dotSoundConfig);
    }

    void OnApplicationQuit() {
    }

    int selected = -1;
    void OnGUI() {
        using ( new GUILayout.AreaScope( new Rect( 80, Screen.height * .5f - 300, Screen.width - 160, 600 ) ) ) {
            if ( dotSoundPlayer == null ) {
                GUILayout.Label ("Waiting to load patch");
                return;
            }

            int newSelected = GUILayout.SelectionGrid( selected, new []{ "1", "2", "3", "4", "5", "6", "7", "8", "9", "10", "11", "12" }, 3, GUILayout.ExpandHeight( true ) );
            if ( newSelected != selected ) {
                dotSoundPlayer.PlayDotConnectedSound ( newSelected );
                selected = newSelected;
            }
        }
    }
}
