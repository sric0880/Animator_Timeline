using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System.Linq;

[System.Serializable]
public class AnimatorData : MonoBehaviour {
	[HideInInspector] public bool isAnimatorOpen = false;
	[HideInInspector] public bool inPlayMode = false;
	[HideInInspector] public float zoom = 0.4f;
	[HideInInspector] public int frameRate = 24;			// frames per second
	[HideInInspector] public int numFrames = 1440;			// number of frames

	public static int StaticFrameRate = 24;
	public static int StaticNumFrames = 1440;

	[SerializeField,HideInInspector]
	private AMTake currentTake;
	private int lastSelectedFrame = 0;

	void Update() {
		AMTake curTake = getCurrentTake();
		if (lastSelectedFrame != curTake.selectedFrame)
		{
			curTake.previewFrame(curTake.selectedFrame);
			if (curTake.selectedFrame == numFrames) //reach the end
				curTake.stopAudio();
			else
				curTake.sampleAudio(curTake.selectedFrame);

			lastSelectedFrame = curTake.selectedFrame;
		}
	}

	void OnDrawGizmos() {
		if(!isAnimatorOpen) return;
		if(currentTake) currentTake.drawGizmos(0.1f);
	}

	public AMTake getCurrentTake() {
		if(!currentTake) currentTake = ScriptableObject.CreateInstance<AMTake>();
		StaticFrameRate = frameRate;
		StaticNumFrames = numFrames;
		return currentTake;
	}
}
