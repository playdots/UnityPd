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

    private static UnityPD _instance;

    void Awake() {
        if ( _instance && _instance != this ) {
            Debug.LogWarning( "[#UnityPd] UnityPD already in scene, destroying" );
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

#if UNITY_EDITOR_WIN
    private static void libpd_EnableAudio() { }
#else
    #if UNITY_IPHONE
    [DllImport( "__Internal" )]
    #else
    [DllImport("AudioPlugin_UnityPd")]
    #endif
    private static extern void libpd_EnableAudio();
#endif

    public static void Init() {
        if ( !_instance ) {
            _instance = FindObjectOfType<UnityPD>();
            if ( !_instance ) {
                _instance = new GameObject( "UnityPD" ).AddComponent<UnityPD>();
            }
        }

        libpd_EnableAudio();

#if UNITY_ANDROID && !UNITY_EDITOR
        CopyPdResourcesToPersistentPath();
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


#if UNITY_EDITOR_WIN
    private static void libpd_clear_search_path() { }
#else
    #if UNITY_IPHONE
    [DllImport( "__Internal" )]
    #else
    [DllImport("AudioPlugin_UnityPd")]
    #endif
    private static extern void libpd_clear_search_path();
#endif

    /// <summary>
    /// clears the search path for pd externals
    /// </summary>
    public static void ClearSearchPath() {
        libpd_clear_search_path();
    }


#if UNITY_EDITOR_WIN
    private static void libpd_add_to_search_path( string path ) { }
#else
    #if UNITY_IPHONE
    [DllImport( "__Internal" )]
    #else
    [DllImport("AudioPlugin_UnityPd")]
    #endif
    private static extern void libpd_add_to_search_path( string path );
#endif

    /// <summary>
    /// adds a directory to the search paths
    /// </summary>
    /// <param name="sym">directory to add</param>
    public static void AddToSearchPath( string sym ) {
        libpd_add_to_search_path( sym );
    }

#if UNITY_EDITOR_WIN
    private static int libpd_GetDollarZero( IntPtr intPtr ) { return -1; }
#else
    #if UNITY_IPHONE
    [DllImport( "__Internal" )]
    #else
    [DllImport("AudioPlugin_UnityPd")]
    #endif
    private static extern int libpd_GetDollarZero( IntPtr intPtr );
#endif

#if UNITY_EDITOR_WIN
    private static IntPtr libpd_OpenPatch( string patchName, string patchDir ) { return default( IntPtr ); }
#else
    #if UNITY_IPHONE
    [DllImport( "__Internal" )]
    #else
    [DllImport("AudioPlugin_UnityPd")]
    #endif
    private static extern IntPtr libpd_OpenPatch( string patchName, string patchDir );
#endif

    public static int OpenPatch( string patchName ) {
#if UNITY_EDITOR_WIN
        return -1;
#else
    #if UNITY_ANDROID && !UNITY_EDITOR
        string patchDir = Path.Combine (Application.persistentDataPath, PATCH_DIR);
    #else
        string patchDir = Path.Combine( Application.streamingAssetsPath, PATCH_DIR );
    #endif
        if ( !File.Exists( Path.Combine( patchDir, patchName ) ) ) {
            throw new FileNotFoundException( patchDir );
        }

        Debug.Log( string.Format( "[#UnityPd] Opening patch {0} in {1}", patchName, patchDir ) );
        var ptr = libpd_OpenPatch( patchName, patchDir );

        if ( ptr == IntPtr.Zero ) {
            throw new IOException( "unable to open patch " + patchName );
        }

        int dollarZero = libpd_GetDollarZero( ptr );
        _openPatches.Add( dollarZero, ptr );

        return dollarZero;
#endif
    }

#if UNITY_EDITOR_WIN
    private static void libpd_ClosePatch( IntPtr patchPtr ) {}
#else
    #if UNITY_IPHONE
    [DllImport( "__Internal" )]
    #else
    [DllImport("AudioPlugin_UnityPd")]
    #endif
    private static extern void libpd_ClosePatch( IntPtr patchPtr );
#endif

    public static void ClosePatch( int patchHandle ) {
#if UNITY_EDITOR_WIN
        return;
#else
        IntPtr patchPtr;
        if ( _openPatches.TryGetValue( patchHandle, out patchPtr ) ) {
            libpd_ClosePatch( patchPtr );
            _openPatches.Remove( patchHandle );
        }
        else
            Debug.LogWarning( "[#UnityPd] Patch " + patchPtr + " not open" );
#endif
    }

    /// <summary>
    /// Copies entire PD directory from StreamingAssets (in the APK) to a persistant, non-packed location
    /// TODO be smarter about not doing this all at once
    /// TODO use coroutines to avoid blocking
    /// </summary>
    private static void CopyPdResourcesToPersistentPath() {
        using ( AndroidJavaClass jc = new AndroidJavaClass( "com.unity3d.player.UnityPlayer" ) ) {
            using ( AndroidJavaObject jo = jc.GetStatic<AndroidJavaObject>( "currentActivity" ) ) {
                using ( AndroidJavaClass pdHelperClass = new AndroidJavaClass( "com.weplaydots.UnityPdHelper.UnityPdHelper" ) ) {
                    pdHelperClass.CallStatic( "SetContext", jo );
                    pdHelperClass.CallStatic( "CopyAssetsFolderToSd", PATCH_DIR, Application.persistentDataPath );
                }
            }
        }
    }

    #endregion

    #region Messages
#if UNITY_EDITOR_WIN
    private static void libpd_float( string receiver, float message ) { }
#else
    #if UNITY_IPHONE
    [DllImport( "__Internal" )]
    #else
    [DllImport("AudioPlugin_UnityPd")]
    #endif
    private static extern void libpd_float( string receiver, float message );
#endif
    public static void SendFloat( string receiver, float message ) {
        libpd_float( receiver, message );
    }
    #endregion
}
