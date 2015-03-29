using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;

public class AnimationLayout {

	private static string[] wrapModeNames = {
		"Once",	
		"Loop",
		"PingPong",
		"ClampForever",
	};
	
	static int wrapModeToIndex(WrapMode wrapMode) {
		switch(wrapMode) {
		case WrapMode.Once:
			return 0;
		case WrapMode.Loop:
			return 1;
		case WrapMode.PingPong:
			return 2;
		case WrapMode.ClampForever:
			return 3;
		default:
			Debug.LogError("Animator: No Index found for WrapMode "+wrapMode.ToString());
			return -1;
		}	
	}
	
	static WrapMode indexToWrapMode(int index) {
		switch(index) {
		case 0:
			return WrapMode.Once;
		case 1:
			return WrapMode.Loop;
		case 2:
			return WrapMode.PingPong;
		case 3:
			return WrapMode.ClampForever;
		default:
			Debug.LogError("Animator: No Wrap Mode found for index "+index);
			return WrapMode.Default;
		}
	}

	public static List<string> stateNames = new List<string>();
	public static List<string> layerNames = new List<string>();
	
	public static void InspectorLayout(AMAnimationTrack sTrack, AMAnimationKey aKey)
	{
		EditorGUILayout.BeginVertical();
		EditorGUIUtility.labelWidth = 100f;
		// animation clip
		stateNames.Clear();
		int selectedIndex = 0, selectedLayer = 0;
		AnimationClip[] clips = sTrack.animator.runtimeAnimatorController.animationClips;
		for(int i = 0; i < sTrack.animator.layerCount; ++i)
		{
			string lName = sTrack.animator.GetLayerName(i);
			layerNames.Add(lName);
			if (lName == aKey.layerName){
				selectedLayer = i;
			}
		}
		for(int i = 0; i < clips.Length; ++i)
		{
			stateNames.Add(clips[i].name);
			if(clips[i].name == aKey.clipName)
			{
				selectedIndex = i;
			}
		}
		if(aKey.setLayerName( layerNames[ EditorGUILayout.Popup("Layer",selectedLayer,layerNames.ToArray()) ] )) {
			// update cache when modifying varaibles
			sTrack.updateCache();
			CustomInspector.updateView(sTrack, aKey.frame);
		}
		if(aKey.setClipName( stateNames[ EditorGUILayout.Popup("Animation",selectedIndex,stateNames.ToArray()) ] )) {
			foreach(AnimationClip clip in clips)
			{
				if (clip.name == aKey.clipName)
				{
					aKey.length = clip.length;
				}
			}
			// update cache when modifying varaibles
			sTrack.updateCache();
			CustomInspector.updateView(sTrack, aKey.frame);
		}
		// wrap mode
		if(aKey.setWrapMode(indexToWrapMode(EditorGUILayout.Popup("Wrap Mode",wrapModeToIndex(aKey.wrapMode),wrapModeNames)))) {
			// update cache when modifying varaibles
			sTrack.updateCache();
			CustomInspector.updateView(sTrack, aKey.frame);
		}

		GUI.enabled = true;	
		EditorGUILayout.EndVertical();
	}
}
