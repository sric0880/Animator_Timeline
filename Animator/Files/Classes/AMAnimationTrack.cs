using UnityEngine;
using System.Collections;
using System.Collections.Generic;
[System.Serializable]
public class AMAnimationTrack : AMTrack {
	// to do
	// sample currently selected clip
	public Animator animator;

	public override string getTrackType() {
		return "Animation";	
	}
	
	public bool setAnimator(Animator anim) {
		if(this.animator != anim) {
			this.animator = anim;
			return true;
		}
		return false;
	}

	// add a new key
	public void addKey(int _frame) {
		foreach(AMAnimationKey key in keys) {
			// if key exists on frame, update key
			if(key.frame == _frame) {
				// update cache
				updateCache();
				return;
			}
		}
		AMAnimationKey a = ScriptableObject.CreateInstance<AMAnimationKey>();
		a.frame = _frame;
		// add a new key
		keys.Add (a);
		// update cache
		updateCache();
	}
	// update cache
	public override void updateCache() {
		// destroy cache
		destroyCache();
		// create new cache
		cache = new List<AMAction>();
		// sort keys
		sortKeys();
		// add all clips to list
		for(int i=0;i<keys.Count;i++) {
			AMAnimationAction a = ScriptableObject.CreateInstance<AMAnimationAction> ();
			a.startFrame = keys[i].frame;
			a.aKey = keys[i] as AMAnimationKey;
			cache.Add (a);
		}
		base.updateCache();
	}
	// preview a frame in the scene view
	public override void previewFrame(float frame) {
		if(!animator) return;
		if(cache.Count <= 0) return;
		for(int i=cache.Count-1;i>=0;i--) {
			if(cache[i].startFrame <= frame) {
				AMAnimationKey key = (cache[i] as AMAnimationAction).aKey;
				if (!key) return;
				string name = key.layerName+"."+key.clipName;
				int layer = animator.GetLayerIndex(key.layerName);
				float normalizedTime = ((float)(frame-cache[i].startFrame)/(float)AnimatorData.StaticFrameRate) /key.length ;
				if (key.wrapMode == WrapMode.Loop)
				{
					normalizedTime = normalizedTime - Mathf.FloorToInt(normalizedTime);
				}
				else if (key.wrapMode == WrapMode.Once)
				{
					if ( normalizedTime >= 1) normalizedTime = 0;
				}
				else if (key.wrapMode == WrapMode.ClampForever)
				{
					normalizedTime = Mathf.Clamp01(normalizedTime);
				}
				else if (key.wrapMode == WrapMode.PingPong)
				{
					normalizedTime = Mathf.FloorToInt(normalizedTime) % 2 == 0 ? 
						normalizedTime - Mathf.FloorToInt(normalizedTime) : Mathf.CeilToInt(normalizedTime) - normalizedTime;
				}
				animator.Play(name, layer, normalizedTime);
				break;
			}
		}
	}
}
