using UnityEngine;
//using UnityEditor;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

[System.Serializable]

public class AMTake :ScriptableObject {
	
	#region Declarations
	public float startFrame = 1f;				// first frame to render
	public float endFrame = 100f;

	public int selectedTrack = -1;			// currently selected track index
	public int selectedFrame = 1;			// currently selected frame (frame to preview, not necessarily in context selection)
//	public int selectedGroup = 0;
	
//	public List<int>trackKeys = new List<int>();
	public List<AMTrack>trackValues = new List<AMTrack>();
	
	public List<int> contextSelection = new List<int>();	// list of all frames included in the context selection
	public List<int> ghostSelection = new List<int>();		// list of all frames included in the ghost selection
	public List<int> contextSelectionTracks = new List<int>();
	
	public int track_count = 1;	// number of tracks created, used to generate unique track ids
//	public int group_count = 0;		// number of groups created, used to generate unique group ids. negative number to differentiate from positive track ids
	
//	public AMGroup rootGroup;
//	public List<int>groupKeys = new List<int>();
//	public List<AMGroup>groupValues = new List<AMGroup>();
	
	public static bool isProLicense = true;
	public AMCameraSwitcherTrack cameraSwitcher;
		
	#endregion
	
	// Adding a new track type
	// =====================
	// Create track class
	// Make sure to add [System.Serializable] to every class
	// Set track class properties
	// Override getTrackType in track class
	// Add track type to showObjectFieldFor in AMTimeline
	// Create an addXTrack method here, and put it in AMTimeline
	// Create track key class, make sure to override CreateClone 
	// Create AMXAction class for track class that overrides execute and implements ToString # TO DO #
	// Override updateCache in track class
	// Create addKey method in track class, and put it in addKey in AMTimeline
	// Add track to timeline action in AMTimeline
	// Add inspector properties to showInspectorPropertiesFor in AMTimeline
	// Override previewFrame method in track class
	// Add track object to timelineSelectObjectFor in AMTimeline (optional)
	// Override getDependencies and updateDependencies in track class
	// Add details to Code View # TO DO #

	
	#region Tracks
	// add translation track
	public void addTranslationTrack() {
		AMTranslationTrack a = ScriptableObject.CreateInstance<AMTranslationTrack>();
		a.setName (getTrackCount());
		a.id = getUniqueTrackID();
		addTrack(a);
		
	}
	
	// add rotation track
	public void addRotationTrack() {
		AMRotationTrack a = ScriptableObject.CreateInstance<AMRotationTrack>();	
		a.setName (getTrackCount());
		a.id = getUniqueTrackID();
		addTrack(a);
	}
	
	// add orientation track
	public void addOrientationTrack() {
		AMOrientationTrack a = ScriptableObject.CreateInstance<AMOrientationTrack>();	
		a.setName (getTrackCount());
		a.id = getUniqueTrackID();
		addTrack(a);
	}
	
	// add animation track
	public void addAnimationTrack() {
		AMAnimationTrack a = ScriptableObject.CreateInstance<AMAnimationTrack>();
		a.setName(getTrackCount());
		a.id = getUniqueTrackID();
		addTrack(a);
	}
	
	// add audio track
	public void addAudioTrack() {
		AMAudioTrack a = ScriptableObject.CreateInstance<AMAudioTrack>();
		a.setName(getTrackCount());
		a.id = getUniqueTrackID();
		addTrack(a);
	}
	
	// add property track
	public void addPropertyTrack() {
		AMPropertyTrack a = ScriptableObject.CreateInstance<AMPropertyTrack>();
		a.setName(getTrackCount());
		a.id = getUniqueTrackID();
		addTrack(a);
	}
	
