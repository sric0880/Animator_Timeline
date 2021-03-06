using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using System.Linq;

[System.Serializable]
public class AMCameraSwitcherTrack : AMTrack {
	
	private Camera[] _cachedAllCameras;
	public Camera[] cachedAllCameras{
		get {
			if(_cachedAllCameras == null) {
				_cachedAllCameras = getAllCameras();	
			}
			return _cachedAllCameras;
		}
		set {
			_cachedAllCameras = value;
		}
	}
	
	public override string getTrackType() {
		return "Camera Switcher";	
	}
	
	// add a new key
	public void addKey(int _frame, Camera camera = null,/*int? type = null, Camera camera = null, Color? color = null,*/ AMCameraSwitcherKey keyToClone = null) {
		foreach(AMCameraSwitcherKey key in keys) {
			// if key exists on frame, update key
			if(key.frame == _frame) {
				if(camera != null) {
					key.camera = camera;
					key.type = 0;
					updateCache();
				}
				return;
			}
		}
		AMCameraSwitcherKey a = ScriptableObject.CreateInstance<AMCameraSwitcherKey>();
		if(keyToClone) {
			a = (AMCameraSwitcherKey) keyToClone.CreateClone();
		} else {
			a.type = 0;
			a.still = !AMTake.isProLicense;
			a.easeType = (int)AMTween.EaseType.easeOutSine;
		}
		a.frame = _frame;
		if(camera != null) {
			a.camera = camera;
			a.type = 0;
		}
		// add a new key
		keys.Add (a);
		// update cache
		updateCache();
	}
	
	public override void updateCache() {
		// destroy cache
		destroyCache();
		// create new cache
		cache = new List<AMAction>();
		// sort keys
		sortKeys();
		for(int i=0;i<keys.Count;i++) {
			// create new action and add it to cache list
			AMCameraSwitcherAction a = ScriptableObject.CreateInstance<AMCameraSwitcherAction> ();
			a.startFrame = keys[i].frame;
			if(keys.Count>(i+1)) a.endFrame = keys[i+1].frame;
			else a.endFrame = -1;
			// targets
			a.startTargetType = (keys[i] as AMCameraSwitcherKey).type;
			if(a.startTargetType == 0) a.startCamera = (keys[i] as AMCameraSwitcherKey).camera;
			else a.startColor = (keys[i] as AMCameraSwitcherKey).color;
			
			if(a.endFrame!=-1) {
				a.endTargetType = (keys[i+1] as AMCameraSwitcherKey).type;
				if(a.endTargetType == 0) a.endCamera = (keys[i+1] as AMCameraSwitcherKey).camera;
				else a.endColor = (keys[i+1] as AMCameraSwitcherKey).color;
			}
			a.cameraFadeType = (keys[i] as AMCameraSwitcherKey).cameraFadeType;
			a.cameraFadeParameters = new List<float>((keys[i] as AMCameraSwitcherKey).cameraFadeParameters);
			a.irisShape = (keys[i] as AMCameraSwitcherKey).irisShape;
			a.still = (keys[i] as AMCameraSwitcherKey).still;
			a.easeType = (keys[i] as AMCameraSwitcherKey).easeType;
			a.customEase = new List<float>(keys[i].customEase);
			// add to cache
			cache.Add (a);
		}
		// update all cameras
		_cachedAllCameras = getAllCameras();
		base.updateCache();
	}
	
