/* 
 * Plugin_UnityPd.cpp
 * Copyright (C) 2017 Playdots, Inc.
 * ----------------------------
 */

#include "AudioPluginUtil.h"
#ifndef UNITY_WIN
#include <z_libpd.h>
#include <mutex>
#endif

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
    
    std::mutex mtx;
    
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
#ifndef UNITY_WIN
        libpd_set_printhook(pdprint);
        
        libpd_init();
        libpd_init_audio(2, 2, state->samplerate);
        
        fprintf(stderr, "Init: %d\n", state->samplerate );
#endif
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
    
    UNITY_AUDIODSP_RESULT UNITY_AUDIODSP_CALLBACK ProcessCallback(UnityAudioEffectState* state, float* inbuffer, float* outbuffer, unsigned int length, int inchannels, int outchannels)
    {
#ifndef UNITY_WIN
        mtx.lock();
        int numTicks = length / libpd_blocksize();
        libpd_process_float(numTicks, inbuffer, outbuffer);
        mtx.unlock();
#endif

        return UNITY_AUDIODSP_OK;
    }
    
#ifndef UNITY_WIN
    extern "C" UNITY_AUDIODSP_EXPORT_API void UnityPd_EnableAudio() {
        mtx.lock();
        if (libpd_start_message(16)) { // request space for 16 elements
            // handle allocation failure, very unlikely in this case
        }
        libpd_add_float(1.0f);
        libpd_finish_message("pd", "dsp");
        mtx.unlock();
    }
    
    extern "C" UNITY_AUDIODSP_EXPORT_API void UnityPd_SendFloat( const char* receiver, float message ) {
        mtx.lock();
        libpd_float(receiver, message);
        mtx.unlock();
    }
    
    extern "C" UNITY_AUDIODSP_EXPORT_API void UnityPd_SendBang( const char* receiver ) {
        mtx.lock();
        libpd_bang(receiver);
        mtx.unlock();
    }
    
    extern "C" UNITY_AUDIODSP_EXPORT_API void UnityPd_SendSymbol( const char* receiver, const char* message ) {
        mtx.lock();
        libpd_symbol(receiver, message);
        mtx.unlock();
    }
    
    extern "C" UNITY_AUDIODSP_EXPORT_API void* UnityPd_OpenPatch( const char* patchName, const char* directory )
    {
        mtx.lock();
        void* patch = libpd_openfile(patchName, directory);
        mtx.unlock();
        
        return patch;
    }
    
    extern "C" UNITY_AUDIODSP_EXPORT_API void UnityPd_ClosePatch( void *patchPtr )
    {
        mtx.lock();
        libpd_closefile(patchPtr);
        mtx.unlock();
    }
    
    extern "C" UNITY_AUDIODSP_EXPORT_API int UnityPd_GetDollarZero( void *patchPtr )
    {
        return libpd_getdollarzero(patchPtr);
    }
    
    extern "C" UNITY_AUDIODSP_EXPORT_API void UnityPd_AddToSearchPath( const char* pathToAdd ) {
        mtx.lock();
        libpd_add_to_search_path(pathToAdd);
        mtx.unlock();
    }
    
    extern "C" UNITY_AUDIODSP_EXPORT_API void UnityPd_ClearSearchPath() {
        mtx.lock();
        libpd_clear_search_path();
        mtx.unlock();
    }
#endif
}
