//
//  Plugin_libpd.cpp
//  AudioPluginDemo
//
//  Created by Eddie Cameron on 3/1/16.
//
//

#include "AudioPluginUtil.h"
#include "z_libpd.h"

const int NUM_CHANNELS = 1;

namespace UnityPd
{
    enum Param
    {
        P_FREQ,
        P_NUM
    };
    
    bool wasTriggered = false;
    
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
    
    void LoadPatch() {
    }
    
#if !UNITY_SPU
    
    /**
     GUI CREATION
     */
    int InternalRegisterEffectDefinition(UnityAudioEffectDefinition& definition)
    {
        int numparams = P_NUM;
        definition.paramdefs = new UnityAudioParameterDefinition [numparams];
        RegisterParameter(definition, "(internal_freq)", "", 0.0f, 10000.0f, 0.0f, 1.0f, 2.0f, P_FREQ);
        return numparams;
    }
    
    /**
     THE SETUP EVENT
     */
    UNITY_AUDIODSP_RESULT UNITY_AUDIODSP_CALLBACK CreateCallback(UnityAudioEffectState* state)
    {
        //        maxiSettings::sampleRate = state->samplerate;
        EffectData* effectdata = new EffectData;
        memset(effectdata, 0, sizeof(EffectData));
        state->effectdata = effectdata;
        InitParametersFromDefinitions(InternalRegisterEffectDefinition, effectdata->data.p);
        
        //setup
        libpd_init();
        libpd_init_audio(2, 2, state->samplerate);
        fprintf(stderr, "Init: %d", state->samplerate );
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
        EffectData::Data* data = &state->GetEffectData<EffectData>()->data;

#if UNITY_SPU
        UNITY_PS3_CELLDMA_GET(&g_EffectData, state->effectdata, sizeof(g_EffectData));
        data = &g_EffectData.data;
#endif
        
        //if retrigger is high, retrigger the note
        if (wasTriggered){
            wasTriggered = false;
            
            //trigger a new note;
        }
        
        int numTicks = length / libpd_blocksize();
        libpd_process_float(numTicks, inbuffer, outbuffer);
        
#if UNITY_SPU
        UNITY_PS3_CELLDMA_PUT(&g_EffectData, state->effectdata, sizeof(g_EffectData));
#endif
        return UNITY_AUDIODSP_OK;
    }
    
    
    
#endif
    
    
    extern "C" UNITY_AUDIODSP_EXPORT_API void* libpd_OpenPatch( const char* patchName, const char* directory )
    {
        void* patch = libpd_openfile(patchName, directory);
        libpd_add_float(1.0f);
        libpd_finish_message("pd", "dsp");
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
