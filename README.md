# tones
Procedural audio generated using unity. 

Based on libpd <https://github.com/libpd/libpd> which is an embeddable version of 
pure-data <https://github.com/pure-data/pure-data> which is a tiny audio synthesis library

This project builds Unity Native Audio <http://docs.unity3d.com/Manual/AudioMixerNativeAudioPlugin.html> 
plugin versions of libpd, and provides the bindings for some common functions.


## How 2 Build
This is distributed with pre-built release binaries inside `UnityPd/Platform`, 
but if you want to build yourself, follow these steps to replace them

### iOS & OSX

#### libPD & Unity native audio plugin
Open XCode project in `DotsSynthNative/NativeCode/Xcode`
iOS Switch target to AudioPlugin_UnityPd_iOS, Destination to Generic iOS device
For OSX Switch target to AudioPluginDemo, Destination as default

To build Debug versions of the plugins, make sure to switch the Targets of both the main Project and the libpd sub-project to Debug

### Android

#### libPD & Unity native audio plugin
Get the JDK! <http://developer.android.com/intl/es/tools/sdk/ndk/index.html>

`cd DotsSynthNative/NativeCode/Android`
`ndk-build` (or `ndk-build DEBUG=true` for a debug enabled .so)

#### UnityPdAndroidHelper
Android also neeeds an additional jar to help Pd with its crazy file system
Get Ant if you don't have it already!

`cd UnityPdAndroidHelper`
`ant jar`

### Windows
Sorry, I couldn't get libpd working on Windows with my limited Windows knowledge. 
But there currently is a dummy plugin + bindings so that developers can work on windows (just with no Pd)


## How 2 Add 2 Unity

Copy the `UnityPd` folder into your project's `Plugins` folder. Unity should be smart 
enough to set up all the plugins for each platform, but check that each Plugin is only enabled on the 
appropriate platform if you have issues.

Add an Audio Mixer Group to your project if you haven't already, and then a `UnityPD` effect to a mixer. 
If you want to purely generate sound with Pd, make sure to turn `Auto Mixer Suspend` on the Mixer Group to
make sure Pd keeps receiving process calls even when no sounds are being generated.


## How 2 Use

UnityPD functions are all defined in `UnityPd.cs`. Currently only simple bindings are included:
- Init()
Call before any other UnityPD functions. This enables Pd's audio processing, and adds the default patch 
folder (`StreamingAssets/pd`) to Pd's search path. On android it also copies the patches to a folder outside
the apk so that Pd can find dependencies

- Deinit()
Closes the UnityPd session

- OpenPatch( string patchName )
Tells Pd to open an instance of the patch of the given name from inside the StreamingAssets/Pd folder. Returns the $0(id)
of the opened patch

- ClosePatch( int patchHandle )
Closes the patch with the given $0 handle

- SendFloat( string receiver, float message )
Send a float to the given receiver (remember receivers are global across all open patches)
