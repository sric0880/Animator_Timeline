using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

public class AMCameraSwitcherAction : AMAction {
	
	public int endFrame;
	public int cameraFadeType;
	public List<float> cameraFadeParameters;
	public Texture2D irisShape;
	public bool still;
	public int startTargetType;	// 0 = camera, 1 = color
	public int endTargetType;	// 0 = camera, 1 = color
	public Camera startCamera;
	public Camera endCamera;
	public Color startColor;
	public Color endColor;

	public override int NumberOfFrames {
		get {
			return endFrame-startFrame; 
		}
	}
	
	public string getParametersString(int codeLanguage) {
		string s = "";
		s += (codeLanguage == 0 ? "new float[]{" : "[");
		for(int i=0;i<cameraFadeParameters.Count;i++) {
			s += cameraFadeParameters[i].ToString();
			if(codeLanguage == 0) s+= "f";
			if(i<=cameraFadeParameters.Count-2) s+= ", ";
		}
		s += (codeLanguage == 0 ? "}" : "]");
		return s;
	}

	public float getTime(int frameRate) {
		return (float)NumberOfFrames/(float)frameRate;	
	}
	public bool hasTargets() {
		if(hasStartTarget() && hasEndTarget()) return true;
		return false;
	}
	public bool hasStartTarget() {
		if(startTargetType == 0 && !startCamera) return false;
		//else if(!startColor) return false;
		return true;
	}
	public bool hasEndTarget() {
		if(endFrame == -1 ||(endTargetType == 0 && !endCamera)) return false;
		//else if(!endColor) return false;
		return true;
	}
	public bool targetsAreEqual() {
		if(startTargetType != endTargetType) return false;
		if(startTargetType == 0 && startCamera != endCamera) return false;
		else if(startTargetType == 1 && startColor != endColor) return false;
		return true;
	}
	public string getStartTargetName() {
		if(startTargetType == 0)
			if(startCamera) return startCamera.gameObject.name;
			else return "None";
		else
			return "Color";
	}
	public string getEndTargetName() {
		if(endTargetType == 0)
			if(endCamera) return endCamera.gameObject.name;
			else return "None";
		else
			return "Color";
	}
	
	public bool isReversed() {
		return AMTween.isTransitionReversed(cameraFadeType,cameraFadeParameters.ToArray());	
	}
}
