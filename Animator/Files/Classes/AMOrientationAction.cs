using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[System.Serializable]
public class AMOrientationAction : AMAction {

	public int endFrame;
	public Transform obj;
	public Transform startTarget;
	public Transform endTarget;
	
	public bool isSetStartPosition = false;
	public bool isSetEndPosition = false;
	public Vector3 startPosition;
	public Vector3 endPosition;
	
	public override int NumberOfFrames {
		get {
			return endFrame-startFrame;
		}
	}
	
	public float getTime(int frameRate) {
		return (float)NumberOfFrames/(float)frameRate;	
	}
	
	public bool isLookFollow() {
		if(startTarget != endTarget) return false;
		return true;
	}

	public Quaternion getQuaternionAtPercent(float percentage, /*Vector3 startPosition, Vector3 endPosition,*/ Vector3? startVector = null, Vector3? endVector = null) {
		if(isLookFollow()) {
			obj.LookAt(startTarget);
			return obj.rotation;
		}
		
		Vector3 _temp = obj.position;
		if(isSetStartPosition) obj.position = (Vector3) startPosition;
		obj.LookAt(startVector ?? startTarget.position);
		Vector3 eStart = obj.eulerAngles;
		if(isSetEndPosition) obj.position = (Vector3) endPosition;
		obj.LookAt(endVector ?? endTarget.position);
		Vector3 eEnd = obj.eulerAngles;
		obj.position = _temp;
		eEnd=new Vector3(AMTween.clerp(eStart.x,eEnd.x,1),AMTween.clerp(eStart.y,eEnd.y,1),AMTween.clerp(eStart.z,eEnd.z,1));

		Vector3 eCurrent = new Vector3();
		
		AMTween.EasingFunction ease;
		AnimationCurve curve = null;
		if(hasCustomEase()) {
			curve = easeCurve;
			ease = AMTween.customEase;
		} else {
			ease = AMTween.GetEasingFunction((AMTween.EaseType)easeType);
		}
		
		eCurrent.x = ease(eStart.x,eEnd.x,percentage,curve);
		eCurrent.y = ease(eStart.y,eEnd.y,percentage,curve);
		eCurrent.z = ease(eStart.z,eEnd.z,percentage,curve);
		
		
		return Quaternion.Euler(eCurrent);
	}
}
