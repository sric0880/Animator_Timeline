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
	}

	public static void updateView(AMTrack track, float _frame)
	{
		// preview
		AMTimeline.window.aData.getCurrentTake().previewFrame(_frame);
		updateView (track);
		AMTimeline.window.Repaint();
	}

	public static void updateView(AMTrack track)
	{
		saveData(track);
		AMTimeline.window.saveAnimatorData();
	}

	public static void saveData(AMTrack track)
	{
		AMTimeline.setDirtyKeys(track);
		AMTimeline.setDirtyCache(track);
	}
	void OnGUI()
	{
		if (aData.inPlayMode) {
			//not editable
			EditorGUILayout.HelpBox("Cannot edit in play mode", MessageType.Info);
			return;
		}
		selectedTrack = aData.getCurrentTake().selectedTrack;
		selectedFrame = aData.getCurrentTake().selectedFrame;
		if(selectedTrack <= -1 || selectedFrame == 0) return;
		AMTrack sTrack = aData.getCurrentTake().getSelectedTrack();
		if (!sTrack.hasKeyOnFrame(selectedFrame)) return;

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
			AMTimeline.window.saveAnimatorData();
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
}
