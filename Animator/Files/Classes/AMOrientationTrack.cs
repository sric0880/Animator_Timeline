using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[System.Serializable]
public class AMOrientationTrack : AMTrack {

	public Transform obj;
	
	public AMTrack cachedTranslationTrackStartTarget = null;
	public AMTrack cachedTranslationTrackEndTarget = null;

	public override string getTrackType() {
		return "Orientation";	
	}
	public bool setObject(Transform obj) {
		if(this.obj != obj) {
			this.obj = obj;
			return true;
		}
		return false;
	}
	// add a new key
	public void addKey(int _frame, Transform target) {
		foreach(AMOrientationKey key in keys) {
			// if key exists on frame, update key
			if(key.frame == _frame) {
				key.target = target;
				// update cache
				updateCache();
				return;
			}
		}
		AMOrientationKey a = ScriptableObject.CreateInstance<AMOrientationKey>();
		a.frame = _frame;
		a.target = target;
		// set default ease type to linear
		a.easeType = (int)AMTween.EaseType.linear;
		// add a new key
		keys.Add (a);
		// update cache
		updateCache();
	}
	public override void updateCache() {
		//_updateCache();
		updateOrientationCache(parentTake);
		base.updateCache();
	}
	
	public void updateOrientationCache(AMTake curTake,bool restoreRotation = false) {
		// save rotation
		Quaternion temp = obj.rotation;
		// sort keys
		sortKeys();
		destroyCache();
		cache = new List<AMAction>();
		AMTranslationTrack translationTrack = curTake.getTranslationTrackForTransform(obj);
		for(int i=0;i<keys.Count;i++) {
			// create new action and add it to cache list
			AMOrientationAction a = ScriptableObject.CreateInstance<AMOrientationAction> ();
			a.startFrame = keys[i].frame;
			if(keys.Count>(i+1)) a.endFrame = keys[i+1].frame;
			else a.endFrame = -1;
			a.obj = obj;
			// targets
			a.startTarget = (keys[i] as AMOrientationKey).target;
			if(a.endFrame!=-1) a.endTarget = (keys[i+1] as AMOrientationKey).target;
			a.easeType = (keys[i] as AMOrientationKey).easeType;
			a.customEase = new List<float>(keys[i].customEase);
			if(translationTrack != null && !a.isLookFollow()) {
				a.isSetStartPosition = true;
				a.startPosition = translationTrack.getPositionAtFrame(a.startFrame);
				a.isSetEndPosition = true;
				a.endPosition = translationTrack.getPositionAtFrame(a.endFrame);
			}

			// add to cache
			cache.Add (a);
		}
		// restore rotation
		if(restoreRotation) obj.rotation = temp;
	}
	
	public Transform getInitialTarget() {
		return (keys[0] as AMOrientationKey).target;
	}
	
	public override void previewFrame(float frame) {

		if(cache == null || cache.Count <= 0) {
			return;
		}
		for(int i=0;i<cache.Count;i++) {
			// before first frame
			if(frame <= (cache[i] as AMOrientationAction).startFrame) {
				if(!(cache[i] as AMOrientationAction).startTarget) return;
				Vector3 startPos;
				if(cachedTranslationTrackStartTarget == null) startPos = (cache[i] as AMOrientationAction).startTarget.position;
				else startPos = (cachedTranslationTrackStartTarget as AMTranslationTrack).getPositionAtFrame((cache[i] as AMOrientationAction).startFrame);
				obj.LookAt(startPos);
				return;
			// between first and last frame
			} else if(frame <= (cache[i] as AMOrientationAction).endFrame) {
				if(!(cache[i] as AMOrientationAction).startTarget || !(cache[i] as AMOrientationAction).endTarget) return;
				float framePositionInPath = frame-(float)cache[i].startFrame;
				if (framePositionInPath<0f) framePositionInPath = 0f;
				float percentage = framePositionInPath/cache[i].NumberOfFrames;
				if((cache[i] as AMOrientationAction).isLookFollow()) obj.rotation = (cache[i] as AMOrientationAction).getQuaternionAtPercent(percentage);
				else {
					Vector3? startPos = (cachedTranslationTrackStartTarget == null ? null : (Vector3?)(cachedTranslationTrackStartTarget as AMTranslationTrack).getPositionAtFrame((cache[i] as AMOrientationAction).startFrame));
					Vector3? endPos = (cachedTranslationTrackEndTarget == null ? null : (Vector3?)(cachedTranslationTrackEndTarget as AMTranslationTrack).getPositionAtFrame((cache[i] as AMOrientationAction).endFrame));
					obj.rotation = (cache[i] as AMOrientationAction).getQuaternionAtPercent(percentage,startPos,endPos);
				}
				return;
			// after last frame
			} else if(i == cache.Count-2) {
				if(!(cache[i] as AMOrientationAction).endTarget) return;
				Vector3 endPos;
				if(cachedTranslationTrackEndTarget == null) endPos = (cache[i] as AMOrientationAction).endTarget.position;
				else endPos = (cachedTranslationTrackEndTarget as AMTranslationTrack).getPositionAtFrame((cache[i] as AMOrientationAction).endFrame);
				obj.LookAt(endPos);
				return;
			}
		}
	}
	
	public Transform getStartTargetForFrame(float frame) {
		foreach(AMOrientationAction action in cache) {
			if(/*((int)frame<action.startFrame)||*/((int)frame>action.endFrame)) continue;
			return action.startTarget;
		}
		return null;
	}
	public Transform getEndTargetForFrame(float frame) {
		if(cache.Count > 1) return (cache[cache.Count-2] as AMOrientationAction).endTarget;
		return null;
	}
	public Transform getTargetForFrame(float frame) {
		if(isFrameBeyondLastKeyFrame(frame)) return getEndTargetForFrame(frame);
		else return getStartTargetForFrame(frame);
	}
	// draw gizmos
	public override void drawGizmos(float gizmo_size) {
		if(!obj) return;
		// draw arrow
		Gizmos.color = new Color(245f/255f,107f/255f,30f/255f,1f);
		Vector3 pos = obj.transform.position;
		float size = (1.2f*(gizmo_size/0.1f));
		if(size < 0.1f) size = 0.1f;
		Vector3 direction = obj.forward * size;
		float arrowHeadAngle = 20f;
		float arrowHeadLength = 0.3f*size;
		
		Gizmos.DrawRay(pos, direction);
        
        Vector3 right = Quaternion.LookRotation(direction) * Quaternion.Euler(0,180+arrowHeadAngle,0) * new Vector3(0,0,1);
        Vector3 left = Quaternion.LookRotation(direction) * Quaternion.Euler(0,180-arrowHeadAngle,0) * new Vector3(0,0,1);
        Gizmos.DrawRay(pos + direction, right * arrowHeadLength);
        Gizmos.DrawRay(pos + direction, left * arrowHeadLength);
	}
	
	public bool isFrameBeyondLastKeyFrame(float frame) {
		if(keys.Count <= 0) return false;
		else if((int)frame > keys[keys.Count-1].frame) return true;
		else return false;
	}

	public bool hasTarget(Transform obj) {
		foreach(AMOrientationAction action in cache) {
			if(action.startTarget == obj || action.endTarget == obj) return true;
		}
		return false;
	}
}
