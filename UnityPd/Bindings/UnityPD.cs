/* <copyright file="UnityPd.cs" company="Playdots, Inc.">
 * Copyright (C) 2017 Playdots, Inc.
 * </copyright>
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

    public static string PdPatchDirectory {
        get {
#if UNITY_ANDROID && !UNITY_EDITOR
            return Path.Combine( Application.persistentDataPath, PATCH_DIR );
#else
            return Path.Combine( Application.streamingAssetsPath, PATCH_DIR );
#endif
        }
    }

    void Awake() {
        if ( _instance && _instance != this ) {
            Logger.LogWarning( "UnityPD already in scene, destroying" );
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
    private static void UnityPd_EnableAudio() { }
#else
    #if UNITY_IPHONE
    [DllImport( "__Internal" )]
    #else
    [DllImport("AudioPlugin_UnityPd")]
    #endif
    private static extern void UnityPd_EnableAudio();
#endif

    public static void Init() {
        if ( !_instance ) {
            _instance = FindObjectOfType<UnityPD>();
            if ( !_instance ) {
                _instance = new GameObject( "UnityPD" ).AddComponent<UnityPD>();
            }
        }

        Logger.Log( "Initializing..." );
        UnityPd_EnableAudio();

#if UNITY_ANDROID && !UNITY_EDITOR
        CopyPdResourcesToPersistentPath();
#endif
        
        AddToSearchPath( PdPatchDirectory );
        Logger.Log( "...Initialized!" );
    }

    public static void Deinit() {
        foreach ( var patchPtr in _openPatches.Values )
            UnityPd_ClosePatch( patchPtr );
        _openPatches.Clear();

        ClearSearchPath();
    }
    #endregion

    #region Patches


#if UNITY_EDITOR_WIN
    private static void UnityPd_ClearSearchPath() { }
#else
    #if UNITY_IPHONE
    [DllImport( "__Internal" )]
    #else
    [DllImport("AudioPlugin_UnityPd")]
    #endif
    private static extern void UnityPd_ClearSearchPath();
#endif

    /// <summary>
    /// clears the search path for pd externals
    /// </summary>
    public static void ClearSearchPath() {
        UnityPd_ClearSearchPath();
    }

    public static void ClearAddedSearchPaths() {
        ClearSearchPath();

        AddToSearchPath( PdPatchDirectory );
    }


#if UNITY_EDITOR_WIN
    private static void UnityPd_AddToSearchPath( string path ) { }
#else
    #if UNITY_IPHONE
    [DllImport( "__Internal" )]
    #else
    [DllImport("AudioPlugin_UnityPd")]
    #endif
    private static extern void UnityPd_AddToSearchPath( string path );
#endif

    /// <summary>
    /// adds a directory to the search paths
    /// </summary>
    /// <param name="sym">directory to add</param>
    public static void AddToSearchPath( string sym ) {
        UnityPd_AddToSearchPath( sym );
    }

#if UNITY_EDITOR_WIN
    private static int UnityPd_GetDollarZero( IntPtr intPtr ) { return -1; }
#else
    #if UNITY_IPHONE
    [DllImport( "__Internal" )]
    #else
    [DllImport("AudioPlugin_UnityPd")]
    #endif
    private static extern int UnityPd_GetDollarZero( IntPtr intPtr );
#endif

#if UNITY_EDITOR_WIN
    private static IntPtr UnityPd_OpenPatch( string patchName, string patchDir ) { return default( IntPtr ); }
#else
    #if UNITY_IPHONE
    [DllImport( "__Internal" )]
    #else
    [DllImport("AudioPlugin_UnityPd")]
    #endif
    private static extern IntPtr UnityPd_OpenPatch( string patchName, string patchDir );
#endif

    public static int OpenPatch( string patchName ) {
#if UNITY_EDITOR_WIN
        return -1;
#else
        string patchDir = PdPatchDirectory;

        if ( !File.Exists( Path.Combine( patchDir, patchName ) ) ) {
            throw new FileNotFoundException( patchDir );
        }

        Logger.Log( string.Format( "Opening patch {0} in {1}", patchName, patchDir ) );
        var ptr = UnityPd_OpenPatch( patchName, patchDir );

        if ( ptr == IntPtr.Zero ) {
            throw new IOException( "unable to open patch " + patchName );
        }

        int dollarZero = UnityPd_GetDollarZero( ptr );
        _openPatches.Add( dollarZero, ptr );

        return dollarZero;
#endif
    }

#if UNITY_EDITOR_WIN
    private static void UnityPd_ClosePatch( IntPtr patchPtr ) {}
#else
    #if UNITY_IPHONE
    [DllImport( "__Internal" )]
    #else
    [DllImport("AudioPlugin_UnityPd")]
    #endif
    private static extern void UnityPd_ClosePatch( IntPtr patchPtr );
#endif

    public static void ClosePatch( int patchHandle ) {
#if UNITY_EDITOR_WIN
        return;
#else
        IntPtr patchPtr;
        if ( _openPatches.TryGetValue( patchHandle, out patchPtr ) ) {
            UnityPd_ClosePatch( patchPtr );
            _openPatches.Remove( patchHandle );
        }
        else
            Logger.LogWarning( "Patch " + patchPtr + " not open" );
#endif
    }

#if UNITY_ANDROID
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
                    pdHelperClass.CallStatic( "CopyAssetsFolderToPersistantData", PATCH_DIR, Application.persistentDataPath );
                }
            }
        }
    }
#endif
    #endregion

    #region Messages
#if UNITY_EDITOR_WIN
    private static void UnityPd_SendFloat( string receiver, float message ) { }
#else
    #if UNITY_IPHONE
    [DllImport( "__Internal" )]
    #else
    [DllImport("AudioPlugin_UnityPd")]
    #endif
    private static extern void UnityPd_SendFloat( string receiver, float message );
#endif
    public static void SendFloat( string receiver, float message ) {
        UnityPd_SendFloat( receiver, message );
    }

#if UNITY_EDITOR_WIN
    private static void UnityPd_SendBang( string receiver ) { }
#else
#if UNITY_IPHONE
    [DllImport( "__Internal" )]
#else
    [DllImport("AudioPlugin_UnityPd")]
#endif
    private static extern void UnityPd_SendBang( string receiver );
#endif
    public static void SendBang( string receiver ) {
        UnityPd_SendBang( receiver );
    }

#if UNITY_EDITOR_WIN
    private static void UnityPd_SendSymbol( string receiver, string message ) { }
#else
#if UNITY_IPHONE
    [DllImport( "__Internal" )]
#else
    [DllImport("AudioPlugin_UnityPd")]
#endif
    private static extern void UnityPd_SendSymbol( string receiver, string message );
#endif
    public static void SendSymbol( string receiver, string message ) {
        UnityPd_SendSymbol( receiver, message );
    }
    #endregion

    private static class Logger {
        [System.Diagnostics.Conditional( "DEVELOPMENT_BUILD" )]
        public static void Log( string logString, UnityEngine.Object context = null ) {
            Debug.Log( "[#UnityPd] " + logString, context );
        }

        [System.Diagnostics.Conditional( "DEVELOPMENT_BUILD" )]
        public static void LogWarning( string logString, UnityEngine.Object context = null ) {
            Debug.LogWarning( "[#UnityPd] " + logString, context );
        }

        [System.Diagnostics.Conditional( "DEVELOPMENT_BUILD" )]
        public static void LogError( string logString, UnityEngine.Object context = null ) {
            Debug.LogError( "[#UnityPd] " + logString, context );
        }
    }
}
