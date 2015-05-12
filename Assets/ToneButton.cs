using UnityEngine;
using System.Collections;

public class ToneButton : MonoBehaviour {

	GameObject gameObj;
	Tone synth;
	
	private string lastTooltip = " ";
	
	// Use this for initialization
	void Start () {
		GameObject gameObj = GameObject.Find("Main Camera");
		synth = (Tone) gameObj.GetComponent(typeof(Tone));
	}

	void OnGUI() {
		GUI.Button(new Rect(100, 100, 60, 60), new GUIContent("A4", "440"));

		GUI.Button(new Rect(100, 200, 60, 60), new GUIContent("A5", "880"));

		//mouse events
		if (Event.current.type == EventType.Repaint && GUI.tooltip != lastTooltip) {

			if (GUI.tooltip != ""){
				synth.TriggerNote(float.Parse(GUI.tooltip.ToString()));
			}

			lastTooltip = GUI.tooltip;
		}
	}
}
