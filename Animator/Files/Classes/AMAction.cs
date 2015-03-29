using UnityEngine;
using System.Collections;
using System.Collections.Generic;

// holds an action to be parsed in game view
[System.Serializable]
public class AMAction : ScriptableObject  {

	public int startFrame;
	public virtual int NumberOfFrames { get {return 1;} }

	public float getWaitTime(int frameRate, float delay) {
		return ((float)startFrame-1f)/(float)frameRate - delay;
	}

	public void destroy() {
		Object.DestroyImmediate(this);	
	}

	public int easeType = (int)AMTween.EaseType.linear; 			// ease type, AMTween.EaseType enum
	public List<float> customEase = new List<float>();
	
	private AnimationCurve _cachedEaseCurve;
	public AnimationCurve easeCurve {
		get {
			if(_cachedEaseCurve == null || _cachedEaseCurve.keys.Length <= 0) _cachedEaseCurve = getCustomEaseCurve();
			return _cachedEaseCurve;
		}
	}
	
	public bool setEaseType(int easeType) {
		if(easeType != this.easeType) {
			this.easeType = easeType;
			if(easeType == 32 && customEase.Count <= 0) {
				// set up default custom ease with linear
				customEase = new List<float>() {
					0f,0f,1f,1f,
					1f,1f,1f,1f
				};
			}
			return true;	
		}
		return false;
	}
	
	public void setCustomEase(AnimationCurve curve) {
		customEase = new List<float>();
		foreach(Keyframe k in curve.keys) {
			customEase.Add(k.time);
			customEase.Add(k.value);
			customEase.Add(k.inTangent);
			customEase.Add(k.outTangent);
		}
		_cachedEaseCurve = null;
	}
	
	public AnimationCurve getCustomEaseCurve() {
		AnimationCurve curve = new AnimationCurve();
		if(customEase.Count < 0) {
			return curve;
		}
		if(customEase.Count % 4 != 0) {
			Debug.LogError("Animator: Error retrieving custom ease.");
			return curve;
		}
		for(int i=0;i<customEase.Count;i+=4) {
			curve.AddKey(new Keyframe(customEase[i],customEase[i+1],customEase[i+2],customEase[i+3]));
		}
		return curve;
	}
	
	public bool hasCustomEase() {
		if(easeType == 32) return true;
		return false;
	}
}
