using UnityEngine;
using UnityEditor;
using System.Collections;

public class CustomInspector : EditorWindow
{

	public AMTrack sTrack = null;
	public int selectedFrame = 0;

	private string[] wrapModeNames = {
		"Once",	
		"Loop",
		"ClampForever",
		"PingPong"
	};
	
	private Texture[] texInterpl = {(Texture)Resources.Load("am_interpl_curve"),(Texture)Resources.Load("am_interpl_linear")};

	void OnGUI()
	{
		if(sTrack == null || selectedFrame == 0) return;
//		Rect rectBtnDeleteKey = new Rect(width_inspector_open-width_inspector_closed-width_button-margin,0f,width_button,height_button);
//		// if frame has no key or isPlaying, return
//		if(_track <= -1 || !sTrack.hasKeyOnFrame(_frame) || isPlaying) {
//			GUI.enabled = false;
//			// disabled delete key button
//			GUI.Button (rectBtnDeleteKey,(getSkinTextureStyleState("delete").background),GUI.skin.GetStyle("ButtonImage"));
//			GUI.enabled = !isPlaying;
//			return;
//		}
//		// delete key button
//		if(GUI.Button (rectBtnDeleteKey,new GUIContent("","Delete Key"),GUI.skin.GetStyle("ButtonImage"))) {
//			deleteKeyFromSelectedFrame();
//			return;
//		}
//		GUI.DrawTexture(new Rect(rectBtnDeleteKey.x+(rectBtnDeleteKey.height-10f)/2f,rectBtnDeleteKey.y+(rectBtnDeleteKey.width-10f)/2f,10f,10f),(getSkinTextureStyleState((rectBtnDeleteKey.Contains(e.mousePosition) ? "delete_hover" : "delete")).background));
//		float width_inspector = width_inspector_open - width_inspector_closed;
//		float start_y = 30f+height_inspector_space;

		#region translation inspector
		if(sTrack is AMTranslationTrack) {
			ShowTranslationInsp();
			return;
		}
		#endregion
//		#region rotation inspector
//		if(sTrack is AMRotationTrack) {
//			AMRotationKey rKey = (AMRotationKey)(sTrack as AMRotationTrack).getKeyOnFrame(_frame);
//			Rect rectQuaternion = new Rect(0f,start_y,width_inspector-margin,40f);
//			// quaternion
//			if(rKey.setRotationQuaternion(EditorGUI.Vector4Field(rectQuaternion,"Quaternion",rKey.getRotationQuaternion()))) {
//				// update cache when modifying varaibles
//				sTrack.updateCache();
//				// preview new position
//				aData.getCurrentTake().previewFrame(aData.getCurrentTake().selectedFrame);
//				// save data
//				setDirtyKeys(aData.getCurrentTake().getTrack(aData.getCurrentTake().selectedTrack));
//				setDirtyCache(aData.getCurrentTake().getTrack(aData.getCurrentTake().selectedTrack));
//				// refresh component
//				refreshGizmos();
//			}
//			// if not last key, show ease
//			if(rKey != (sTrack as AMRotationTrack).keys[(sTrack as AMRotationTrack).keys.Count-1]) {
//				Rect recEasePicker = new Rect(0f, rectQuaternion.y+rectQuaternion.height+height_inspector_space,width_inspector-margin,0f);
//				if((sTrack as AMRotationTrack).getActionIndexForFrame(_frame)>-1) {
//					showEasePicker(sTrack,rKey,aData,recEasePicker.x,recEasePicker.y,recEasePicker.width);
//				}
//			}
//			return;
//		}
//		#endregion
//		#region orientation inspector
//		if(sTrack is AMOrientationTrack) {
//			AMOrientationKey oKey = (AMOrientationKey)(sTrack as AMOrientationTrack).getKeyOnFrame(_frame);
//			// target
//			Rect rectLabelTarget = new Rect(0f,start_y,50f,22f);
//			GUI.Label(rectLabelTarget, "Target");
//			Rect rectObjectTarget = new Rect(rectLabelTarget.x+rectLabelTarget.width+3f,rectLabelTarget.y+3f,width_inspector-rectLabelTarget.width-3f-margin-width_button,16f);
//			if(oKey.setTarget((Transform)EditorGUI.ObjectField(rectObjectTarget,oKey.target,typeof(Transform),true))) {
//				// update cache when modifying varaibles
//				(sTrack as AMOrientationTrack).updateCache();
//				// preview
//				aData.getCurrentTake().previewFrame(aData.getCurrentTake().selectedFrame);
//				// save data
//				setDirtyKeys(aData.getCurrentTake().getTrack(aData.getCurrentTake().selectedTrack));
//				setDirtyCache(aData.getCurrentTake().getTrack(aData.getCurrentTake().selectedTrack));
//				// refresh component
//				refreshGizmos();
//			}
//			Rect rectNewTarget = new Rect(width_inspector-width_button-margin,rectLabelTarget.y,width_button,width_button);
//			if(GUI.Button(rectNewTarget, "+")) {
//				GenericMenu addTargetMenu = new GenericMenu();
//				addTargetMenu.AddItem(new GUIContent("With Translation"), false, addTargetWithTranslationTrack, oKey);
//				addTargetMenu.AddItem(new GUIContent("Without Translation"), false, addTargetWithoutTranslationTrack, oKey);
//				addTargetMenu.ShowAsContext();
//			}
//			// if not last key, show ease
//			if(oKey != (sTrack as AMOrientationTrack).keys[(sTrack as AMOrientationTrack).keys.Count-1]) {
//				int oActionIndex = (sTrack as AMOrientationTrack).getActionIndexForFrame(_frame);
//				if(oActionIndex>-1 && (sTrack.cache[oActionIndex] as AMOrientationAction).startTarget != (sTrack.cache[oActionIndex] as AMOrientationAction).endTarget) {
//					Rect recEasePicker = new Rect(0f, rectNewTarget.y+rectNewTarget.height+height_inspector_space,width_inspector-margin,0f);
//					showEasePicker(sTrack,oKey,aData,recEasePicker.x,recEasePicker.y,recEasePicker.width);
//				}
//			}
//			return;
//		}
//		#endregion
//		#region animation inspector
//		if(sTrack is AMAnimationTrack) {
//			AMAnimationKey aKey	= (AMAnimationKey)(sTrack as AMAnimationTrack).getKeyOnFrame(_frame);
//			// animation clip
//			Rect rectLabelAnimClip = new Rect(0f,start_y,100f,22f);
//			GUI.Label(rectLabelAnimClip,"Animation Clip");
//			Rect rectObjectField = new Rect(rectLabelAnimClip.x+rectLabelAnimClip.width+2f,rectLabelAnimClip.y+3f,width_inspector-rectLabelAnimClip.width-margin,16f);
//			if(aKey.setAmClip((AnimationClip)EditorGUI.ObjectField(rectObjectField,aKey.amClip,typeof(AnimationClip),false))) {
//				// update cache when modifying varaibles
//				sTrack.updateCache();
//				// preview new position
//				aData.getCurrentTake().previewFrame(aData.getCurrentTake().selectedFrame);
//				// save data
//				setDirtyKeys(aData.getCurrentTake().getTrack(aData.getCurrentTake().selectedTrack));
//				setDirtyCache(aData.getCurrentTake().getTrack(aData.getCurrentTake().selectedTrack));
//				// refresh component
//				refreshGizmos();
//			}
//			// wrap mode
//			Rect rectLabelWrapMode = new Rect(0f,rectLabelAnimClip.y+rectLabelAnimClip.height+height_inspector_space,85f,22f);
//			GUI.Label(rectLabelWrapMode,"Wrap Mode");
//			Rect rectPopupWrapMode = new Rect(rectLabelWrapMode.x+rectLabelWrapMode.width,rectLabelWrapMode.y+3f,120f,22f);
//			if(aKey.setWrapMode(indexToWrapMode(EditorGUI.Popup(rectPopupWrapMode,wrapModeToIndex(aKey.wrapMode),wrapModeNames)))) {
//				// update cache when modifying varaibles
//				sTrack.updateCache();
//				// preview new position
//				aData.getCurrentTake().previewFrame(aData.getCurrentTake().selectedFrame);
//				// save data
//				setDirtyKeys(aData.getCurrentTake().getTrack(aData.getCurrentTake().selectedTrack));
//				setDirtyCache(aData.getCurrentTake().getTrack(aData.getCurrentTake().selectedTrack));
//				// refresh component
//				refreshGizmos();
//			}
//			// crossfade
//			Rect rectLabelCrossfade = new Rect(0f,rectLabelWrapMode.y+rectPopupWrapMode.height+height_inspector_space,85f,22f);
//			GUI.Label (rectLabelCrossfade, "Crossfade");
//			Rect rectToggleCrossfade = new Rect(rectLabelCrossfade.x+rectLabelCrossfade.width,rectLabelCrossfade.y+2f,20f,rectLabelCrossfade.height);
//			if(aKey.setCrossFade(EditorGUI.Toggle(rectToggleCrossfade,aKey.crossfade))) {
//				// update cache when modifying varaibles
//				sTrack.updateCache();
//				// preview new position
//				aData.getCurrentTake().previewFrame(aData.getCurrentTake().selectedFrame);
//				// save data
//				setDirtyKeys(aData.getCurrentTake().getTrack(aData.getCurrentTake().selectedTrack));
//				setDirtyCache(aData.getCurrentTake().getTrack(aData.getCurrentTake().selectedTrack));
//				// refresh component
//				refreshGizmos();	
//			}
//			Rect rectLabelCrossFadeTime = new Rect(rectToggleCrossfade.x+rectToggleCrossfade.width+10f,rectLabelCrossfade.y,35f,rectToggleCrossfade.height);
//			if(!aKey.crossfade) GUI.enabled = false;
//			GUI.Label(rectLabelCrossFadeTime,"Time");
//			Rect rectFloatFieldCrossFade = new Rect(rectLabelCrossFadeTime.x+rectLabelCrossFadeTime.width+margin,rectLabelCrossFadeTime.y+3f,40f,rectLabelCrossFadeTime.height);
//			if(aKey.setCrossfadeTime(EditorGUI.FloatField(rectFloatFieldCrossFade,aKey.crossfadeTime))) {
//				// update cache when modifying varaibles
//				sTrack.updateCache();
//				// save data
//				setDirtyKeys(aData.getCurrentTake().getTrack(aData.getCurrentTake().selectedTrack));
//				setDirtyCache(aData.getCurrentTake().getTrack(aData.getCurrentTake().selectedTrack));
//				// refresh component
//				refreshGizmos();	
//			}
//			Rect rectLabelSeconds = new Rect(rectFloatFieldCrossFade.x+rectFloatFieldCrossFade.width+margin,rectLabelCrossFadeTime.y,20f,rectLabelCrossFadeTime.height);
//			GUI.Label(rectLabelSeconds,"s");
//			GUI.enabled = true;
//		}
//		#endregion
//		#region audio inspector
//		if(sTrack is AMAudioTrack) {
//			AMAudioKey auKey = (AMAudioKey)(sTrack as AMAudioTrack).getKeyOnFrame(_frame);
//			// audio clip
//			Rect rectLabelAudioClip = new Rect(0f,start_y,80f,22f);
//			GUI.Label(rectLabelAudioClip,"Audio Clip");
//			Rect rectObjectField = new Rect(rectLabelAudioClip.x+rectLabelAudioClip.width+margin,rectLabelAudioClip.y+3f,width_inspector-rectLabelAudioClip.width-margin,16f);
//			if(auKey.setAudioClip((AudioClip)EditorGUI.ObjectField(rectObjectField,auKey.audioClip,typeof(AudioClip),false))) {
//				// update cache when modifying varaibles
//				sTrack.updateCache();
//				// save data
//				setDirtyKeys(aData.getCurrentTake().getTrack(aData.getCurrentTake().selectedTrack));
//				setDirtyCache(aData.getCurrentTake().getTrack(aData.getCurrentTake().selectedTrack));
//				// refresh component
//				refreshGizmos();
//				
//			}
//			Rect rectLabelLoop = new Rect(0f,rectLabelAudioClip.y+rectLabelAudioClip.height+height_inspector_space,80f,22f);
//			// loop audio
//			GUI.Label (rectLabelLoop,"Loop");
//			Rect rectToggleLoop = new Rect(rectLabelLoop.x+rectLabelLoop.width+margin,rectLabelLoop.y+2f,22f,22f);
//			if(auKey.setLoop(EditorGUI.Toggle(rectToggleLoop,auKey.loop))) {
//				// update cache when modifying varaibles
//				sTrack.updateCache();
//				// save data
//				setDirtyKeys(aData.getCurrentTake().getTrack(aData.getCurrentTake().selectedTrack));
//				setDirtyCache(aData.getCurrentTake().getTrack(aData.getCurrentTake().selectedTrack));
//				// refresh component
//				refreshGizmos();	
//			}
//			return;
//		}
//		#endregion 
//		#region property inspector
//		if(sTrack is AMPropertyTrack) {
//			AMPropertyKey pKey = (AMPropertyKey)(sTrack as AMPropertyTrack).getKeyOnFrame(_frame);
//			// value
//			string propertyLabel = (sTrack as AMPropertyTrack).getTrackType();
//			Rect rectField = new Rect(0f,start_y,width_inspector-margin,22f);
//			#region morph channels
//			if((sTrack as AMPropertyTrack).valueType == (int) AMPropertyTrack.ValueType.MorphChannels) {
//				Rect rectEasePicker = new Rect(0f,start_y,0f,0f);
//				// ease picker
//				if(pKey != (sTrack as AMPropertyTrack).keys[(sTrack as AMPropertyTrack).keys.Count-1]) {
//					rectEasePicker.width = width_inspector-margin;
//					rectEasePicker.height = 22f;
//					showEasePicker(sTrack,pKey,aData,rectEasePicker.x,rectEasePicker.y,rectEasePicker.width);
//				}
//				string[] channelNames = (sTrack as AMPropertyTrack).getMorphNames();
//				int numChannels = channelNames.Length;
//				float scrollview_y = rectEasePicker.y+rectEasePicker.height+height_inspector_space;
//				Rect rectMorphScrollView = new Rect(0f,scrollview_y,width_inspector-margin,rect.height-scrollview_y);
//				Rect rectMorphView = new Rect(0f,0f,rectMorphScrollView.width-20f,numChannels*44f+(numChannels)*height_inspector_space);
//				inspectorScrollView = GUI.BeginScrollView(rectMorphScrollView,inspectorScrollView,rectMorphView);
//				List<float> megaMorphChannels = new List<float>(pKey.morph);
//				int indexChannel = -1;	// channel to set to 100, all others to 0
//				Rect rectMorphLabel = new Rect(0f,0f,100f,22f);
//				Rect rectMorphSelectButton = new Rect(width_inspector-margin-18f-(rectMorphView.height>rectMorphScrollView.height ? 20f : 0f),0f,18f,16f);
//				Rect rectMorphFloatField = new Rect(rectMorphSelectButton.x-50f-margin,0f,50f,22f);
//				Rect rectMorphSlider = new Rect(0f,0f,width_inspector-margin*3f-rectMorphFloatField.width-rectMorphSelectButton.width-(rectMorphView.height>rectMorphScrollView.height ? 20f : 0f),22f);
//				for(int k=0;k<numChannels;k++) {
//					if(megaMorphChannels.Count <= k) megaMorphChannels.Add(0f);
//					// label
//					if(k>0) rectMorphLabel.y += 44f+height_inspector_space;
//					GUI.Label(rectMorphLabel, k+" - "+channelNames[k]);
//					// slider
//					rectMorphSlider.y = rectMorphLabel.y+22f;
//					megaMorphChannels[k] = GUI.HorizontalSlider(rectMorphSlider, megaMorphChannels[k],0f,100f);
//					// float field
//					rectMorphFloatField.y = rectMorphSlider.y;
//					megaMorphChannels[k] = EditorGUI.FloatField(rectMorphFloatField,"",megaMorphChannels[k]);
//					megaMorphChannels[k] = Mathf.Clamp(megaMorphChannels[k],0f,100f);
//					// select this button
//					rectMorphSelectButton.y = rectMorphSlider.y;
//					if(GUI.Button(rectMorphSelectButton,"")) indexChannel = k;
//					Rect rectTextureSelectThis = new Rect(rectMorphSelectButton);
//					rectTextureSelectThis.x += 3f;
//					rectTextureSelectThis.width -= 5f;
//					rectTextureSelectThis.y += 3f;
//					rectTextureSelectThis.height -= 6f;
//					GUI.DrawTexture(rectTextureSelectThis,getSkinTextureStyleState("select_this").background);
//				}
//				if(indexChannel != -1) {
//					registerUndo("Set Morph");
//					// set value
//					if(megaMorphChannels[indexChannel] != 100f) {
//						megaMorphChannels[indexChannel] = 100f;
//					} else {
//						// select exclusive
//						for(int k=0;k<numChannels;k++) {
//							if(k != indexChannel) megaMorphChannels[k] = 0f;
//						}
//					}
//				}
//				megaMorphChannels = megaMorphChannels.GetRange(0,numChannels);
//				if(pKey.setValueMegaMorph(megaMorphChannels)) {
//					// update cache when modifying varaibles
//					sTrack.updateCache();
//					// preview new value
//					aData.getCurrentTake().previewFrame(aData.getCurrentTake().selectedFrame);
//					// save data
//					setDirtyKeys(aData.getCurrentTake().getTrack(aData.getCurrentTake().selectedTrack));
//					setDirtyCache(aData.getCurrentTake().getTrack(aData.getCurrentTake().selectedTrack));
//					// refresh component
//					refreshGizmos();	
//				}
//				GUI.EndScrollView();
//				#endregion
//				// int value
//			} else if(((sTrack as AMPropertyTrack).valueType == (int) AMPropertyTrack.ValueType.Integer) || ((sTrack as AMPropertyTrack).valueType == (int) AMPropertyTrack.ValueType.Long)) {
//				if(pKey.setValue(EditorGUI.IntField(rectField,propertyLabel,Convert.ToInt32(pKey.val)))) {
//					// update cache when modifying varaibles
//					sTrack.updateCache();
//					// preview new value
//					aData.getCurrentTake().previewFrame(aData.getCurrentTake().selectedFrame);
//					// save data
//					setDirtyKeys(aData.getCurrentTake().getTrack(aData.getCurrentTake().selectedTrack));
//					setDirtyCache(aData.getCurrentTake().getTrack(aData.getCurrentTake().selectedTrack));
//					// refresh component
//					refreshGizmos();	
//				}
//			} else if(((sTrack as AMPropertyTrack).valueType == (int) AMPropertyTrack.ValueType.Float)||((sTrack as AMPropertyTrack).valueType == (int) AMPropertyTrack.ValueType.Double)) {
//				if(pKey.setValue(EditorGUI.FloatField(rectField,propertyLabel,(float)(pKey.val)))) {
//					// update cache when modifying varaibles
//					sTrack.updateCache();
//					// preview new value
//					aData.getCurrentTake().previewFrame(aData.getCurrentTake().selectedFrame);
//					// save data
//					setDirtyKeys(aData.getCurrentTake().getTrack(aData.getCurrentTake().selectedTrack));
//					setDirtyCache(aData.getCurrentTake().getTrack(aData.getCurrentTake().selectedTrack));
//					// refresh component
//					refreshGizmos();	
//				}
//			} else if((sTrack as AMPropertyTrack).valueType == (int) AMPropertyTrack.ValueType.Vector2) {
//				rectField.height = 40f;
//				if(pKey.setValue(EditorGUI.Vector2Field(rectField,propertyLabel,pKey.vect2))) {
//					// update cache when modifying varaibles
//					sTrack.updateCache();
//					// preview new value
//					aData.getCurrentTake().previewFrame(aData.getCurrentTake().selectedFrame);
//					// save data
//					setDirtyKeys(aData.getCurrentTake().getTrack(aData.getCurrentTake().selectedTrack));
//					setDirtyCache(aData.getCurrentTake().getTrack(aData.getCurrentTake().selectedTrack));
//					// refresh component
//					refreshGizmos();	
//				}
//			} else if((sTrack as AMPropertyTrack).valueType == (int) AMPropertyTrack.ValueType.Vector3) {
//				rectField.height = 40f;
//				if(pKey.setValue(EditorGUI.Vector3Field(rectField,propertyLabel,pKey.vect3))) {
//					// update cache when modifying varaibles
//					sTrack.updateCache();
//					// preview new value
//					aData.getCurrentTake().previewFrame(aData.getCurrentTake().selectedFrame);
//					// save data
//					setDirtyKeys(aData.getCurrentTake().getTrack(aData.getCurrentTake().selectedTrack));
//					setDirtyCache(aData.getCurrentTake().getTrack(aData.getCurrentTake().selectedTrack));
//					// refresh component
//					refreshGizmos();	
//				}
//			} else if((sTrack as AMPropertyTrack).valueType == (int) AMPropertyTrack.ValueType.Color) {
//				rectField.height = 22f;
//				if(pKey.setValue(EditorGUI.ColorField(rectField,propertyLabel,pKey.color))) {
//					// update cache when modifying varaibles
//					sTrack.updateCache();
//					// preview new value
//					aData.getCurrentTake().previewFrame(aData.getCurrentTake().selectedFrame);
//					// save data
//					setDirtyKeys(aData.getCurrentTake().getTrack(aData.getCurrentTake().selectedTrack));
//					setDirtyCache(aData.getCurrentTake().getTrack(aData.getCurrentTake().selectedTrack));
//					// refresh component
//					refreshGizmos();	
//				}
//			}else if((sTrack as AMPropertyTrack).valueType == (int) AMPropertyTrack.ValueType.Rect) {
//				rectField.height = 60f;
//				if(pKey.setValue(EditorGUI.RectField(rectField,propertyLabel,pKey.rect))) {
//					// update cache when modifying varaibles
//					sTrack.updateCache();
//					// preview new value
//					aData.getCurrentTake().previewFrame(aData.getCurrentTake().selectedFrame);
//					// save data
//					setDirtyKeys(aData.getCurrentTake().getTrack(aData.getCurrentTake().selectedTrack));
//					setDirtyCache(aData.getCurrentTake().getTrack(aData.getCurrentTake().selectedTrack));
//					// refresh component
//					refreshGizmos();	
//				}
//			}
//			// property ease, show if not last key (check for action; there is no rotation action for last key). do not show for morph channels, because it is shown before the parameters
//			if((sTrack as AMPropertyTrack).valueType != (int)AMPropertyTrack.ValueType.MorphChannels && pKey != (sTrack as AMPropertyTrack).keys[(sTrack as AMPropertyTrack).keys.Count-1]) {
//				Rect rectEasePicker = new Rect(0f,rectField.y+rectField.height+height_inspector_space,width_inspector-margin,0f);
//				showEasePicker(sTrack,pKey,aData,rectEasePicker.x,rectEasePicker.y,rectEasePicker.width);
//			}
//			return;
//		}
//		#endregion
//		#region event inspector
//		if(sTrack is AMEventTrack) {
//			AMEventKey eKey = (AMEventKey)(sTrack as AMEventTrack).getKeyOnFrame(_frame);
//			// value
//			if(indexMethodInfo == -1 || cachedMethodInfo.Count<=0) {
//				Rect rectLabel = new Rect(0f,start_y,width_inspector-margin*2f-20f,22f);
//				GUI.Label(rectLabel,"No usable methods found.");
//				Rect rectButton = new Rect(width_inspector-20f-margin,start_y+1f,20f,20f);
//				if(GUI.Button(rectButton,"?")) {
//					EditorUtility.DisplayDialog("Usable Methods","Methods should be made public and be placed in scripts that are not directly derived from Component or Behaviour to be used in the Event Track (MonoBehaviour is fine).","Okay");
//				}	
//				return;
//			}
//			Rect rectPopup = new Rect(0f,start_y,width_inspector-margin,22f);
//			indexMethodInfo = EditorGUI.Popup(rectPopup,indexMethodInfo,getMethodNames());
//			// if index out of range
//			if((indexMethodInfo < cachedMethodInfo.Count)) {
//				// process change
//				if(eKey.setMethodInfo(cachedMethodInfoComponents[indexMethodInfo],cachedMethodInfo[indexMethodInfo],cachedParameterInfos)) {						// update cache when modifying varaibles
//					sTrack.updateCache();
//					// save data
//					setDirtyKeys(aData.getCurrentTake().getTrack(aData.getCurrentTake().selectedTrack));
//					setDirtyCache(aData.getCurrentTake().getTrack(aData.getCurrentTake().selectedTrack));
//					// deselect fields
//					GUIUtility.keyboardControl = 0;	
//				}
//			}
//			if(cachedParameterInfos.Length > 1) {
//				// if method has more than 1 parameter, set sendmessage to false, and disable toggle
//				if(eKey.setUseSendMessage(false)) {
//					sTrack.updateCache();
//					// save data
//					setDirtyKeys(aData.getCurrentTake().getTrack(aData.getCurrentTake().selectedTrack));
//					setDirtyCache(aData.getCurrentTake().getTrack(aData.getCurrentTake().selectedTrack));
//				}
//				GUI.enabled = false;	// disable sendmessage toggle
//			}
//			bool showObjectMessage = false;
//			Type showObjectType = null;
//			foreach(ParameterInfo p in cachedParameterInfos) {
//				Type elemType = p.ParameterType.GetElementType();
//				if(elemType != null && (elemType.BaseType == typeof(UnityEngine.Object) || elemType.BaseType == typeof(UnityEngine.Behaviour))) {
//					showObjectMessage = true;
//					showObjectType = elemType;
//					break;
//				}
//			}
//			Rect rectLabelObjectMessage = new Rect(0f,rectPopup.y+rectPopup.height,width_inspector-margin*2f-20f,0f);
//			if(showObjectMessage) {
//				rectLabelObjectMessage.height = 22f;
//				Rect rectButton = new Rect(width_inspector-20f-margin,rectLabelObjectMessage.y+1f,20f,20f);
//				GUI.color = Color.red;
//				GUI.Label (rectLabelObjectMessage,"* Use Object[] instead!");
//				GUI.color = Color.white;
//				if(GUI.Button (rectButton,"?")) {
//					EditorUtility.DisplayDialog("Use Object[] Parameter Instead","Array types derived from Object, such as GameObject[], cannot be cast correctly on runtime.\n\nUse UnityEngine.Object[] as a parameter type and then cast to (GameObject[]) in your method.\n\nIf you're trying to pass components"+(showObjectType != typeof(GameObject) ? " (such as "+showObjectType.ToString()+")":"")+", you should get them from the casted GameObjects on runtime.\n\nPlease see the documentation for more information.","Okay");
//				}
//				
//			}
//			Rect rectLabelSendMessage = new Rect(0f,rectLabelObjectMessage.y+rectLabelObjectMessage.height+height_inspector_space,150f,20f);
//			GUI.Label (rectLabelSendMessage,"Use SendMessage");
//			Rect rectToggleSendMessage = new Rect(rectLabelSendMessage.x+rectLabelSendMessage.width+margin,rectLabelSendMessage.y,20f,20f);
//			if(eKey.setUseSendMessage(GUI.Toggle(rectToggleSendMessage,eKey.useSendMessage,""))) {
//				sTrack.updateCache();
//				// save data
//				setDirtyKeys(aData.getCurrentTake().getTrack(aData.getCurrentTake().selectedTrack));
//				setDirtyCache(aData.getCurrentTake().getTrack(aData.getCurrentTake().selectedTrack));
//			}
//			GUI.enabled = !isPlaying;
//			Rect rectButtonSendMessageInfo = new Rect(width_inspector-20f-margin,rectLabelSendMessage.y,20f,20f);
//			if(GUI.Button (rectButtonSendMessageInfo,"?")) {
//				EditorUtility.DisplayDialog("SendMessage vs. Invoke","SendMessage can only be used with methods that have no more than one parameter (which can be an array).\n\nAnimator will use Invoke when SendMessage is disabled, which is slightly faster but requires caching when the take is played. Use SendMessage if caching is a problem.","Okay");
//			}
//			if(cachedParameterInfos.Length > 0) {
//				// show method parameters
//				float scrollview_y = rectLabelSendMessage.y+rectLabelSendMessage.height+height_inspector_space;
//				Rect rectScrollView = new Rect(0f,scrollview_y,width_inspector-margin,rect.height-scrollview_y);
//				float width_view = width_inspector-margin-(height_event_parameters > rectScrollView.height ? 20f+margin : 0f);
//				Rect rectView = new Rect(0f,0f,width_view,height_event_parameters);
//				inspectorScrollView = GUI.BeginScrollView(rectScrollView,inspectorScrollView,rectView);
//				Rect rectField = new Rect(0f, 0f, width_view,20f);
//				float height_all_fields = 0f;
//				// there are parameters
//				for(int i=0;i<cachedParameterInfos.Length;i++) {
//					rectField.y += height_inspector_space;
//					if(i > 0) height_all_fields += height_inspector_space;
//					// show field for each parameter
//					float height_field = 0f;
//					if(showFieldFor(rectField, i.ToString(),cachedParameterInfos[i].Name,eKey.parameters[i],cachedParameterInfos[i].ParameterType, 0, ref height_field)) {
//						sTrack.updateCache();
//						// save data
//						setDirtyKeys(aData.getCurrentTake().getTrack(aData.getCurrentTake().selectedTrack));
//						setDirtyCache(aData.getCurrentTake().getTrack(aData.getCurrentTake().selectedTrack));
//					}
//					rectField.y += height_field;
//					height_all_fields += height_field;
//				}
//				GUI.EndScrollView();
//				height_all_fields += height_inspector_space;
//				if(height_event_parameters != height_all_fields) height_event_parameters = height_all_fields;
//			}
//			return;
//		}
//		#endregion
//		#region camerea switcher inspector
//		if(sTrack is AMCameraSwitcherTrack) {
//			AMCameraSwitcherKey cKey = (AMCameraSwitcherKey)(sTrack as AMCameraSwitcherTrack).getKeyOnFrame(_frame);
//			bool showExtras = false;
//			bool notLastKey = cKey != (sTrack as AMCameraSwitcherTrack).keys[(sTrack as AMCameraSwitcherTrack).keys.Count-1];
//			if(notLastKey) {
//				int cActionIndex = (sTrack as AMCameraSwitcherTrack).getActionIndexForFrame(_frame);
//				showExtras = cActionIndex>-1 && !(sTrack.cache[cActionIndex] as AMCameraSwitcherAction).targetsAreEqual();
//			}
//			float height_cs = 44f+height_inspector_space + (showExtras ? 22f*3f+height_inspector_space*3f : 0f);
//			Rect rectScrollView = new Rect(0f,start_y,width_inspector-margin,rect.height-start_y);
//			Rect rectView = new Rect(0f,0f,rectScrollView.width-(height_cs > rectScrollView.height ? 20f : 0f),height_cs);
//			inspectorScrollView = GUI.BeginScrollView(rectScrollView,inspectorScrollView,rectView);
//			Rect rectLabelType = new Rect(0f,0f,56f,22f);
//			GUI.Label(rectLabelType,"Type");
//			Rect rectSelGridType = new Rect(rectLabelType.x+rectLabelType.width+margin,rectLabelType.y,rectView.width-margin-rectLabelType.width,22f);
//			if(cKey.setType(GUI.SelectionGrid(rectSelGridType,cKey.type,new string[]{"Camera", "Color"},2))) {
//				// update cache when modifying varaibles
//				(sTrack as AMCameraSwitcherTrack).updateCache();
//				// preview
//				aData.getCurrentTake().previewFrame(aData.getCurrentTake().selectedFrame);
//				// save data
//				setDirtyKeys(aData.getCurrentTake().getTrack(aData.getCurrentTake().selectedTrack));
//				setDirtyCache(aData.getCurrentTake().getTrack(aData.getCurrentTake().selectedTrack));
//				// refresh component
//				refreshGizmos();
//			}
//			// camera
//			Rect rectLabelCameraColor = new Rect(0f,rectLabelType.y+rectLabelType.height+height_inspector_space,56f,22f);
//			GUI.Label(rectLabelCameraColor,(cKey.type == 0 ? "Camera" : "Color"));
//			Rect rectCameraColor = new Rect(rectLabelCameraColor.x+rectLabelCameraColor.width+margin,rectLabelCameraColor.y+3f,rectView.width-rectLabelCameraColor.width-margin,16f);
//			if(cKey.type == 0) {
//				if(cKey.setCamera((Camera)EditorGUI.ObjectField(rectCameraColor,cKey.camera,typeof(Camera),true))) {
//					// update cache when modifying varaibles
//					(sTrack as AMCameraSwitcherTrack).updateCache();
//					// preview
//					aData.getCurrentTake().previewFrame(aData.getCurrentTake().selectedFrame);
//					// save data
//					setDirtyKeys(aData.getCurrentTake().getTrack(aData.getCurrentTake().selectedTrack));
//					setDirtyCache(aData.getCurrentTake().getTrack(aData.getCurrentTake().selectedTrack));
//					// refresh component
//					refreshGizmos();
//				}
//			} else {
//				if(cKey.setColor(EditorGUI.ColorField(rectCameraColor,cKey.color))) {
//					// update cache when modifying varaibles
//					(sTrack as AMCameraSwitcherTrack).updateCache();
//					// preview
//					aData.getCurrentTake().previewFrame(aData.getCurrentTake().selectedFrame);
//					// save data
//					setDirtyKeys(aData.getCurrentTake().getTrack(aData.getCurrentTake().selectedTrack));
//					setDirtyCache(aData.getCurrentTake().getTrack(aData.getCurrentTake().selectedTrack));
//					// refresh component
//					refreshGizmos();
//				}
//			}
//			GUI.enabled = true;
//			// if not last key, show transition and ease
//			if(notLastKey && showExtras) {
//				// transition picker
//				Rect rectTransitionPicker = new Rect(0f,rectLabelCameraColor.y+rectLabelCameraColor.height+height_inspector_space,rectView.width,22f);
//				showTransitionPicker(sTrack,cKey,rectTransitionPicker.x,rectTransitionPicker.y,rectTransitionPicker.width);
//				if(cKey.cameraFadeType != (int)AMTween.Fade.None) {
//					// ease picker
//					Rect rectEasePicker = new Rect(0f,rectTransitionPicker.y+rectTransitionPicker.height+height_inspector_space,rectView.width,22f);
//					showEasePicker(sTrack,cKey,aData,rectEasePicker.x,rectEasePicker.y,rectEasePicker.width);
//					// render texture
//					Rect rectLabelRenderTexture = new Rect(0f,rectEasePicker.y+rectEasePicker.height+height_inspector_space,175f,22f);
//					GUI.Label(rectLabelRenderTexture,"Render Texture (Pro Only)");
//					Rect rectToggleRenderTexture = new Rect(rectView.width-22f,rectLabelRenderTexture.y,22f,22f);
//					if(cKey.setStill(!GUI.Toggle(rectToggleRenderTexture,!cKey.still,""))) {
//						// update cache when modifying varaibles
//						(sTrack as AMCameraSwitcherTrack).updateCache();
//						// preview
//						aData.getCurrentTake().previewFrame(aData.getCurrentTake().selectedFrame);
//						// save data
//						setDirtyKeys(aData.getCurrentTake().getTrack(aData.getCurrentTake().selectedTrack));
//						setDirtyCache(aData.getCurrentTake().getTrack(aData.getCurrentTake().selectedTrack));
//						// refresh component
//						refreshGizmos();
//					}
//				}
//			}
//			GUI.EndScrollView();
//			return;
//		}
//		#endregion
	}

