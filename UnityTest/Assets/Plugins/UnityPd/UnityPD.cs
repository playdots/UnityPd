/* UnityPD.cs
 * ----------------------------
 */
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Runtime.CompilerServices;
using System;
using System.IO;

/// <summary>
/// Bridge for PdCalls
/// </summary>
public class UnityPD : MonoBehaviour {
    const string PATCH_DIR = "pd";

    [DllImport( "AudioPlugin_UnityPd")]
    private static extern int libpd_GetDollarZero( IntPtr intPtr );

    [DllImport("AudioPlugin_UnityPd")]
    private static extern IntPtr libpd_OpenPatch( string patchName, string directory );

    public static IntPtr OpenPatch( string patchName ) {
        string patchDir = Path.Combine (Application.streamingAssetsPath, PATCH_DIR);

        if( !File.Exists( Path.Combine( patchDir, patchName ) ) ) {
            throw new FileNotFoundException( patchDir );
        }

        var ptr =  libpd_OpenPatch( patchName, patchDir );

        if(ptr == IntPtr.Zero)
        {
            throw new IOException("unable to open patch " + patchName );
        }

        return ptr;
    }

    [DllImport("AudioPlugin_UnityPd")]
    private static extern void libpd_ClosePatch( IntPtr patchPtr );
    public static void ClosePatch( IntPtr patchPtr ) {
        libpd_ClosePatch( patchPtr );
    }
}
