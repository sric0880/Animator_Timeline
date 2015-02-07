using UnityEngine;
using UnityEditor;
using System.Reflection;
using System.Collections;
using System.Collections.Generic;
using System;

public class EventLayout{

	const BindingFlags methodFlags = BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly;

	private static ParameterInfo[] cachedParameterInfos = new ParameterInfo[]{};
	private static List<MethodInfo> cachedMethodInfo = new List<MethodInfo>();
	private static List<string> cachedMethodNames = new List<string>();
	private static List<Component> cachedMethodInfoComponents = new List<Component>();
	private static int cachedIndexMethodInfo = -1;
	private static int indexMethodInfo {
		get { return cachedIndexMethodInfo; }
		set {
			if(cachedIndexMethodInfo != value) {
				cachedIndexMethodInfo = value;
				cacheSelectedMethodParameterInfos();
			}
			cachedIndexMethodInfo = value;
		}
	}
	private static Dictionary<string,bool> arrayFieldFoldout = new Dictionary<string, bool>();	// used to store the foldout values for arrays in event methods

	private static void updateCachedMethodInfo(GameObject go) {
		if(!go) return;
		cachedMethodInfo.Clear();
		cachedMethodNames.Clear();
		cachedMethodInfoComponents.Clear();
		Component[] arrComponents = go.GetComponents(typeof(Component));
		foreach(Component c in arrComponents) {
			if(c.GetType().BaseType == typeof(Component) || c.GetType().BaseType == typeof(Behaviour)) continue;
			MethodInfo[] methodInfos = c.GetType().GetMethods(methodFlags);
			foreach(MethodInfo methodInfo in methodInfos) {
				if((methodInfo.Name == "Start") || (methodInfo.Name == "Update") || (methodInfo.Name == "Main")) continue;
				cachedMethodNames.Add(getMethodInfoSignature(methodInfo));
				cachedMethodInfo.Add(methodInfo);
				cachedMethodInfoComponents.Add(c);
			}
		}	
	}

	private static void cacheSelectedMethodParameterInfos() {
		if(cachedMethodInfo == null || indexMethodInfo == -1 || (indexMethodInfo>=cachedMethodInfo.Count)) {
			cachedParameterInfos = new ParameterInfo[]{};
			arrayFieldFoldout = new Dictionary<string, bool>();	// reset array foldout dictionary
			return;
		}
		cachedParameterInfos = cachedMethodInfo[indexMethodInfo].GetParameters();
	}

	public static void resetIndexMethodInfo() {
		indexMethodInfo = -1;	
	}

	private static string getMethodInfoSignature(MethodInfo methodInfo) {
		ParameterInfo[] parameters = methodInfo.GetParameters();
		// loop through parameters, add them to signature
		string methodString = methodInfo.Name + " (";
		for(int i=0;i<parameters.Length;i++) {
			methodString += typeStringBrief(parameters[i].ParameterType);
			if(i<parameters.Length-1) methodString += ", ";
		}
		methodString += ")";
		return methodString;
	}	

	private static string typeStringBrief(Type t) {
		if(t.IsArray) return typeStringBrief(t.GetElementType())+"[]";
		if(t == typeof(int)) return "int";
		if(t == typeof(long)) return "long";
		if(t == typeof(float)) return "float";
		if(t == typeof(double)) return "double";
		if(t == typeof(Vector2)) return "Vector2";
		if(t == typeof(Vector3)) return "Vector3";
		if(t == typeof(Vector4)) return "Vector4";
		if(t == typeof(Color)) return "Color";
		if(t == typeof(Rect)) return "Rect";
		if(t == typeof(string)) return "string";
		if(t == typeof(char)) return "char";
		return t.Name;
	}

	private static string[] getMethodNames() {
		// get all method names from every comonent on GameObject, and update methodinfo cache
		return cachedMethodNames.ToArray();
	}

