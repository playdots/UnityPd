#import "UnityAppController.h"

#include "AudioPluginInterface.h"

//////////////////////////////////////////////////////////////////////////////////////
// Dots custom subclass of UnityAppController
// See UnityAppController.m/h here http://docs.unity3d.com/Manual/StructureOfXcodeProject.html
//////////////////////////////////////////////////////////////////////////////////////

@interface UnityPdAppController : UnityAppController {}
@end

@implementation UnityPdAppController

- (void)preStartUnity {
    [super preStartUnity];
    UnityRegisterAudioPlugin(&UnityGetAudioEffectDefinitions); // intialize native audio plugins
}

@end

IMPL_APP_CONTROLLER_SUBCLASS( UnityPdAppController )
