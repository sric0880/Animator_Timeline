using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[System.Serializable]
public class AMTranslationAction : AMAction {

	public int endFrame;
	public Transform obj;
	public Vector3[] path;

	public override int NumberOfFrames {
		get {
			return endFrame-startFrame;
		}
	}
	
	public float getTime(int frameRate) {
		return (float)NumberOfFrames/(float)frameRate;	
	}
}