	public static void InspectorLayout(AMEventTrack sTrack, AMEventKey eKey)
	{
		// update methodinfo cache
		updateCachedMethodInfo(sTrack.obj);
		// set index to method info index
		if(cachedMethodInfo.Count>0) {
			if(eKey.methodInfo != null) {
				for (int i = 0; i< cachedMethodInfo.Count;i++) {
					if(cachedMethodInfo[i] == eKey.methodInfo) {
						indexMethodInfo = i;
					}
				}
			}
		}

		EditorGUILayout.BeginVertical();
		EditorGUIUtility.labelWidth = 100f;
		// value
		if(cachedMethodInfo.Count<=0) {
			EditorGUILayout.HelpBox("No usable methods found. Methods should be made public and be placed in scripts that are not directly derived from Component or Behaviour to be used in the Event Track (MonoBehaviour is fine).", MessageType.Info);
			return;
		}
		indexMethodInfo = EditorGUILayout.Popup("Methods:", indexMethodInfo, getMethodNames());
		// if index out of range
		if(indexMethodInfo == -1) return;
		if((indexMethodInfo < cachedMethodInfo.Count)) {
			// process change
			if(eKey.setMethodInfo(cachedMethodInfoComponents[indexMethodInfo],cachedMethodInfo[indexMethodInfo],cachedParameterInfos)) {						// update cache when modifying varaibles
				sTrack.updateCache();
				// save data
				CustomInspector.saveData(sTrack);
				// deselect fields
				GUIUtility.keyboardControl = 0;	
			}
		}
		if(cachedParameterInfos.Length > 1) {
			// if method has more than 1 parameter, set sendmessage to false, and disable toggle
			if(eKey.setUseSendMessage(false)) {
				sTrack.updateCache();
				// save data
				CustomInspector.saveData(sTrack);
			}
			GUI.enabled = false;	// disable sendmessage toggle
		}
		bool showObjectMessage = false;
		Type showObjectType = null;
		foreach(ParameterInfo p in cachedParameterInfos) {
			Type elemType = p.ParameterType.GetElementType();
			if(elemType != null && (elemType.BaseType == typeof(UnityEngine.Object) || elemType.BaseType == typeof(UnityEngine.Behaviour))) {
				showObjectMessage = true;
				showObjectType = elemType;
				break;
			}
		}
		if(showObjectMessage) {
			GUI.color = Color.red;
			GUILayout.Label ("* Use Object[] instead!");
			GUI.color = Color.white;
			if(GUILayout.Button ("?")) {
				EditorUtility.DisplayDialog("Use Object[] Parameter Instead","Array types derived from Object, such as GameObject[], cannot be cast correctly on runtime.\n\nUse UnityEngine.Object[] as a parameter type and then cast to (GameObject[]) in your method.\n\nIf you're trying to pass components"+(showObjectType != typeof(GameObject) ? " (such as "+showObjectType.ToString()+")":"")+", you should get them from the casted GameObjects on runtime.\n\nPlease see the documentation for more information.","Okay");
			}
		}
		if(eKey.setUseSendMessage(EditorGUILayout.Toggle("Use SendMessage", eKey.useSendMessage))) {
			sTrack.updateCache();
			// save data
			CustomInspector.saveData(sTrack);
		}
		GUI.enabled = true;
		if(GUILayout.Button ("?")) {
			EditorUtility.DisplayDialog("SendMessage vs. Invoke","SendMessage can only be used with methods that have no more than one parameter (which can be an array).\n\nAnimator will use Invoke when SendMessage is disabled, which is slightly faster but requires caching when the take is played. Use SendMessage if caching is a problem.","Okay");
		}
		if(cachedParameterInfos.Length > 0) {
//			 show method parameters
			for(int i=0;i<cachedParameterInfos.Length;i++) {
				// show field for each parameter
				if(showFieldFor(i.ToString(),cachedParameterInfos[i].Name,eKey.parameters[i],cachedParameterInfos[i].ParameterType, 0)) {
					sTrack.updateCache();
					CustomInspector.saveData(sTrack);
				}
			}
		}

		EditorGUILayout.EndVertical();
	}

