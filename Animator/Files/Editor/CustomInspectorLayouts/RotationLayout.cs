using UnityEngine;
using UnityEditor;
using System.Collections;

public class RotationLayout {

	public static void InspectorLayout(AMRotationTrack sTrack, AMRotationKey rKey)
	{
		GUILayout.BeginVertical();
		// quaternion
		rKey.rotationValues = EditorGUILayout.Vector3Field("Quaternion:",rKey.rotationValues);
		if(rKey.setRotation(rKey.rotationValues)) {
			// update cache when modifying varaibles
			sTrack.updateCache();
			CustomInspector.updateView(sTrack, rKey.frame);
		}
		// if not last key, show ease
		if(rKey != sTrack.keys[sTrack.keys.Count-1]) {
			if(sTrack.getActionIndexForFrame(rKey.frame)>-1) {
				CustomInspector.showEasePicker(sTrack,rKey);
			}
		}
		GUILayout.EndVertical();
	}
}