	// add event track
	public void addEventTrack() {
		AMEventTrack a = ScriptableObject.CreateInstance<AMEventTrack>();
		a.setName(getTrackCount());
		a.id = getUniqueTrackID();
		addTrack(a);
	}
	public void addTrack(AMTrack track) {
		trackValues.Add(track);
		track.parentTake = this;
	}	
	// add camera swithcer track
	public void addCameraSwitcherTrack() {
		if(cameraSwitcher) {
			return;	
		}
		AMCameraSwitcherTrack a = ScriptableObject.CreateInstance<AMCameraSwitcherTrack>();
		a.setName(getTrackCount());
		a.id = getUniqueTrackID();
		addTrack(a);
		cameraSwitcher = a;
	}
	// select a track by index
	public void selectTrack(int index, bool isShiftDown, bool isControlDown) {
		bool isInContextSelection = contextSelectionTracks.Contains(index);
		if(!isShiftDown && !isControlDown) {
			if(selectedTrack != index) {
				selectedTrack = index;
				if(!isInContextSelection) {
					// clear context selection
					contextSelection = new List<int>();
					contextSelectionTracks = new List<int>();
				}
			}
		} 
		
		if(!isInContextSelection)
			contextSelectionTracks.Add(index);
		else if(isControlDown && selectedTrack != index && !isShiftDown) {
			contextSelectionTracks.Remove(index);
		}
		// select range
		if(selectedTrack != -1 && isShiftDown) {
			List<int> range = getTrackIDsForRange(selectedTrack, index);
			foreach(int track_id in range) {
				if(!contextSelectionTracks.Contains(track_id)) contextSelectionTracks.Add(track_id);
			}
		}
	}
	public AMTrack getSelectedTrack() {
		return getTrack(selectedTrack);
	}
	// get the new index for a new track
	public int getTrackCount() {
		return trackValues.Count;
	}
	public AMTrack getTrack(int id) {
		return trackValues.Find(delegate(AMTrack track)
		{
			return track.id == id;
		});
	}
	public void deleteTrack(int id) {
		trackValues.RemoveAll(delegate(AMTrack track) {
			if (track.id == id) {
				if (cameraSwitcher == track) cameraSwitcher = null;
				return true;
			}else return false;
		});
	}
	// get track ids from range, exclusive
	public List<int> getTrackIDsForRange(int start_id, int end_id) {
		if(start_id == end_id) return new List<int>();
		List<int> track_ids = new List<int>();
		bool foundStartID = false;
		for(int i = 0; i < getTrackCount(); i++) {
			if(trackValues[i].id == start_id || trackValues[i].id == end_id) {
				if (!foundStartID) {
					foundStartID = true;
					continue;
				}
				else break;
			}
			if (foundStartID) track_ids.Add(trackValues[i].id);
		}
		return track_ids;
	}
	public int getUniqueTrackID() {
		track_count++;
		foreach(AMTrack track in trackValues) {
			if(track.id >= track_count) track_count = track.id+1;
		}
		return track_count;
	}
	// move track, just after the destination track. if dest track id == -1, then put it to the first.
	public void moveTrack(int source_track_id, int dest_track_id=-1) {
		int source_index = trackValues.FindIndex(delegate (AMTrack track){
			return track.id == source_track_id;
		});
		int dest_index = dest_track_id==-1? -1 : trackValues.FindIndex(delegate (AMTrack track){
			return track.id == dest_track_id;
		});
		if (source_index == -1) return;
		if (source_index == dest_index || source_index == dest_index + 1) return;
		int i =source_index;
		while(true)
		{
			//swap track
			int next=i<dest_index?i+1:i-1;
			AMTrack temp = trackValues[i];
			trackValues[i] = trackValues[next];
			trackValues[next] = temp;
			i = next;
			if (next == dest_index || next == dest_index + 1) break;
		}
	}
	#endregion
	#region Frames/Keys
	// select a frame
	public void selectFrame(int track, int num, float numFramesToRender, bool isShiftDown, bool isControlDown) {
		selectedFrame = num;
		selectTrack (track, isShiftDown, isControlDown);

		if((selectedFrame<startFrame)||(selectedFrame>endFrame)) {
			startFrame = selectedFrame;
			endFrame = startFrame+(int)numFramesToRender-1;
		}
	}

	public class Morph {
		public GameObject obj;
		public MethodInfo methodInfo;
		public Component component;
		public List<float> morph;
		
