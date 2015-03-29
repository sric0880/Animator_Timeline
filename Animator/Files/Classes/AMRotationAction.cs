using UnityEngine;
using System.Collections;

[System.Serializable]
public class AMRotationAction : AMAction {

	//public int type = 0; // 0 = Rotate To, 1 = Look At
	public int endFrame;
	public Transform obj;
	public Quaternion startRotation;
	public Quaternion endRotation;
	
	public override int NumberOfFrames {
		get {
			return endFrame-startFrame;
		}
	}
	public float getTime(int frameRate) {
		return (float)NumberOfFrames/(float)frameRate;	
	}

	public Quaternion getStartQuaternion() {
		return startRotation;	
	}
	public Quaternion getEndQuaternion() {
		return endRotation;	
	}
}
