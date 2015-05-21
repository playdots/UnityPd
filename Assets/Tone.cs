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
	public int polyphony = 32;

	public OscillatorType type;

	[Tooltip("Milliseconds")]
	public float noteDuration = 0f;

	[Header("Envelope")]
	[Range(0.0f,5000.0f)]
	[Tooltip("Milliseconds")]
	public float attack = 2.43f;

	[Range(0.0f,5000.0f)]
	[Tooltip("Milliseconds")]
	public float decay = 855f;

	[Range(-100.0f,0f)]
	[Tooltip("Decibels")]
	public float sustain = -49f;

	[Range(0.0f,5000.0f)]
	[Tooltip("Milliseconds")]
	public float release = 519f;


	/**
	 * 
	 */
	private static int sampleRate = 44100;

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

	/**
	 * called when the inspector is updated
	 */
	void OnValidate() {
		if (isInitialized) {
			//set the oscillator type
			SetOscType(type);
			//set the envelope values
			SetEnvelope(attack / 1000f, decay / 1000f, dbToGain(sustain), release / 1000f);
			noteDuration = Mathf.Max(noteDuration, 0);
			SetNoteDuration(noteDuration / 1000f);
		}
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
		for (int i = 0; i < data.Length; i = i + channels){
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
				return;
			}
		}
	}

	public void SetOscType(OscillatorType type){
		for (var i = 0; i < voices.Length; i++){
			voices[i].SetOscType(type);
		}
	}

	public void SetEnvelope(float attack, float decay, float sustain, float release){
		for (var i = 0; i < voices.Length; i++){
			voices[i].SetEnvelope(attack, decay, sustain, release);
		}
	}

	/**
	 * set the note duration (sustain time) of each of the voices
	 */
	public void SetNoteDuration(float noteDuration){
		for (var i = 0; i < voices.Length; i++){
			voices[i].SetSustainTime(noteDuration);
		}
	}

	/**
	 * UTILITIES
	 */

	/**
	 * returns the value of decibels on a gain scale
	 */
	private float dbToGain(float db){
		return Mathf.Pow(2f, db / 6f);
	}

	/**
	 * converts gain values (0-1) to decibels
	 */
	private float gainToDb(float gain){
		return 20f * Mathf.Log10(gain);	
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

		public OscillatorType GetOscType(){
			return this.osc.type;
		}

		public void SetOscType(OscillatorType type){
			this.osc.type = type;
		}

		/**
		 * Set the envelope values
		 */
		public void SetEnvelope(float attack, float decay, float sustain, float release){
			this.env.attackTime = attack;
			this.env.decayTime = decay;
			this.env.sustainValue = sustain;
			this.env.releaseTime = release;
		}

		/**
		 * Set the sustain time in seconds
		 */
		public void SetSustainTime(float sustainTime){
			this.env.sustainTime = sustainTime;
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
			float increment = (float)(this.frequency / sampleRate);
			switch (this.type) {
			case OscillatorType.Sine: 
				increment *=  2.0f * (float) Math.PI;
				for (var i = 0; i < data.Length; i++){
					this.phase = this.phase + increment;
					data[i] = (float) Math.Sin(this.phase);
				}
				break;
			case OscillatorType.Square: 
				for (var i = 0; i < data.Length; i++){
					this.phase = this.phase + increment;
					this.phase = this.phase % 1;
					data[i] = this.phase < 0.5 ? -1 : 1;
				}
				break;
			case OscillatorType.Sawtooth: 
				for (var i = 0; i < data.Length; i++){
					this.phase = this.phase + increment;
					this.phase = this.phase % 1;
					data[i] = (1.0f - this.phase) * 2 - 1;
				}
				break;
			case OscillatorType.Triangle: 
				for (var i = 0; i < data.Length; i++){
					this.phase = this.phase + increment;
					this.phase = this.phase % 1;
					if (this.phase > 0.5){
						data[i] = (this.phase - 0.5f) * 4.0f - 1.0f;
					} else {
						data[i] = -this.phase * 4.0f + 1.0f;
					}

				}
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

		//the slope of the envelope for each of the phases
		private float attackStep = 0;
		private float sustainSamples = 0;
		
		//decay and release are exponential
		private float decaySamples = 0;
		private float releaseSamples = 0;

		//the progress through the current phase
		private float phaseProgress = 0;


		/**
		 * ENVELOPE TIMING
		 */
		public float attackTime {
			set { attackStep = (1.0f / sampleRate) / value;}
			get { return (1.0f / sampleRate) / attackStep; }
		}
		public float decayTime {
			set { decaySamples = (int) (value * sampleRate);}
			get { return (float) (decaySamples / sampleRate); }

		}
		public float sustainTime {
			set { sustainSamples = (int) (value * sampleRate); Debug.Log (sustainSamples);}
			get { return (float) (sustainSamples / sampleRate); }
		}
		public float releaseTime {
			set { releaseSamples = (int) (value * sampleRate);}
			get { return (float) (releaseSamples / sampleRate); }
		}

		public float sustainValue = 0.35f;

		public Envelope(){
			//set some initial values
			attackTime = 0.00243f;
			decayTime = 0.855f;
			sustainValue = 0.0035f;
			sustainTime = 0.0f;
			releaseTime = 0.519f;
		}


		private float currentSample = 0.0f;

		private float decaySlope = 0.5f;
						
		public void OnAudioProcess(ref float[] data){
			float progress;
			for (int i = 0; i < data.Length; i++){
				switch(this.phase){
				case EnvelopePhase.Attack:
					currentSample += attackStep;
					if (currentSample >= 1){
						currentSample = 1;
						this.phase++;
						phaseProgress = 0;
					}
					break;
				case EnvelopePhase.Decay:
//					currentSample = decayBase + currentSample * decayCoef;
					progress = phaseProgress / decaySamples;
					progress = Mathf.Pow(progress, decaySlope);
					currentSample = Mathf.Lerp(1, sustainValue, progress);
					if (currentSample <= sustainValue){
						currentSample = sustainValue;
						this.phase++;
						phaseProgress = 0;
					}
					break;
				case EnvelopePhase.Sustain:
					phaseProgress++;
					if (phaseProgress > sustainSamples){
						this.phase++;
						phaseProgress = 0;
					}
					break;
				case EnvelopePhase.Release:
					progress = phaseProgress / releaseSamples;
					progress = Mathf.Pow(progress, decaySlope);
					currentSample = Mathf.Lerp(sustainValue, 0, progress);
					if (currentSample <= 0){
						currentSample = 0;
						this.phase++;
						phaseProgress = 0;
					}
					break;
				}
				data[i] = currentSample * data[i];
				phaseProgress++;
			}
		}

		private float calcCoef(float rate, float targetRatio) {
			return Mathf.Exp(-Mathf.Log((1.0f + targetRatio) / targetRatio) / rate);
		}
	
		/**
		 * returns true if the synth does not need
		 * to be processed
		 */
		public Boolean OnStandby(){
			return this.phase == EnvelopePhase.Standby;
		}

		/**
		 * trigger the envelope attack
		 */
		public void TriggerAttack(){
			this.phase = EnvelopePhase.Attack;
		}
	}
}
