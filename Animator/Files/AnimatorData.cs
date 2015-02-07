using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System.Linq;

[System.Serializable]
public class AnimatorData : MonoBehaviour {
	// hide
	[HideInInspector] public bool isPlaying {
		get {
			if(nowPlayingTake != null && !isPaused) return true;
			return false;
		}
	}
	[HideInInspector] public bool isAnimatorOpen = false;
	[HideInInspector] public bool inPlayMode = false;
	[HideInInspector] public float zoom = 0.4f;
	[HideInInspector] public float elapsedTime = 0f;

	[HideInInspector]
	[SerializeField]
	private AMTake currentTake;

	// private
	private AMTake nowPlayingTake = null;

	private bool isPaused = false;
	private bool isLooping = false;
	private float takeTime = 0f;
	
//	public object Invoker(object[] args) {
//		switch((int)args[0]) {
//			// check if is playing
//			case 0:
//				return isPlaying;
//			// get take name
//			case 1:
////				return takeName;
//				return "";
//			// play
//			case 2:
//				Play(true,0f,(bool)args[1]);
//				break;
//			// stop
//			case 3:
//				StopLoop();
//				break;
//			// pause
//			case 4:
//				PauseLoop();
//				break;
//			// resume
//			case 5:
//				ResumeLoop();
//				break;
//			// play from time
//			case 6:
//				Play(false,(float)args[1],(bool)args[2]);
//				break;
//			// play from frame
//			case 7:
//				Play(true,(float)((int)args[1]),(bool)args[2]);
//				break;
//			// preview frame
//			case 8:
//				PreviewValue(true,(float)args[1]);
//				break;
//			// preview time
//			case 9:
//				PreviewValue(false,(float)args[1]);
//				break;
//			// running time
//			case 10:
//				if(nowPlayingTake == null) return 0f;
//				else return elapsedTime;
//			// total time
//			case 11:
//				if(nowPlayingTake == null) return 0f;
//				else return (float) nowPlayingTake.numFrames / (float) nowPlayingTake.frameRate;
//			case 12:
//				if(nowPlayingTake == null) return false;
//				return isPaused;
//			default:
//				break;
//		}
//		return null;
//	}
	
	void OnDrawGizmos() {
		if(!isAnimatorOpen) return;
		if(currentTake) currentTake.drawGizmos(0.1f, inPlayMode);
	}
	
	void Update() {
		if(isPaused || nowPlayingTake == null) return;
		elapsedTime += Time.deltaTime;
		if(elapsedTime >= takeTime) {
			nowPlayingTake.stopAudio();
			if(isLooping) Execute(nowPlayingTake);
			else nowPlayingTake = null;
		}
	}
		
	public void Play(bool isFrame, float value, bool loop) {
		nowPlayingTake = currentTake;
		if(nowPlayingTake) {
			isLooping = loop;
			Execute (nowPlayingTake, isFrame, value);
		}
	}
	
	public void PreviewValue(bool isFrame, float value) {
		if(!currentTake) return;
		float startFrame = value;
		if(!isFrame) startFrame *= currentTake.frameRate;	// convert time to frame
		currentTake.previewFrameInvoker(startFrame);
	}
	
	public void Execute(AMTake take, bool isFrame = true, float value = 0f /* frame or time */) {
		if(nowPlayingTake != null)
			AMTween.Stop();
		// delete AMCameraFade
		float startFrame = value;
		float startTime = value;
		if(!isFrame) startFrame *= take.frameRate;	// convert time to frame
		if(isFrame) startTime /= take.frameRate;	// convert frame to time
		take.executeActions(startFrame);
		elapsedTime = startTime;
		takeTime = (float)take.numFrames/(float)take.frameRate;
		nowPlayingTake = take;
		
	}
	
	public void PauseLoop() {
		if(nowPlayingTake == null) return;
		isPaused = true;
		nowPlayingTake.stopAudio();
		AMTween.Pause();
		
	}
	
	public void ResumeLoop() {
		if(nowPlayingTake == null) return;
		AMTween.Resume();	
		isPaused = false;
	}
	
	public void StopLoop() {
		if(nowPlayingTake == null) return;
		nowPlayingTake.stopAudio();
		nowPlayingTake.stopAnimations();
		nowPlayingTake = null;
		isLooping = false;
		isPaused = false;
		AMTween.Stop();
	}

	public void addTake() {
		currentTake = ScriptableObject.CreateInstance<AMTake>();
		// set defaults
		currentTake.frameRate = 24;
		currentTake.numFrames = 1440;
		currentTake.startFrame = 1;
		currentTake.selectedFrame = 1;
		currentTake.selectedTrack = -1;
		currentTake.playbackSpeedIndex = 2;
		currentTake.trackKeys = new List<int>();
		currentTake.trackValues = new List<AMTrack>();
		currentTake.contextSelectionTracks = new List<int>();
	}
	
	public AMTake getCurrentTake() {
		return currentTake;	
	}

	public List<GameObject> getDependencies(AMTake _take = null)
	{
		if(_take != null) return _take.getDependencies().ToList();
		else return null;
	}
	
	public List<GameObject> updateDependencies(List<GameObject> newReferences, List<GameObject> oldReferences) {
		if(currentTake) return currentTake.updateDependencies(newReferences,oldReferences);
		else return null;
	}
	
}