	private static bool showFieldFor(string id, string name, AMEventParameter parameter, Type t, int level) {
		name = name+"("+typeStringBrief(t)+")";
		bool saveChanges = false;
		if(t.IsArray) {
			if(t.GetElementType().IsArray) {
				GUILayout.Label("Multi-dimensional arrays are currently unsupported.");
				return false;
			}
			if(!arrayFieldFoldout.ContainsKey(id)) arrayFieldFoldout.Add(id,true);
			arrayFieldFoldout[id] = EditorGUILayout.Foldout(arrayFieldFoldout[id], name);
			if(arrayFieldFoldout[id]) {
				// show elements if folded out
				if(parameter.lsArray.Count <= 0) {
					AMEventParameter a = ScriptableObject.CreateInstance<AMEventParameter>();
					a.setValueType(t.GetElementType());
					parameter.lsArray.Add(a);
					saveChanges = true;
				}
				for(int i=0; i<parameter.lsArray.Count;i++) {
					if((showFieldFor(id+"_"+i,"("+i.ToString()+")",parameter.lsArray[i],t.GetElementType(),(level+1)))&&!saveChanges) saveChanges = true;
				}
				// add to array button
				GUIStyle styleLabelRight = new GUIStyle(GUI.skin.label);
				styleLabelRight.alignment = TextAnchor.MiddleRight;
				GUILayout.Label(typeStringBrief(t.GetElementType()),styleLabelRight);
				if(parameter.lsArray.Count<=1) GUI.enabled = false;
				if(GUILayout.Button("-")) {
					parameter.lsArray[parameter.lsArray.Count-1].destroy();
					parameter.lsArray.RemoveAt(parameter.lsArray.Count-1);
					saveChanges = true;
				}
				if(GUILayout.Button("+")) {
					AMEventParameter a = ScriptableObject.CreateInstance<AMEventParameter>();
					a.setValueType(t.GetElementType());
					parameter.lsArray.Add(a);
					saveChanges = true;
				}
			}
		}else if(t == typeof(bool)) {
			// int field
			if(parameter.setBool(EditorGUILayout.Toggle(name,parameter.val_bool))) saveChanges = true;
		}else if((t == typeof(int))||(t == typeof(long))) {
			// int field
			if(parameter.setInt(EditorGUILayout.IntField(name,(int)parameter.val_int))) saveChanges = true;
		}else if((t == typeof(float))||(t == typeof(double))) {
			// float field
			if(parameter.setFloat(EditorGUILayout.FloatField(name,(float)parameter.val_float))) saveChanges = true;
		}else if(t == typeof(Vector2)) {
			// vector2 field
			if(parameter.setVector2(EditorGUILayout.Vector2Field(name,(Vector2)parameter.val_vect2))) saveChanges = true;
		}else if(t == typeof(Vector3)) {
			// vector3 field
			if(parameter.setVector3(EditorGUILayout.Vector3Field(name,(Vector3)parameter.val_vect3))) saveChanges = true;
		}else if(t == typeof(Vector4)) {
			// vector4 field
			if(parameter.setVector4(EditorGUILayout.Vector4Field(name,(Vector4)parameter.val_vect4))) saveChanges = true;
		}else if(t == typeof(Color)) {
			// color field
			if(parameter.setColor(EditorGUILayout.ColorField(name,(Color)parameter.val_color))) saveChanges = true;
		}else if(t == typeof(Rect)) {
			// rect field
			if(parameter.setRect(EditorGUILayout.RectField(name,(Rect)parameter.val_rect))) saveChanges = true;
		}else if(t == typeof(string)) {
			// set default
			if(parameter.val_string == null) parameter.val_string = "";
			// string field
			if(parameter.setString(EditorGUILayout.TextField(name,(string)parameter.val_string))) saveChanges = true;
		}else if(t == typeof(char)) {
			// set default
			if(parameter.val_string == null) parameter.val_string = "";
			// char (string) field
			GUILayout.Label(name);
			if(parameter.setString(GUILayout.TextField(parameter.val_string,1))) saveChanges = true;
		}else if(t == typeof(GameObject)) {
			// label
			GUILayout.Label(name);
			// GameObject field
			GUI.skin = null;
			EditorGUIUtility.LookLikeControls();
			if(parameter.setObject(EditorGUILayout.ObjectField(parameter.val_obj,typeof(GameObject),true))) saveChanges = true;
			EditorGUIUtility.LookLikeControls();
		}else if(t.BaseType == typeof(Behaviour) || t.BaseType == typeof(Component)) {
			// label
			GUILayout.Label(name);
			EditorGUIUtility.LookLikeControls();
			// field
			if(t == typeof(Transform)) { if(parameter.setObject(EditorGUILayout.ObjectField(parameter.val_obj,typeof(Transform),true))) saveChanges = true; }
			else if(t == typeof(MeshFilter)) { if(parameter.setObject(EditorGUILayout.ObjectField(parameter.val_obj,typeof(MeshFilter),true))) saveChanges = true; }
			else if(t == typeof(TextMesh)) { if(parameter.setObject(EditorGUILayout.ObjectField(parameter.val_obj,typeof(TextMesh),true))) saveChanges = true; }
			else if(t == typeof(MeshRenderer)) { if(parameter.setObject(EditorGUILayout.ObjectField(parameter.val_obj,typeof(MeshRenderer),true))) saveChanges = true; }
			//else if(t == typeof(ParticleSystem)) { if(parameter.setObject(EditorGUI.ObjectField(parameter.val_obj,typeof(ParticleSystem),true))) saveChanges = true; }
			else if(t == typeof(TrailRenderer)) { if(parameter.setObject(EditorGUILayout.ObjectField(parameter.val_obj,typeof(TrailRenderer),true))) saveChanges = true; }
			else if(t == typeof(LineRenderer)) { if(parameter.setObject(EditorGUILayout.ObjectField(parameter.val_obj,typeof(LineRenderer),true))) saveChanges = true; }
			else if(t == typeof(LensFlare)) { if(parameter.setObject(EditorGUILayout.ObjectField(parameter.val_obj,typeof(LensFlare),true))) saveChanges = true; }
			// halo
			else if(t == typeof(Projector)) { if(parameter.setObject(EditorGUILayout.ObjectField(parameter.val_obj,typeof(Projector),true))) saveChanges = true; }
			else if(t == typeof(Rigidbody)) { if(parameter.setObject(EditorGUILayout.ObjectField(parameter.val_obj,typeof(Rigidbody),true))) saveChanges = true; }
			else if(t == typeof(CharacterController)) { if(parameter.setObject(EditorGUILayout.ObjectField(parameter.val_obj,typeof(CharacterController),true))) saveChanges = true; }
			else if(t == typeof(BoxCollider)) { if(parameter.setObject(EditorGUILayout.ObjectField(parameter.val_obj,typeof(BoxCollider),true))) saveChanges = true; }
			else if(t == typeof(SphereCollider)) { if(parameter.setObject(EditorGUILayout.ObjectField(parameter.val_obj,typeof(SphereCollider),true))) saveChanges = true; }
			else if(t == typeof(CapsuleCollider)) { if(parameter.setObject(EditorGUILayout.ObjectField(parameter.val_obj,typeof(CapsuleCollider),true))) saveChanges = true; }
			else if(t == typeof(MeshCollider)) { if(parameter.setObject(EditorGUILayout.ObjectField(parameter.val_obj,typeof(MeshCollider),true))) saveChanges = true; }
			else if(t == typeof(WheelCollider)) { if(parameter.setObject(EditorGUILayout.ObjectField(parameter.val_obj,typeof(WheelCollider),true))) saveChanges = true; }
			else if(t == typeof(TerrainCollider)) { if(parameter.setObject(EditorGUILayout.ObjectField(parameter.val_obj,typeof(TerrainCollider),true))) saveChanges = true; }
			else if(t == typeof(InteractiveCloth)) { if(parameter.setObject(EditorGUILayout.ObjectField(parameter.val_obj,typeof(InteractiveCloth),true))) saveChanges = true; }
			else if(t == typeof(SkinnedCloth)) { if(parameter.setObject(EditorGUILayout.ObjectField(parameter.val_obj,typeof(SkinnedCloth),true))) saveChanges = true; }
			else if(t == typeof(ClothRenderer)) { if(parameter.setObject(EditorGUILayout.ObjectField(parameter.val_obj,typeof(ClothRenderer),true))) saveChanges = true; }
			else if(t == typeof(HingeJoint)){ if(parameter.setObject(EditorGUILayout.ObjectField(parameter.val_obj,typeof(HingeJoint),true))) saveChanges = true; }
			else if(t == typeof(FixedJoint)) { if(parameter.setObject(EditorGUILayout.ObjectField(parameter.val_obj,typeof(FixedJoint),true))) saveChanges = true; }
			else if(t == typeof(SpringJoint)) { if(parameter.setObject(EditorGUILayout.ObjectField(parameter.val_obj,typeof(SpringJoint),true))) saveChanges = true; }
			else if(t == typeof(CharacterJoint)) { if(parameter.setObject(EditorGUILayout.ObjectField(parameter.val_obj,typeof(CharacterJoint),true))) saveChanges = true; }
			else if(t == typeof(ConfigurableJoint)) { if(parameter.setObject(EditorGUILayout.ObjectField(parameter.val_obj,typeof(ConfigurableJoint),true))) saveChanges = true; }
			else if(t == typeof(ConstantForce)) { if(parameter.setObject(EditorGUILayout.ObjectField(parameter.val_obj,typeof(ConstantForce),true))) saveChanges = true; }
			//else if(t == typeof(NavMeshAgent)) { if(parameter.setObject(EditorGUI.ObjectField(rectObjectField,parameter.val_obj,typeof(NavMeshAgent),true))) saveChanges = true; }
			//else if(t == typeof(OffMeshLink)) { if(parameter.setObject(EditorGUI.ObjectField(rectObjectField,parameter.val_obj,typeof(OffMeshLink),true))) saveChanges = true; }
			else if(t == typeof(AudioListener)) { if(parameter.setObject(EditorGUILayout.ObjectField(parameter.val_obj,typeof(AudioListener),true))) saveChanges = true; }
			else if(t == typeof(AudioSource)) { if(parameter.setObject(EditorGUILayout.ObjectField(parameter.val_obj,typeof(AudioSource),true))) saveChanges = true; }
			else if(t == typeof(AudioReverbZone)) { if(parameter.setObject(EditorGUILayout.ObjectField(parameter.val_obj,typeof(AudioReverbZone),true))) saveChanges = true; }
			else if(t == typeof(AudioLowPassFilter)) { if(parameter.setObject(EditorGUILayout.ObjectField(parameter.val_obj,typeof(AudioLowPassFilter),true))) saveChanges = true; }
			else if(t == typeof(AudioHighPassFilter)) { if(parameter.setObject(EditorGUILayout.ObjectField(parameter.val_obj,typeof(AudioHighPassFilter),true))) saveChanges = true; }
			else if(t == typeof(AudioEchoFilter)) { if(parameter.setObject(EditorGUILayout.ObjectField(parameter.val_obj,typeof(AudioEchoFilter),true))) saveChanges = true; }
			else if(t == typeof(AudioDistortionFilter)) { if(parameter.setObject(EditorGUILayout.ObjectField(parameter.val_obj,typeof(AudioDistortionFilter),true))) saveChanges = true; }
			else if(t == typeof(AudioReverbFilter)) { if(parameter.setObject(EditorGUILayout.ObjectField(parameter.val_obj,typeof(AudioReverbFilter),true))) saveChanges = true; }
			else if(t == typeof(AudioChorusFilter)) { if(parameter.setObject(EditorGUILayout.ObjectField(parameter.val_obj,typeof(AudioChorusFilter),true))) saveChanges = true; }
			else if(t == typeof(Camera)) { if(parameter.setObject(EditorGUILayout.ObjectField(parameter.val_obj,typeof(Camera),true))) saveChanges = true; }
			else if(t == typeof(Skybox)) { if(parameter.setObject(EditorGUILayout.ObjectField(parameter.val_obj,typeof(Skybox),true))) saveChanges = true; }
			// flare layer
			else if(t == typeof(GUILayer)) { if(parameter.setObject(EditorGUILayout.ObjectField(parameter.val_obj,typeof(GUILayer),true))) saveChanges = true; }
			else if(t == typeof(Light)) { if(parameter.setObject(EditorGUILayout.ObjectField(parameter.val_obj,typeof(Light),true))) saveChanges = true; }
			//else if(t == typeof(LightProbeGroup)) { if(parameter.setObject(EditorGUI.ObjectField(rectObjectField,parameter.val_obj,typeof(LightProbeGroup),true))) saveChanges = true; }
			else if(t == typeof(OcclusionArea)) { if(parameter.setObject(EditorGUILayout.ObjectField(parameter.val_obj,typeof(OcclusionArea),true))) saveChanges = true; }
			//else if(t == typeof(OcclusionPortal)) { if(parameter.setObject(EditorGUI.ObjectField(rectObjectField,parameter.val_obj,typeof(OcclusionPortal),true))) saveChanges = true; }
			//else if(t == typeof(LODGroup)) { if(parameter.setObject(EditorGUI.ObjectField(rectObjectField,parameter.val_obj,typeof(LODGroup),true))) saveChanges = true; }
			else if(t == typeof(GUITexture)) { if(parameter.setObject(EditorGUILayout.ObjectField(parameter.val_obj,typeof(GUITexture),true))) saveChanges = true; }
			else if(t == typeof(GUIText)) { if(parameter.setObject(EditorGUILayout.ObjectField(parameter.val_obj,typeof(GUIText),true))) saveChanges = true; }
			else if(t == typeof(Animation)) { if(parameter.setObject(EditorGUILayout.ObjectField(parameter.val_obj,typeof(Animation),true))) saveChanges = true; }
			else if(t == typeof(NetworkView)) { if(parameter.setObject(EditorGUILayout.ObjectField(parameter.val_obj,typeof(NetworkView),true))) saveChanges = true; }
			// wind zone
			else {
				
				if(t.BaseType == typeof(Behaviour))
				{ if(parameter.setObject(EditorGUILayout.ObjectField(parameter.val_obj,typeof(Behaviour),true))) saveChanges = true; }
				else { if(parameter.setObject(EditorGUILayout.ObjectField(parameter.val_obj,typeof(Component),true))) saveChanges = true; }
				
			}
			//return;
		}else if(t == typeof(UnityEngine.Object)) {
			GUILayout.Label(name);
			if(parameter.setObject(EditorGUILayout.ObjectField(parameter.val_obj,typeof(UnityEngine.Object),true))) saveChanges = true;
		} else if(t == typeof(Component)) {
			GUILayout.Label(name);
			if(parameter.setObject(EditorGUILayout.ObjectField(parameter.val_obj,typeof(Component),true))) saveChanges = true;
		}else {
			GUILayout.Label("Unsupported parameter type "+t.ToString()+".");
		}
		return saveChanges;
	}
}
