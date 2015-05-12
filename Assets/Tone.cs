using UnityEngine;
using System.Collections;
using System;

public class Tone : MonoBehaviour {

	/**
	 * oscillator types
	 */ 
	public enum OscillatorType {
		Sine, Square, Triangle, Sawtooth
	}

	/**
	 * public members
	 */
	[Range(0.0f,1.0f)]
	public float gain = 0.5f;

	public OscillatorType type = OscillatorType.Sine;

	private float phase = 0.0f;
	private static int sampleRate = 44100;

	public int polyphony = 8;
	
	private SynthVoice[] voices;

	private Boolean isInitialized = false;


	// Use this for initialization
	void Start () {
		sampleRate = AudioSettings.outputSampleRate;
		voices = new SynthVoice[polyphony];
		for (int i = 0; i < polyphony; i++) {
			voices[i] = new SynthVoice ();
		}
		isInitialized = true;
	}
	
	void OnAudioFilterRead(float[] data, int channels){
		if (isInitialized) {
			for (int v = 0; v < voices.Length; v++){

				SynthVoice voice = voices[v];

				if (!voice.IsSilent()){
					//each voice gets it's own buffer
					float[] voiceData = new float[data.Length];
					voice.OnAudioProcess (ref voiceData);
					
					//mix the buffers
					for (int i = 0; i < data.Length; i = i + channels){
						data[i] += voiceData[i];
					}
				}
			}
		}
		//apply the gain 
		for (int i = 0; i < data.Length; i = i + channels){
			//mix the buffers
			data[i] = gain * data[i];
			//make it stereo
			if (channels == 2){
				data[i + 1] = data[i];
			}
		}
	}

	public void TriggerNote(float freq){
		//take the first voice which is ready
		for (int i = 0; i < voices.Length; i++) {
			if (voices[i].IsSilent()){
				voices[i].TriggerNote (freq);
				Debug.Log(i);
				return;
			}
		}
	}

	/**
	 * A single voice of the synth
	 * 
	 * an oscillator type and an ADSR envelope
	 */
	class SynthVoice {

		private Oscillator osc;

		private Envelope env;

		public SynthVoice(){
			this.osc = new Oscillator();
			this.env = new Envelope();
		}

		public void OnAudioProcess(ref float[] data){
			if (!env.OnStandby()) {
				osc.OnAudioProcess(ref data);
				env.OnAudioProcess(ref data);
			}
		}

		public void TriggerNote(float freq){
			osc.frequency = freq;
			env.TriggerAttack ();
		}

		public bool IsSilent(){
			return env.OnStandby ();
		}
	}

	/**
	 * An oscillator
	 */
	class Oscillator {

		public OscillatorType type = OscillatorType.Sine;

		private float phase = 0.0f;

		public float frequency = 440.0f;

		public void OnAudioProcess(ref float[] data){
			switch (this.type) {
				case OscillatorType.Sine: 
				float increment = (float)(this.frequency * 2 * Math.PI / sampleRate);
				for (var i = 0; i < data.Length; i++){
					this.phase = this.phase + increment;
					// this is where we copy audio data to make them “available” to Unity
					data[i] = (float) Math.Sin(phase);
//					if (this.phase > 2 * Math.PI) this.phase = 0;
				}
				break;
				case OscillatorType.Square: 
					break;
				case OscillatorType.Sawtooth: 
					break;
				case OscillatorType.Triangle: 
					break;
			}
		}
	}


	/**
	 * An ADSR Envelope
	 */
	class Envelope {

		public enum EnvelopePhase {
			Attack, Decay, Sustain, Release, Standby
		}
		
		public EnvelopePhase phase = EnvelopePhase.Standby;

		//the envelop timing in terms of samples
		private int attackSamples = 0;
		private int decaySamples = 0;
		private int sustainSamples = 0;
		private int releaseSamples = 0;

		/**
		 * ENVELOPE TIMING
		 */
		public float attackTime {
			set { attackSamples = (int) (value * sampleRate);}
			get { return (float) (attackSamples / sampleRate); }
		}
		public float decayTime {
			set { decaySamples = (int) (value * sampleRate);}
			get { return (float) (decaySamples / sampleRate); }
		}
		public float sustainTime {
			set { sustainSamples = (int) (value * sampleRate);}
			get { return (float) (sustainSamples / sampleRate); }
		}
		public float releaseTime {
			set { releaseSamples = (int) (value * sampleRate);}
			get { return (float) (releaseSamples / sampleRate); }
		}

		public Envelope(){
			//set some initial values
			attackTime = 0.2f;
			decayTime = 0.4f;
			sustainTime = 1f;
			releaseTime = 5f;
		}

		//sustain value
		public float sustainValue = 0.2f;
	

		//the envelope counter
		private int envelopeSamples = 0;
						
		public void OnAudioProcess(ref float[] data){
			int start = 0;
			int end = 0;
			float min = 0;
			float max = 0;
			float pow = 1;
			switch(this.phase){
				case EnvelopePhase.Attack:
					start = 0;
					end = attackSamples;
					min = 0;
					max = 1;
					break;
				case EnvelopePhase.Decay:
					start = attackSamples;
					end = attackSamples + decaySamples;
					min = 1;
					max = sustainValue;
					pow = 0.5f;
					break;
				case EnvelopePhase.Sustain:
					start = attackSamples + decaySamples;
					end = attackSamples + decaySamples + sustainSamples;
					min = sustainValue;
					max = sustainValue;
					break;
				case EnvelopePhase.Release:
					start = attackSamples + decaySamples + sustainSamples;
					end = attackSamples + decaySamples + sustainSamples + releaseSamples;
					min = sustainValue;
					max = 0;
					pow = 0.5f;
					break;
			}
			float envAmount = 0;
			for (int i = 0; i < data.Length; i++){
				if (envelopeSamples == end){
					this.phase++;
					data[i] *= max;
				} else {
					float progress = (float) (envelopeSamples - start) / (end - start);
					if (pow != 1){
						progress = Mathf.Pow(progress, pow);
					}
					if (i == 0){
						envAmount = Mathf.Lerp(min, max, progress);
					}
					data[i] *= Mathf.Lerp(min, max, progress);
				}
				envelopeSamples++;
			}
			//Debug.Log (envAmount);
		}
	
		/**
		 * returns true if the synth does not need
		 * to be processed
		 */
		public Boolean OnStandby(){
			return this.phase == EnvelopePhase.Standby;
		}

		/**
		 * trigger the envelop
		 */
		public void TriggerAttack(){
			envelopeSamples = 0;
			this.phase = EnvelopePhase.Attack;
		}
	}
}