		public Morph(GameObject obj, MethodInfo methodInfo, Component component, List<float> morph) {
			this.obj = obj;
			this.methodInfo = methodInfo;
			this.component = component;
			this.morph = new List<float>(morph);
		}
		
		public void blendMorph(List<float> new_morph) {
			// if previous morph channel == 0, apply new morph
			for(int i=0;i<(morph.Count >= new_morph.Count ? morph.Count : new_morph.Count);i++) {
				if(i >= new_morph.Count) break;
				else if(i >= morph.Count) morph.Add(0f);
				if(morph[i] == 0f) morph[i] = new_morph[i];	
			}
		}
	}
	
	public void addMorph(GameObject obj, MethodInfo methodInfo, Component component, List<float> morph) {
		foreach(Morph m in morphs) {
			if(m.obj == obj && m.component == component) {
				// found morph, blend
				m.blendMorph(morph);
				return;
			}
		}
		// add new morph
		Morph _m = new Morph(obj,methodInfo,component,morph);
		morphs.Add(_m);
	}
	
	List<Morph> morphs;
	
	// preview a frame
	public void previewFrame(float _frame, bool renderStill = true) {
		List<AMOrientationTrack> tracksOrientaton = new List<AMOrientationTrack>();
		List<AMRotationTrack> tracksRotation = new List<AMRotationTrack>();
		morphs = new List<Morph>();
		
		foreach(AMTrack track in trackValues) {
			if(track is AMAudioTrack) continue;
			else if(track is AMOrientationTrack) tracksOrientaton.Add (track as AMOrientationTrack);
			else {
				if(track is AMRotationTrack) tracksRotation.Add(track as AMRotationTrack);	// if rotation, add to list and preview
				if(track is AMAnimationTrack) (track as AMAnimationTrack).previewFrame(_frame);
				else if(track is AMPropertyTrack) (track as AMPropertyTrack).previewFrame(_frame,false);
				else track.previewFrame(_frame);
			}
		}
		// preview orientation
		foreach(AMOrientationTrack track in tracksOrientaton) {
			track.cachedTranslationTrackStartTarget = getTranslationTrackForTransform(track.getStartTargetForFrame(_frame));
			track.cachedTranslationTrackEndTarget = getTranslationTrackForTransform(track.getEndTargetForFrame(_frame));
			track.previewFrame(_frame);
		}
		if(tracksOrientaton.Count > 0) {
			// preview rotation, second pass
			foreach(AMRotationTrack track in tracksRotation) {
				track.previewFrame(_frame);
			}
		}
		// preview morph targets
		foreach(Morph m in morphs) {
			previewMorph(m);	
		}
	}
	
	public void previewMorph(Morph m) {
		//if(valueType == (int)ValueType.MorphChannels) {
			for(int i=0;i<m.morph.Count;i++) {
				if(i >= m.morph.Count) break;
				m.methodInfo.Invoke(m.component, new object[]{i,m.morph[i]});
			}
		//}
		// update transform to refresh scene
		if(!Application.isPlaying) m.obj.transform.position = new Vector3(m.obj.transform.position.x,m.obj.transform.position.y,m.obj.transform.position.z);
	}

	public AMTranslationTrack getTranslationTrackForTransform(Transform obj) {
		if(!obj) return null;
		foreach(AMTrack track in trackValues) {
			if((track is AMTranslationTrack) && (track as AMTranslationTrack).obj == obj)
				return track as AMTranslationTrack;
		}
		return null;
	}
	
	// delete keys after a frame
	public void deleteKeysAfter(int frame) {
		bool didDeleteKeys;
		foreach(AMTrack track in trackValues) {
			didDeleteKeys = false;
			for(int i=0;i<track.keys.Count;i++) {
				if(track.keys[i].frame > frame) {
					// destroy key
					track.keys[i].destroy();
					// remove from list
					track.keys.RemoveAt (i);
					didDeleteKeys = true;
					i--;
				}
				if(didDeleteKeys) track.updateCache();
			}
		}
	}
	
