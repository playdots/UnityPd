//
//  Plugin_libpd.cpp
//  AudioPluginDemo
//
//  Created by Eddie Cameron on 3/1/16.
//
//

#include "AudioPluginUtil.h"
#include "z_libpd.h"

namespace UnityPd
{
    enum Param
    {
        P_NUM
    };
    struct EffectData
    {
        
        struct Data
        {
            float p[P_NUM];
        };
        union
        {
            Data data;
            unsigned char pad[(sizeof(Data) + 15) & ~15]; // This entire structure must be a multiple of 16 bytes (and and instance 16 byte aligned) for PS3 SPU DMA requirements
        };
    };
#if !UNITY_SPU
    
    /**
     GUI CREATION
     */
    int InternalRegisterEffectDefinition(UnityAudioEffectDefinition& definition)
    {
        int numparams = P_NUM;
        definition.paramdefs = new UnityAudioParameterDefinition [numparams];
        definition.channels = 2;
        return numparams;
    }
    
    void pdprint(const char *s) {
        fprintf(stderr, "[#PD] %s", s);
    }
    
    /**
     THE SETUP EVENT
     */
    UNITY_AUDIODSP_RESULT UNITY_AUDIODSP_CALLBACK CreateCallback(UnityAudioEffectState* state)
    {
        EffectData* effectdata = new EffectData;
        memset(effectdata, 0, sizeof(EffectData));
        state->effectdata = effectdata;
        InitParametersFromDefinitions(InternalRegisterEffectDefinition, effectdata->data.p);
        
        //setup
        libpd_set_printhook(pdprint);
        
        libpd_init();
        libpd_init_audio(2, 2, state->samplerate);
        
        fprintf(stderr, "Init: %d\n", state->samplerate );
        
        return UNITY_AUDIODSP_OK;
    }
    
    UNITY_AUDIODSP_RESULT UNITY_AUDIODSP_CALLBACK ReleaseCallback(UnityAudioEffectState* state)
    {
        EffectData::Data* data = &state->GetEffectData<EffectData>()->data;
        delete data;
        
        return UNITY_AUDIODSP_OK;
    }
    
    UNITY_AUDIODSP_RESULT UNITY_AUDIODSP_CALLBACK SetFloatParameterCallback(UnityAudioEffectState* state, int index, float value)
    {
        EffectData::Data* data = &state->GetEffectData<EffectData>()->data;
        if(index < 0 || index >= P_NUM)
            return UNITY_AUDIODSP_ERR_UNSUPPORTED;
        data->p[index] = value;
        
        return UNITY_AUDIODSP_OK;
    }
    
    UNITY_AUDIODSP_RESULT UNITY_AUDIODSP_CALLBACK GetFloatParameterCallback(UnityAudioEffectState* state, int index, float* value, char *valuestr)
    {
        EffectData::Data* data = &state->GetEffectData<EffectData>()->data;
        if(index < 0 || index >= P_NUM)
            return UNITY_AUDIODSP_ERR_UNSUPPORTED;
        if(value != NULL)
            *value = data->p[index];
        if(valuestr != NULL)
            valuestr[0] = 0;
        return UNITY_AUDIODSP_OK;
    }
    
    int UNITY_AUDIODSP_CALLBACK GetFloatBufferCallback (UnityAudioEffectState* state, const char* name, float* buffer, int numsamples)
    {
        return UNITY_AUDIODSP_OK;
    }
    
#endif
    
#if !UNITY_PS3 || UNITY_SPU
    
    
#if UNITY_SPU
    EffectData	g_EffectData __attribute__((aligned(16)));
    extern "C"
#endif
    UNITY_AUDIODSP_RESULT UNITY_AUDIODSP_CALLBACK ProcessCallback(UnityAudioEffectState* state, float* inbuffer, float* outbuffer, unsigned int length, int inchannels, int outchannels)
    {
        int numTicks = length / libpd_blocksize();
        
        libpd_process_float(numTicks, inbuffer, outbuffer);
        
        return UNITY_AUDIODSP_OK;
    }
    
#endif
    
    extern "C" UNITY_AUDIODSP_EXPORT_API void libpd_EnableAudio() {
        libpd_add_float(1.0f);
        libpd_finish_message("pd", "dsp");
    }
    
    extern "C" UNITY_AUDIODSP_EXPORT_API void* libpd_OpenPatch( const char* patchName, const char* directory )
    {
        void* patch = libpd_openfile(patchName, directory);
        return patch;
    }
    
    extern "C" UNITY_AUDIODSP_EXPORT_API void libpd_ClosePatch( void *patchPtr )
    {
        libpd_closefile(patchPtr);
    }
    
    extern "C" UNITY_AUDIODSP_EXPORT_API int libpd_GetDollarZero( void *patchPtr )
    {
        return libpd_getdollarzero(patchPtr);
    }
}