	void ShowTranslationInsp()
	{
		AMTranslationKey tKey = (AMTranslationKey)(sTrack as AMTranslationTrack).getKeyOnFrame(selectedFrame);
		// translation interpolation
		GUILayout.BeginVertical();
		
		GUILayout.Label("Interpolation:");
		int selectedInterpl = GUILayout.SelectionGrid(tKey.interp,texInterpl,2);
		if(tKey.setInterpolation(selectedInterpl))
		{
			sTrack.updateCache();
			// select the current frame
			//TODO
			//				timelineSelectFrame(aData.getCurrentTake().selectedTrack,aData.getCurrentTake().selectedFrame);
			//				// save data
			//				setDirtyKeys(aData.getCurrentTake().getTrack(aData.getCurrentTake().selectedTrack));
			//				setDirtyCache(aData.getCurrentTake().getTrack(aData.getCurrentTake().selectedTrack));
			//				// refresh component
			//				refreshGizmos();
		}
		// translation position
		
		if(tKey.setPosition(EditorGUILayout.Vector3Field("Position:",tKey.position))) {
			// update cache when modifying varaibles
			sTrack.updateCache();
			//TODO
			// preview new position
			//				aData.getCurrentTake().previewFrame(aData.getCurrentTake().selectedFrame);
			//				// save data
			//				setDirtyKeys(aData.getCurrentTake().getTrack(aData.getCurrentTake().selectedTrack));
			//				setDirtyCache(aData.getCurrentTake().getTrack(aData.getCurrentTake().selectedTrack));
			//				// refresh component
			//				refreshGizmos();
		}
		GUILayout.EndVertical();
		
		// if not only key, show ease
		//			bool isTKeyLastFrame = tKey == (sTrack as AMTranslationTrack).keys[(sTrack as AMTranslationTrack).keys.Count-1];
		//			
		//			if(!isTKeyLastFrame) {
		//				Rect recEasePicker = new Rect(0f, rectPosition.y+rectPosition.height+height_inspector_space,width_inspector-margin,0f);
		//				if(!isTKeyLastFrame && tKey.interp == (int)AMTranslationKey.Interpolation.Linear) {
		//					showEasePicker(sTrack,tKey,aData,recEasePicker.x,recEasePicker.y,recEasePicker.width);
		//				} else {
		//					showEasePicker(sTrack,(sTrack as AMTranslationTrack).getActionStartKeyFor(tKey.frame),aData,recEasePicker.x,recEasePicker.y,recEasePicker.width);
		//				}
		//			}
	}	


	void showRotationInsp()
	{

	}
}
