using UnityEngine;
using System.Collections;

[System.Serializable]
public class AMAnimationAction : AMAction {

	public AMAnimationKey aKey;

	public override int NumberOfFrames {
		get {
			if (!aKey) return -1;
			if(aKey.wrapMode != WrapMode.Once) return -1;
			return Mathf.CeilToInt(aKey.length* AnimatorData.StaticFrameRate);
		}
	}
}
