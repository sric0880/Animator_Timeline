using UnityEngine;
using System.Collections;

[System.Serializable]
public class AMAudioAction : AMAction {
	
	public AMAudioKey aKey;
	
	public override int NumberOfFrames
	{
		get { 
			if(!aKey.audioClip) return -1;
			if(aKey.loop) return -1;
			return Mathf.CeilToInt(aKey.audioClip.length * AnimatorData.StaticFrameRate);
		}
	}
}
