using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System;
[System.Serializable]
public class AMPropertyAction : AMAction {
	
	public int valueType;
	
	public Component component;
	public int endFrame;
	public string propertyName;
	public string fieldName;
	public string methodName;
	public string[] methodParameterTypes;
	private MethodInfo cachedMethodInfo;
	private PropertyInfo cachedPropertyInfo;
	private FieldInfo cachedFieldInfo;
	public PropertyInfo propertyInfo{
		get {
			if(cachedPropertyInfo != null) return cachedPropertyInfo;
			if(!component || propertyName == null) return null;
			cachedPropertyInfo = component.GetType().GetProperty(propertyName);
			return cachedPropertyInfo;
		}
		set {
			if(value != null) propertyName = value.Name;
			else propertyName = null;
			cachedPropertyInfo = value;
			
		}
	}
	public FieldInfo fieldInfo{
		get {
			if(cachedFieldInfo != null) return cachedFieldInfo;
			if(!component || fieldName == null) return null;
			cachedFieldInfo = component.GetType().GetField(fieldName);
			return cachedFieldInfo;
		}
		set {
			if(value != null) fieldName = value.Name;
			else fieldName = null;
			cachedFieldInfo = value;
		}
	}		// holds a field such as variables for user scripts, should be null if property is used
	public MethodInfo methodInfo{
		get {
			if(cachedMethodInfo != null) return cachedMethodInfo;
			if(!component || methodName == null) return null;
			Type[] t = new Type[methodParameterTypes.Length];
			for(int i=0;i<methodParameterTypes.Length;i++) t[i] = Type.GetType(methodParameterTypes[i]);
			cachedMethodInfo = component.GetType().GetMethod(methodName,t);
			return cachedMethodInfo;
		}
		set {
			if(value != null) methodName = value.Name;
			else methodName = null;
			cachedMethodInfo = value;
		}
	}
	public double start_val;		// value as double (includes int/long)
	public Vector2 start_vect2;
	public Vector3 start_vect3;
	public Color start_color;
	public Rect start_rect;
	
	public double end_val;			// value as double (includes int/long)
	public Vector2 end_vect2;
	public Vector3 end_vect3;
	public Color end_color;
	public Rect end_rect;
	
	public List<float> start_morph;
	public List<float> end_morph;
	
	public override int NumberOfFrames {
		get {
			return endFrame-startFrame;
		}
	}
	public float getTime(int frameRate) {
		return (float)NumberOfFrames/(float)frameRate;	
	}

	public string getName() {
		if(fieldInfo != null) return fieldInfo.Name;
		else if(propertyInfo != null) return propertyInfo.Name;
		else if(methodInfo != null) {
			if(valueType == (int)AMPropertyTrack.ValueType.MorphChannels) return "Morph";
		}
		return "Unknown";
	}

	public string getFloatArrayString(int codeLanguage, List<float> ls) {
		string s = "";	
		if(codeLanguage == 0) s += "new float[]{";
		else s+= "[";
		for(int i=0;i<ls.Count;i++) {
			s += ls[i].ToString();
			if(codeLanguage == 0) s += "f";
			if(i<ls.Count-1) s+= ", ";
		}
		if(codeLanguage == 0) s+= "}";
		else s += "]";
		return s;
	}
	public string getValueString(bool brief) {
		string s = "";
		if(AMPropertyTrack.isValueTypeNumeric(valueType)) {
			//s+= start_val.ToString();
			s += formatNumeric(start_val);
			if(!brief && endFrame != -1) s += " -> "+formatNumeric(end_val);
			//if(!brief && endFrame != -1) s += " -> "+end_val.ToString();
		} else if(valueType == (int)AMPropertyTrack.ValueType.Vector2) {
			s+= start_vect2.ToString();
			if(!brief && endFrame != -1) s += " -> "+end_vect2.ToString(); 
		} else if(valueType == (int)AMPropertyTrack.ValueType.Vector3) {
			s+= start_vect3.ToString();
			if(!brief && endFrame != -1) s += " -> "+end_vect3.ToString();
		} else if(valueType == (int)AMPropertyTrack.ValueType.Color) {
			//return null; 
			s+= start_color.ToString();
			if(!brief && endFrame != -1) s += " -> "+end_color.ToString();
		} else if(valueType == (int)AMPropertyTrack.ValueType.Rect) {
			//return null; 
			s+= start_rect.ToString();
			if(!brief && endFrame != -1) s+= " -> "+end_rect.ToString();
		}
		return s;
	}
	// use for floats
	private string formatNumeric(float input) {
		double _input = (input < 0f ? input*-1f : input);
		if(_input < 1f) {
			if(_input >= 0.01f) return input.ToString("N3");
			else if (_input >= 0.001f) return input.ToString("N4");
			else if (_input >= 0.0001f) return input.ToString("N5");
			else if (_input >= 0.00001f) return input.ToString("N6");
			else return input.ToString();
		}
		return input.ToString("N2");
	}
	// use for doubles
	private string formatNumeric(double input) {
		double _input = (input < 0d ? input*-1d : input);
		if(_input < 1d) {
			if(_input >= 0.01d) return input.ToString("N3");
			else if (_input >= 0.001d) return input.ToString("N4");
			else if (_input >= 0.0001d) return input.ToString("N5");
			else if (_input >= 0.00001d) return input.ToString("N6");
			else return input.ToString();
		}
		return input.ToString("N2");
	}
	
