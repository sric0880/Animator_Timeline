using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;
using System;

public class PropertyLayout {

	public static void InspectorLayout(AMPropertyTrack sTrack, AMPropertyKey pKey)
	{
		// value
		string propertyLabel = (sTrack as AMPropertyTrack).getTrackType();
		#region morph channels
		if(sTrack.valueType == (int) AMPropertyTrack.ValueType.MorphChannels) {
			// ease picker
			if(pKey != sTrack.keys[sTrack.keys.Count-1]) {
				CustomInspector.showEasePicker(sTrack,pKey);
			}
			string[] channelNames = sTrack.getMorphNames();
			int numChannels = channelNames.Length;
			List<float> megaMorphChannels = new List<float>(pKey.morph);
			int indexChannel = -1;	// channel to set to 100, all others to 0
			for(int k=0;k<numChannels;k++) {
				if(megaMorphChannels.Count <= k) megaMorphChannels.Add(0f);
				// label
				GUILayout.Label(k+" - "+channelNames[k]);
				// slider
				megaMorphChannels[k] = GUILayout.HorizontalSlider(megaMorphChannels[k],0f,100f);
				// float field
				megaMorphChannels[k] = EditorGUILayout.FloatField("",megaMorphChannels[k]);
				megaMorphChannels[k] = Mathf.Clamp(megaMorphChannels[k],0f,100f);
				// select this button
				if(GUILayout.Button("choose this")) indexChannel = k;
			}
			if(indexChannel != -1) {
				// set value
				if(megaMorphChannels[indexChannel] != 100f) {
					megaMorphChannels[indexChannel] = 100f;
				} else {
					// select exclusive
					for(int k=0;k<numChannels;k++) {
						if(k != indexChannel) megaMorphChannels[k] = 0f;
					}
				}
			}
			megaMorphChannels = megaMorphChannels.GetRange(0,numChannels);
			if(pKey.setValueMegaMorph(megaMorphChannels)) {
				// update cache when modifying varaibles
				sTrack.updateCache();
				CustomInspector.updateView(sTrack, pKey.frame);
			}
			#endregion
			// int value
		} else if((sTrack.valueType == (int) AMPropertyTrack.ValueType.Integer) || ((sTrack as AMPropertyTrack).valueType == (int) AMPropertyTrack.ValueType.Long)) {
			if(pKey.setValue(EditorGUILayout.IntField(propertyLabel,Convert.ToInt32(pKey.val)))) {
				// update cache when modifying varaibles
				sTrack.updateCache();
				// preview new value
				CustomInspector.updateView(sTrack, pKey.frame);
			}
		} else if((sTrack.valueType == (int) AMPropertyTrack.ValueType.Float)||((sTrack as AMPropertyTrack).valueType == (int) AMPropertyTrack.ValueType.Double)) {
			if(pKey.setValue(EditorGUILayout.FloatField(propertyLabel,(float)(pKey.val)))) {
				// update cache when modifying varaibles
				sTrack.updateCache();
				CustomInspector.updateView(sTrack, pKey.frame);
			}
		} else if(sTrack.valueType == (int) AMPropertyTrack.ValueType.Vector2) {
			if(pKey.setValue(EditorGUILayout.Vector2Field(propertyLabel,pKey.vect2))) {
				// update cache when modifying varaibles
				sTrack.updateCache();
				CustomInspector.updateView(sTrack, pKey.frame);
			}
		} else if(sTrack.valueType == (int) AMPropertyTrack.ValueType.Vector3) {
			if(pKey.setValue(EditorGUILayout.Vector3Field(propertyLabel,pKey.vect3))) {
				// update cache when modifying varaibles
				sTrack.updateCache();
				CustomInspector.updateView(sTrack, pKey.frame);
			}
		} else if(sTrack.valueType == (int) AMPropertyTrack.ValueType.Color) {
			if(pKey.setValue(EditorGUILayout.ColorField(propertyLabel,pKey.color))) {
				// update cache when modifying varaibles
				sTrack.updateCache();
				CustomInspector.updateView(sTrack, pKey.frame);
			}
		}else if(sTrack.valueType == (int) AMPropertyTrack.ValueType.Rect) {
			if(pKey.setValue(EditorGUILayout.RectField(propertyLabel,pKey.rect))) {
				// update cache when modifying varaibles
				sTrack.updateCache();
				CustomInspector.updateView(sTrack, pKey.frame);
			}
		}
		// property ease, show if not last key (check for action; there is no rotation action for last key). do not show for morph channels, because it is shown before the parameters
		if((sTrack as AMPropertyTrack).valueType != (int)AMPropertyTrack.ValueType.MorphChannels && pKey != (sTrack as AMPropertyTrack).keys[(sTrack as AMPropertyTrack).keys.Count-1]) {
			CustomInspector.showEasePicker(sTrack,pKey);
		}

	}
}
