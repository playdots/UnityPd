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

    IntPtr patch;

    void Start() {
        patch = UnityPD.OpenPatch( patchName );
    }

    void OnApplicationQuit() {
        UnityPD.ClosePatch( patch );
    }
}
