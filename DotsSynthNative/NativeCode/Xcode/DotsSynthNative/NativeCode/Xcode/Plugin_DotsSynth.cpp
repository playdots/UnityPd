#include "AudioPluginUtil.h"
#include "Tonic/Tonic.h"

namespace DotsSynth
{
	enum Param
	{
		P_FREQ,
		P_NUM
	};
		
	struct EffectData
	{
		struct Data
		{
			float p[P_NUM];
            Random random;
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
		RegisterParameter(definition, "Frequency", "hz", 20.0f, 10000.0f, 440.0f, 1.0f, 2.0f, P_FREQ);
		return numparams;
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

	static float freqs[] = { 100, 225, 330, 470, 700, 1030, 1500, 2280, 3300, 4700, 9000 };

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

        /**
            THE BUFFER FILLING LOOP
         */

		for(unsigned int n = 0; n < length; n++)
		{
			for(int i = 0; i < inchannels; i++)
			{
                *outbuffer++ = data->random.GetFloat(-1, 1);
            }
		}

#if UNITY_SPU
		UNITY_PS3_CELLDMA_PUT(&g_EffectData, state->effectdata, sizeof(g_EffectData));
#endif
		return UNITY_AUDIODSP_OK;
	}

#endif
}
