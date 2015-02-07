using UnityEngine;
using UnityEditor;
using System.Collections;

public class AnimationLayout {
	
	private static string[] wrapModeNames = {
		"Once",	
		"Loop",
		"ClampForever",
		"PingPong"
	};

	static int wrapModeToIndex(WrapMode wrapMode) {
		switch(wrapMode) {
		case WrapMode.Once:
			return 0;
		case WrapMode.Loop:
			return 1;
		case WrapMode.ClampForever:
			return 2;
		case WrapMode.PingPong:
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
			return WrapMode.ClampForever;
		case 3:
			return WrapMode.PingPong;
		default:
			Debug.LogError("Animator: No Wrap Mode found for index "+index);
			return WrapMode.Default;
		}
	}

	public static void InspectorLayout(AMAnimationTrack sTrack, AMAnimationKey aKey)
	{
		EditorGUILayout.BeginVertical();
		EditorGUIUtility.labelWidth = 100f;
		// animation clip
		if(aKey.setAmClip((AnimationClip)EditorGUILayout.ObjectField("Animation Clip",aKey.amClip,typeof(AnimationClip),false))) {
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
		// crossfade
		if(aKey.setCrossFade(EditorGUILayout.Toggle("Crossfade", aKey.crossfade))) {
			// update cache when modifying varaibles
			sTrack.updateCache();
			CustomInspector.updateView(sTrack, aKey.frame);
		}
		if(!aKey.crossfade) GUI.enabled = false;
		if(aKey.setCrossfadeTime(EditorGUILayout.FloatField("Time (sec)",aKey.crossfadeTime))) {
			// update cache when modifying varaibles
			sTrack.updateCache();
			CustomInspector.updateView(sTrack);
		}
		GUI.enabled = true;	
		EditorGUILayout.EndVertical();
	}
}