	// delete keys after a frame
	public void deleteKeysBefore(int frame) {
		bool didDeleteKeys;
		foreach(AMTrack track in trackValues) {
			didDeleteKeys = false;
			for(int i=0;i<track.keys.Count;i++) {
				if(track.keys[i].frame < frame) {
					// destroy key
					track.keys[i].destroy();
					// remove from list
					track.keys.RemoveAt (i);
					didDeleteKeys = true;
					i--;
				}
				if(didDeleteKeys) track.updateCache();
			}
		}
	}
	
	public void shiftOutOfBoundsKeysOnSelectedTrack() {
		int offset = getSelectedTrack().shiftOutOfBoundsKeys();	
		if(contextSelection.Count<=0) return;
		for(int i=0;i<contextSelection.Count;i++) {
			contextSelection[i] += offset;	
		}
		// shift all keys on all tracks
		foreach(AMTrack track in trackValues) {
			if(track.id==selectedTrack) continue;
			track.offsetKeysFromBy(1,offset);
		}
	}
	
	public void shiftOutOfBoundsKeysOnTrack(AMTrack _track) {
		int offset = _track.shiftOutOfBoundsKeys();	
		if(contextSelection.Count<=0) return;
		for(int i=0;i<contextSelection.Count;i++) {
			contextSelection[i] += offset;	
		}
		// shift all keys on all tracks
		foreach(AMTrack track in trackValues) {
			if(track.id==_track.id) continue;
			track.offsetKeysFromBy(0,offset);
		}
	}
	
	public void deleteSelectedKeysFromTrack(int track_id) {
		bool didDeleteKeys = false;
		AMTrack track = getTrack(track_id);
		for(int i=0; i<track.keys.Count;i++) {
			if(!isFrameInContextSelection(track.keys[i].frame)) continue;
			track.keys[i].destroy();
			track.keys.Remove(track.keys[i]);
			i--;
			didDeleteKeys = true;
		}
		if(didDeleteKeys) track.updateCache();
	}
	
	// does take have keys beyond frame
	public bool hasKeyAfter(int frame) {
		foreach(AMTrack track in trackValues) {
			if(track.keys.Count>0) {
				// check last key on each track
				if(track.keys[track.keys.Count-1].frame > frame) return true;	
			}
		}
		return false;
	}
	
	#endregion
	#region Context Selection
	public bool isFrameInContextSelection(int frame) {
		for(int i=0;i<contextSelection.Count;i+=2) {
			if(frame >= contextSelection[i] && frame <= contextSelection[i+1]) return true;
		}
		return false;
	}
	
	public bool isFrameInGhostSelection(int frame) {
		if(ghostSelection == null) return false;
		for(int i=0;i<ghostSelection.Count;i+=2) {
			if(frame >= ghostSelection[i] && frame <= ghostSelection[i+1]) return true;
		}
		return false;
	}
	
	public bool isFrameSelected(int frame) {
		if(hasGhostSelection()) {
				return isFrameInGhostSelection(frame);
		}
		return isFrameInContextSelection(frame);
	}
	
	public void contextSelectFrame(int frame, bool toggle) {
		// if already exists return, toggle if true
		for(int i=0;i<contextSelection.Count;i+=2) {
			if(frame >= contextSelection[i] && frame <= contextSelection[i+1]) {
				if(toggle) {
					if(frame == contextSelection[i] && frame == contextSelection[i+1]) {
						// remove single frame range
						contextSelection.RemoveAt(i);
						contextSelection.RemoveAt(i);
					} else if (frame == contextSelection[i]) {
						// shift first frame forward
						contextSelection[i]++;
					} else if (frame == contextSelection[i+1]) {
						// shift last frame backwards
						contextSelection[i+1]++;	
					} else {
						// split range
						int start = contextSelection[i];
						int end = contextSelection[i+1];
						// remove range
						contextSelection.RemoveAt(i);
						contextSelection.RemoveAt(i);
						// add range left
						contextSelection.Add(start);
						contextSelection.Add (frame-1);
						// add range right
						contextSelection.Add(frame+1);
						contextSelection.Add(end);
						contextSelection.Sort();
					}
				}
				return;
			}
		}
		// add twice, as a range
		contextSelection.Add(frame);
		contextSelection.Add(frame);
		contextSelection.Sort();
	}
	
