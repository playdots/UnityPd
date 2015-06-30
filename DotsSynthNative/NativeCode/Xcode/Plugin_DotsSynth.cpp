#include "AudioPluginUtil.h"
#include "Synth.h"

namespace DotsSynth
{
	enum Param
	{
		P_FREQ,
        P_TRIGGER,
        P_OSC,
        P_ATTACK,
        P_DECAY,
        P_SUSTAIN,
        P_DURATION,
        P_RELEASE,
		P_NUM
	};
    
    Synth synth;
    
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

#if !UNITY_SPU

    /**
        GUI CREATION
     */
	int InternalRegisterEffectDefinition(UnityAudioEffectDefinition& definition)
	{
		int numparams = P_NUM;
		definition.paramdefs = new UnityAudioParameterDefinition [numparams];
        RegisterParameter(definition, "(internal_freq)", "", 0.0f, 10000.0f, 0.0f, 1.0f, 2.0f, P_FREQ);
        RegisterParameter(definition, "(internal_trigger)", "", 0.0f, 1.0f, 0.0f, 1.0f, 1.0f, P_TRIGGER);
        RegisterParameter(definition, "Oscillator", "",0.0f, 3.0f, 0.0f, 1.0f, 1.0f, P_OSC);
        RegisterParameter(definition, "Attack", "ms", 0.0f, 3000.0f, 5.0f, 1.0f, 2.0f, P_ATTACK);
        RegisterParameter(definition, "Decay", "ms", 0.0f, 3000.0f, 100.0f, 1.0f, 2.0f, P_DECAY);
        RegisterParameter(definition, "Sustain", "db", -60.0f, 0.0f, -49.0f, 1.0f, 2.0f, P_SUSTAIN);
        RegisterParameter(definition, "Duration", "ms", 0.0f, 3000.0f, 0.0f, 1.0f, 1.0f, P_DURATION);
        RegisterParameter(definition, "Release", "ms",0.0f, 3000.0f, 100.0f, 1.0f, 2.0f, P_RELEASE);
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
        synth.setSampleRate(state->samplerate);
        
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
        if (index == P_TRIGGER){
            wasTriggered = true;
        }
        if (index == P_OSC){
            synth.setOscType((int) data->p[P_OSC]);
        }
        //recompute the adsr if any of the values were changed
        if (index == P_ATTACK || index == P_DECAY ||
            index == P_SUSTAIN || index == P_DURATION || index == P_RELEASE)
        {
            

            synth.setEnvelope(data->p[P_ATTACK], data->p[P_DECAY], data->p[P_SUSTAIN],
                              data->p[P_DURATION], data->p[P_RELEASE]);
        }
        //if the value is either the frequency or the velocity
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
            synth.triggerNote(data->p[P_FREQ]);
        }
        
        /**
            THE BUFFER FILLING LOOP
         */
        for(unsigned int n = 0; n < length; n++)
        {
        
            float sample = 0;
            
            sample += synth.tick();
            
            //copy it over for the number of channels there are
            for(int i = 0; i < inchannels; i++){
                *outbuffer++ = sample;
            }
		}

#if UNITY_SPU
		UNITY_PS3_CELLDMA_PUT(&g_EffectData, state->effectdata, sizeof(g_EffectData));
#endif
		return UNITY_AUDIODSP_OK;
	}
    
    

#endif
}
