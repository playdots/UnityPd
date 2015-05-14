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
		MakeNoteButtonAndHandleClick (new Rect (100, 100, 500, 180), "A4", 440);
		MakeNoteButtonAndHandleClick (new Rect (100, 300, 500, 180), "A5", 880);
	}
	
	private void MakeNoteButtonAndHandleClick( Rect buttonRect, string noteName, float noteFreq ) {
		bool button = GUI.RepeatButton (buttonRect, noteName);
		
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
