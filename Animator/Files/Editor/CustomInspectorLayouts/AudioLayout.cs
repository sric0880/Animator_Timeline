using UnityEngine;
using UnityEditor;
using System.Collections;

public class AudioLayout{

	public static void InspectorLayout(AMAudioTrack sTrack, AMAudioKey auKey)
	{
		EditorGUILayout.BeginVertical();
		EditorGUIUtility.labelWidth = 80f;
		// audio clip
		if(auKey.setAudioClip((AudioClip)EditorGUILayout.ObjectField("Audio Clip", auKey.audioClip,typeof(AudioClip),false))) {
			// update cache when modifying varaibles
			sTrack.updateCache();
			CustomInspector.updateView(sTrack);
		}
		// loop audio
		if(auKey.setLoop(EditorGUILayout.Toggle("Loop", auKey.loop))) {
			// update cache when modifying varaibles
			sTrack.updateCache();
			CustomInspector.updateView(sTrack);
		}
		EditorGUILayout.EndVertical();
	}
}
