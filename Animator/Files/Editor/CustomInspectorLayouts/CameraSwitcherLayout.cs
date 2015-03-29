using UnityEngine;
using UnityEditor;
using System.Collections;

public class CameraSwitcherLayout {

	public static void InspectorLayout(AMCameraSwitcherTrack sTrack, AMCameraSwitcherKey cKey)
	{
		bool showExtras = false;
		bool notLastKey = cKey != sTrack.keys[sTrack.keys.Count-1];
		if(notLastKey) {
			int cActionIndex = (sTrack as AMCameraSwitcherTrack).getActionIndexForFrame(cKey.frame);
			showExtras = cActionIndex>-1 && !(sTrack.cache[cActionIndex] as AMCameraSwitcherAction).targetsAreEqual();
		}
		GUILayout.Label("Type:");
		if(cKey.setType(GUILayout.SelectionGrid(cKey.type,new string[]{"Camera", "Color"},2))) {
			// update cache when modifying varaibles
			(sTrack as AMCameraSwitcherTrack).updateCache();
			CustomInspector.updateView(sTrack, cKey.frame);
		}
		// camera
		GUILayout.Label((cKey.type == 0 ? "Camera:" : "Color:"));
		if(cKey.type == 0) {
			if(cKey.setCamera((Camera)EditorGUILayout.ObjectField(cKey.camera,typeof(Camera),true))) {
				// update cache when modifying varaibles
				(sTrack as AMCameraSwitcherTrack).updateCache();
				CustomInspector.updateView(sTrack, cKey.frame);
			}
		} else {
			if(cKey.setColor(EditorGUILayout.ColorField(cKey.color))) {
				// update cache when modifying varaibles
				(sTrack as AMCameraSwitcherTrack).updateCache();
				CustomInspector.updateView(sTrack, cKey.frame);
			}
		}
		GUI.enabled = true;
		// if not last key, show transition and ease
		if(notLastKey && showExtras) {
			// transition picker
			showTransitionPicker(sTrack,cKey);
			if(cKey.cameraFadeType != (int)AMTween.Fade.None) {
				// ease picker
				CustomInspector.showEasePicker(sTrack,cKey);
				// render texture
				if(cKey.setStill(!EditorGUILayout.Toggle("Render Texture (Pro Only)", !cKey.still))) {
					// update cache when modifying varaibles
					(sTrack as AMCameraSwitcherTrack).updateCache();
					CustomInspector.updateView(sTrack, cKey.frame);
				}
			}
		}
	}

	public static void showTransitionPicker(AMTrack track, AMCameraSwitcherKey key) {
		GUILayout.BeginHorizontal();
		GUILayout.BeginVertical();
		GUILayout.Space(1f);
		GUILayout.Label ("Fade:");
		GUILayout.EndVertical();
		GUILayout.BeginVertical();
		GUILayout.Space(3f);
		int index = 0;
		for(int i=0;i<AMTween.TransitionOrder.Length;i++) {
			if(AMTween.TransitionOrder[i] == key.cameraFadeType) {
				index = i;
				break;
			}
		}
		int newIndex = EditorGUILayout.Popup(index, AMTween.TransitionNames);
		if(key.setCameraFadeType(AMTween.TransitionOrder[newIndex])) {
			// reset parameters
			AMTransitionPicker.setDefaultParametersForKey(ref key);
			// update cache when modifying variables
			track.updateCache();
			// save data
			AMTimeline.window.saveAnimatorData();
			// preview current frame
			AMTimeline.window.aData.getCurrentTake().previewFrame(key.frame);
			// refresh values
			AMTransitionPicker.refreshValues();
		}
		
		GUILayout.EndVertical();
		if(GUILayout.Button("Preview")) {
			AMTransitionPicker.setValues(key, track);
			EditorWindow.GetWindow (typeof (AMTransitionPicker));
		}
		GUILayout.Space(1f);
		GUILayout.EndHorizontal();	
	}
}
