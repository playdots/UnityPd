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

    #if UNITY_IPHONE
    [DllImport ("__Internal")]
    #else
    [DllImport("AudioPlugin_UnityPd")]
    #endif   
    private static extern int libpd_GetDollarZero( IntPtr intPtr );

    #if UNITY_IPHONE
    [DllImport ("__Internal")]
    #else
    [DllImport("AudioPlugin_UnityPd")]
    #endif
    private static extern IntPtr libpd_OpenPatch( string patchName, string directory );

    public static IntPtr OpenPatch( string patchName ) {
        string patchDir = Path.Combine (Application.streamingAssetsPath, PATCH_DIR);

        // Android voodoo to load the patch. TODO: use Android APIs to copy whole folder?
        #if UNITY_ANDROID// && !UNITY_EDITOR
        string patchJar = Application.persistentDataPath + "/" + patchName;

        if (File.Exists(patchJar))
        {
            Debug.Log("Patch already unpacked");
            File.Delete(patchJar);

            if (File.Exists(patchJar))
            {
                Debug.Log("Couldn't delete");               
            }
        }

        WWW dataStream = new WWW( Path.Combine( patchDir, patchName ) );

        // Hack to wait till download is done
        while(!dataStream.isDone) 
        {
        }

        if (!String.IsNullOrEmpty(dataStream.error))
        {
            Debug.Log("### WWW ERROR IN DATA STREAM:" + dataStream.error);
        }

        File.WriteAllBytes(patchJar, dataStream.bytes);

        patchDir = Application.persistentDataPath;
        #else
        if( !File.Exists( Path.Combine( patchDir, patchName ) ) ) {
            throw new FileNotFoundException( patchDir );
        }
        #endif

        var ptr = libpd_OpenPatch( patchName, patchDir );

        if(ptr == IntPtr.Zero)
        {
            throw new IOException("unable to open patch " + patchName );
        }

        return ptr;
    }

    #if UNITY_IPHONE
    [DllImport ("__Internal")]
    #else
    [DllImport("AudioPlugin_UnityPd")]
    #endif    
    private static extern void libpd_ClosePatch( IntPtr patchPtr );
    public static void ClosePatch( IntPtr patchPtr ) {
        libpd_ClosePatch( patchPtr );
    }
}
