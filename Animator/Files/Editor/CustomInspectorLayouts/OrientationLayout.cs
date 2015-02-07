using UnityEngine;
using UnityEditor;
using System.Collections;

public class OrientationLayout {

	public static void InspectorLayout(AMOrientationTrack sTrack, AMOrientationKey oKey)
	{
		// target
		GUILayout.BeginHorizontal();
		GUILayout.Label("Target:");
		if(oKey.setTarget((Transform)EditorGUILayout.ObjectField(oKey.target,typeof(Transform),true))) {
			// update cache when modifying varaibles
			(sTrack as AMOrientationTrack).updateCache();
			CustomInspector.updateView(sTrack, oKey.frame);
		}
		GUILayout.EndHorizontal();
		if(GUILayout.Button("New Target")) {
			GenericMenu addTargetMenu = new GenericMenu();
			addTargetMenu.AddItem(new GUIContent("With Translation"), false, addTargetWithTranslationTrack, oKey);
			addTargetMenu.AddItem(new GUIContent("Without Translation"), false, addTargetWithoutTranslationTrack, oKey);
			addTargetMenu.ShowAsContext();
		}
		// if not last key, show ease
		if(oKey != sTrack.keys[sTrack.keys.Count-1]) {
			int oActionIndex = sTrack.getActionIndexForFrame(oKey.frame);
			if(oActionIndex>-1 && (sTrack.cache[oActionIndex] as AMOrientationAction).startTarget != (sTrack.cache[oActionIndex] as AMOrientationAction).endTarget) {
				CustomInspector.showEasePicker(sTrack,oKey);
			}
		}	
	}

	private static void addTargetWithTranslationTrack(object key) {
		AMTimeline.window.addTarget(key, true);
	}
	private static void addTargetWithoutTranslationTrack(object key) {
		AMTimeline.window.addTarget(key, false);
	}

}
