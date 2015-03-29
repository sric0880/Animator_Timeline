using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

[System.Serializable]
public class AMEventTrack : AMTrack {
	
	public GameObject obj;
	
	public override string getTrackType() {
		return "Event";	
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
			AMEventAction a = ScriptableObject.CreateInstance<AMEventAction> ();
			a.startFrame = keys[i].frame;
			a.component = (keys[i] as AMEventKey).component;
			a.methodInfo = (keys[i] as AMEventKey).methodInfo;
			a.parameters = (keys[i] as AMEventKey).parameters;
			a.useSendMessage = (keys[i] as AMEventKey).useSendMessage;
			cache.Add (a);
		}
		base.updateCache();
	}
	public void setObject(GameObject obj) {
		this.obj = obj;
	}
	public bool isObjectUnique(GameObject obj) {
		if(this.obj != obj) return true;
		return false;
	}
		// add a new key
	public void addKey(int _frame) {
		foreach(AMEventKey key in keys) {
			// if key exists on frame, do nothing
			if(key.frame == _frame) {
				return;
			}
		}
		AMEventKey a = ScriptableObject.CreateInstance<AMEventKey> ();
		a.frame = _frame;
		a.component = null;
		a.methodName = null;
		a.parameters = null;
		// add a new key
		keys.Add (a);
		// update cache
		updateCache();
	}
	public bool hasSameEventsAs(AMEventTrack _track) {
			if(_track.obj == obj)
				return true;
			return false;
	}
}