	// make selection from start frame to end frame
	public void contextSelectFrameRange(int startFrame, int endFrame) {
		// if selected only one frame
		if(startFrame == endFrame) {
			contextSelectFrame(endFrame,false);
			return;
		}
		int _endFrame = endFrame;
		if(endFrame < startFrame) {
			endFrame = startFrame;
			startFrame = _endFrame;
		}
		// check for previous selection
		for(int i=0;i<contextSelection.Count;i+=2) {
			// new selection engulfs previous selection
			if(startFrame <= contextSelection[i] && endFrame >= contextSelection[i+1]) {
				// remove previous selection
				contextSelection.RemoveAt(i);
				contextSelection.RemoveAt(i);
				i-=2;
			// previous selection engulfs new selection
			} else if(contextSelection[i] <= startFrame && contextSelection[i+1] >= endFrame) {
				// do nothing
				return;
			}
		}
		// add new selection
		contextSelection.Add(startFrame);
		contextSelection.Add(endFrame);
		contextSelection.Sort();
	}
	
	public void contextSelectAllFrames(int numFrames) {
		contextSelection = new List<int>();
		contextSelection.Add(1);
		contextSelection.Add(numFrames);
	}
	
	public bool contextSelectionHasKeys() {
		foreach(AMKey key in getSelectedTrack().keys) {
			for(int i=0;i<contextSelection.Count;i+=2) {
				// if selection start frame > frame, break out of sorted list
				if(contextSelection[i] > key.frame) break;
				if(contextSelection[i] <= key.frame && contextSelection[i+1] >= key.frame) return true;
			}
		}
		return false;
	}
	
	public AMKey[] getContextSelectionKeysForTrack(AMTrack track) {
		List<AMKey> keys = new List<AMKey>();
		foreach(AMKey key in track.keys) {
			for(int i=0;i<contextSelection.Count;i+=2) {
				// if selection start frame > frame, break out of sorted list
				if(contextSelection[i] > key.frame) break;
				if(contextSelection[i] <= key.frame && contextSelection[i+1] >= key.frame) keys.Add(key);
			}
		}
		return keys.ToArray();
	}
	
	// offset context selection frames by an amount. can be positive or negative
	public void offsetContextSelectionFramesBy(int offset) {
		if(offset == 0) return;
		if(contextSelection.Count <= 0) return;
		foreach(int track_id in contextSelectionTracks) {
			bool shouldUpdateCache = false;
			List<AMKey> keysToDelete = new List<AMKey>();
			AMTrack _track = getTrack(track_id);
			foreach(AMKey key in _track.keys) {
				for(int i=0;i<contextSelection.Count;i+=2) {
					// move context selection
					if(contextSelection[i] <= key.frame && contextSelection[i+1] >= key.frame) {
						// if there is already a key in the new frame position, mark for deletion
						bool keyToOverwriteInContextSelection = false;
						if(_track.hasKeyOnFrame(key.frame+offset)) {
							// check if the key is in the selection
							for(int j = 0;j<contextSelection.Count;j+=2) {
								if(contextSelection[j] <= (key.frame+offset) && contextSelection[j+1] >= (key.frame+offset)) {
									keyToOverwriteInContextSelection = true;
									break;
								}
							}
							// if not key is not in selection, mark for deletion
							if(!keyToOverwriteInContextSelection) keysToDelete.Add(_track.getKeyOnFrame(key.frame+offset));
						}
						key.frame += offset;
						if(!shouldUpdateCache) shouldUpdateCache = true;
						break;
					}
				}
				
			}
			// delete keys that were overwritten
			foreach( AMKey key in keysToDelete) {
				_track.keys.Remove(key);
				key.destroy();
			}
			// release references
			keysToDelete = new List<AMKey>();
			// update cache
			if(shouldUpdateCache) {
				_track.updateCache();
			}
		}
		// update context selection
		for(int i=0;i<contextSelection.Count;i++) {
			// move context selection
			contextSelection[i] += offset;	
		}
		// clear ghost selection
		ghostSelection = new List<int>();
		
	}
	
