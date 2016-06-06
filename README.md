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
`ndk-build`

#### UnityPdAndroidHelper
Android also neeeds an additional jar to help Pd with its crazy file system
Get Ant if you don't have it already!

`cd UnityPdAndroidHelper`
`ant jar`

### Windows
Sorry, I couldn't get libpd working on Windows with my limited Windows knowledge. 
But there currently is a dummy plugin + bindings so that developers can work on windows (just with no Pd)

## How 2 Add 2 Unity

TODO

## How 2 Use

TODO
