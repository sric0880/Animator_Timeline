using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[System.Serializable]
public class AMAudioTrack : AMTrack {
	
	public AudioSource audioSource;
	
	public override string getTrackType() {
		return "Audio";	
	}
	
	public bool setAudioSource(AudioSource audioSource) {
		if(this.audioSource != audioSource) {
			this.audioSource = audioSource;
			return true;
		}
		return false;
	}
	public override void updateCache() {
		// destroy cache
		destroyCache();
		// create new cache
		cache = new List<AMAction>();
		// sort keys
		sortKeys();
		// add all clips to list
		for(int i=0;i<keys.Count;i++) {
			AMAudioAction a = ScriptableObject.CreateInstance<AMAudioAction> ();
			a.startFrame = keys[i].frame;
			a.aKey = keys[i] as AMAudioKey;
			cache.Add (a);
		}
		base.updateCache();
	}
	// add a new key
	public void addKey(int _frame, AudioClip _clip, bool _loop) {
		foreach(AMAudioKey key in keys) {
			// if key exists on frame, update key
			if(key.frame == _frame) {
				key.audioClip = _clip;
				key.loop = _loop;
				// update cache
				updateCache();
				return;
			}
		}
		AMAudioKey a = ScriptableObject.CreateInstance<AMAudioKey>();
		a.frame = _frame;
		a.audioClip = _clip;
		a.loop = _loop;
		// add a new key
		keys.Add (a);
		// update cache
		updateCache();
	}
	
	// sample audio at frame
	public void sampleAudio(float frame) {
		if(!audioSource) return;
		for(int i=cache.Count-1;i>=0;i--) {
			AMAudioKey key = (cache[i] as AMAudioAction).aKey;
			if(!key.audioClip) return;
			if(cache[i].startFrame == frame) {
				audioSource.clip = key.audioClip;
				audioSource.loop = key.loop;
				audioSource.pitch = 1;
				audioSource.time = 0;
				audioSource.Play();
				
				return;
			}else if (cache[i].startFrame < frame){
				// get time
//				time = ((frame-cache[i].startFrame)/frameRate);
//				// if loop is set to false and is beyond length, then return
//				if(!key.loop && time > key.audioClip.length) return;
//				// find time based on length
//				time = time % key.audioClip.length;
			}
		}
	}

	public void stopAudio() {
		if(!audioSource) return;
		if(audioSource.isPlaying) audioSource.Stop();
	}
}
