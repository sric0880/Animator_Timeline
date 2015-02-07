using UnityEngine;
using UnityEditor;
using System.Collections;

public class CustomInspector : EditorWindow
{
	public AnimatorData aData;
	private int selectedTrack = -1;
	private int selectedFrame = 0;

	void OnEnable() {
		this.title = "Inspector";
		
		loadAnimatorData();
	}

	public static void updateView(AMTrack track, float _frame)
	{
		// preview
		AMTimeline.window.aData.getCurrentTake().previewFrame(_frame);
		updateView (track);
	}

	public static void updateView(AMTrack track)
	{
		saveData(track);
		// refresh component
		AMTimeline.window.refreshGizmos();
	}

	public static void saveData(AMTrack track)
	{
		AMTimeline.setDirtyKeys(track);
		AMTimeline.setDirtyCache(track);
	}
	void OnGUI()
	{
		selectedTrack = aData.getCurrentTake().selectedTrack;
		selectedFrame = aData.getCurrentTake().selectedFrame;
		AMTrack sTrack = aData.getCurrentTake().getSelectedTrack();
		if(selectedTrack <= -1 || selectedFrame == 0 || !sTrack.hasKeyOnFrame(selectedFrame)) return;

		if(sTrack is AMTranslationTrack) {
			AMTranslationTrack tt = (sTrack as AMTranslationTrack);
			AMTranslationKey tKey = (AMTranslationKey)tt.getKeyOnFrame(selectedFrame);
			TranslationLayout.InspectorLayout(tt, tKey);
			return;
		}else if(sTrack is AMRotationTrack) {
			AMRotationTrack tt = (sTrack as AMRotationTrack);
			AMRotationKey tKey = (AMRotationKey)tt.getKeyOnFrame(selectedFrame);
			RotationLayout.InspectorLayout(tt, tKey);
			return;
		}else if(sTrack is AMOrientationTrack){
			AMOrientationTrack tt = (sTrack as AMOrientationTrack);
			AMOrientationKey tKey = (AMOrientationKey)tt.getKeyOnFrame(selectedFrame);
			OrientationLayout.InspectorLayout(tt, tKey);
			return;
		}else if(sTrack is AMAnimationTrack) {
			AMAnimationTrack tt = (sTrack as AMAnimationTrack);
			AMAnimationKey tKey = (AMAnimationKey)tt.getKeyOnFrame(selectedFrame);
			AnimationLayout.InspectorLayout(tt, tKey);
		}else if(sTrack is AMAudioTrack) {
			AMAudioTrack tt = (sTrack as AMAudioTrack);
			AMAudioKey tKey = (AMAudioKey)tt.getKeyOnFrame(selectedFrame);
			AudioLayout.InspectorLayout(tt, tKey);
			return;
		}else if(sTrack is AMPropertyTrack) {
			AMPropertyTrack tt = (sTrack as AMPropertyTrack);
			AMPropertyKey tKey = (AMPropertyKey)tt.getKeyOnFrame(selectedFrame);
			PropertyLayout.InspectorLayout(tt, tKey);
			return;
		} else if(sTrack is AMEventTrack) {
			AMEventTrack tt = (sTrack as AMEventTrack);
			AMEventKey tKey = (AMEventKey)tt.getKeyOnFrame(selectedFrame);
			EventLayout.InspectorLayout(tt, tKey);
			return;
		} else if(sTrack is AMCameraSwitcherTrack) {
			AMCameraSwitcherTrack tt = (sTrack as AMCameraSwitcherTrack);
			AMCameraSwitcherKey tKey = (AMCameraSwitcherKey)tt.getKeyOnFrame(selectedFrame);
			CameraSwitcherLayout.InspectorLayout(tt, tKey);
			return;
		}
	}

	public static bool showEasePicker(AMTrack track, AMKey key) {
		bool didUpdate = false;
		GUILayout.Label("Ease: ");
		GUILayout.BeginHorizontal();
		if(key.setEaseType(EditorGUILayout.Popup(key.easeType,AMTimeline.easeTypeNames))) {
			// update cache when modifying varaibles
			track.updateCache();
			// preview new position
			AMTimeline.window.aData.getCurrentTake().previewFrame(key.frame);
			// save data
			AMTimeline.window.refreshGizmos();
			// refresh component
			didUpdate = true;
			// refresh values
			AMEasePicker.refreshValues();
		}
		if(GUILayout.Button("Preview", GUILayout.Height(18f))) {
			AMEasePicker.setValues(key, track);
			EditorWindow.GetWindow (typeof (AMEasePicker));
		}
		GUILayout.EndHorizontal();	
		return didUpdate;
	}	


	void loadAnimatorData()
	{
		GameObject go = GameObject.Find ("AnimatorData");
		if(go) {
			aData = (AnimatorData) go.GetComponent ("AnimatorData");
		}
	}
}