	public override void previewFrame(float frame) {
		
		if(cache == null || cache.Count <= 0) {
			return;
		}
		
		bool isPreview = !Application.isPlaying;

		for(int i=0;i<cache.Count;i++) {
			// before first frame
			if(frame <= (cache[i] as AMCameraSwitcherAction).startFrame) {
				AMCameraFade.reset();
				if(!(cache[i] as AMCameraSwitcherAction).hasStartTarget()) return;
				
				if((cache[i] as AMCameraSwitcherAction).startTargetType == 0) {
					AMTween.SetTopCamera((cache[i] as AMCameraSwitcherAction).startCamera,cachedAllCameras);
				} else {
					showColor((cache[i] as AMCameraSwitcherAction).startColor,isPreview);
					// or color # TO DO #
				}
				return;
			// between first and last frame
			} else if(frame <= (cache[i] as AMCameraSwitcherAction).endFrame) {
				
				if(!(cache[i] as AMCameraSwitcherAction).hasStartTarget() || !(cache[i] as AMCameraSwitcherAction).hasEndTarget()) return;
				// targets are equal
				if((cache[i] as AMCameraSwitcherAction).targetsAreEqual()) {
					AMCameraFade.reset();
					if((cache[i] as AMCameraSwitcherAction).startTargetType == 0) {
						AMTween.SetTopCamera((cache[i] as AMCameraSwitcherAction).startCamera,cachedAllCameras);
					} else {
						showColor((cache[i] as AMCameraSwitcherAction).startColor,isPreview);
						// or color # TO DO #
					}
				} else {
					AMCameraFade.clearRenderTexture();
					//Very important core code!!
					previewCameraFade(frame, (cache[i] as AMCameraSwitcherAction), isPreview);
				}
				return;
			// after last frame
			} else if(i == cache.Count-2) {
				AMCameraFade.reset();
				if(!(cache[i] as AMCameraSwitcherAction).hasEndTarget()) return;
				// use camera (cache[i] as AMCameraSwitcherAction) endTarget
				if((cache[i] as AMCameraSwitcherAction).endTargetType == 0) {
					AMTween.SetTopCamera((cache[i] as AMCameraSwitcherAction).endCamera,cachedAllCameras);
				} else {
					showColor((cache[i] as AMCameraSwitcherAction).endColor,isPreview);
					// or color # TO DO #
				}
				return;
			}
		}
	}
	
	public Camera[] getAllCameras() {
		List<Camera> lsCameras = new List<Camera>();
		foreach(AMCameraSwitcherKey key in keys) {
			if(key.type == 0 && key.camera) lsCameras.Add(key.camera);
		}
		return lsCameras.Distinct().ToArray();
	}
	
	public Texture[] getAllTextures() {
		List<Texture> lsTextures = new List<Texture>();
		foreach(AMCameraSwitcherKey key in keys) {
			if(key.irisShape && (AMCameraFade.needsTexture(key.cameraFadeType))) lsTextures.Add(key.irisShape);
		}
		return lsTextures.Distinct().ToArray();
	}
	
