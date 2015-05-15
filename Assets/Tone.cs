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
	
	public int polyphony = 32;

	public OscillatorType type;

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

	/*
	 * hack which allows the oscillator type to be set from the inspector
	 * remove for production
	 */
	void Update(){
		if (isInitialized) {
			if (voices[0].GetOscType() != type){
				SetOscType(type);
			}
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
				return;
			}
		}
	}

	public void SetOscType(OscillatorType type){
		for (var i = 0; i < voices.Length; i++){
			voices[i].SetOscType(type);
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

		public OscillatorType GetOscType(){
			return this.osc.type;
		}

		public void SetOscType(OscillatorType type){
			this.osc.type = type;
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
		private float decayStep = 0;
		private float releaseStep = 0;
		private float sustainSamples = 0;

		/**
		 * ENVELOPE TIMING
		 */
		public float attackTime {
			set { attackStep = (1.0f / sampleRate) / value;}
			get { return (1.0f / sampleRate) / attackStep; }
		}
		public float decayTime {
			set { decayStep = (1.0f / sampleRate) / value;}
			get { return (1.0f / sampleRate) / decayStep; }
		}
		public float sustainTime {
			set { sustainSamples = (int) (value * sampleRate);}
			get { return (float) (sustainSamples / sampleRate); }
		}
		public float releaseTime {
			set { releaseStep = (1.0f / sampleRate) / value;}
			get { return (1.0f / sampleRate) / releaseStep; }
		}

		public Envelope(){
			//set some initial values
			attackTime = 0.00243f;
			decayTime = 0.855f;
			sustainTime = 0.0f;
			releaseTime = 0.54f;
		}

		//sustain value
		public float sustainValue = 0.0035f;
	

		//the sustain counter
		private int sustainSamplesPast = 0;

		private float currentSample = 0.0f;
						
		public void OnAudioProcess(ref float[] data){
			for (int i = 0; i < data.Length; i++){
				switch(this.phase){
				case EnvelopePhase.Attack:
					currentSample += attackStep;
					if (currentSample >= 1){
						currentSample = 1;
						this.phase++;
					}
					break;
				case EnvelopePhase.Decay:
					currentSample -= decayStep;
					if (currentSample <= sustainValue){
						this.phase++;
						sustainSamplesPast = 0;
					}
					break;
				case EnvelopePhase.Sustain:
					sustainSamplesPast++;
					if (sustainSamplesPast > sustainSamples){
						this.phase++;
					}
					break;
				case EnvelopePhase.Release:
					currentSample -= releaseStep;
					if (currentSample <= 0){
						currentSample = 0;
						this.phase++;
					}
					break;
				}
				data[i] = currentSample * data[i];
			}
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
