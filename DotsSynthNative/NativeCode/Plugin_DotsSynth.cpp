#include "AudioPluginUtil.h"

namespace Vocoder
{
	enum Param
	{
		P_GAIN,
		P_RESONANCE,
		P_FMTSHIFT,
		P_FMTSCALE,
		P_NUM
	};

	struct Filter
	{
		float p1, p2;
		float a1, a2;
		float z1, z2;
		float z3, z4;
		float gain;
		inline float Process(float input)
		{
			input += 1.0e-11f; // Kill denormals
			float iir1 = input - p1 - a1 * z1 - a2 * z2; z2 = z1; z1 = iir1; p1 = input;
			float iir2 =  iir1 - p2 - a1 * z3 - a2 * z4; z4 = z3; z3 = iir2; p2 = iir1;
			return iir2 * gain;
		}
	};
	
	struct HeterodyneFilter
	{
		UnityComplexNumber phase;
		UnityComplexNumber phaseinc;
		UnityComplexNumber lpf1;
		UnityComplexNumber bpf1;
		UnityComplexNumber lpf2;
		UnityComplexNumber bpf2;
		float cut;
		float bw;
		float gain;
		inline float Process(float input)
		{
			input += 1.0e-11f; // Kill denormals
			UnityComplexNumber h = phase * input; phase = phase * phaseinc;
			lpf1 = lpf1 + bpf1 * cut; bpf1 = bpf1 + (h    - lpf1 - bpf1) * cut;
			lpf2 = lpf2 + bpf2 * cut; bpf2 = bpf2 + (lpf1 - lpf2 - bpf2) * cut;
			return lpf2.Magnitude() * (1.0f - cut - bw);
		}
	};
	
	const int NUMBANDS = 11;
		
	struct EffectData
	{
		struct Data
		{
			float p[P_NUM];
			HeterodyneFilter analysis[8][NUMBANDS];
			Filter synthesis[8][NUMBANDS];
		};
		union
		{
			Data data;
			unsigned char pad[(sizeof(Data) + 15) & ~15]; // This entire structure must be a multiple of 16 bytes (and and instance 16 byte aligned) for PS3 SPU DMA requirements
		};
	};

#if !UNITY_SPU

	int InternalRegisterEffectDefinition(UnityAudioEffectDefinition& definition)
	{
		int numparams = P_NUM;
		definition.paramdefs = new UnityAudioParameterDefinition [numparams];
		RegisterParameter(definition, "Gain", "dB", -100.0f, 0.0f, -30.0f, 1.0f, 1.0f, P_GAIN);
		RegisterParameter(definition, "Resonance", "", 0.0f, 1.0f, 0.7f, 1.0f, 1.0f, P_RESONANCE);
		RegisterParameter(definition, "Formant Shift", "Hz", -500.0f, 500.0f, 0.0f, 1.0f, 3.0f, P_FMTSHIFT);
		RegisterParameter(definition, "Formant Scale", "x", 0.1f, 5.0f, 1.0f, 1.0f, 3.0f, P_FMTSCALE);
		definition.flags |= UnityAudioEffectDefinitionFlags_IsSideChainTarget;
		return numparams;
	}

	UNITY_AUDIODSP_RESULT UNITY_AUDIODSP_CALLBACK CreateCallback(UnityAudioEffectState* state)
	{
		EffectData* effectdata = new EffectData;
		memset(effectdata, 0, sizeof(EffectData));
		state->effectdata = effectdata;
		InitParametersFromDefinitions(InternalRegisterEffectDefinition, effectdata->data.p);
		for(int j = 0; j < NUMBANDS; j++)
			for(int i = 0; i < 8; i++)
				effectdata->data.analysis[i][j].phase.Set(1.0f, 0.0f);
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

		float gain = powf(10.0f, 0.05f * data->p[P_GAIN]);
		float* sidechainBuffer = state->sidechainbuffer;
		float maxfreq = (float)state->samplerate;
		float sampletime = 1.0f / state->samplerate;
		float w0 = 2.0f * kPI * sampletime;
		for(int j = 0; j < NUMBANDS; j++)
		{
			float f = freqs[j] * data->p[P_FMTSCALE] + data->p[P_FMTSHIFT];
			if(f < 10.0f)
				f = 10.0f;
			else if(f > maxfreq)
				f = maxfreq;
			float w = f * w0;
			float r = 0.99f + 0.0099f * data->p[P_RESONANCE];
			float c = -2.0f * r * cosf(w);
			r *= r;
			float g = (0.5f - 0.5f * r) / powf(1.2f, -(float)j);
			UnityComplexNumber phaseinc; phaseinc.Set(cosf(-w), sinf(-w));
			for(int i = 0; i < inchannels; i++)
			{
				HeterodyneFilter& a = data->analysis[i][j];
				a.phaseinc = phaseinc;
				a.gain = 1.0f;
				a.cut = 0.01f;
				a.bw = 0.001f;
				data->synthesis[i][j].a1 = c;
				data->synthesis[i][j].a2 = r;
				data->synthesis[i][j].gain = g;
			}
		}

		for(unsigned int n = 0; n < length; n++)
		{
			for(int i = 0; i < inchannels; i++)
			{
				float input = (*inbuffer++) * gain + 1.0e-11f;
				float sidechainInput = *sidechainBuffer++;
				float sum = 0.0f;
				for(int j = 0; j < NUMBANDS; j++)
					sum += data->synthesis[i][j].Process(input) * data->analysis[i][j].Process(sidechainInput);
				*outbuffer++ = sum;
			}
		}

#if UNITY_SPU
		UNITY_PS3_CELLDMA_PUT(&g_EffectData, state->effectdata, sizeof(g_EffectData));
#endif
		return UNITY_AUDIODSP_OK;
	}

#endif
}
