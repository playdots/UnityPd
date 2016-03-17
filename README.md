# tones
Procedural audio generated using unity. 

## How 2 Build
Current build setup will copy all output into `UnityPd/<PLATFORM>`

### iOS & OSX

#### libPD & Unity native audio plugin
Open XCode project in `DotsSynthNative/NativeCode/Xcode`
iOS Switch target to AudioPlugin_UnityPd_iOS, Destination to Gnereic iOS device
For OSX Switch target to AudioPluginDemo, Destination as default


### Android

#### libPD & Unity native audio plugin
Get the JDK! http://developer.android.com/intl/es/tools/sdk/ndk/index.html

`cd DotsSynthNative/NativeCode/Android`
`ndk-build`

#### UnityPdAndroidHelper
Android also neeeds an additional jar to help Pd with its crazy file system
Get Ant if you don't have it already!

`cd UnityPdAndroidHelper`
`ant jar`
