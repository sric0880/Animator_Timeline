using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[System.Serializable]
public class AMRotationTrack : AMTrack {
	
	[SerializeField]
	private Transform _obj;
	public Transform obj{
		get {
			return _obj;	
		}
		set {
			if(value != null && cache.Count <= 0) cachedInitialRotation = value.rotation;
			_obj = value;
			
		}
	}
	public Quaternion cachedInitialRotation;
	
	public override string getTrackType() {
		return "Rotation";	
	}
	// add a new key
	public void addKey(int _frame, Quaternion _rotation) {
		foreach(AMRotationKey key in keys) {
			// if key exists on frame, update key
			if(key.frame == _frame) {
				key.rotation = _rotation;
				// update cache
				updateCache();
				return;
			}
		}
		AMRotationKey a = ScriptableObject.CreateInstance<AMRotationKey>();
		a.frame = _frame;
		a.rotation = _rotation;
		// set default ease type to linear
		a.easeType = (int)AMTween.EaseType.linear;
		// add a new key
		keys.Add (a);
		// update cache
		updateCache();
	}

	// update cache (optimized)
	public override void updateCache() {

		// sort keys
		sortKeys();
		destroyCache();
		cache = new List<AMAction>();
		for(int i=0;i<keys.Count;i++) {
			
			// or create new action and add it to cache list
				AMRotationAction a = ScriptableObject.CreateInstance<AMRotationAction> ();
				//a.type = (keys[i] as AMRotationKey).type;
				a.startFrame = keys[i].frame;
				if(keys.Count>(i+1)) a.endFrame = keys[i+1].frame;
				else a.endFrame = -1;
				a.obj = obj;
				// quaternions
				a.startRotation = (keys[i] as AMRotationKey).rotation;
				if(a.endFrame!=-1) a.endRotation = (keys[i+1] as AMRotationKey).rotation;
				
				a.easeType = (keys[i] as AMRotationKey).easeType;
				a.customEase = new List<float>(keys[i].customEase);
				// add to cache
				cache.Add (a);
		}
		base.updateCache();

	}
	// preview a frame in the scene view
	public override void previewFrame(float frame) {
		if(!obj) return;
		if(cache.Count <= 0) return;
		if(cache[0] == null) updateCache();
		// if before or equal to first frame, or is the only frame
		if((frame <= (float) cache[0].startFrame)||((cache[0] as AMRotationAction).endFrame == -1)) {
			obj.rotation = (cache[0] as AMRotationAction).getStartQuaternion();
			return;
		}
		// if beyond or equal to last frame
		if(frame >= (float) (cache[cache.Count-2] as AMRotationAction).endFrame) {
			obj.rotation = (cache[cache.Count-2] as AMRotationAction).getEndQuaternion();
			return;
		}
		// if lies on rotation action
		foreach(AMRotationAction action in cache) {
			if((frame<(float)action.startFrame)||(frame>(float)action.endFrame)) continue;
			// if on startFrame
			if(frame == (float)action.startFrame) {
				obj.rotation = action.getStartQuaternion();
				return;	
			}
			// if on endFrame
			if(frame == (float)action.endFrame) {
				obj.rotation = action.getEndQuaternion();
				return;	
			}
			// else find Quaternion using easing function

			AMTween.EasingFunction ease;
			AnimationCurve curve = null;
			
			if(action.hasCustomEase()) {
				ease = AMTween.customEase;
				curve = action.easeCurve;
			} else {
				ease = AMTween.GetEasingFunction((AMTween.EaseType)action.easeType);
			}
			
			float framePositionInAction = frame-(float)action.startFrame;
			if (framePositionInAction<0f) framePositionInAction = 0f;
			float percentage = framePositionInAction/action.NumberOfFrames;
			
			Quaternion qStart = action.getStartQuaternion();
			Quaternion qEnd = action.getEndQuaternion();
			Quaternion qCurrent = new Quaternion();
			
			qCurrent.x = ease(qStart.x,qEnd.x,percentage,curve);
			qCurrent.y = ease(qStart.y,qEnd.y,percentage,curve);
			qCurrent.z = ease(qStart.z,qEnd.z,percentage,curve);
			qCurrent.w = ease(qStart.w,qEnd.w,percentage,curve);	

			obj.rotation = qCurrent;

			return;
		}
	}
	public Quaternion getRotationAtFrame(float frame) {
		// if before or equal to first frame, or is the only frame
		if((frame <= (float) cache[0].startFrame)||((cache[0] as AMRotationAction).endFrame == -1)) {
			//obj.rotation = (cache[0] as AMRotationAction).getStartQuaternion();
			return (cache[0] as AMRotationAction).getStartQuaternion();
		}
		// if beyond or equal to last frame
		if(frame >= (float) (cache[cache.Count-2] as AMRotationAction).endFrame) {
			//obj.rotation = (cache[cache.Count-2] as AMRotationAction).getEndQuaternion();
			return (cache[cache.Count-2] as AMRotationAction).getEndQuaternion();
		}
		// if lies on rotation action
		foreach(AMRotationAction action in cache) {
			if((frame<(float)action.startFrame)||(frame>(float)action.endFrame)) continue;
			// if on startFrame
			if(frame == (float)action.startFrame) {
				return action.getStartQuaternion();
			}
			// if on endFrame
			if(frame == (float)action.endFrame) {
				return action.getEndQuaternion();	
			}
			// else find Quaternion using easing function

			AMTween.EasingFunction ease;
			AnimationCurve curve = null;
			
			if(action.hasCustomEase()) {
				ease = AMTween.customEase;
				curve = action.easeCurve;
			} else {
				ease = AMTween.GetEasingFunction((AMTween.EaseType)action.easeType);
			}
			
			float framePositionInAction = frame-(float)action.startFrame;
			if (framePositionInAction<0f) framePositionInAction = 0f;
			float percentage = framePositionInAction/action.NumberOfFrames;
			
			Quaternion qStart = action.getStartQuaternion();
			Quaternion qEnd = action.getEndQuaternion();
			Quaternion qCurrent = new Quaternion();
			
			qCurrent.x = ease(qStart.x,qEnd.x,percentage,curve);
			qCurrent.y = ease(qStart.y,qEnd.y,percentage,curve);
			qCurrent.z = ease(qStart.z,qEnd.z,percentage,curve);
			qCurrent.w = ease(qStart.w,qEnd.w,percentage,curve);

			return qCurrent;
		}
		Debug.LogError("Animator: Could not get "+obj.name+" rotation at frame '"+frame+"'");
		return new Quaternion(0f,0f,0f,0f);
	}
	public Vector3 getInitialRotation() {
		return (keys[0] as AMRotationKey).getRotation();
	}
}
