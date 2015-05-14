using UnityEngine;
using System.Collections;


public class ToneButton : MonoBehaviour {
	
	GameObject gameObj;
	Tone synth;
	
	private string notePressed = " ";
	
	// Use this for initialization
	void Start () {
		GameObject gameObj = GameObject.Find("Main Camera");
		synth = (Tone) gameObj.GetComponent(typeof(Tone));
	}
	
	void OnGUI() {
		MakeNoteButtonAndHandleClick ("A4", 100, 440);
		MakeNoteButtonAndHandleClick ("A5", 200, 880);
	}
	
	private void MakeNoteButtonAndHandleClick( string noteName, float top, float noteFreq ) {
		bool button = GUI.RepeatButton (new Rect (100, top, 60, 60), noteName);
		
		if (button) {
			// prevent the note from triggering multiple times
			if (notePressed != noteName) {
				notePressed = noteName;
				synth.TriggerNote (noteFreq);
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