	public int getStartMorphNameIndex(int numChannels) {
		return getMorphNameIndex(start_morph, numChannels);
	}
	
	public int getEndMorphNameIndex(int numChannels) {
		return getMorphNameIndex(end_morph, numChannels);
	}
	
	private int getMorphNameIndex(List<float> morph, int count) {
		int index = -1;
		bool allZeroes = true;
		if(morph.Count < count) count = morph.Count;
		for(int i=0;i<count;i++) {
			if(allZeroes && morph[i] != 0f) allZeroes = false;
			if(morph[i] > 0f && morph[i] < 100f) {
				index = -1;
				break;
			}
			if(morph[i] == 100f) {
				if(index != -1) {
					index = -1;
					break;
				}
				index = i;
			}
		}
		if(allZeroes) index = -2;
		return index;
	}
	
	public bool targetsAreEqual() {
		if(valueType == (int)AMPropertyTrack.ValueType.Integer || valueType == (int)AMPropertyTrack.ValueType.Long || valueType == (int)AMPropertyTrack.ValueType.Float || valueType == (int)AMPropertyTrack.ValueType.Double)
			return start_val == end_val;
		if(valueType == (int)AMPropertyTrack.ValueType.Vector2) return (start_vect2 == end_vect2); 
		if(valueType == (int)AMPropertyTrack.ValueType.Vector3) return (start_vect3 == end_vect3);
		if(valueType == (int)AMPropertyTrack.ValueType.Color) return (start_color == end_color); //return start_color.ToString()+" -> "+end_color.ToString();
		if(valueType == (int)AMPropertyTrack.ValueType.Rect) return (start_rect == end_rect); //return start_rect.ToString()+" -> "+end_rect.ToString();
		if(valueType == (int)AMPropertyTrack.ValueType.MorphChannels) {
			if(start_morph == null || end_morph == null) return false;
			for(int i=0;i<start_morph.Count;i++) {
				if(end_morph.Count <= i || start_morph[i] != end_morph[i]) return false;
			}
			return true;
		}
		Debug.LogError("Animator: Invalid ValueType "+valueType);
		return false;
	}
	
	public object getStartValue() {
		if(valueType == (int)AMPropertyTrack.ValueType.Integer) return Convert.ToInt32(start_val);
		if(valueType == (int)AMPropertyTrack.ValueType.Long) return Convert.ToInt64(start_val);
		if(valueType == (int)AMPropertyTrack.ValueType.Float) return Convert.ToSingle(start_val);
		if(valueType == (int)AMPropertyTrack.ValueType.Double) return start_val;
		if(valueType == (int)AMPropertyTrack.ValueType.Vector2) return start_vect2; 
		if(valueType == (int)AMPropertyTrack.ValueType.Vector3) return start_vect3;
		if(valueType == (int)AMPropertyTrack.ValueType.Color) return start_color; //return start_color.ToString()+" -> "+end_color.ToString();
		if(valueType == (int)AMPropertyTrack.ValueType.Rect) return start_rect; //return start_rect.ToString()+" -> "+end_rect.ToString();
		return "Unknown";
	}
	public object getEndValue() {
		if(valueType == (int)AMPropertyTrack.ValueType.Integer) return Convert.ToInt32(end_val);
		if(valueType == (int)AMPropertyTrack.ValueType.Long) return Convert.ToInt64(end_val);
		if(valueType == (int)AMPropertyTrack.ValueType.Float) return Convert.ToSingle(end_val);
		if(valueType == (int)AMPropertyTrack.ValueType.Double) return end_val;
		if(valueType == (int)AMPropertyTrack.ValueType.Vector2) return end_vect2; 
		if(valueType == (int)AMPropertyTrack.ValueType.Vector3) return end_vect3;
		if(valueType == (int)AMPropertyTrack.ValueType.Color) return end_color; //return start_color.ToString()+" -> "+end_color.ToString();
		if(valueType == (int)AMPropertyTrack.ValueType.Rect) return end_rect; //return start_rect.ToString()+" -> "+end_rect.ToString();
		return "Unknown";
	}
}
