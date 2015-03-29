using UnityEngine;
using System.Collections;

[System.Serializable]
public class AMAnimationKey : AMKey {

	public WrapMode wrapMode = WrapMode.Once;	// animation wrap mode
	public string clipName = "";
	public string layerName = "";
	public float length = 0f;

	public bool setWrapMode(WrapMode wrapMode) {
		if(this.wrapMode != wrapMode) {
			this.wrapMode = wrapMode;
			return true;
		}
		return false;
	}

	public bool setClipName(string name) {
		if(clipName != name) {
			clipName = name;
			return true;
		}
		return false;
	}

	public bool setLayerName(string name) {
		if(layerName != name) {
			layerName = name;
			return true;
		}
		return false;
	}

	// copy properties from key
	public override AMKey CreateClone ()
	{
		
		AMAnimationKey a = ScriptableObject.CreateInstance<AMAnimationKey>();
		a.frame = frame;
		a.clipName = clipName;
		a.wrapMode = wrapMode;
		a.layerName = layerName;
		a.length = length;
		
		return a;
	}
}
