#include "AudioPluginUtil.h"
#include "Tonic.h"
#include "maximilian.h"

#define POLYPHONY 8

using namespace Tonic;

namespace DotsSynth
{
	enum Param
	{
		P_FREQ,
		P_NUM
	};
    
    
    //the oscillators and envelopes
    maxiOsc OSC[POLYPHONY];
    maxiEnvelope ADSR[POLYPHONY];
    double oscPitches[POLYPHONY];
    
    
    //These are the control values for the envelope
    double adsrEnv[8]={1,5,0.125,100,0.125,200,0,1000};
    
    double pitches[] = {440, 554.365, 659.255, 830.609, 987.767};
    int numberOfNotes = 5;
    int noteNumber = 0;
    
    //the metronome
    maxiOsc timer;
    
    int currentCount,lastCount,voice=0;//these values are used to check if we have a new beat this sample
    
		
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
		RegisterParameter(definition, "Frequency", "hz", 20.0f, 10000.0f, 440.0f, 1.0f, 2.0f, P_FREQ);
		return numparams;
	}

    /**
        THE SETUP EVENT
    */
	UNITY_AUDIODSP_RESULT UNITY_AUDIODSP_CALLBACK CreateCallback(UnityAudioEffectState* state)
	{
        setSampleRate(state->samplerate);
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

            currentCount=(int)timer.phasor(2);//this sets up a metronome that ticks 8 times a second
            
            if (lastCount!=currentCount) {//if we have a new timer int this sample, play the sound
                
                if (voice==6) {
                    voice=0;
                }
                //set a new pitch
                oscPitches[voice] = pitches[noteNumber];
                noteNumber++;
                noteNumber = noteNumber % numberOfNotes;
                
                //trigger the envelope
                ADSR[voice].trigger(0, adsrEnv[0]);//trigger the envelope from the start

                //increment the voice
                voice++;
                
                lastCount=0;
            }
        
            //osc.sinewave(data->p[P_FREQ]);
            float sample = 0;
            
            for (int i = 0; i < POLYPHONY; i++){
                sample += ADSR[i].line(8,adsrEnv) * OSC[i].saw(oscPitches[i]);
            }            
            
            
            for(int i = 0; i < inchannels; i++)
			{
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
