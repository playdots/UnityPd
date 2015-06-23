using UnityEngine;
using System.Collections;
using UnityEngine.Audio;


public class ToneButton : MonoBehaviour {
	
	private GameObject gameObj;
	public AudioMixer mixer;
	
	private string notePressed = " ";
	
	// Use this for initialization
	void Start () {
		GameObject gameObj = GameObject.Find("DotSynth");
//		synth.SetFloat ("frequency", 220);
	}
	
	void OnGUI() {
		MakeNoteButtonAndHandleClick (new Rect (100, 100, 500, 180), "C2", 440);
		MakeNoteButtonAndHandleClick (new Rect (100, 300, 500, 180), "D2", 440 * 1.5f);
	}
	
	private void MakeNoteButtonAndHandleClick( Rect buttonRect, string noteName, float noteFreq ) {
		bool button = GUI.RepeatButton (buttonRect, noteName);
		
		if (button) {
			// prevent the note from triggering multiple times
			if (notePressed != noteName) {
				notePressed = noteName;
				Debug.Log(noteFreq);
				mixer.SetFloat ("DotsFrequency", noteFreq);
				mixer.SetFloat ("DotsVelocity", Random.value);
			} 
		} else {
			if ( Event.current.type == EventType.Repaint && 
			    notePressed == noteName) 
			{
				notePressed = " ";
			}
		}
	}
}