	private int ghost_selection_total_offset = 0;
	public void offsetGhostSelectionBy(int offset) {
		// update ghost selection
		for(int i=0;i<ghostSelection.Count;i++) {
			// move ghost selection
			ghostSelection[i] += offset;
		}
		ghost_selection_total_offset += offset;
	}
	
	// copy the values from the context selection to the ghost selection
	public void setGhostSelection() {
		ghostSelection = new List<int>();
		ghost_selection_total_offset = 0;
		foreach(int frame in contextSelection) ghostSelection.Add(frame);
	}
	
	public bool hasGhostSelection() {
		if(ghostSelection == null || ghostSelection.Count>0) return true;
		return false;
	}
	
	public int[] getKeyFramesInGhostSelection(int startFrame, int endFrame, int track_id) {
		List<int> key_frames = new List<int>();
		if(track_id <= -1) return key_frames.ToArray();
		foreach(AMKey key in getTrack(track_id).keys) {
			if(key.frame+ghost_selection_total_offset < startFrame) continue;
			if(key.frame+ghost_selection_total_offset > endFrame) break;
			if(isFrameInContextSelection(key.frame)) key_frames.Add(key.frame+ghost_selection_total_offset);
		}
		return key_frames.ToArray();
	}
	#endregion
	#region Other Fns

	public void maintainTake() {
		foreach(AMTrack track in trackValues) {
			if(!track.parentTake) track.parentTake = this;
		}
	}
	
	public void sampleAudio(float frame) {
		foreach(AMTrack track in trackValues) {
			if(track is AMAudioTrack) (track as AMAudioTrack).sampleAudio(frame);
		}
	}
	
	public void stopAudio() {
		foreach(AMTrack track in trackValues) {
				if(!(track is AMAudioTrack)) continue;
				(track as AMAudioTrack).stopAudio();
		}
	}
	
	public void drawGizmos(float gizmo_size) {
		foreach(AMTrack track in trackValues) {
			if(track is AMTranslationTrack || track is AMOrientationTrack)
				track.drawGizmos(gizmo_size);
		}
	}
	
	public void maintainCaches() {
		// re-updates cache if there are null values
		foreach(AMTrack track in trackValues) {
				bool shouldUpdateCache = false;
				foreach(AMAction action in track.cache) {
					if(action == null) { 
						shouldUpdateCache = true;
						break;
					}
				}
				if(shouldUpdateCache) {
					track.updateCache();
				}
			}
	}

	public void setupCameraSwitcher(float fromFrame = 0f) {
		if(!cameraSwitcher || cameraSwitcher.keys.Count <= 0) return;
		cameraSwitcher.cachedAllCameras = cameraSwitcher.getAllCameras();
		//cameraSwitcher.getActionForFrame(fromFrame);
		AMCameraSwitcherKey cKey = (cameraSwitcher.keys[0] as AMCameraSwitcherKey);
		if(cKey.type == 0) {
			if(cKey.camera) AMTween.SetTopCamera(cKey.camera,cameraSwitcher.cachedAllCameras);
		} else if(cKey.type == 1) {
			AMTween.ShowColor(cKey.color);
		}
	}

	public float getTracksHeight(float height_track, float height_track_foldin) {
		float height = 0;
		foreach(AMTrack track in trackValues) {
			if(track.foldout) height += height_track;
			else height += height_track_foldin;
		}	
		return height;
	}
	public float getTrackY(int element_id, float height_track, float height_track_foldin) {
		float height = 0;
		foreach(AMTrack track in trackValues) {
			if(track.id == element_id) {
				return height;
			}
			if(track.foldout) height += height_track;
			else height += height_track_foldin;
		}
		return height;
	}
	#endregion
	
	public void destroy() {
		foreach(AMTrack track in trackValues) {
			track.destroy();
		}
		DestroyImmediate(this);
	}
}
