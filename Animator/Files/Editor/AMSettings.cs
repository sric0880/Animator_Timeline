using UnityEngine;
using UnityEditor;
using System.Collections;

public class AMSettings : EditorWindow {
	public static AMSettings window = null;
	
	public AnimatorData aData;	
	
	private int numFrames;
	private int frameRate;
	private bool saveChanges = false;

	void OnEnable() {
		window = this;
		this.title = "Settings";

		loadAnimatorData();

	}
	void OnDisable() {
		window = null;
		if((aData)&& saveChanges) {
			bool saveNumFrames = true;
			if((numFrames < aData.getCurrentTake().numFrames) && (aData.getCurrentTake().hasKeyAfter(numFrames))) {
				if(!EditorUtility.DisplayDialog("Data Will Be Lost","You will lose some keys beyond frame "+numFrames+" if you continue.", "Continue Anway","Cancel")) {
					saveNumFrames = false;
				}
			}
			if(saveNumFrames) {
				// save numFrames
				aData.getCurrentTake().numFrames = numFrames;
				aData.getCurrentTake().deleteKeysAfter(numFrames);
		
				// save data
				foreach(AMTrack track in aData.getCurrentTake().trackValues) {
						EditorUtility.SetDirty(track);
				}
			}
			// save frameRate
			aData.getCurrentTake().frameRate = frameRate;
			EditorWindow.GetWindow (typeof (AMTimeline)).Repaint();
			// save data
			EditorUtility.SetDirty(aData);
		}
	}
	void OnGUI() {		
		GUILayout.BeginVertical();
		numFrames = EditorGUILayout.IntField("Number of Frames:", numFrames);
		if(numFrames <= 0) numFrames = 1;
		frameRate = EditorGUILayout.IntField("Frame Rate (Fps):", frameRate);
		if(frameRate <= 0) frameRate = 1;
		GUILayout.BeginHorizontal();
			if(GUILayout.Button("Apply")) {
				saveChanges = true;
				this.Close();	
			}
			if(GUILayout.Button ("Cancel")) {
				saveChanges = false;
				this.Close();	
			}
		GUILayout.EndHorizontal();
		GUILayout.EndVertical();
	}
//	void OnHierarchyChange()
//	{
//		if(!aData) loadAnimatorData();
//	}
//	public void reloadAnimatorData() {
//		aData = null;
//		loadAnimatorData();
//	}
	void loadAnimatorData()
	{
		GameObject go = GameObject.Find ("AnimatorData");
		if(go) {
			aData = (AnimatorData) go.GetComponent ("AnimatorData");
			numFrames = aData.getCurrentTake().numFrames;
			frameRate = aData.getCurrentTake().frameRate;
		}
	}
}
