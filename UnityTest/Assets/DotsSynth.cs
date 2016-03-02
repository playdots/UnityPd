using UnityEngine;
using System.Collections;

using UnityEngine.Audio;

public class DotsSynth : MonoBehaviour {

	public AudioMixer mixer;

	private float triggerNumber = 0;


	public void triggerNote(float freq){
		triggerNumber++;
		triggerNumber = triggerNumber % 2;
		mixer.SetFloat ("DotsSynthFrequency", freq);
		mixer.SetFloat ("DotsSynthTrigger", triggerNumber);
	}

}
