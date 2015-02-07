using UnityEngine;
using UnityEditor;
using System.Collections;

public class TranslationLayout {

	private static Texture[] texInterpl = {(Texture)Resources.Load("am_interpl_curve"),(Texture)Resources.Load("am_interpl_linear")};

	public static void InspectorLayout(AMTranslationTrack sTrack, AMTranslationKey tKey)
	{
		GUILayout.BeginVertical();

		GUILayout.Label("Interpolation:");
		int selectedInterpl = GUILayout.SelectionGrid(tKey.interp,texInterpl,2);
		if(tKey.setInterpolation(selectedInterpl))
		{
			sTrack.updateCache();
			// select the current frame
//			timelineSelectFrame(aData.getCurrentTake().selectedTrack,aData.getCurrentTake().selectedFrame);
			// save data
			CustomInspector.updateView(sTrack);
		}

		// translation position
		if(tKey.setPosition(EditorGUILayout.Vector3Field("Position:",tKey.position))) {
			// update cache when modifying varaibles
			sTrack.updateCache();
			CustomInspector.updateView(sTrack, tKey.frame);
		}
	
//		if not only key, show ease
		bool isTKeyLastFrame = tKey == sTrack.keys[sTrack.keys.Count-1];
		if(!isTKeyLastFrame) {
			if(tKey.interp == (int)AMTranslationKey.Interpolation.Linear) {
				CustomInspector.showEasePicker(sTrack,tKey);
			} else {
				CustomInspector.showEasePicker(sTrack,sTrack.getActionStartKeyFor(tKey.frame));
			}	
		}
		GUILayout.EndVertical();
	}


}