	private void previewCameraFade(float frame, AMCameraSwitcherAction action, bool isPreview) {
		// if transition is None, show end camera / color
		if(action.cameraFadeType == (int)AMTween.Fade.None) {
			// reset camera fade if visible
			// camera
			if(action.endTargetType == 0) {
				if(action.endCamera) AMTween.SetTopCamera(action.endCamera, cachedAllCameras);	
				AMCameraFade.reset();
			} else {
				showColor(action.endColor,isPreview);
			}
			return;
		}
		// Get camerafade
		AMCameraFade cf = AMCameraFade.getCameraFade(isPreview);
		if(Application.isPlaying) cf.keepAlivePreview = true;
		cf.isReset = false;
		bool isReversed = action.isReversed();
		int firstTargetType = (isReversed ? action.endTargetType : action.startTargetType);
		int secondTargetType = (isReversed ? action.startTargetType : action.endTargetType);
		// Set render texture or colors if render texture is used
		setRenderTexture(cf, frame, firstTargetType, secondTargetType, isReversed, action, isPreview);
		setColors(cf, firstTargetType, secondTargetType, isReversed, action);
		
		if(cf.irisShape != action.irisShape) cf.irisShape = action.irisShape;
		cf.mode = action.cameraFadeType;
		cf.setupMaterials();
		cf.r = action.cameraFadeParameters.ToArray();
		
		// calculate and set value
		AMTween.EasingFunction ease;
		AnimationCurve curve = null;
		
		if(action.hasCustomEase()) {
			ease = AMTween.customEase;
			curve = action.easeCurve;
		} else {
			ease = AMTween.GetEasingFunction((AMTween.EaseType)action.easeType);
		}
		float percentage = (float)(frame-action.startFrame)/(float)(action.endFrame-action.startFrame);
		float value = ease(1f,0f,percentage,curve);
		cf.value = value;
		cf.percent = percentage;
		
	}
	private void setColors(AMCameraFade cf, int firstTargetType, int secondTargetType, bool isReversed, AMCameraSwitcherAction action) {
		//if(firstTargetType != 1 && secondTargetType != 1) return;
		Color firstColor = (isReversed ? action.endColor : action.startColor);
		Color secondColor = (isReversed ? action.startColor : action.endColor);
		
		if(firstTargetType == 1) {
			cf.colorTex = firstColor;
			cf.hasColorTex = true;	
		} else {
			cf.hasColorTex = false;	
		}
		
		if(secondTargetType == 1) {
			cf.colorBG = secondColor;
			cf.hasColorBG = true;
		} else {
			cf.hasColorBG = false;
		}
		
		// send event to game view to repaint OnGUI
		if(!Application.isPlaying && (firstTargetType == 1 || secondTargetType == 1)) {
			cf.transform.position = new Vector3(cf.transform.position.x,cf.transform.position.y,cf.transform.position.z);
		}
	}
	// set render texture or colors if render texture is used (stills handled in AMTake)
	private void setRenderTexture(AMCameraFade cf, float frame, int firstTargetType, int secondTargetType, bool isReversed, AMCameraSwitcherAction action, bool isPreview) {
		
		
		Camera firstCamera = (isReversed ? action.endCamera : action.startCamera);
		Camera secondCamera = (isReversed ? action.startCamera : action.endCamera);
		
		if(isReversed && frame == action.startFrame) {
			if(firstTargetType == 0) AMTween.SetTopCamera(firstCamera,cachedAllCameras);
		} else {
			if(secondTargetType == 0) AMTween.SetTopCamera(secondCamera,cachedAllCameras);
		}
		
		if(action.still || (firstTargetType != 0 && secondTargetType != 0)) return;
		
		bool isPro = AMTake.isProLicense;
		// first target is camera, set render texture
		if(firstTargetType == 0) {
			// if should update render texture
			if(/*!cf.tex ||*/ cf.shouldUpdateRenderTexture || (isPro && (!firstCamera.targetTexture || !cf.isRenderTextureSetupFor(firstCamera)/*|| firstCamera.targetTexture != cf.tex*/))) {
				if(isPro) {
					cf.setupRenderTexture(firstCamera);
					
				} else {
					cf.useRenderTexture = false;
					// show place-holder if non-pro
					cf.colorTex = Color.white;
					cf.tex2d = (Texture2D)Resources.Load("am_indie_placeholder");
					cf.hasColorTex = false;
					if(!cf.placeholder) cf.placeholder = true;
				}
			}
		}
	}
	
	private void showColor(Color color, bool isPreview) {
		AMCameraFade cf = AMCameraFade.getCameraFade(isPreview);
		bool shouldRepaint = false;
		if(!cf.hasColorTex || cf.colorTex != color) {
			cf.colorTex = color;
			cf.hasColorTex = true;
			shouldRepaint = true;
		}
		if(cf.isReset) {
			cf.isReset = false;	
			shouldRepaint = true;
		}
		if(cf.hasColorBG) {
			cf.hasColorBG = false;
			shouldRepaint = true;
		}
		if(cf.value != 1f) {
			cf.value = 1f;
			cf.percent = 0f;
			shouldRepaint = true;
		}
		if(cf.mode != 0) {
			cf.mode = 0;
			shouldRepaint = true;
		}
		// send event to game view to repaint OnGUI
		if(!Application.isPlaying && shouldRepaint) cf.transform.position = new Vector3(cf.transform.position.x,cf.transform.position.y,cf.transform.position.z);
	}
}
