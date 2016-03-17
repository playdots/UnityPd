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

    private static UnityPD instance;

    void Awake() {
        if ( instance && instance != this ) {
            Debug.LogWarning( "UnityPD already in scene" );
            Destroy( gameObject );
            return;
        }

        gameObject.name = "UnityPD";
        DontDestroyOnLoad( gameObject );
    }

    void OnApplicationQuit() {
        Deinit();
    }

    #region Control
    private static Dictionary<int, IntPtr> _openPatches = new Dictionary<int, IntPtr>();

#if UNITY_IPHONE
    [DllImport ("__Internal")]
#else
    [DllImport("AudioPlugin_UnityPd")]
#endif   
    private static extern void libpd_EnableAudio();

    public static void Init() {
        if ( !instance ) {
            instance = FindObjectOfType<UnityPD>();
            if ( !instance ) {
                instance = new GameObject( "UnityPD" ).AddComponent<UnityPD>();
            }
        }

        libpd_EnableAudio();

#if UNITY_ANDROID && !UNITY_EDITOR
        CopyPdResourcesToPersitantPath();
        AddToSearchPath( Path.Combine( Application.persistentDataPath, PATCH_DIR ) );
#else
        AddToSearchPath( Path.Combine( Application.streamingAssetsPath, PATCH_DIR ) );
#endif
    }

    public static void Deinit() {
        foreach ( var patchPtr in _openPatches.Values )
            libpd_ClosePatch( patchPtr );
        _openPatches.Clear();
    }
    #endregion

    #region Patches

#if UNITY_IPHONE
    [DllImport ("__Internal")]
#else
    [DllImport("AudioPlugin_UnityPd")]
#endif
    private static extern void libpd_clear_search_path();

    /// <summary>
    /// clears the search path for pd externals
    /// </summary>
    public static void ClearSearchPath() {
        libpd_clear_search_path();
    }


#if UNITY_IPHONE
    [DllImport ("__Internal")]
#else
    [DllImport("AudioPlugin_UnityPd")]
#endif
    private static extern void libpd_add_to_search_path( string sym );

    /// <summary>
    /// adds a directory to the search paths
    /// </summary>
    /// <param name="sym">directory to add</param>
    public static void AddToSearchPath( string sym ) {
        libpd_add_to_search_path( sym );
    }

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

    public static int OpenPatch( string patchName ) {
#if UNITY_ANDROID && !UNITY_EDITOR
        string patchDir = Path.Combine (Application.persistentDataPath, PATCH_DIR);
#else
        string patchDir = Path.Combine( Application.streamingAssetsPath, PATCH_DIR );
#endif
        if( !File.Exists( Path.Combine( patchDir, patchName ) ) ) {
            throw new FileNotFoundException( patchDir );
        }

        Debug.Log( string.Format( "Trying to open patch {0} in {1}", patchName, patchDir ) );
        var ptr = libpd_OpenPatch( patchName, patchDir );

        if(ptr == IntPtr.Zero)
        {
            throw new IOException("unable to open patch " + patchName );
        }

        int dollarZero = libpd_GetDollarZero( ptr );
        _openPatches.Add( dollarZero, ptr );

        return dollarZero;
    }

#if UNITY_IPHONE
    [DllImport ("__Internal")]
#else
    [DllImport("AudioPlugin_UnityPd")]
#endif    
    private static extern void libpd_ClosePatch( IntPtr patchPtr );
    public static void ClosePatch( int patchHandle ) {
        IntPtr patchPtr;
        if ( _openPatches.TryGetValue( patchHandle, out patchPtr ) ) {
            libpd_ClosePatch( patchPtr );
            _openPatches.Remove( patchHandle );
        }
        else
            Debug.LogWarning( "Patch " + patchPtr + " not open" );
    }

    /// <summary>
    /// Copies entire PD directory from StreamingAssets (in the APK) to a persistant, non-packed location
    /// TODO be smarter about not doing this all at once
    /// TODO use coroutines to avoid blocking
    /// </summary>
    private static void CopyPdResourcesToPersistentPath() {
        // TODO android java fun
        using ( AndroidJavaClass jc = new AndroidJavaClass("com.unity3d.player.UnityPlayer") ) {
            using ( AndroidJavaObject jo = jc.GetStatic<AndroidJavaObject>("currentActivity") ) {
                using ( AndroidJavaClass pdHelperClass = new AndroidJavaClass( "com.weplaydots.UnityPdHelper.UnityPdHelper" ) ) {
                    pdHelperClass.CallStatic( "SetContext", jo );
                    pdHelperClass.CallStatic( "CopyAssetsFolderToSd", PATCH_DIR, Application.persistentDataPath );
                    Debug.Log( "Assets copied" );
                }
            }
        }
    }

    #endregion

    #region Messages
#if UNITY_IPHONE
    [DllImport ("__Internal")]
#else
    [DllImport("AudioPlugin_UnityPd")]
#endif 
    private static extern void libpd_float( string receiver, float message );
    public static void SendFloat( string receiver, float message ) {
        libpd_float( receiver, message );
    }
    #endregion
}
