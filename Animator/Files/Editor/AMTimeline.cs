using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;
using System;
public class AMTimeline : EditorWindow {
	
	[MenuItem ("Window/Animator Timeline Editor")]
    static void Init () {
		EditorWindow.GetWindow (typeof (AMTimeline));
    }
	
	#region Declarations	
	
	public static AMTimeline window = null;
	private CustomInspector customInspector = null;
	
	private AnimatorData _aData;
	public AnimatorData aData {
		get {
			return _aData;	
		}
		set {
			_aData = value;
			EventLayout.resetIndexMethodInfo();
		}
	}// AnimatorData component, holds all data
	private Vector2 scrollViewValue;			// current value in scrollview (vertical)
	private string[] playbackSpeed = {"x.25","x.5","x1","x2","x4"};
	private float[] playbackSpeedValue = {.25f,.5f,1f,2f,4f};
	private float cachedZoom = 0f;
	private float numFramesToRender;			// number of frames to render, based on window size
	private bool isPlaying;						// is preview player playing
	private float playerStartTime;				// preview player start time
	private int playerStartFrame;				// preview player start frame
	public static string[] easeTypeNames = {
		"easeInQuad",
		"easeOutQuad",
		"easeInOutQuad",
		"easeInCubic",
		"easeOutCubic",
		"easeInOutCubic",
		"easeInQuart",
		"easeOutQuart",
		"easeInOutQuart",
		"easeInQuint",
		"easeOutQuint",
		"easeInOutQuint",
		"easeInSine",
		"easeOutSine",
		"easeInOutSine",
		"easeInExpo",
		"easeOutExpo",
		"easeInOutExpo",
		"easeInCirc",
		"easeOutCirc",
		"easeInOutCirc",
		"linear",
		"spring",
		/* GFX47 MOD START */
		//bounce,
		"easeInBounce",
		"easeOutBounce",
		"easeInOutBounce",
		/* GFX47 MOD END */
		"easeInBack",
		"easeOutBack",
		"easeInOutBack",
		/* GFX47 MOD START */
		//elastic,
		"easeInElastic",
		"easeOutElastic",
		"easeInOutElastic",
		/* GFX47 MOD END */
		//"punch"
		"Custom"
	};

	public enum MessageBoxType {
		Info = 0,
		Warning = 1,
		Error = 2
	}
	public enum DragType {
		None = -1,
		ContextSelection = 0,
		MoveSelection = 1,
		TimeScrub = 2,
		GroupElement = 3,
		ResizeTrack = 4,
		FrameScrub = 5,
		TimelineScrub = 8,
		ResizeAction = 9,
		ResizeHScrollbarLeft = 10,
		ResizeHScrollbarRight = 11,
		CursorZoom = 12,
		CursorHand = 13
	}
	public enum ElementType {
		None = -1,
		TimeScrub = 0,
		Track = 3,
		ResizeTrack = 4,
		Button = 5,
		Other = 6,
		FrameScrub = 7,
		TimelineScrub = 8,
		ResizeAction = 9,
		TimelineAction = 10,
		ResizeHScrollbarLeft = 11,
		ResizeHScrollbarRight = 12,
		CursorZoom = 13,
		HScrollbarThumb = 14,
		CursorHand = 15
	}
	public enum CursorType {
		None = -1,
		Zoom = 1,
		Hand = 2
	}
	[SerializeField]
	public enum Track {
		Translation = 0,
		Rotation = 1,
		Orientation = 2,
		Animation = 3,
		Audio = 4,
		Property = 5,
		Event = 6,
		CameraSwitcher = 7
	}
	
	// dimensions
	private float margin = 2f;
	private float width_track = 150f;
	private float width_track_min = 135f;
	private float height_track = 58f;
	private float height_track_foldin = 20f;
	private float height_action_min = 45f;
	private float height_menu_bar = /*30f*/20f;
	private float height_indicator_offset_y = 33f;		// indicator vertical offset
	private float width_indicator_line = 3f;
	private float width_indicator_head = 11f;
	private float height_indicator_head = 12f;
	private float width_scrollbar = 17f;
	private float width_button = 22f;
	private float height_button = 22f;
	private float height_group = 20f;
	private float height_element_position = 2f;
	private float width_scrub_control = 45f;
	private float width_frame_birdseye_min = 7f;
	// dynamic dimensions
	private float current_width_frame;
	private float current_height_frame;
	// colors
	private Color colBirdsEyeFrames = new Color(210f/255f,210f/255f,210f/255f,1f);
	// textures
	private Texture tex_cursor_zoomin = (Texture)Resources.Load ("am_cursor_zoomin");
	private Texture tex_cursor_zoomout = (Texture)Resources.Load ("am_cursor_zoomout");
	private Texture tex_cursor_zoom_blank = (Texture)Resources.Load ("am_cursor_zoom_blank");
	private Texture tex_cursor_zoom = null;
	private Texture tex_cursor_grab = (Texture)Resources.Load ("am_cursor_grab");
	private Texture tex_icon_track = (Texture)Resources.Load ("am_icon_track");
	private Texture tex_icon_delete_track = (Texture)Resources.Load ("am_icon_delete_track");
	private Texture tex_icon_keyframe = (Texture)Resources.Load ("am_icon_keyframe");
	private Texture tex_icon_delete_keyframe = (Texture)Resources.Load ("am_icon_delete_keyframe");
	private Texture tex_icon_next_key = (Texture)Resources.Load("tex_next_key");
	private Texture tex_icon_prev_key = (Texture)Resources.Load("tex_prev_key");
	private Texture tex_icon_play = (Texture)Resources.Load("tex_nav_play");
	private Texture tex_icon_stop = (Texture)Resources.Load("tex_nav_stop");
	private Texture tex_icon_first_key = (Texture)Resources.Load("tex_nav_skip_back");
	private Texture tex_icon_last_key = (Texture)Resources.Load("tex_nav_skip_forward");

	private Texture tex_element_position = (Texture)Resources.Load ("am_element_position");
	private Texture texFrKey = (Texture)Resources.Load("am_key");
	private Texture texFrSet = (Texture)Resources.Load("am_frame_set");
	//private Texture texFrU = (Texture)Resources.Load("am_frame");
	//private Texture texFrM = (Texture)Resources.Load("am_frame-m");
	//private Texture texFrUS = (Texture)Resources.Load("am_frame-s"); 
	//private Texture texFrMS = (Texture)Resources.Load("am_frame-m-s"); 
	//private Texture texFrUG = (Texture)Resources.Load("am_frame-g"); 
	private Texture texKeyBirdsEye = (Texture)Resources.Load("am_key_birdseye"); 
	private Texture texIndLine = (Texture)Resources.Load("am_indicator_line");
	private Texture texIndHead = (Texture)Resources.Load("am_indicator_head");
//	private Texture texProperties = (Texture)Resources.Load("am_information");	
//	private Texture texRightArrow = (Texture)Resources.Load("am_nav_right");// inspector right arrow
//	private Texture texLeftArrow = (Texture)Resources.Load("am_nav_left");	// inspector left arrow

	private Texture texBoxBorder = (Texture)Resources.Load("am_box_border");
	private Texture texBoxRed = (Texture)Resources.Load("am_box_red");
	//private Texture texBoxBlue = (Texture)Resources.Load("am_box_blue");
	private Texture texBoxLightBlue = (Texture)Resources.Load("am_box_lightblue");
	private Texture texBoxDarkBlue = (Texture)Resources.Load("am_box_darkblue");
	private Texture texBoxGreen = (Texture)Resources.Load("am_box_green");
	private Texture texBoxPink = (Texture)Resources.Load("am_box_pink");
	private Texture texBoxYellow = (Texture)Resources.Load("am_box_yellow");
	private Texture texBoxOrange = (Texture)Resources.Load("am_box_orange");
	private Texture texBoxPurple = (Texture)Resources.Load("am_box_purple");
	private Texture texIconTranslation = (Texture)Resources.Load("am_icon_translation");
	private Texture texIconRotation = (Texture)Resources.Load("am_icon_rotation");
	private Texture texIconAnimation = (Texture)Resources.Load("am_icon_animation");
	private Texture texIconAudio = (Texture)Resources.Load("am_icon_audio");
	private Texture texIconProperty = (Texture)Resources.Load("am_icon_property");
	private Texture texIconEvent = (Texture)Resources.Load("am_icon_event");
	private Texture texIconOrientation = (Texture)Resources.Load("am_icon_orientation");
	private Texture texIconCameraSwitcher = (Texture)Resources.Load("am_icon_cameraswitcher");
	
	// temporary variables
	private bool time_numbering = false;
	private bool isPlayMode = false; 		// whether the user is in play mode, used to close AMTimeline when in play mode
	private int isRenamingTrack = -1;		// the track the is user renaming, -1 if not renaming a track

	private GenericMenu menu = new GenericMenu(); 			// add track menu
	private GenericMenu menu_drag = new GenericMenu(); 		// add track menu, on drag to window
	private GenericMenu contextMenu = new GenericMenu();	// context selection menu
	private float height_all_tracks = 0f;

	// context selection variables
	private bool isControlDown = false;
	private bool isShiftDown = false;
	private bool isDragging = false;
	private int dragType = -1;
	private bool cachedIsDragging = false;				// used to determine dragging is started or stopped
	private float scrubSpeed = 0f;
	private int startDragFrame = 0;
	private int ghostStartDragFrame = 0;	// temporary value used to drag the ghost selection
	private int endDragFrame = 0;
	private int contextMenuFrame = 0;
	//private int contextSelectionTrack = 0;
	private Vector2 contextSelectionRange = new Vector2(0f,0f);			// holds the first and last frame of the copied selection
	//private List<AMKey> contextSelectionKeysBuffer = new List<AMKey>();	// stores the copied context selection keys
	private List<List<AMKey>> contextSelectionKeysBuffer = new List<List<AMKey>>();
	private List<AMTrack> contextSelectionTracksBuffer = new List<AMTrack>();
	private List<int> cachedContextSelection = new List<int>();			// cache context selection when user copies, used for paste
	private bool isChangingTimeControl = false;
	private bool isChangingFrameControl = false;
	private int startScrubFrame = 0;
	private Vector2 startScrubMousePosition = new Vector2(0f,0f);
	private Vector2 startZoomMousePosition = new Vector2(0f,0f);
	private Vector2 zoomDirectionMousePosition = new Vector2(0f,0f);
	private Vector2 cachedZoomMousePosition = new Vector2(0f,0f);
	private Vector2 endHandMousePosition = new Vector2(0f,0f);
	private int justFinishedHandDragTicker = 0;
	private int handDragAccelaration = 0;
	private bool didPeakZoom = false;
	private bool wasZoomingIn = false;
	private float startZoomValue = 0f;
	private int startZoomXOverFrame = 0;
	private int mouseOverFrame = 0;					// the frame number that the mouse X and Y is over, 0 if one
	private int mouseXOverFrame = 0;				// the frame number that the mouse X is over, 0 if none
	private int mouseOverTrack = -1;				// mouse over frame track, -1 if no track
	private int mouseOverElement = -1;
	private int mouseXOverHScrollbarFrame = 0;
	private Vector2 mouseOverGroupElement = new Vector2(-1,-1);	// group_id, track id
	private bool mouseOverSelectedFrame = false;
	private Vector2 currentMousePosition = new Vector2(0f,0f);
	private Vector2 draggingGroupElement = new Vector2(-1,-1);
	private int draggingGroupElementType = -1;
	private bool mouseAboveGroupElements = false;	// is mouse above group elements? used to determine whether a dropped group element should be placed in the first position
	private int ticker = 0;
	private int tickerSpeed = 50;
	private float scrollAmountVertical = 0f;
	private List<GameObject> objects_window = new List<GameObject>();
	private int startResizeActionFrame = -1;
	private int resizeActionFrame = -1;
	private int endResizeActionFrame = -1;
	private float[] arrKeyRatiosLeft;
	private float[] arrKeyRatiosRight;
	AMKey[] arrKeysLeft;
	AMKey[] arrKeysRight;
	private bool justStartedHandGrab = false;
	//double click variables
	private double doubleClickTime = 0.3f;
	private double doubleClickCachedTime = 0f;
	private string doubleClickElementID = null;
	private bool cursorHand = false;
	public static bool shouldCheckDependencies = false;

	#endregion
	
	#region Main
	
	void OnEnable() {
		this.title = "Animator";
		this.minSize = new Vector2(810f,190f);
		window = this;
		GameObject go = GameObject.Find ("AnimatorData");
		if(go) {
			aData = (AnimatorData) go.GetComponent ("AnimatorData");
			if(aData) {
				aData.isAnimatorOpen = true;
				aData.getCurrentTake().maintainTake();	// upgrade take to current version if necessary
				// save data
				EditorUtility.SetDirty(aData);
				// preview last selected frame
				if(!isPlayMode) aData.getCurrentTake().previewFrame((float)aData.getCurrentTake().selectedFrame);
			}
		}

		// set default current dimensions of frames
		//current_width_frame = width_frame;
		//current_height_frame = height_frame;
		// set is playing to false
		isPlaying = false;
		
		// add track menu
		buildAddTrackMenu();
		// playmode callback
		EditorApplication.playmodeStateChanged = OnPlayMode;
		
		// check for pro license
		AMTake.isProLicense = PlayerSettings.advancedLicense;
	}

	void OnPlayMode() {
		bool justHitPlay = EditorApplication.isPlayingOrWillChangePlaymode && !EditorApplication.isPlaying;
		// entered playmode
		if(justHitPlay) {
			if(aData) {
				aData.inPlayMode = true;
				EditorUtility.SetDirty(aData);
			}
			aData = null;
			//repaintBuffer = 0;	// used to repaint after user exits play mode
			if(dragType == (int)DragType.TimeScrub || dragType == (int)DragType.FrameScrub) dragType = (int)DragType.None;
			// destroy camerafade
			if(AMCameraFade.hasInstance() && AMCameraFade.isPreview()) {
				AMCameraFade.destroyImmediateInstance();
			}
		// exit playmode
		}else if(!EditorApplication.isPlayingOrWillChangePlaymode) {
			// recheck for component
			GameObject go = GameObject.Find ("AnimatorData");
			if(go) {
				aData = (AnimatorData) go.GetComponent ("AnimatorData");
				aData.inPlayMode = false;
				//maintainCachesIn = 10;
			}
			//this.Repaint();
			//repaintBuffer = repaintRefreshRate;
			// reset inspector selected methodinfo
			EventLayout.resetIndexMethodInfo();
			// preview selected frame
			if(aData) aData.getCurrentTake().previewFrame(aData.getCurrentTake().selectedFrame);
			// check for pro license
			AMTake.isProLicense = PlayerSettings.advancedLicense;
		}
	}
	void OnDisable() {
		window = null;
		if(aData) {
			// preview first frame
			aData.getCurrentTake().previewFrame(1f);
			// tell component that animator has been closed
			aData.isAnimatorOpen = false;
			// save data
			EditorUtility.SetDirty(aData);
			// refresh component
			refreshGizmos();
			// stop audio if it's playing
			aData.getCurrentTake().stopAudio();
			// reset property select track
			//aData.propertySelectTrack = null;
		}
		if(AMCameraFade.hasInstance() && AMCameraFade.isPreview()) {
			AMCameraFade.destroyImmediateInstance();
		}
		if (customInspector != null) customInspector.Close();
	}
	void Update() {
		isPlayMode = EditorApplication.isPlayingOrWillChangePlaymode;
		
		if(isPlayMode) return;
		// drag logic
		if(!isPlaying) {
			processDragLogic();
		}

		// if preview is playing
		if(isPlaying || dragType == (int)DragType.TimeScrub || dragType == (int)DragType.FrameScrub) {
			float timeRunning = Time.realtimeSinceStartup-playerStartTime; 
			// determine current frame
			float curFrame = aData.getCurrentTake().selectedFrame;
			// if scrubbing
			if(dragType == (int)DragType.TimeScrub || dragType == (int)DragType.FrameScrub) {
				if(scrubSpeed < 0) scrubSpeed*=5;
				curFrame += Mathf.CeilToInt(scrubSpeed);
				curFrame = Mathf.Clamp(curFrame,1,aData.getCurrentTake().numFrames);
			} else {
				// determine speed
				float speed = (float)aData.getCurrentTake().frameRate*playbackSpeedValue[aData.getCurrentTake().playbackSpeedIndex];
				curFrame = playerStartFrame+timeRunning*speed;
				if(Mathf.FloorToInt(curFrame) > aData.getCurrentTake().numFrames) {
					// loop
					playerStartTime = Time.realtimeSinceStartup;
					curFrame = curFrame-aData.getCurrentTake().numFrames;
					playerStartFrame = Mathf.FloorToInt(curFrame);
					if(playerStartFrame <= 0) playerStartFrame = 1;
					// stop audio
					aData.getCurrentTake().stopAudio();
				}
			}
			// select the appropriate frame
			if(Mathf.FloorToInt(curFrame) != aData.getCurrentTake().selectedFrame) {
				aData.getCurrentTake().selectFrame(aData.getCurrentTake().selectedTrack,Mathf.FloorToInt(curFrame),numFramesToRender,false,false);
				if(dragType != (int)DragType.TimeScrub && dragType != (int)DragType.FrameScrub) {
					// sample audio
					aData.getCurrentTake().sampleAudioAtFrame(Mathf.FloorToInt(curFrame),playbackSpeedValue[aData.getCurrentTake().playbackSpeedIndex]);
				}
				this.Repaint();
			}
			aData.getCurrentTake().previewFrame(curFrame);
		} else {
			
			// process property if it has been selected
			//processSelectProperty();
			// update methodinfo cache if necessary, used for event track inspector
//			processUpdateMethodInfoCache();
		}
	}
	void OnGUI() {

		if (EditorApplication.isPlayingOrWillChangePlaymode) {
			this.ShowNotification(new GUIContent("Play Mode"));
			return;
		}
		
		if(tickerSpeed <= 0) tickerSpeed = 1;
		ticker = (ticker+1)%tickerSpeed;
		EditorGUIUtility.LookLikeControls();
		// reset mouse over element
		mouseOverElement = (int)ElementType.None;
		mouseOverFrame = 0;
		mouseXOverFrame = 0;
		mouseOverTrack = -1;
		mouseOverGroupElement = new Vector2(0,0);
		int difference = 0;
		height_action_min = 45f;

		#region no data component
		if(!aData) {
			// recheck for component
			GameObject go = GameObject.Find ("AnimatorData");
			if(go) {
				aData = (AnimatorData) go.GetComponent ("AnimatorData");
			} 
			if (!aData) {
				// no data component message
				MessageBox("Animator requires an AnimatorData component in your scene.",MessageBoxType.Info);
				if(GUILayout.Button ("Add Component")) {
					// create component
					if(!go) go = new GameObject("AnimatorData");	
					go.AddComponent ("AnimatorData");
					aData = (AnimatorData) go.GetComponent ("AnimatorData");
					aData.isAnimatorOpen = true;
					// add take
					aData.addTake();
					// save data
					EditorUtility.SetDirty(aData);
					EditorUtility.SetDirty(aData.getCurrentTake());
				}
				return;
			}	
		}

		#endregion

		#region temporary variables
		Rect rectWindow = new Rect(0f,0f,position.width,position.height);
		Event e = Event.current;
//		 get global mouseposition
		Vector2 globalMousePosition = getGlobalMousePosition(e);
		// resize track
		if(dragType == (int)DragType.ResizeTrack) {
			width_track += ( e.mousePosition.x-currentMousePosition.x );
			width_track = Mathf.Clamp(width_track,width_track_min,position.width-70f);
		}

		currentMousePosition = e.mousePosition;
		bool clickedZoom = false;
		#endregion
		#region drag logic events
		bool wasDragging = false;
		if (e.type == EventType.mouseDrag && EditorWindow.mouseOverWindow==this) {
			isDragging = true;
	    }else if ((dragType == (int)DragType.CursorZoom && EditorWindow.mouseOverWindow!=this) || e.type == EventType.mouseUp ||Event.current.rawType == EventType.MouseUp /*|| e.mousePosition.y < 0f*/) {
			if(isDragging) {
				wasDragging = true;
				isDragging = false;
			}
	    }
		#endregion
		#region keyboard events
		if (e.Equals(Event.KeyboardEvent("[enter]")) || e.Equals(Event.KeyboardEvent("return"))) {
			// apply renaming when pressing enter
			cancelTextEditting();
			if(isChangingTimeControl) isChangingTimeControl = false;
			if(isChangingFrameControl) isChangingFrameControl = false;
			// deselect keyboard focus
			GUIUtility.keyboardControl = 0;
			GUIUtility.ExitGUI();
		}
		// check if control or shift are down
		isControlDown = e.control || e.command;
		isShiftDown = e.shift;
		#endregion
		#region set cursor
		int customCursor = (int)CursorType.None;
		bool showCursor = true;
		 if(isRenamingTrack <= -1 && dragType==(int)DragType.CursorHand) {
			cursorHand = true;
			showCursor = false;
			customCursor = (int)CursorType.Hand;
			mouseOverElement = (int)ElementType.CursorHand;
			// unused button to catch clicks
			GUI.Button(rectWindow,"","label");
		} else if(dragType==(int)DragType.CursorZoom || (!cursorHand && e.alt && EditorWindow.mouseOverWindow==this)) {
			showCursor = false;
			customCursor = (int)CursorType.Zoom;
			if(!isDragging) { 
				if(isControlDown) tex_cursor_zoom = tex_cursor_zoomout;
				else tex_cursor_zoom = tex_cursor_zoomin;
			}
			mouseOverElement = (int)ElementType.CursorZoom;
			if(!wasDragging) {
				if(GUI.Button(rectWindow,"","label")) {
					if(isControlDown) {
						if(aData.zoom < 1f) {
							aData.zoom += 0.2f;
							if(aData.zoom > 1f) aData.zoom = 1f;
							clickedZoom = true;
						}
					} else {
						if(aData.zoom > 0f) {
							aData.zoom -= 0.2f;
							if(aData.zoom < 0f) aData.zoom = 0f;
							clickedZoom = true;
						}
					}
					
				}
			}
		} else {
			if(!showCursor) showCursor = true;
			cursorHand = false;
		}
		if(Screen.showCursor != showCursor) {
			Screen.showCursor = showCursor;
		}
		if(isRenamingTrack != -1) EditorGUIUtility.AddCursorRect(rectWindow,MouseCursor.Text);
		else if(dragType == (int)DragType.TimeScrub || dragType == (int)DragType.FrameScrub || dragType == (int)DragType.MoveSelection) EditorGUIUtility.AddCursorRect(rectWindow,MouseCursor.SlideArrow);
		else if(dragType == (int)DragType.ResizeTrack || dragType == (int)DragType.ResizeAction || dragType == (int)DragType.ResizeHScrollbarLeft || dragType == (int)DragType.ResizeHScrollbarRight) EditorGUIUtility.AddCursorRect(rectWindow,MouseCursor.ResizeHorizontal);
		#endregion
		#region calculations
		processHandDragAcceleration();
		// calculate number of frames to render
		calculateNumFramesToRender(clickedZoom, e);
		//current_height_frame = (oData.disableTimelineActions ? height_track : height_frame);
		// if is playing, disable all gui elements
		GUI.enabled = !(isPlaying);
		// if selected frame is out of range
		if(aData.getCurrentTake().selectedFrame>aData.getCurrentTake().numFrames) {
			// select last frame
			timelineSelectFrame(aData.getCurrentTake().selectedTrack,aData.getCurrentTake().numFrames);
		}
		// get number of tracks in current take, use for tracks and keys, disabling zoom slider
		int trackCount = aData.getCurrentTake().getTrackCount();
		#endregion

		//
		//
		//
		//
		//
		//

		#region menu bar
		GUI.DrawTexture(new Rect(0f,0f, position.width, height_menu_bar-2f),EditorStyles.toolbar.normal.background);

		#region toggle play button
		Rect rectBtnTogglePlay = new Rect(margin,0f,24f,height_button);
		// change label if already playing
		GUI.enabled = (aData.getCurrentTake().rootGroup != null && aData.getCurrentTake().rootGroup.elements.Count > 0 ? true : false);
		if(GUI.Button (rectBtnTogglePlay,new GUIContent("",isPlaying? tex_icon_stop:tex_icon_play, isPlaying?"Pause":"Play"),EditorStyles.toolbarButton)) {
			if(isChangingTimeControl) isChangingTimeControl = false;
			if(isChangingFrameControl) isChangingFrameControl = false;
			// cancel renaming
			cancelTextEditting();
			// toggle play
			playerTogglePlay();
		}
		if(rectBtnTogglePlay.Contains(e.mousePosition) && mouseOverElement == (int)ElementType.None) {
			mouseOverElement = (int)ElementType.Button;
		}
		#endregion

		#region select first frame button
		Rect rectBtnSkipBack = new Rect(rectBtnTogglePlay.x + rectBtnTogglePlay.width + margin,rectBtnTogglePlay.y,width_button,height_button);
		if(GUI.Button (rectBtnSkipBack, new GUIContent("", tex_icon_first_key, "First Key"),EditorStyles.toolbarButton)) timelineSelectFrame(aData.getCurrentTake().selectedTrack,1);
		if(rectBtnSkipBack.Contains(e.mousePosition) && mouseOverElement == (int)ElementType.None) {
			mouseOverElement = (int)ElementType.Button;
		}
		#endregion

		#region select previous key
		Rect rectBtnPrevKey = new Rect(rectBtnSkipBack.x+rectBtnSkipBack.width,rectBtnSkipBack.y,width_button,height_button);
		if(GUI.Button (rectBtnPrevKey,new GUIContent("", tex_icon_prev_key, "Prev. Key"),EditorStyles.toolbarButton)) {
			timelineSelectPrevKey();
		}
		if(rectBtnPrevKey.Contains(e.mousePosition) && mouseOverElement == (int)ElementType.None) mouseOverElement = (int)ElementType.Button;
		#endregion

		#region frame control
		GUIStyle fieldStyle = new GUIStyle(EditorStyles.numberField);
		fieldStyle.alignment = TextAnchor.MiddleCenter;
		Rect rectFrameControl = new Rect(rectBtnPrevKey.x+rectBtnPrevKey.width,rectBtnPrevKey.y,width_scrub_control,height_button);
		int selectedFrame = EditorGUI.IntField(new Rect(rectFrameControl.x+2f,rectFrameControl.y+2f,rectFrameControl.width-4f,rectFrameControl.height-8f),aData.getCurrentTake().selectedFrame,fieldStyle);
		selectFrame(selectedFrame);
		if(rectFrameControl.Contains(e.mousePosition) && mouseOverElement == (int)ElementType.None) {
			mouseOverElement = (int)ElementType.Other;
		}
		#endregion
		
		#region select next key
		Rect rectBtnNextKey = new Rect(rectFrameControl.x+rectFrameControl.width,rectFrameControl.y,width_button,height_button);
		if(GUI.Button (rectBtnNextKey, new GUIContent("", tex_icon_next_key, "Next Key"), EditorStyles.toolbarButton)) {
			timelineSelectNextKey();
		}
		if(rectBtnNextKey.Contains(e.mousePosition) && mouseOverElement == (int)ElementType.None) mouseOverElement = (int)ElementType.Button;
		#endregion

		#region select last frame button
		GUI.enabled = (aData.getCurrentTake().rootGroup != null && aData.getCurrentTake().rootGroup.elements.Count > 0 ? !isPlaying : false);
		Rect rectSkipForward = new Rect(rectBtnNextKey.x+rectBtnNextKey.width,rectBtnNextKey.y,width_button,height_button);
		if(GUI.Button (rectSkipForward, new GUIContent("", tex_icon_last_key, "Last Key") ,EditorStyles.toolbarButton)) timelineSelectFrame(aData.getCurrentTake().selectedTrack,aData.getCurrentTake().numFrames);
		if(rectSkipForward.Contains(e.mousePosition) && mouseOverElement == (int)ElementType.None) {
			mouseOverElement = (int)ElementType.Button;
		}
		#endregion

		#region playback speed popup
		Rect rectPopupPlaybackSpeed = new Rect(rectSkipForward.x+rectSkipForward.width+margin,rectSkipForward.y,34f,height_button);
		aData.getCurrentTake().playbackSpeedIndex = EditorGUI.Popup(rectPopupPlaybackSpeed,aData.getCurrentTake().playbackSpeedIndex,playbackSpeed,EditorStyles.toolbarButton);
		#endregion

		#region settings
		Rect rectLabelSettings = new Rect(rectPopupPlaybackSpeed.x+rectPopupPlaybackSpeed.width+margin,rectPopupPlaybackSpeed.y,200f,height_button);
		
		GUIStyle styleLabelMenu = new GUIStyle(EditorStyles.toolbarButton);
		styleLabelMenu.normal.background = null;
		string strSettings = "Settings: "+aData.getCurrentTake().numFrames+" Frames; "+aData.getCurrentTake().frameRate+" Fps";
		rectLabelSettings.width = GUI.skin.label.CalcSize(new GUIContent(strSettings)).x;
		GUI.Label (rectLabelSettings,strSettings,styleLabelMenu);
		Rect rectBtnModify = new Rect(rectLabelSettings.x+rectLabelSettings.width+margin,rectLabelSettings.y,60f,height_button);
		if(GUI.Button(rectBtnModify,"Modify",EditorStyles.toolbarButton)) {
			EditorWindow windowSettings = ScriptableObject.CreateInstance<AMSettings>();
			windowSettings.ShowUtility();


		}
		if(rectBtnModify.Contains(e.mousePosition) && mouseOverElement == (int)ElementType.None) mouseOverElement = (int)ElementType.Button;
		#endregion

		#region switch button

		Rect rectBtnSwitch = new Rect(rectBtnModify.x+rectBtnModify.width+margin,rectBtnModify.y,70f,height_button);
		GUIStyle styleBtnSwitch = new GUIStyle(EditorStyles.toolbarButton);
		GUIContent contentBtnSwitch = new GUIContent("Show time","Show time instead of frame numbers");
		if(time_numbering) {
			styleBtnSwitch.normal.background = styleBtnSwitch.onNormal.background;
			styleBtnSwitch.hover.background = styleBtnSwitch.onNormal.background;
			contentBtnSwitch.text = "Show frames";
			contentBtnSwitch.tooltip = "Show frames instead of time numbers";
		}
		if(GUI.Button(rectBtnSwitch, contentBtnSwitch, styleBtnSwitch)) {
			if(!time_numbering) time_numbering = true;
			else time_numbering = false;
		}
		if(rectBtnSwitch.Contains(e.mousePosition) && mouseOverElement == (int)ElementType.None) mouseOverElement = (int)ElementType.Button;
		#endregion
		
		#endregion

		//
		//
		//
		//
		//
		//
		//

		bool birdseye = (current_width_frame <= width_frame_birdseye_min ? true : false);
		GUI.enabled = !isPlaying;

		#region control bar
		GUI.DrawTexture(new Rect(0f,height_menu_bar-2f, position.width, height_menu_bar-2f), EditorStyles.toolbar.normal.background);

		#region new track button
		Rect rectBtnNewTrack = new Rect(margin,height_menu_bar-2f, width_button ,height_button);
		if(GUI.Button (rectBtnNewTrack, new GUIContent("", tex_icon_track, "Add Track"), EditorStyles.toolbarButton)) {
			if(objects_window.Count > 0) objects_window = new List<GameObject>();
			if(menu.GetItemCount() <= 0) buildAddTrackMenu();
			menu.ShowAsContext();
		}
		if(rectBtnNewTrack.Contains(e.mousePosition) && mouseOverElement == (int)ElementType.None) {
			mouseOverElement = (int)ElementType.Button;
		}
		#endregion

		#region delete track button
		Rect rectBtnDeleteElement = new Rect(rectBtnNewTrack.x + rectBtnNewTrack.width,rectBtnNewTrack.y,width_button,height_button);
		if(aData.getCurrentTake().selectedGroup>=0) GUI.enabled = false;
		if (aData.getCurrentTake().selectedGroup >= 0 &&  (trackCount <= 0 || (aData.getCurrentTake().contextSelectionTracks != null && aData.getCurrentTake().contextSelectionTracks.Count <= 0))) GUI.enabled = false;
		else GUI.enabled = !isPlaying;
		string strTitleDeleteTrack = (aData.getCurrentTake().contextSelectionTracks != null && aData.getCurrentTake().contextSelectionTracks.Count > 1 ? "Tracks" : "Track");
		if(GUI.Button (rectBtnDeleteElement,new GUIContent("",tex_icon_delete_track, "Delete Track"), EditorStyles.toolbarButton)) {
			cancelTextEditting();
			if(aData.getCurrentTake().contextSelectionTracks.Count > 0) {
				string strMsgDeleteTrack = (aData.getCurrentTake().contextSelectionTracks.Count > 1 ? "multiple tracks" : "track '"+aData.getCurrentTake().getSelectedTrack().name+"'");
				
				if((EditorUtility.DisplayDialog("Delete "+strTitleDeleteTrack,"Are you sure you want to delete "+strMsgDeleteTrack+"?","Delete","Cancel"))) {
					isRenamingTrack = -1;
					// delete camera fade
					if(aData.getCurrentTake().selectedTrack != -1 && aData.getCurrentTake().getSelectedTrack() == aData.getCurrentTake().cameraSwitcher && AMCameraFade.hasInstance() && AMCameraFade.isPreview()) {
						AMCameraFade.destroyImmediateInstance();
					}
					foreach(int track_id in aData.getCurrentTake().contextSelectionTracks) {
						aData.getCurrentTake().deleteTrack(track_id);
					}
					aData.getCurrentTake().contextSelectionTracks = new List<int>();
					//aData.getCurrentTake().deleteTrack(aData.getCurrentTake().selectedTrack);
					// save data
					setDirtyTracks(aData.getCurrentTake());
					// refresh gizmos
					refreshGizmos();
					// deselect track
					aData.getCurrentTake().selectedTrack = -1;
					// deselect group
					aData.getCurrentTake().selectedGroup = 0;
				}
			} else {
				bool delete = true;
				bool deleteContents = false;
				AMGroup grp = aData.getCurrentTake().getGroup(aData.getCurrentTake().selectedGroup);
				if(grp.elements.Count > 0) {
					int choice = EditorUtility.DisplayDialogComplex("Delete Contents?","'"+grp.group_name+"' contains contents that can be deleted with the group.","Delete Contents","Keep Contents","Cancel");
					if(choice == 2) delete = false;
					else if(choice == 0) deleteContents = true;
					if(delete) {
						aData.getCurrentTake().deleteSelectedGroup(deleteContents);
					}
				} else {
					aData.getCurrentTake().deleteSelectedGroup(deleteContents);
				}
			}
		}
		if(rectBtnDeleteElement.Contains(e.mousePosition) && mouseOverElement == (int)ElementType.None) {
			mouseOverElement = (int)ElementType.Button;
		}

		#endregion

		# region insert key
		Rect rectBtnInsertKey = new Rect(rectBtnDeleteElement.x+rectBtnDeleteElement.width,rectBtnDeleteElement.y,width_button,height_button);
		if(GUI.Button (rectBtnInsertKey,new GUIContent("", tex_icon_keyframe, "Add Keyframe"),EditorStyles.toolbarButton)) {
			addKeyToSelectedFrame();
		}
		if(rectBtnInsertKey.Contains(e.mousePosition) && mouseOverElement == (int)ElementType.None) mouseOverElement = (int)ElementType.Button;
		#endregion

		#region delete key
		if(aData.getCurrentTake().selectedTrack <= -1 || !aData.getCurrentTake().getSelectedTrack().hasKeyOnFrame(selectedFrame) || isPlaying)
			GUI.enabled = false;
		Rect rectBtnDelKey = new Rect(rectBtnInsertKey.x+rectBtnInsertKey.width,rectBtnInsertKey.y,width_button,height_button);
		if(GUI.Button (rectBtnDelKey,new GUIContent("", tex_icon_delete_keyframe, "Delete Keyframe"),EditorStyles.toolbarButton)) {
			deleteKeyFromSelectedFrame();
		}
		if(rectBtnDelKey.Contains(e.mousePosition) && mouseOverElement == (int)ElementType.None) mouseOverElement = (int)ElementType.Button;
		#endregion

		GUI.enabled = !isPlaying;

		#endregion

		//
		//
		//
		//
		//
		//
		//
		//

		#region key numbering
		Rect rectKeyNumbering = new Rect(width_track,2*height_menu_bar+2f-22f,position.width-width_track-20f,20f);
		if(rectKeyNumbering.Contains(e.mousePosition) && (mouseOverElement == (int)ElementType.None)) {
			mouseOverElement = (int)ElementType.TimelineScrub;
		}
		int key_dist = 5;
		if(numFramesToRender >= 100) key_dist = Mathf.FloorToInt(numFramesToRender/100)*10;
		int firstMarkedKey = (int)aData.getCurrentTake().startFrame;
		if(firstMarkedKey % key_dist != 0 && firstMarkedKey != 1) {
			firstMarkedKey += key_dist-firstMarkedKey % key_dist;
		}
		float lastNumberX = -1f;
		for(int i=firstMarkedKey;i<=(int)aData.getCurrentTake().endFrame;i+=key_dist) {
				float newKeyNumberX = width_track+current_width_frame*(i-(int)aData.getCurrentTake().startFrame)-1f;
				string key_number; 
				if(time_numbering) key_number = frameToTime(i,(float)aData.getCurrentTake().frameRate).ToString("N2");
				else key_number = i.ToString();
				Rect rectKeyNumber = new Rect(newKeyNumberX,height_menu_bar,GUI.skin.label.CalcSize(new GUIContent(key_number)).x,height_menu_bar);
				bool didCutLabel = false;
				if(rectKeyNumber.x+rectKeyNumber.width >= position.width-20f) {
					rectKeyNumber.width = position.width-20f-rectKeyNumber.x;
					didCutLabel = true;
				}
				if(!(didCutLabel && aData.getCurrentTake().endFrame==aData.getCurrentTake().numFrames)) {
					if(rectKeyNumber.x > lastNumberX+3f) {
						GUI.Label(rectKeyNumber,key_number);
						lastNumberX = rectKeyNumber.x+GUI.skin.label.CalcSize(new GUIContent(key_number)).x;
					}
				}
				if(i == 1) i--;
		}
		#endregion

		//
		//
		//
		//
		
		#region main scrollview
		height_all_tracks = aData.getCurrentTake().getElementsHeight(0,height_track,height_track_foldin,height_group);
		float height_scrollview = position.height-2*height_menu_bar+4f - width_scrollbar;
		// check if mouse is beyond tracks and dragging group element
		difference = 0;
		// drag up
		if(dragType == (int)DragType.GroupElement && globalMousePosition.y <= height_menu_bar*2+2f) {
			difference = Mathf.CeilToInt((height_menu_bar*2+2f)-globalMousePosition.y);
			scrollAmountVertical = -difference;	// set scroll amount
		// drag down
		} else if(dragType == (int)DragType.GroupElement && globalMousePosition.y >= position.height-width_scrollbar) {
			difference = Mathf.CeilToInt(globalMousePosition.y-(position.height-width_scrollbar));
			scrollAmountVertical = difference; // set scroll amount
		} else {
			scrollAmountVertical = 0f;
		}

		Rect rectScrollView = new Rect(0f,height_menu_bar*2-4f,position.width,height_scrollview);
		Rect rectView = new Rect(0f,0f,rectScrollView.width-width_scrollbar,(height_all_tracks > rectScrollView.height ? height_all_tracks : rectScrollView.height));
		scrollViewValue = GUI.BeginScrollView(rectScrollView,scrollViewValue,rectView,false,true);
			scrollViewValue.y = Mathf.Clamp(scrollViewValue.y, 0f, height_all_tracks-height_scrollview);
			Vector2 scrollViewBounds = new Vector2(scrollViewValue.y,scrollViewValue.y+height_scrollview); // min and max y displayed onscreen
			GUILayout.BeginHorizontal(GUILayout.Height(height_all_tracks));
				GUILayout.BeginVertical (GUILayout.Width (width_track));
					float track_y = 0f;		// the next track's y position
					// tracks vertical start
					for(int i=0;i<aData.getCurrentTake().rootGroup.elements.Count;i++) {
						if(track_y > scrollViewBounds.y) break;	// if start y is beyond max y
						int id = aData.getCurrentTake().rootGroup.elements[i];
						AMTrack _track = aData.getCurrentTake().getTrack(id);
						showTrack(_track, id, ref track_y, e, scrollViewBounds);
					}
				// draw element position indicator
				if(dragType == (int)DragType.GroupElement) {
					if(mouseOverElement != (int)ElementType.Track) {
						float element_position_y;
						if(e.mousePosition.y < (height_menu_bar*2)) element_position_y = 2f;
						else element_position_y = track_y;
						GUI.DrawTexture(new Rect(0f,element_position_y-height_element_position,width_track,height_element_position),tex_element_position);			
					}
				}
				GUILayout.EndVertical ();
				GUILayout.BeginVertical ();
					// frames vertical	
					GUILayout.BeginHorizontal (GUILayout.Height (height_track));
					mouseXOverFrame = (int)aData.getCurrentTake().startFrame+Mathf.CeilToInt((e.mousePosition.x-width_track)/current_width_frame)-1;
					if(dragType == (int)DragType.CursorHand && justStartedHandGrab) {
						startScrubFrame = mouseXOverFrame;
						justStartedHandGrab = false;
					}
					track_y = 0f;	// reset track y
					showFramesForGroup(0,ref track_y,e, birdseye, scrollViewBounds);
					GUILayout.EndHorizontal();
				GUILayout.EndVertical();
			GUILayout.EndHorizontal();
		GUI.EndScrollView();
		#endregion



		//
		//
		//
		//
		//
		//
		//

		#region horizontal scrollbar
		// check if mouse is over inspector and scroll if dragging
		if(globalMousePosition.y >= (2*height_menu_bar+2f)) {
			difference = 0;
			// drag right, over inspector
			if(globalMousePosition.x >= position.width-5f) {
				difference = Mathf.CeilToInt(globalMousePosition.x-5f);
				tickerSpeed = Mathf.Clamp(50-Mathf.CeilToInt(difference/1.5f),1,50);
				tickerSpeed /= 10;
				// if mouse over inspector, set mouseOverElement to Other
				mouseOverElement = (int)ElementType.Other;
				if(dragType == (int)DragType.MoveSelection || dragType == (int)DragType.ContextSelection || dragType == (int)DragType.ResizeAction) {
					if(ticker == 0) {
						aData.getCurrentTake().startFrame = Mathf.Clamp(++aData.getCurrentTake().startFrame,1,aData.getCurrentTake().numFrames);
						mouseXOverFrame = Mathf.Clamp((int)aData.getCurrentTake().startFrame+(int)numFramesToRender,1,aData.getCurrentTake().numFrames);
					} else {
						mouseXOverFrame = Mathf.Clamp((int)aData.getCurrentTake().startFrame+(int)numFramesToRender,1,aData.getCurrentTake().numFrames);
					}
				}
			// drag left, over tracks
			} else if(globalMousePosition.x <= width_track-5f) {
				difference = Mathf.CeilToInt((width_track-5f)-globalMousePosition.x);
				tickerSpeed = Mathf.Clamp(50-Mathf.CeilToInt(difference/1.5f),1,50);
				if(dragType == (int)DragType.MoveSelection || dragType == (int)DragType.ContextSelection || dragType == (int)DragType.ResizeAction) {
					if(ticker == 0) {
						aData.getCurrentTake().startFrame = Mathf.Clamp(--aData.getCurrentTake().startFrame,1,aData.getCurrentTake().numFrames);
						mouseXOverFrame = Mathf.Clamp((int)aData.getCurrentTake().startFrame-2,1,aData.getCurrentTake().numFrames);
					} else {
						mouseXOverFrame = Mathf.Clamp((int)aData.getCurrentTake().startFrame,1,aData.getCurrentTake().numFrames);
					}
				}
			}
		}
		Rect rectHScrollbar = new Rect(width_track,position.height-width_scrollbar+2f,position.width-width_track-21f,width_scrollbar-2f);
		float frame_width_HScrollbar = ((rectHScrollbar.width-44f)/((float)aData.getCurrentTake().numFrames-1f));
		Rect rectResizeHScrollbarLeft = new Rect(rectHScrollbar.x+18f+frame_width_HScrollbar*(aData.getCurrentTake().startFrame-1f),rectHScrollbar.y+2f,10f, width_scrollbar);
		Rect rectResizeHScrollbarRight = new Rect(rectHScrollbar.x+18f+frame_width_HScrollbar*(aData.getCurrentTake().endFrame-1f)-3f,rectHScrollbar.y+2f,10f,width_scrollbar);
		Rect rectHScrollbarThumb = new Rect(rectResizeHScrollbarLeft.x,rectResizeHScrollbarLeft.y-2f,rectResizeHScrollbarRight.x-rectResizeHScrollbarLeft.x+rectResizeHScrollbarRight.width,rectResizeHScrollbarLeft.height);
		rectHScrollbar.width += 4f;
		// if number of frames fit on screen, disable horizontal scrollbar and set startframe to 1
		if(aData.getCurrentTake().numFrames<numFramesToRender) {
			GUI.HorizontalScrollbar(rectHScrollbar,1f,1f,1f,1f);
			aData.getCurrentTake().startFrame = 1;
		} else {
			mouseXOverHScrollbarFrame = Mathf.CeilToInt(aData.getCurrentTake().numFrames*((e.mousePosition.x-rectHScrollbar.x-GUI.skin.horizontalScrollbarLeftButton.fixedWidth)/(rectHScrollbar.width - GUI.skin.horizontalScrollbarLeftButton.fixedWidth*2)));
			if(!rectResizeHScrollbarLeft.Contains(e.mousePosition) && !rectResizeHScrollbarRight.Contains(e.mousePosition) && EditorWindow.mouseOverWindow==this && dragType != (int)DragType.ResizeHScrollbarLeft && dragType != (int)DragType.ResizeHScrollbarRight && mouseOverElement != (int)ElementType.ResizeHScrollbarLeft && mouseOverElement != (int)ElementType.ResizeHScrollbarRight)
				aData.getCurrentTake().startFrame = Mathf.Clamp((int) GUI.HorizontalScrollbar(rectHScrollbar,(float)aData.getCurrentTake().startFrame,(int)numFramesToRender-1f,1f,aData.getCurrentTake().numFrames),1,aData.getCurrentTake().numFrames);
			else Mathf.Clamp(GUI.HorizontalScrollbar(rectHScrollbar,(float)aData.getCurrentTake().startFrame,(int)numFramesToRender-1f,1f,aData.getCurrentTake().numFrames),1f,aData.getCurrentTake().numFrames);
			// scrollbar bg overlay (used to hide inconsistent thumb)
			GUI.Box(new Rect(rectHScrollbar.x+18f,rectHScrollbar.y,rectHScrollbar.width-18f*2f,rectHScrollbar.height),"",GUI.skin.horizontalScrollbar);
			// scrollbar thumb overlay (used to hide inconsistent thumb)
			GUI.Box(rectHScrollbarThumb,"",GUI.skin.horizontalScrollbarThumb);
			
			if(GUI.enabled && !isDragging) {
				EditorGUIUtility.AddCursorRect(rectResizeHScrollbarLeft,MouseCursor.ResizeHorizontal);
				EditorGUIUtility.AddCursorRect(rectResizeHScrollbarRight,MouseCursor.ResizeHorizontal);
			}
			// show horizontal scrollbar
			if(rectResizeHScrollbarLeft.Contains(e.mousePosition) && customCursor == (int)CursorType.None) {
				
				mouseOverElement = (int)ElementType.ResizeHScrollbarLeft;
			}	else if(rectResizeHScrollbarRight.Contains(e.mousePosition) && customCursor == (int)CursorType.None) {
				mouseOverElement = (int)ElementType.ResizeHScrollbarRight;
			}
		}
		aData.getCurrentTake().endFrame = aData.getCurrentTake().startFrame+(int)numFramesToRender-1;
		if(rectHScrollbar.Contains(e.mousePosition) && mouseOverElement == (int)ElementType.None) {
			mouseOverElement = (int)ElementType.Other;
		}
		
		#endregion

		#region horizontal scrollbar tooltip
		string strHScrollbarLeftTooltip = (time_numbering ? frameToTime((int)aData.getCurrentTake().startFrame,(float)aData.getCurrentTake().frameRate).ToString("N2") : aData.getCurrentTake().startFrame.ToString());
		string strHScrollbarRightTooltip = (time_numbering ? frameToTime((int)aData.getCurrentTake().endFrame,(float)aData.getCurrentTake().frameRate).ToString("N2") : aData.getCurrentTake().endFrame.ToString());
		GUIStyle styleLabelCenter = new GUIStyle(GUI.skin.label);
		styleLabelCenter.alignment = TextAnchor.MiddleCenter;
		Vector2 _label_size;
		if(customCursor == (int)CursorType.None && ((mouseOverElement == (int)ElementType.ResizeHScrollbarLeft && !isDragging) || dragType == (int)DragType.ResizeHScrollbarLeft) && (dragType != (int)DragType.ResizeHScrollbarRight)) {
			_label_size = GUI.skin.button.CalcSize(new GUIContent(strHScrollbarLeftTooltip));
			_label_size.x += 2f;
			GUI.Label(new Rect(rectResizeHScrollbarLeft.x+rectResizeHScrollbarLeft.width/2f-_label_size.x/2f,rectResizeHScrollbarLeft.y-22f,_label_size.x,20f),strHScrollbarLeftTooltip,GUI.skin.button);	
		}
		if(customCursor == (int)CursorType.None && ((mouseOverElement == (int)ElementType.ResizeHScrollbarRight && !isDragging) || dragType == (int)DragType.ResizeHScrollbarRight) && (dragType != (int)DragType.ResizeHScrollbarLeft)) {
			_label_size = GUI.skin.button.CalcSize(new GUIContent(strHScrollbarRightTooltip));
			_label_size.x += 2f;
			GUI.Label(new Rect(rectResizeHScrollbarRight.x+rectResizeHScrollbarRight.width/2f-_label_size.x/2f,rectResizeHScrollbarRight.y-22f,_label_size.x,20f),strHScrollbarRightTooltip,GUI.skin.button);	
		}
		#endregion
		//
		//
		//
		//
		//
		//

		#region resize track bar
		Drawing.DrawLine(new Vector2(width_track, height_menu_bar*2-4f), new Vector2(width_track, position.height), new Color(46f/255f, 46f/255f, 46f/255f), 2f );
		Rect rectResizeTrack = new Rect(width_track-2f, height_menu_bar*2-4f, 4f,height_scrollview+width_scrollbar);
		if(rectResizeTrack.Contains(e.mousePosition) && mouseOverElement == (int)ElementType.None) {
			mouseOverElement = (int)ElementType.ResizeTrack;
		}
		if(GUI.enabled) EditorGUIUtility.AddCursorRect(rectResizeTrack,MouseCursor.ResizeHorizontal);
		GUI.enabled = (aData.getCurrentTake().rootGroup != null && aData.getCurrentTake().rootGroup.elements.Count > 0 ? !isPlaying : false);
		#endregion

		#region indicator
		drawIndicator(aData.getCurrentTake().selectedFrame);
		#endregion
		

		#region click window
		if(GUI.Button (new Rect(0f,0f,position.width,position.height),"","label") && dragType != (int)DragType.TimelineScrub && dragType != (int)DragType.ResizeAction) {

			if(aData.getCurrentTake().contextSelectionTracks != null && aData.getCurrentTake().contextSelectionTracks.Count > 0) {
				aData.getCurrentTake().contextSelectionTracks = new List<int>();
			}
			if(aData.getCurrentTake().contextSelection != null && aData.getCurrentTake().contextSelection.Count > 0) {
				aData.getCurrentTake().contextSelection = new List<int>();
			}
			if(aData.getCurrentTake().ghostSelection  != null && aData.getCurrentTake().ghostSelection.Count > 0) {
				aData.getCurrentTake().ghostSelection = new List<int>();
			}
			
			if(objects_window.Count > 0) objects_window = new List<GameObject>();
			if(isRenamingTrack != -1) isRenamingTrack = -1;
			if(isChangingTimeControl) isChangingTimeControl = false;
			if(isChangingFrameControl) isChangingFrameControl = false;
			// if clicked on inspector, do nothing
			if(e.mousePosition.y > (float)height_menu_bar*2 && e.mousePosition.x > position.width) return;
			if(aData.getCurrentTake().selectedTrack != -1) aData.getCurrentTake().selectedTrack = -1;
			
			if(objects_window.Count > 0) objects_window = new List<GameObject>();
			
		}
		#endregion
		
		#region drag logic
		if(dragType == (int)DragType.GroupElement) {
			// show element near cursor
			Rect rectDragElement = new Rect(e.mousePosition.x+10f,e.mousePosition.y-5f,90f,20f);
			string dragElementName = "Unknown";
			Texture dragElementIcon = null;
			float dragElementIconWidth = 12f;
			if (draggingGroupElementType == (int)ElementType.Track) {
				AMTrack dragTrack = aData.getCurrentTake().getTrack((int)draggingGroupElement.y);
				dragElementName = dragTrack.name;
				dragElementIcon = getTrackIconTexture(dragTrack);
			}
			dragElementName = trimString(dragElementName,8);
			if(dragElementIcon) GUI.DrawTexture(new Rect(rectDragElement.x+3f+(draggingGroupElementType == (int)ElementType.Track ? 1.45f : 0f),rectDragElement.y+rectDragElement.height/2-dragElementIconWidth/2,dragElementIconWidth,dragElementIconWidth),dragElementIcon);
			GUI.Label(new Rect(rectDragElement.x+15f+4f,rectDragElement.y,rectDragElement.width-15f-4f,rectDragElement.height),dragElementName);
		}
		mouseAboveGroupElements = e.mousePosition.y < (height_menu_bar*2);
		if(!aData.getCurrentTake().rootGroup || aData.getCurrentTake().rootGroup.elements.Count<=0) {
			EditorGUI.HelpBox(new Rect(5f,height_menu_bar*2+4f,width_track - 10f ,40f),"Click the add track icon above",MessageType.Info);
		}
		#endregion

		#region custom cursor
		if(customCursor != (int)CursorType.None) {
			if(customCursor == (int)CursorType.Zoom) {
				if(!tex_cursor_zoom) tex_cursor_zoom = tex_cursor_zoomin;
				if(tex_cursor_zoom == tex_cursor_zoomin && aData.zoom <= 0f) tex_cursor_zoom = tex_cursor_zoom_blank;
				else if(tex_cursor_zoom == tex_cursor_zoomout && aData.zoom >= 1f) tex_cursor_zoom = tex_cursor_zoom_blank;
				GUI.DrawTexture(new Rect(e.mousePosition.x-6f,e.mousePosition.y-5f,16f,16f),tex_cursor_zoom);	
			} else if(customCursor ==(int)CursorType.Hand) {
				GUI.DrawTexture(new Rect(e.mousePosition.x-8f,e.mousePosition.y-7f,16f,16f),(tex_cursor_grab));		
			}
		}
		#endregion
		if(e.alt && !isDragging) startZoomXOverFrame = mouseXOverFrame;
	}	


	
	#endregion
	
	#region Functions
	
	#region Static
	
	public static void MessageBox(string message, MessageBoxType type) {

		MessageType messageType;
		if (type == MessageBoxType.Error) messageType = MessageType.Error;
		else if (type == MessageBoxType.Warning) messageType = MessageType.Warning;
		else messageType = MessageType.Info;
		
		EditorGUILayout.HelpBox(message,messageType);
	}	

	public static float frameToTime(int frame,float frameRate) {
		return (float)Math.Round((float)frame/frameRate,2);	
	}
	public static int timeToFrame(float time,float frameRate) {
		return Mathf.FloorToInt(time*frameRate);	
	}

	#endregion
	
	#region Show/Draw

	bool showTrack(AMTrack _track, int track_index, ref float track_y, Event e, Vector2 scrollViewBounds) 
	{
		Rect rectTrack = new Rect(0f,track_y,width_track,(_track.foldout ? height_track : height_track_foldin));
		track_y += (_track.foldout ? height_track : height_track_foldin);
		// track is beyond bounds
		if(track_y < scrollViewBounds.x || rectTrack.y > scrollViewBounds.y) {
			return false;
		}
		// returns true if mouse over track
		bool mouseOverTrack = false;
		
		//rendering the selection context
		bool isTrackSelected = aData.getCurrentTake().selectedTrack == track_index;
		bool isTrackContextSelected = aData.getCurrentTake().contextSelectionTracks.Contains(track_index);
		GUIStyle myFoldoutStyle = new GUIStyle(EditorStyles.foldout);
		
		if(isTrackSelected || isTrackContextSelected)
		{
			Color color;
			if(isTrackSelected) color = new Color(100f/255f, 149f/255f, 237f/255f);
			else color = new Color(58f/255f, 95f/255f, 205f/255f);

			myFoldoutStyle.normal.textColor = color;
			myFoldoutStyle.onNormal.textColor = color;
			myFoldoutStyle.hover.textColor = color;
			myFoldoutStyle.onHover.textColor = color;
			myFoldoutStyle.focused.textColor = color;
			myFoldoutStyle.onFocused.textColor = color;
			myFoldoutStyle.active.textColor = color;
			myFoldoutStyle.onActive.textColor = color;
		}

		// renaming track
		if(isRenamingTrack != track_index) {
			if(GUI.Button(new Rect(rectTrack.x+12f,rectTrack.y,rectTrack.width-12f, height_track_foldin),"","label")) {
				timelineSelectTrack(track_index);
				if(didDoubleClick("track"+ track_index+"foldout")) {
					cancelTextEditting();
					isRenamingTrack = track_index;
				}
			}
		}
		else{
			GUI.SetNextControlName("RenameTrack"+track_index);
			_track.name = GUI.TextField(new Rect(rectTrack.x+12f,rectTrack.y,rectTrack.width-14f, 16f),_track.name,20);
			GUI.FocusControl("RenameTrack"+track_index);
		}
		//track clicked
		if(GUI.Button(new Rect(rectTrack.x+12f, rectTrack.y, rectTrack.width-((_track is AMPropertyTrack)?60f:12f), _track.foldout ? 39f : height_track_foldin),"","label")) {
			timelineSelectTrack(track_index);
		}
		// set track icon texture
		Texture texIcon = getTrackIconTexture(_track);

		// draw line gap
		Drawing.DrawLine(new Vector2(rectTrack.x,rectTrack.yMax), rectTrack.max , Color.gray );
		GUI.BeginGroup(rectTrack);
		// track start, foldin
		bool isFoldout = _track.foldout;
		_track.foldout = EditorGUI.Foldout(new Rect(0,0,rectTrack.width,height_track_foldin), _track.foldout, _track.name, myFoldoutStyle);
		if(_track.foldout)
		{
			if(!isFoldout) timelineSelectTrack(track_index);
			// track type
			Rect rectTrackIcon = new Rect(12f,20f,12f,12f);
			GUI.DrawTexture(rectTrackIcon,texIcon);
			string trackType = _track.getTrackType();
			Rect rectTrackType = new Rect(rectTrackIcon.x+rectTrackIcon.width+2f,height_track-39f,rectTrack.width-20f,15f);
			if((_track is AMPropertyTrack)&&(trackType == "Not Set"))
				rectTrackType.width -= 48f;
			GUI.Label(rectTrackType,trackType);
			// if property track, show set property button
			if(_track is AMPropertyTrack) {
				if(!(_track as AMPropertyTrack).obj) GUI.enabled = false;
				GUIStyle styleButtonSet = new GUIStyle(GUI.skin.button);
				styleButtonSet.clipping	= TextClipping.Overflow;
				if(GUI.Button(new Rect(width_track-54f,height_track-38f,44f,15f),"Set",styleButtonSet)) {
					// show property select window 
					AMPropertySelect.setValues((_track as AMPropertyTrack));
					EditorWindow.GetWindow (typeof (AMPropertySelect));
					timelineSelectTrack(track_index);
				}
				GUI.enabled = !isPlaying;
			}
			// track object
			showObjectFieldFor(_track, width_track, new Rect(10f, 39f, width_track-16f, 16f));
			// track end
		}
		else
		{
			if(isFoldout) timelineSelectTrack(track_index);
		}
		GUI.EndGroup();

		if(rectTrack.Contains(e.mousePosition) && mouseOverElement == (int)ElementType.None) {
			mouseOverTrack = true;
			mouseOverElement = (int)ElementType.Track;
			mouseOverGroupElement = new Vector2(0, track_index);
		}

		// draw element position texture after track
		if(dragType == (int)DragType.GroupElement && mouseOverElement == (int)ElementType.Track && mouseOverGroupElement.y == track_index) {
			GUI.DrawTexture(new Rect(rectTrack.x,rectTrack.y+rectTrack.height-height_element_position,rectTrack.width,height_element_position),tex_element_position);	
		}
		return mouseOverTrack;
	}

	void showFramesForGroup(int group_id, ref float track_y, Event e, bool birdseye, Vector2 scrollViewBounds) {
		if(track_y > scrollViewBounds.y) return;	// if start y is beyond max y
		AMGroup grp = aData.getCurrentTake().getGroup(group_id);
		foreach(int id in grp.elements) {
			if(track_y > scrollViewBounds.y) return;	// if start y is beyond max y
			showFrames(aData.getCurrentTake().getTrack(id),ref track_y,e,birdseye, scrollViewBounds);
		}
	}

	void showFrames(AMTrack _track, ref float track_y, Event e, bool birdseye, Vector2 scrollViewBounds) {
		//string tooltip = "";
		int t = _track.id;
		int selectedTrack = aData.getCurrentTake().selectedTrack;
		// frames start

		float numFrames = (aData.getCurrentTake().numFrames < numFramesToRender ? aData.getCurrentTake().numFrames : numFramesToRender);
		Rect rectFrames = new Rect(width_track,track_y,current_width_frame*numFrames,height_track);
		if(!_track.foldout) track_y += height_track_foldin;
		else track_y += height_track;
		if(track_y < scrollViewBounds.x) return; // if end y is before min y
		float _current_height_frame = (_track.foldout ? current_height_frame : height_track_foldin);
		#region frames
		GUI.BeginGroup(rectFrames);
			// draw frames
			bool selected;
			bool ghost = isDragging && aData.getCurrentTake().hasGhostSelection();
			bool isTrackSelected = t == selectedTrack || aData.getCurrentTake().contextSelectionTracks.Contains(t);
			Rect rectFramesBirdsEye = new Rect(0f,0f,rectFrames.width,_current_height_frame);
			float width_birdseye = current_height_frame*0.5f;
			if(birdseye) {
				GUI.color = colBirdsEyeFrames;
				GUI.DrawTexture(rectFramesBirdsEye,EditorGUIUtility.whiteTexture);
			} else {
				texFrSet.wrapMode = TextureWrapMode.Repeat;
				float startPos = aData.getCurrentTake().startFrame % 5f;
				GUI.DrawTextureWithTexCoords(rectFramesBirdsEye,texFrSet,new Rect(startPos/5f,0f,numFrames/5f,1f));
				float birdsEyeFadeAlpha = (1f-(current_width_frame-width_frame_birdseye_min))/1.2f;
				if(birdsEyeFadeAlpha > 0f) {
					GUI.color = new Color(colBirdsEyeFrames.r,colBirdsEyeFrames.g,colBirdsEyeFrames.b,birdsEyeFadeAlpha);
					GUI.DrawTexture(rectFramesBirdsEye,EditorGUIUtility.whiteTexture);
				}
			}
			GUI.color = new Color(72f/255f,72f/255f,72f/255f,1f);
			GUI.DrawTexture(new Rect(rectFramesBirdsEye.x,rectFramesBirdsEye.y,rectFramesBirdsEye.width,1f),EditorGUIUtility.whiteTexture);
			GUI.DrawTexture(new Rect(rectFramesBirdsEye.x,rectFramesBirdsEye.y+rectFramesBirdsEye.height-1f,rectFramesBirdsEye.width,1f),EditorGUIUtility.whiteTexture);
			GUI.color = Color.white;
			// draw birds eye selection
			if(isTrackSelected) {
				if(ghost) {
					// dragging only one frame that has a key. do not show ghost selection
					if(birdseye && aData.getCurrentTake().contextSelection.Count == 2 && aData.getCurrentTake().contextSelection[0] == aData.getCurrentTake().contextSelection[1] && _track.hasKeyOnFrame(aData.getCurrentTake().contextSelection[0])) {
						GUI.color = new Color(0f,0f,1f,.5f);
						GUI.DrawTexture(new Rect(current_width_frame*(aData.getCurrentTake().ghostSelection[0]-aData.getCurrentTake().startFrame)-width_birdseye/2f+current_width_frame/2f,0f,width_birdseye,_current_height_frame),texKeyBirdsEye);
						GUI.color = Color.white;
					} else if(aData.getCurrentTake().ghostSelection != null) {
						// birds eye ghost selection
						GUI.color = new Color(156f/255f,162f/255f,216f/255f,.9f);
						for(int i=0;i<aData.getCurrentTake().ghostSelection.Count;i+=2) {
							int contextFrameStart = aData.getCurrentTake().ghostSelection[i];
							int contextFrameEnd = aData.getCurrentTake().ghostSelection[i+1];
							if(contextFrameStart < (int)aData.getCurrentTake().startFrame) contextFrameStart = (int)aData.getCurrentTake().startFrame;
							if(contextFrameEnd > (int)aData.getCurrentTake().endFrame) contextFrameEnd = (int)aData.getCurrentTake().endFrame;
							float contextWidth = (contextFrameEnd-contextFrameStart+1)*current_width_frame;
							GUI.DrawTexture(new Rect(rectFramesBirdsEye.x+(contextFrameStart-aData.getCurrentTake().startFrame)*current_width_frame,rectFramesBirdsEye.y+1f,contextWidth,rectFramesBirdsEye.height-2f),EditorGUIUtility.whiteTexture);
						}
						// draw birds eye ghost key frames
						GUI.color = new Color(0f,0f,1f,.5f);
						foreach(int _key_frame in aData.getCurrentTake().getKeyFramesInGhostSelection((int)aData.getCurrentTake().startFrame,(int)aData.getCurrentTake().endFrame, t)) {
							if(birdseye)
								GUI.DrawTexture(new Rect(current_width_frame*(_key_frame-aData.getCurrentTake().startFrame)-width_birdseye/2f+current_width_frame/2f,0f,width_birdseye,_current_height_frame),texKeyBirdsEye);
							else {
								Rect rectFrame = new Rect(current_width_frame*(_key_frame-aData.getCurrentTake().startFrame),0f,current_width_frame,_current_height_frame);
								GUI.DrawTexture(new Rect(rectFrame.x+2f,rectFrame.y+rectFrame.height-(rectFrame.width-4f)-2f,rectFrame.width-4f,rectFrame.width-4f),texFrKey);
							}
						}
						GUI.color = Color.white;
					}
				} else if(aData.getCurrentTake().contextSelection.Count > 0 && /*do not show single frame selection in birdseye*/!(birdseye && aData.getCurrentTake().contextSelection.Count == 2 && aData.getCurrentTake().contextSelection[0] == aData.getCurrentTake().contextSelection[1])) {
					// birds eye context selection
					for(int i=0;i<aData.getCurrentTake().contextSelection.Count;i+=2) {
						//GUI.color = new Color(121f/255f,127f/255f,184f/255f,(birdseye ? 1f : .9f));
						GUI.color = new Color(86f/255f,95f/255f,178f/255f,.8f);
						int contextFrameStart = aData.getCurrentTake().contextSelection[i];
						int contextFrameEnd = aData.getCurrentTake().contextSelection[i+1];
						if(contextFrameStart < (int)aData.getCurrentTake().startFrame) contextFrameStart = (int)aData.getCurrentTake().startFrame;
						if(contextFrameEnd > (int)aData.getCurrentTake().endFrame) contextFrameEnd = (int)aData.getCurrentTake().endFrame;
						float contextWidth = (contextFrameEnd-contextFrameStart+1)*current_width_frame;
						Rect rectContextSelection = new Rect(rectFramesBirdsEye.x+(contextFrameStart-aData.getCurrentTake().startFrame)*current_width_frame,rectFramesBirdsEye.y+1f,contextWidth,rectFramesBirdsEye.height-2f);
						GUI.DrawTexture(rectContextSelection,EditorGUIUtility.whiteTexture);
						if(dragType!=(int)DragType.ContextSelection) EditorGUIUtility.AddCursorRect(rectContextSelection,MouseCursor.SlideArrow);
					}
					GUI.color = Color.white;
				}
			}
			// birds eye keyframe information, used to draw buttons in proper order
			List<int> birdseyeKeyFrames = new List<int>();
			List<Rect> birdseyeKeyRects = new List<Rect>();
			if(birdseye) {
				// draw birds eye keyframe textures, prepare button rects
				foreach(AMKey key in _track.keys) {
					selected = ((isTrackSelected) && aData.getCurrentTake().isFrameSelected(key.frame));	
					//_track.sortKeys();
					if(key.frame < aData.getCurrentTake().startFrame) continue;
					if(key.frame > aData.getCurrentTake().endFrame) break;
					Rect rectKeyBirdsEye = new Rect(current_width_frame*(key.frame-aData.getCurrentTake().startFrame)-width_birdseye/2f+current_width_frame/2f,0f,width_birdseye,_current_height_frame);
					if(selected) GUI.color = Color.blue;
					GUI.DrawTexture(rectKeyBirdsEye,texKeyBirdsEye);
					GUI.color = Color.white;
					birdseyeKeyFrames.Add(key.frame);
					birdseyeKeyRects.Add(rectKeyBirdsEye);
				}
				// birds eye buttons
				if(birdseyeKeyFrames.Count > 0) {
				for(int i=birdseyeKeyFrames.Count-1;i>=0;i--) {
					selected = ((isTrackSelected) && aData.getCurrentTake().isFrameSelected(birdseyeKeyFrames[i]));	
					if(dragType!=(int)DragType.MoveSelection && dragType!=(int)DragType.ContextSelection && isRenamingTrack == -1 && mouseOverFrame == 0 && birdseyeKeyRects[i].Contains(e.mousePosition) && mouseOverElement == (int)ElementType.None) {
						mouseOverFrame = birdseyeKeyFrames[i];
						mouseOverTrack = t;
						mouseOverSelectedFrame = (selected);
					}
					if(selected && dragType!=(int)DragType.ContextSelection) EditorGUIUtility.AddCursorRect(birdseyeKeyRects[i],MouseCursor.SlideArrow);
				}
			}
		} else {
			selected = (isTrackSelected);	
			foreach(AMKey key in _track.keys) {
				if(!key) continue;
				//_track.sortKeys();
				if(key.frame < aData.getCurrentTake().startFrame) continue;
				if(key.frame > aData.getCurrentTake().endFrame) break;
				Rect rectFrame = new Rect(current_width_frame*(key.frame-aData.getCurrentTake().startFrame),0f,current_width_frame,_current_height_frame);
				GUI.DrawTexture(new Rect(rectFrame.x+2f,rectFrame.y+rectFrame.height-(rectFrame.width-4f)-2f,rectFrame.width-4f,rectFrame.width-4f),texFrKey);
			}
		}
		// click on empty frames
		if(GUI.Button(rectFramesBirdsEye,"","label") && dragType == (int)DragType.None) {
			int prevFrame = aData.getCurrentTake().selectedFrame;
			bool clickedOnBirdsEyeKey = false;
			for(int i=birdseyeKeyFrames.Count-1;i>=0;i--) {
				if(birdseyeKeyFrames[i] >  (int)aData.getCurrentTake().endFrame) continue;
				if(birdseyeKeyFrames[i] <  (int)aData.getCurrentTake().startFrame) break;
				if(birdseyeKeyRects[i].Contains(e.mousePosition)) {
					clickedOnBirdsEyeKey = true;
					// left click
					if (e.button == 0) {
						// select the frame
						timelineSelectFrame(t,birdseyeKeyFrames[i]);
						// add frame to context selection
						contextSelectFrame(birdseyeKeyFrames[i],prevFrame);
					// right click
					} else if (e.button == 1) {
						
						// select track
						timelineSelectTrack(t);
						// if context selection is empty, select frame
						buildContextMenu(birdseyeKeyFrames[i]);
						// show context menu
						contextMenu.ShowAsContext();
					}
					break;
				}
			}
			if(!clickedOnBirdsEyeKey) {
				int _frame_num_birdseye = (int)aData.getCurrentTake().startFrame+Mathf.CeilToInt(e.mousePosition.x/current_width_frame)-1;
				// left click
				if (e.button == 0) {
					// select the frame
					timelineSelectFrame(t,_frame_num_birdseye);
					// add frame to context selection
					contextSelectFrame(_frame_num_birdseye,prevFrame);
				// right click
				} else if (e.button == 1) {
					timelineSelectTrack(t);
					// if context selection is empty, select frame
					buildContextMenu(_frame_num_birdseye);
					// show context menu
					contextMenu.ShowAsContext();
				}
			}
		}
		if(isRenamingTrack == -1 && mouseOverFrame == 0 && e.mousePosition.x >= rectFramesBirdsEye.x && e.mousePosition.x <= (rectFramesBirdsEye.x+rectFramesBirdsEye.width)) {
				if(rectFramesBirdsEye.Contains(e.mousePosition) && mouseOverElement == (int)ElementType.None) {
					mouseOverFrame = mouseXOverFrame;
					mouseOverTrack = t;
				}
				mouseOverSelectedFrame = ((isTrackSelected) && aData.getCurrentTake().isFrameSelected(mouseXOverFrame));
		}
		#endregion
		if( _track.foldout) {
			#region timeline actions
			//AudioClip audioClip = null;
			bool drawEachAction = false;
			if(_track is AMAnimationTrack || _track is AMAudioTrack) drawEachAction = true;	// draw each action with seperate textures and buttons for these tracks
			int _startFrame = (int)aData.getCurrentTake().startFrame;
			int _endFrame = (int)(_startFrame+numFrames-1);
			int action_startFrame, action_endFrame, renderFrameStart, renderFrameEnd;
			int cached_action_startFrame = -1, cached_action_endFrame = -1;
			Texture texBox = texBoxBorder;
			#region group textures / buttons (performance increase)
			Rect rectTimelineActions = new Rect(0f,_current_height_frame,0f,height_track-current_height_frame);	// used to group textures into one draw call
			if(!drawEachAction) {
				if(_track.cache.Count > 0) {
					if(_track is AMTranslationTrack && _track.cache.Count > 1) {
						// translation track, from first action frame to end action frame
						cached_action_startFrame = _track.cache[0].startFrame;
						cached_action_endFrame = (_track.cache[_track.cache.Count-1] as AMTranslationAction).endFrame;
						texBox = texBoxGreen;
					} else if(_track is AMRotationTrack && _track.cache.Count > 1) {
						// rotation track, from first action start frame to last action start frame
						cached_action_startFrame = _track.cache[0].startFrame;
						cached_action_endFrame = _track.cache[_track.cache.Count-1].startFrame;
						texBox = texBoxYellow;
					} else if(_track is AMOrientationTrack && _track.cache.Count > 1) {
						// orientation track, from first action start frame to last action start frame
						cached_action_startFrame = _track.cache[0].startFrame;
						cached_action_endFrame = _track.cache[_track.cache.Count-1].startFrame;
						texBox = texBoxOrange;
					} else if(_track is AMPropertyTrack) {
						// property track, full track width
						cached_action_startFrame = _startFrame;
						cached_action_endFrame = _endFrame;
						texBox = texBoxLightBlue;
					} else if(_track is AMEventTrack) {
						// event track, from first action start frame to end frame
						cached_action_startFrame = _track.cache[0].startFrame;
						cached_action_endFrame = _endFrame;
						texBox = texBoxDarkBlue;
					} else if(_track is AMCameraSwitcherTrack) {
						// camera switcher track, full track width
						cached_action_startFrame = _startFrame;
						cached_action_endFrame = _endFrame;
						texBox = texBoxPurple;
					}
				}
				if(cached_action_startFrame > 0 && cached_action_endFrame > 0) {
					if(cached_action_startFrame <= _startFrame) {
						rectTimelineActions.x = 0f;
					} else {
						rectTimelineActions.x = (cached_action_startFrame-_startFrame)*current_width_frame;
					}
					if(cached_action_endFrame >= _endFrame) {
						rectTimelineActions.width = rectFramesBirdsEye.width;
					} else {
						rectTimelineActions.width = (cached_action_endFrame-(_startFrame >= cached_action_startFrame ? _startFrame : cached_action_startFrame)+1)*current_width_frame;
					}
					// draw timeline action texture
				
					if(rectTimelineActions.width > 0f) GUI.DrawTexture(rectTimelineActions,texBox);
				}
				
			}
			#endregion
			string txtInfo;
			Rect rectBox;
			// draw box for each action in track
			bool didClampBackwards = false;	// whether or not clamped backwards, used to break infinite loop
			int last_action_startFrame = -1;
			for(int i=0;i<_track.cache.Count;i++) {
				#region calculate dimensions
				int clamped = 0; // 0 = no clamp, -1 = backwards clamp, 1 = forwards clamp
				if(_track.cache[i] == null) {
					// if cache is null, recheck for component and update caches
					aData = (AnimatorData)GameObject.Find ("AnimatorData").GetComponent("AnimatorData");
					aData.getCurrentTake().maintainCaches();
				}
				if((_track is AMAudioTrack)&&((_track.cache[i] as AMAudioAction).getNumberOfFrames(aData.getCurrentTake().frameRate)) > -1 && (_track.cache[i].startFrame + (_track.cache[i] as AMAudioAction).getNumberOfFrames(aData.getCurrentTake().frameRate)<=aData.getCurrentTake().numFrames)) {
					// based on audio clip length
					action_startFrame = _track.cache[i].startFrame;
					action_endFrame = _track.cache[i].startFrame + (_track.cache[i] as AMAudioAction).getNumberOfFrames(aData.getCurrentTake().frameRate);
					//audioClip = (_track.cache[i] as AMAudioAction).audioClip;
					// if intersects new audio clip, then cut
					if(i<_track.cache.Count-1) {
						if(action_endFrame > _track.cache[i+1].startFrame) action_endFrame = _track.cache[i+1].startFrame;
					}
				} else if((_track is AMAnimationTrack)&&((_track.cache[i] as AMAnimationAction).getNumberOfFrames(aData.getCurrentTake().frameRate)) > -1 && (_track.cache[i].startFrame + (_track.cache[i] as AMAnimationAction).getNumberOfFrames(aData.getCurrentTake().frameRate) <= aData.getCurrentTake().numFrames)) {
					// based on animation clip length
					action_startFrame = _track.cache[i].startFrame;
					action_endFrame = _track.cache[i].startFrame + (_track.cache[i] as AMAnimationAction).getNumberOfFrames(aData.getCurrentTake().frameRate);
					// if intersects new animation clip, then cut
					if(i<_track.cache.Count-1) {
						if(action_endFrame > _track.cache[i+1].startFrame) action_endFrame = _track.cache[i+1].startFrame;
					}
				} else if((i==0) && (!didClampBackwards) && (_track is AMPropertyTrack || _track is AMCameraSwitcherTrack)) {
					// clamp behind if first action
					action_startFrame = 1;
					action_endFrame =  _track.cache[0].startFrame;
					i--;
					didClampBackwards = true;
					clamped = -1;
				} else if((_track is AMAnimationTrack) || (_track is AMAudioTrack)|| (_track is AMPropertyTrack) || (_track is AMEventTrack) || (_track is AMCameraSwitcherTrack)) {
					// single frame tracks (clamp box to last frame) (if audio track not set, clamp)
					action_startFrame = _track.cache[i].startFrame;
					if(i<_track.cache.Count-1) {
						action_endFrame = _track.cache[i+1].startFrame;
					} else {
						clamped = 1;
						action_endFrame = _endFrame;
						if(action_endFrame>aData.getCurrentTake().numFrames) action_endFrame =  aData.getCurrentTake().numFrames+1;
					}
				} else {
					// tracks with start frame and end frame (do not clamp box, stop before last key)
					if(_track.cache[i].getNumberOfFrames()<=0) continue;
					action_startFrame = _track.cache[i].startFrame;
					action_endFrame = _track.cache[i].startFrame+_track.cache[i].getNumberOfFrames();
				}
				if(action_startFrame > _endFrame) {
					last_action_startFrame = action_startFrame;
					continue;
				} 
				if(action_endFrame < _startFrame) {
					last_action_startFrame = action_startFrame;
					continue;
				}
				if(i >= 0) txtInfo = getInfoTextForAction(_track, _track.cache[i],false,clamped);
				else txtInfo = getInfoTextForAction(_track, _track.cache[0],true,clamped);
				float rectLeft, rectWidth;;
				float rectTop = current_height_frame;
				float rectHeight = height_track-current_height_frame;
				// set info box position and dimensions
				bool showLeftAnchor = true;
				bool showRightAnchor = true;
				if(action_startFrame < _startFrame) {
					rectLeft = 0f;
					renderFrameStart = _startFrame;
					showLeftAnchor = false;
				} else {
					rectLeft = (action_startFrame-_startFrame)*current_width_frame;
					renderFrameStart = action_startFrame;
				}
				if(action_endFrame > _endFrame) {
					renderFrameEnd = _endFrame;
					showRightAnchor = false;
				} else {
					renderFrameEnd = action_endFrame;
				}
				rectWidth = (renderFrameEnd-renderFrameStart+1)*current_width_frame;
				rectBox = new Rect(rectLeft,rectTop,rectWidth,rectHeight);
				#endregion
				#region draw action
				if(_track is AMAnimationTrack) texBox = texBoxRed;
				else if(_track is AMPropertyTrack) texBox = texBoxLightBlue;
				else if(_track is AMTranslationTrack) texBox = texBoxGreen;
				else if(_track is AMAudioTrack) texBox = texBoxPink;
				else if(_track is AMRotationTrack) texBox = texBoxYellow;
				else if(_track is AMOrientationTrack) texBox = texBoxOrange;
				else if(_track is AMEventTrack) texBox = texBoxDarkBlue;
				else if(_track is AMCameraSwitcherTrack) texBox = texBoxPurple;
				else texBox = texBoxBorder;
				if(drawEachAction) {
					GUI.DrawTexture(rectBox,texBox);
					//if(audioClip) GUI.DrawTexture(rectBox,AssetPreview.GetAssetPreview(audioClip));
				}
				// info tex label
				GUIStyle styleTxtInfo = new GUIStyle(GUI.skin.label);
				styleTxtInfo.normal.textColor = Color.white;
				styleTxtInfo.alignment = TextAnchor.MiddleCenter;
				bool isLastAction;
				if(_track is AMPropertyTrack || _track is AMCameraSwitcherTrack || _track is AMEventTrack) isLastAction = (i == _track.cache.Count-1);
				else if(_track is AMAudioTrack || _track is AMAnimationTrack) isLastAction = false;
				else isLastAction = (i == _track.cache.Count-2);
				if(rectBox.width > 5f) EditorGUI.DropShadowLabel(new Rect(rectBox.x,rectBox.y,rectBox.width-(!isLastAction ? current_width_frame : 0f),rectBox.height),txtInfo,styleTxtInfo);
				// if clicked on info box, select the starting frame for action. show tooltip if text does not fit
				if(drawEachAction && GUI.Button (rectBox, "","label") && dragType != (int)DragType.ResizeAction) {
					int prevFrame = aData.getCurrentTake().selectedFrame;
					// timeline select
					timelineSelectFrame(t,(clamped == -1 ? action_endFrame : action_startFrame));
					// clear and add frame to context selection
					contextSelectFrame((clamped == -1 ? action_endFrame : action_startFrame),prevFrame);	
				}
				#endregion
				#region draw anchors
				if(showLeftAnchor) {
					Rect rectBoxAnchorLeft = new Rect(rectBox.x-1f,rectBox.y,2f,rectBox.height);
					GUI.DrawTexture(rectBoxAnchorLeft,texBoxBorder);
					Rect rectBoxAnchorLeftOffset = new Rect(rectBoxAnchorLeft);
					rectBoxAnchorLeftOffset.width += 6f;
					rectBoxAnchorLeftOffset.x -= 3f;
					// info box anchor cursor 
					if(i>=0) {
						EditorGUIUtility.AddCursorRect(new Rect(rectBoxAnchorLeftOffset.x+1f,rectBoxAnchorLeftOffset.y,rectBoxAnchorLeftOffset.width-2f,rectBoxAnchorLeftOffset.height),MouseCursor.ResizeHorizontal);
						if(rectBoxAnchorLeftOffset.Contains(e.mousePosition) && (mouseOverElement == (int)ElementType.None || mouseOverElement == (int)ElementType.TimelineAction)) {
							mouseOverElement = (int)ElementType.ResizeAction;
							if(dragType == (int)DragType.None) {
								if(_track.hasKeyOnFrame(last_action_startFrame)) startResizeActionFrame = last_action_startFrame;
								else startResizeActionFrame = -1;
								resizeActionFrame = action_startFrame;
								if(_track is AMAnimationTrack || _track is AMAudioTrack) {
									endResizeActionFrame = _track.getKeyFrameAfterFrame(action_startFrame,false);
								} else endResizeActionFrame = action_endFrame;
								mouseOverTrack = t;
								arrKeyRatiosLeft = _track.getKeyFrameRatiosInBetween(startResizeActionFrame,resizeActionFrame);
								arrKeyRatiosRight = _track.getKeyFrameRatiosInBetween(resizeActionFrame,endResizeActionFrame);
								arrKeysLeft = _track.getKeyFramesInBetween(startResizeActionFrame,resizeActionFrame);
								arrKeysRight = _track.getKeyFramesInBetween(resizeActionFrame,endResizeActionFrame);
							}
						}
					}
				}
				// draw right anchor if last timeline action
				if(showRightAnchor && isLastAction) {
					Rect rectBoxAnchorRight = new Rect(rectBox.x+rectBox.width-1f,rectBox.y,2f,rectBox.height);
					GUI.DrawTexture(rectBoxAnchorRight,texBoxBorder);
					Rect rectBoxAnchorRightOffset = new Rect(rectBoxAnchorRight);
					rectBoxAnchorRightOffset.width += 6f;
					rectBoxAnchorRightOffset.x -= 3f;
					EditorGUIUtility.AddCursorRect(new Rect(rectBoxAnchorRightOffset.x+1f,rectBoxAnchorRightOffset.y,rectBoxAnchorRightOffset.width-2f,rectBoxAnchorRightOffset.height),MouseCursor.ResizeHorizontal);
					if(rectBoxAnchorRightOffset.Contains(e.mousePosition) && (mouseOverElement == (int)ElementType.None || mouseOverElement == (int)ElementType.TimelineAction)) {
						mouseOverElement = (int)ElementType.ResizeAction;
						if(dragType == (int)DragType.None) {
							startResizeActionFrame = action_startFrame;
							resizeActionFrame = action_endFrame;
							endResizeActionFrame = -1;
							mouseOverTrack = t;
							arrKeyRatiosLeft = _track.getKeyFrameRatiosInBetween(startResizeActionFrame,resizeActionFrame);
							arrKeyRatiosRight = _track.getKeyFrameRatiosInBetween(resizeActionFrame,endResizeActionFrame);
							arrKeysLeft = _track.getKeyFramesInBetween(startResizeActionFrame,resizeActionFrame);
							arrKeysRight = _track.getKeyFramesInBetween(resizeActionFrame,endResizeActionFrame);
						}
					}
				}
				#endregion
				last_action_startFrame = action_startFrame;
			}
			if(!drawEachAction) {
				// timeline action button
				if(GUI.Button(rectTimelineActions,/*new GUIContent("",tooltip)*/"","label") && dragType == (int)DragType.None) {
					int _frame_num_action = (int)aData.getCurrentTake().startFrame+Mathf.CeilToInt(e.mousePosition.x/current_width_frame)-1;
					AMAction _action = _track.getActionContainingFrame(_frame_num_action);
					int prevFrame = aData.getCurrentTake().selectedFrame;
					// timeline select
					timelineSelectFrame(t,_action.startFrame);
					// clear and add frame to context selection
					contextSelectFrame(_action.startFrame,prevFrame);
				}
			}
			
			
		#endregion
		}
		GUI.EndGroup();
	}

	void showInspectorPropertiesFor(Rect rect, int _track, int _frame, Event e) {		
		// if there are no tracks, return
		if(aData.getCurrentTake().getTrackCount() <= 0) return;
	}

	void showObjectFieldFor(AMTrack amTrack, float width_track, Rect rect) {
		if(rect.width < 22f) return;
		// show object field for track, used in OnGUI. Needs to be updated for every track type.
		// add objectfield for every track type
		// translation
		if(amTrack is AMTranslationTrack) {
			(amTrack as AMTranslationTrack).obj = (Transform)EditorGUI.ObjectField(rect,(amTrack as AMTranslationTrack).obj,typeof(Transform),true/*,GUILayout.Width (width_track-padding_track*2)*/);
		}
		// rotation
		else if(amTrack is AMRotationTrack) {
			(amTrack as AMRotationTrack).obj = 	(Transform)EditorGUI.ObjectField(rect,(amTrack as AMRotationTrack).obj,typeof(Transform),true);
		}
		// rotation
		else if(amTrack is AMOrientationTrack) {
			if((amTrack as AMOrientationTrack).setObject((Transform)EditorGUI.ObjectField(rect,(amTrack as AMOrientationTrack).obj,typeof(Transform),true))) {
				amTrack.updateCache();
			}
		}
		// animation
		else if(amTrack is AMAnimationTrack) {
			//GameObject _old = (amTrack as AMAnimationTrack).obj;
			if((amTrack as AMAnimationTrack).setObject((GameObject)EditorGUI.ObjectField(rect,(amTrack as AMAnimationTrack).obj,typeof(GameObject),true))) {
				//Animation _temp = (Animation)(amTrack as AMAnimationTrack).obj.GetComponent("Animation");
				if((amTrack as AMAnimationTrack).obj != null) {
					if((amTrack as AMAnimationTrack).obj.animation == null) {
						(amTrack as AMAnimationTrack).obj = null;
						EditorUtility.DisplayDialog("No Animation Component","You must add an Animation component to the GameObject before you can use it in an Animation Track.","Okay");
					}
				}
			}
		}
		// audio
		else if(amTrack is AMAudioTrack) {
			if((amTrack as AMAudioTrack).setAudioSource((AudioSource)EditorGUI.ObjectField(rect,(amTrack as AMAudioTrack).audioSource,typeof(AudioSource),true))) {
				if((amTrack as AMAudioTrack).audioSource != null) {
					(amTrack as AMAudioTrack).audioSource.playOnAwake = false;
				}
			}
		}
		// property
		else if(amTrack is AMPropertyTrack) {
			GameObject propertyGameObject = (GameObject)EditorGUI.ObjectField(rect,(amTrack as AMPropertyTrack).obj,typeof(GameObject),true);
			if((amTrack as AMPropertyTrack).isObjectUnique(propertyGameObject)) {
				bool changePropertyGameObject = true;
				if((amTrack.keys.Count > 0)&&(!EditorUtility.DisplayDialog("Data Will Be Lost","You will lose all of the keyframes on track '"+amTrack.name+"' if you continue.", "Continue Anway","Cancel"))) {
					changePropertyGameObject = false;
				}
				if(changePropertyGameObject) {
					// delete all keys
					if (amTrack.keys.Count > 0) {
						amTrack.deleteAllKeys();
						amTrack.updateCache();
					}
					(amTrack as AMPropertyTrack).setObject(propertyGameObject);
				}
			}
		}
		// event
		else if(amTrack is AMEventTrack) {
			GameObject eventGameObject = (GameObject)EditorGUI.ObjectField(rect,(amTrack as AMEventTrack).obj,typeof(GameObject),true);
			if((amTrack as AMEventTrack).isObjectUnique(eventGameObject)) {
				bool changeEventGameObject = true;
				if((amTrack.keys.Count > 0)&&(!EditorUtility.DisplayDialog("Data Will Be Lost","You will lose all of the keyframes on track '"+amTrack.name+"' if you continue.", "Continue Anway","Cancel"))) {
					changeEventGameObject = false;
				}
				
				if(changeEventGameObject) {
					// delete all keys
					if (amTrack.keys.Count > 0) {
						amTrack.deleteAllKeys();
						amTrack.updateCache();
					}
					(amTrack as AMEventTrack).setObject(eventGameObject);
				}
			}
		} else if(amTrack is AMCameraSwitcherTrack) {
			// do nothing
		}
		EditorGUIUtility.LookLikeControls();
	}
	void showAlertMissingObjectType(string type) {
		EditorUtility.DisplayDialog("Missing "+type,"You must add a "+type+" to the track before you can add keys.","Okay");
	}

	void drawIndicator(int frame) {
		// draw the indicator texture on the timeline
		int _startFrame = (int)aData.getCurrentTake().startFrame;
		// abort if frame not rendered
		if(frame<_startFrame) return;
		if(frame>(_startFrame+numFramesToRender-1)) return;
		// offset frame based on render start frame
		frame -= _startFrame;
		// draw textures
		GUI.DrawTexture (new Rect(width_track+(frame)*current_width_frame+(current_width_frame/2)-width_indicator_head/2-1f,height_indicator_offset_y,width_indicator_head,height_indicator_head),texIndHead);
		GUI.DrawTexture (new Rect(width_track+(frame)*current_width_frame+(current_width_frame/2)-width_indicator_line/2-1f,height_indicator_offset_y+height_indicator_head,width_indicator_line,position.height-(height_indicator_offset_y+height_indicator_head)-width_scrollbar+2f),texIndLine);
	}
	
	#endregion
	
	#region Process/Calculate
	
	void processDragLogic() {
		#region hand tool acceleration
		if(justFinishedHandDragTicker > 0) {
			justFinishedHandDragTicker--;
			if(justFinishedHandDragTicker <= 0) {				
				handDragAccelaration = (int)((endHandMousePosition.x-currentMousePosition.x)*1.5f);
			}
		}
		#endregion
		bool justStartedDrag = false;
		bool justFinishedDrag = false;
		if(isDragging != cachedIsDragging) {
			if(isDragging) justStartedDrag = true;
			else justFinishedDrag = true;
			cachedIsDragging = isDragging;	
		}
		#region just started drag
		// set start and end drag frames
		if(justStartedDrag) {
			if(isRenamingTrack != -1 ) return;
			#region track / group
			if(mouseOverElement == (int)ElementType.Track) {
				draggingGroupElement = mouseOverGroupElement;
				draggingGroupElementType = mouseOverElement;
				dragType = (int)DragType.GroupElement;
				timelineSelectTrack((int)mouseOverGroupElement.y);
			}
			#endregion
			#region frame
			// if dragged from frame
			else if(mouseOverFrame!=0) {
				// change track if necessary
				if(!aData.getCurrentTake().contextSelectionTracks.Contains(mouseOverTrack) && aData.getCurrentTake().selectedTrack != mouseOverTrack) timelineSelectTrack(mouseOverTrack);

				// if dragged from selected frame, move
				if(mouseOverSelectedFrame) {
					dragType = (int)DragType.MoveSelection;
					aData.getCurrentTake().setGhostSelection();
				} else {
					// else, start context selection
					dragType = (int)DragType.ContextSelection;
				}
				startDragFrame = mouseOverFrame;
				ghostStartDragFrame = startDragFrame;
				endDragFrame = mouseOverFrame;
			#endregion
			#region time scrub
			// if dragged from time scrub
			} else if(mouseOverElement == (int)ElementType.TimeScrub) {
				// set start scrub mouse position
				startScrubMousePosition = currentMousePosition;
				dragType = (int)DragType.TimeScrub;
			#endregion
			#region frame scrub
			} else if(mouseOverElement == (int)ElementType.FrameScrub) {
				// set start scrub mouse position
				startScrubMousePosition = currentMousePosition;
				dragType = (int)DragType.FrameScrub;
			#endregion
			#region resize track
			}else if (mouseOverElement == (int)ElementType.ResizeTrack) {
				startScrubMousePosition = currentMousePosition;
				dragType = (int)DragType.ResizeTrack;
			#endregion
			#region timeline scrub
			}else if (mouseOverElement == (int)ElementType.TimelineScrub) {
				dragType = (int)DragType.TimelineScrub;
			#endregion
			#region resize action
			} else if(mouseOverElement == (int)ElementType.ResizeAction) {
				if(aData.getCurrentTake().selectedTrack != mouseOverTrack) timelineSelectTrack(mouseOverTrack);
				dragType = (int)DragType.ResizeAction;
			#endregion
			#region resize horizontal scrollbar left
			} else if(mouseOverElement == (int)ElementType.ResizeHScrollbarLeft) {
				dragType = (int)DragType.ResizeHScrollbarLeft;
			#endregion
			#region resize horizontal scrollbar right
			} else if(mouseOverElement == (int)ElementType.ResizeHScrollbarRight) {
				dragType = (int)DragType.ResizeHScrollbarRight;
			#endregion
			#region cursor zoom
			} else if(mouseOverElement == (int)ElementType.CursorZoom) {
				startZoomMousePosition = currentMousePosition;
				zoomDirectionMousePosition = currentMousePosition;
				startZoomValue = aData.zoom;
				dragType = (int)DragType.CursorZoom;
				didPeakZoom = false;
			#endregion
			#region cursor hand
			} else if (mouseOverElement == (int)ElementType.CursorHand) {
				//startScrubFrame = mouseXOverFrame;
				justStartedHandGrab = true;
				dragType = (int)DragType.CursorHand;
			#endregion
			} else {
				// if did not drag from a draggable element
				dragType = (int)DragType.None;
			}
			// reset drag
			justStartedDrag = false;
		#endregion
		#region just finished drag
		// if finished drag
		} else if(justFinishedDrag) {
			// if finished drag onto frame x, update end drag frame
			if(mouseXOverFrame!=0) {
				endDragFrame = mouseXOverFrame;	
			}
			// if finished move selection
			if(dragType == (int)DragType.MoveSelection) {
				aData.getCurrentTake().offsetContextSelectionFramesBy(endDragFrame-startDragFrame);
				checkForOutOfBoundsFramesOnSelectedTrack();
				// preview selected frame
				aData.getCurrentTake().previewFrame(aData.getCurrentTake().selectedFrame);
			// if finished context selection
			} else if(dragType == (int)DragType.ContextSelection) {
				contextSelectFrameRange(startDragFrame,endDragFrame);
			// if finished timeline scrub
			} else if(dragType == (int)DragType.TimeScrub || dragType == (int)DragType.FrameScrub) {
				// do nothing
			// if finished dragging group element
			} else if(dragType == (int)DragType.GroupElement) {
				processDropGroupElement(draggingGroupElementType,draggingGroupElement,mouseOverElement,mouseOverGroupElement);
			} else if(dragType == (int)DragType.ResizeAction) {
				aData.getCurrentTake().getSelectedTrack().deleteDuplicateKeys();
				// preview selected frame
				aData.getCurrentTake().previewFrame(aData.getCurrentTake().selectedFrame);
			} else if(dragType == (int)DragType.CursorZoom) {
				tex_cursor_zoom = null;	
			} else if(dragType == (int)DragType.CursorHand) {
				endHandMousePosition = currentMousePosition;
				justFinishedHandDragTicker = 1;
				//startScrubFrame = 0;	
			}
			dragType = (int)DragType.None;
			// reset drag
			justFinishedDrag = false;
		#endregion
		#region is dragging
		// if is dragging
		} else if(isDragging) {
			#region move selection
			// if moving selection, offset selection
			if(dragType == (int)DragType.MoveSelection) {
				if(mouseXOverFrame != endDragFrame) {
					endDragFrame = mouseXOverFrame;
					aData.getCurrentTake().offsetGhostSelectionBy(endDragFrame-ghostStartDragFrame);
					ghostStartDragFrame = endDragFrame;
				}
			#endregion
			#region group element
			} else if(dragType == (int)DragType.GroupElement) {
				scrollViewValue.y += scrollAmountVertical/6f;
			#endregion
			#region context selection
			} else if(dragType == (int)DragType.ContextSelection) {
				if(mouseXOverFrame!=0) {
					endDragFrame = mouseXOverFrame;	
				}
				contextSelectFrameRange(startDragFrame,endDragFrame);
			#endregion
			#region time scrub / frame scrub
			// if is dragging time or frame scrub, set scrub speed
			} else if(dragType == (int)DragType.TimeScrub || dragType == (int)DragType.FrameScrub) {
				setScrubSpeed();
			#endregion
			#region timeline scrub
			} else if(dragType ==(int)DragType.TimelineScrub) {
				int frame = mouseXOverFrame;
				if(frame < (int)aData.getCurrentTake().startFrame) frame = (int)aData.getCurrentTake().startFrame;
				else if (frame > (int)aData.getCurrentTake().endFrame) frame = (int)aData.getCurrentTake().endFrame;
				selectFrame(frame);	
			#endregion
			#region resize action
			// resize action
			} else if(dragType ==(int)DragType.ResizeAction && mouseXOverFrame > 0) {
				AMTrack selTrack = aData.getCurrentTake().getSelectedTrack();
				if(selTrack.hasKeyOnFrame(resizeActionFrame)) {
					AMKey selKey = selTrack.getKeyOnFrame(resizeActionFrame);
					if((startResizeActionFrame == -1 || mouseXOverFrame > startResizeActionFrame) && (endResizeActionFrame == -1 || mouseXOverFrame < endResizeActionFrame)) {
						if(selKey.frame != mouseXOverFrame) {
							
							if(arrKeysLeft.Length > 0 && (mouseXOverFrame-startResizeActionFrame-1) < arrKeysLeft.Length) {
								// do nothing
							} else if(arrKeysRight.Length > 0 && (endResizeActionFrame-mouseXOverFrame-1) < arrKeysRight.Length) {
								// do nothing
							} else {
								selKey.frame = mouseXOverFrame;
								resizeActionFrame = mouseXOverFrame;
								if(arrKeysLeft.Length > 0 && (mouseXOverFrame-startResizeActionFrame-1) <= arrKeysLeft.Length) {
									for(int i=0;i<arrKeysLeft.Length;i++) {
										arrKeysLeft[i].frame = startResizeActionFrame+i+1;
									}
								} else if(arrKeysRight.Length > 0 && (endResizeActionFrame-mouseXOverFrame-1) <= arrKeysRight.Length) {
									for(int i=0;i<arrKeysRight.Length;i++) {
										arrKeysRight[i].frame = resizeActionFrame+i+1;
									}
								} else {
									// update left
									int lastFrame = startResizeActionFrame;
									for(int i=0;i<arrKeysLeft.Length;i++) {
										arrKeysLeft[i].frame = Mathf.FloorToInt((resizeActionFrame-startResizeActionFrame)*arrKeyRatiosLeft[i]+startResizeActionFrame);	
										if(arrKeysLeft[i].frame <= lastFrame) {
											arrKeysLeft[i].frame = lastFrame+1;
											
										}
										if(arrKeysLeft[i].frame >= resizeActionFrame) arrKeysLeft[i].frame = resizeActionFrame-1;			// after last
										lastFrame = arrKeysLeft[i].frame;
									}
									
									// update right
									lastFrame = resizeActionFrame;
									for(int i=0;i<arrKeysRight.Length;i++) {
										arrKeysRight[i].frame = Mathf.FloorToInt((endResizeActionFrame-resizeActionFrame)*arrKeyRatiosRight[i]+resizeActionFrame);	
										if(arrKeysRight[i].frame <= lastFrame) {
											arrKeysRight[i].frame = lastFrame+1;
										}
										if(arrKeysRight[i].frame >= endResizeActionFrame) arrKeysRight[i].frame = endResizeActionFrame-1;	// after last
										lastFrame = arrKeysRight[i].frame;
									}
								}
								// update cache
								selTrack.updateCache();
							}
							
						}
					}
				}
			#endregion
			#region resize horizontal scrollbar left
			} else if(dragType == (int)DragType.ResizeHScrollbarLeft) {
				if(mouseXOverHScrollbarFrame <= 0) aData.getCurrentTake().startFrame = 1;
				else if(mouseXOverHScrollbarFrame > aData.getCurrentTake().numFrames) aData.getCurrentTake().startFrame = aData.getCurrentTake().numFrames;
				else aData.getCurrentTake().startFrame = mouseXOverHScrollbarFrame;
			#endregion
			#region resize horizontal scrollbar right
			}else if(dragType == (int)DragType.ResizeHScrollbarRight) {
				if(mouseXOverHScrollbarFrame <= 0) aData.getCurrentTake().endFrame = 1;
				else if(mouseXOverHScrollbarFrame > aData.getCurrentTake().numFrames) aData.getCurrentTake().endFrame = aData.getCurrentTake().numFrames;
				else aData.getCurrentTake().endFrame = mouseXOverHScrollbarFrame;
				int min = Mathf.FloorToInt((position.width-width_track-18f)/(height_track-height_action_min));
				if(aData.getCurrentTake().startFrame > aData.getCurrentTake().endFrame-min) aData.getCurrentTake().startFrame = aData.getCurrentTake().endFrame-min;
			#endregion
			#region cursor zoom
			} else if(dragType == (int)DragType.CursorZoom) {
				if(didPeakZoom) {
					if(wasZoomingIn && currentMousePosition.x <= cachedZoomMousePosition.x) {
						// direction change	
						startZoomValue = aData.zoom;
						zoomDirectionMousePosition = currentMousePosition;
					} else if(!wasZoomingIn && currentMousePosition.x >= cachedZoomMousePosition.x) {
						// direction change	
						startZoomValue = aData.zoom;
						zoomDirectionMousePosition = currentMousePosition;
					}
					didPeakZoom = false;
				}
				float zoomValue = startZoomValue+(zoomDirectionMousePosition.x-currentMousePosition.x)/300f;
				if(zoomValue < 0f) {
					zoomValue = 0f;
					cachedZoomMousePosition = currentMousePosition;
					wasZoomingIn = true;
					didPeakZoom = true;
				} else if(zoomValue > 1f) {
					zoomValue = 1f;	
					cachedZoomMousePosition = currentMousePosition;
					wasZoomingIn = false;
					didPeakZoom = true;
				}
				if(zoomValue < aData.zoom) tex_cursor_zoom = tex_cursor_zoomin;
				else if(zoomValue > aData.zoom) tex_cursor_zoom = tex_cursor_zoomout;
				aData.zoom = zoomValue;
			}
			#endregion
		}
		#endregion
	}	

	void processHandDragAcceleration() {
		float speed = (int)((Mathf.Clamp(Mathf.Abs(handDragAccelaration),0,200)/12)*(aData.zoom+0.2f));
		if(handDragAccelaration > 0) {
			
			if(aData.getCurrentTake().endFrame < aData.getCurrentTake().numFrames) {
				aData.getCurrentTake().startFrame += speed;
				aData.getCurrentTake().endFrame += speed;
				if(ticker % 2 == 0) handDragAccelaration--;
			} else {
				handDragAccelaration = 0;
			}
		} else if(handDragAccelaration < 0) {
			if(aData.getCurrentTake().startFrame > 1f) {
				aData.getCurrentTake().startFrame -= speed;
				aData.getCurrentTake().endFrame -= speed;
				if(ticker % 2 == 0) handDragAccelaration++;
			} else {
				handDragAccelaration = 0;
			}
		}
	}
	void processDropGroupElement(int sourceType, Vector2 sourceElement, int destType, Vector2 destElement) {
		if(destType == (int)ElementType.Track) {
			if(sourceType == (int)ElementType.Track) {
					// drop track on track
					if((int)sourceElement.y != (int)destElement.y)
						aData.getCurrentTake().moveToGroup((int)sourceElement.y,(int)destElement.x,false,(int)destElement.y);
			}
		} else {
			// drop on window, move to root group
			if(sourceType == (int)ElementType.Track) {
				aData.getCurrentTake().moveToGroup((int)sourceElement.y,0,mouseAboveGroupElements);
			}
		}
		// re-select track to update selected group
		if(sourceType == (int)ElementType.Track) timelineSelectTrack((int)sourceElement.y);
		// scroll to the track
		float scrollTo = -1f;
		scrollTo = aData.getCurrentTake().getElementY((int)sourceElement.y,height_track,height_track_foldin,height_group);
		setScrollViewValue(scrollTo);
	}
	public static void recalculateNumFramesToRender() {
		if(window) window.cachedZoom = -1f;
	}
	void calculateNumFramesToRender(bool clickedZoom, Event e) {
		int min = Mathf.FloorToInt((position.width-width_track-width_scrollbar)/(height_track-height_action_min));
		int _mouseXOverFrame =(int) aData.getCurrentTake().startFrame+Mathf.CeilToInt((e.mousePosition.x-width_track)/current_width_frame)-1;
		// move frames with hand cursor
		if(dragType == (int)DragType.CursorHand && !justStartedHandGrab) {
			if(_mouseXOverFrame != startScrubFrame) {
				float numFrames =  aData.getCurrentTake().endFrame-aData.getCurrentTake().startFrame;
				float dist_hand_drag = startScrubFrame-_mouseXOverFrame;
				aData.getCurrentTake().startFrame +=dist_hand_drag;
				aData.getCurrentTake().endFrame +=dist_hand_drag;
				if(aData.getCurrentTake().startFrame < 1f) {
					aData.getCurrentTake().startFrame = 1f;
					aData.getCurrentTake().endFrame += numFrames-(aData.getCurrentTake().endFrame-aData.getCurrentTake().startFrame);
				} else if(aData.getCurrentTake().endFrame > aData.getCurrentTake().numFrames) {
					aData.getCurrentTake().endFrame = aData.getCurrentTake().numFrames;
					aData.getCurrentTake().startFrame -= numFrames-(aData.getCurrentTake().endFrame-aData.getCurrentTake().startFrame);
				}
			}
		// calculate the number of frames to render based on zoom
		}else if(aData.zoom != cachedZoom && dragType != (int)DragType.ResizeHScrollbarLeft && dragType != (int)DragType.ResizeHScrollbarRight) {
			//numFramesToRender
			numFramesToRender = AMTween.easeInExpo(0f,1f,aData.zoom)*((float)aData.getCurrentTake().numFrames-min)+min;
			// frame dimensions
			current_width_frame = Mathf.Clamp((position.width-width_track- width_scrollbar )/numFramesToRender,0f,(height_track-height_action_min));
			current_height_frame = Mathf.Clamp(current_width_frame*2f,20f,40f);
			float half = 0f;
			// zoom out			
			if(aData.getCurrentTake().endFrame-aData.getCurrentTake().startFrame+1 < Mathf.FloorToInt(numFramesToRender)) {
				if(dragType == (int)DragType.CursorZoom) {
					int newPosFrame = (int)aData.getCurrentTake().startFrame+Mathf.CeilToInt((startZoomMousePosition.x-width_track)/current_width_frame)-1;
					int _diff = startZoomXOverFrame - newPosFrame;
					aData.getCurrentTake().startFrame += _diff;
					aData.getCurrentTake().endFrame += _diff;	
				} else {
					
					half = (((int)numFramesToRender-(aData.getCurrentTake().endFrame-aData.getCurrentTake().startFrame+1))/2f);
					aData.getCurrentTake().startFrame -= Mathf.FloorToInt(half);
					aData.getCurrentTake().endFrame += Mathf.CeilToInt(half);
					// clicked zoom out
					if(clickedZoom) {
						int newPosFrame = (int)aData.getCurrentTake().startFrame+Mathf.CeilToInt((e.mousePosition.x-width_track)/current_width_frame)-1;
						int _diff = _mouseXOverFrame - newPosFrame;
						aData.getCurrentTake().startFrame += _diff;
						aData.getCurrentTake().endFrame += _diff;
					} 
				}
			// zoom in
			} else if(aData.getCurrentTake().endFrame-aData.getCurrentTake().startFrame+1 > Mathf.FloorToInt(numFramesToRender)) {
				//targetPos = ((float)startZoomXOverFrame)/((float)aData.getCurrentTake().endFrame);
				half = (((aData.getCurrentTake().endFrame-aData.getCurrentTake().startFrame+1)-numFramesToRender)/2f);
				//float scrubby_startframe = (float)aData.getCurrentTake().startFrame+half;
				aData.getCurrentTake().startFrame += Mathf.FloorToInt(half);
				aData.getCurrentTake().endFrame -= Mathf.CeilToInt(half);
				int targetFrame = 0;
				// clicked zoom in
				if(clickedZoom) {
					int newPosFrame = (int)aData.getCurrentTake().startFrame+Mathf.CeilToInt((e.mousePosition.x-width_track)/current_width_frame)-1;
					int _diff = _mouseXOverFrame - newPosFrame;
					aData.getCurrentTake().startFrame += _diff;
					aData.getCurrentTake().endFrame += _diff;
				// scrubby zoom in
				} else if(dragType == (int)DragType.CursorZoom) {
					if(dragType != (int)DragType.CursorZoom) {
						// scrubby zoom slider to indicator
						targetFrame = aData.getCurrentTake().selectedFrame;
						float dist_scrubbyzoom = Mathf.Round(targetFrame-Mathf.FloorToInt(aData.getCurrentTake().startFrame+numFramesToRender/2f));
						int offset = Mathf.RoundToInt(dist_scrubbyzoom*(1f-AMTween.linear(0f,1f,aData.zoom)));
						aData.getCurrentTake().startFrame += offset;
						aData.getCurrentTake().endFrame += offset;
					} else {
						// scrubby zoom cursor to mouse position
						int newPosFrame = (int)aData.getCurrentTake().startFrame+Mathf.CeilToInt((startZoomMousePosition.x-width_track)/current_width_frame)-1;
						int _diff = startZoomXOverFrame - newPosFrame;
						aData.getCurrentTake().startFrame += _diff;
						aData.getCurrentTake().endFrame += _diff;
					}
				}
				
			}
			// if beyond boundaries, adjust
			int diff = 0;
			if(aData.getCurrentTake().endFrame > aData.getCurrentTake().numFrames) {
				diff = 	(int)aData.getCurrentTake().endFrame-aData.getCurrentTake().numFrames;
				aData.getCurrentTake().endFrame -= diff;
				aData.getCurrentTake().startFrame += diff;
			} else if(aData.getCurrentTake().startFrame < 1) {
				diff = 	1-(int)aData.getCurrentTake().startFrame;
				aData.getCurrentTake().startFrame -= diff;	
				aData.getCurrentTake().endFrame += diff;
			}
			if(half*2 < (int)numFramesToRender) aData.getCurrentTake().endFrame++;
			cachedZoom = aData.zoom;
			return;
		}
		// calculates the number of frames to render based on window width
		if(aData.getCurrentTake().startFrame < 1) aData.getCurrentTake().startFrame = 1;
		
		if(aData.getCurrentTake().endFrame < aData.getCurrentTake().startFrame+min) aData.getCurrentTake().endFrame = aData.getCurrentTake().startFrame+min;
		if(aData.getCurrentTake().endFrame > aData.getCurrentTake().numFrames) aData.getCurrentTake().endFrame = aData.getCurrentTake().numFrames;
		if(aData.getCurrentTake().startFrame > aData.getCurrentTake().endFrame-min) aData.getCurrentTake().startFrame = aData.getCurrentTake().endFrame-min;
		numFramesToRender = aData.getCurrentTake().endFrame-aData.getCurrentTake().startFrame+1;
		current_width_frame = Mathf.Clamp((position.width-width_track- width_scrollbar)/numFramesToRender,0f,(height_track-height_action_min));
		current_height_frame = Mathf.Clamp(current_width_frame*2f,20f,40f);
		if(dragType == (int)DragType.ResizeHScrollbarLeft || dragType == (int)DragType.ResizeHScrollbarRight) {
			aData.zoom = AMTween.easeInExpoReveresed(0f,1f,(numFramesToRender-min)/((float)aData.getCurrentTake().numFrames-min));
			cachedZoom = aData.zoom;
		}
	}
	
	#endregion
	
	#region Timeline/Timeline Manipulation
	
	void timelineSelectTrack(int _track) {
		// select a track from the timeline
		cancelTextEditting();
		if(aData.getCurrentTake().getTrackCount()<=0) return;
		// select track
		aData.getCurrentTake().selectTrack(_track,isShiftDown,isControlDown);
		// set active object
		timelineSelectObjectFor(aData.getCurrentTake().getTrack(_track));
	}

	void timelineSelectFrame(int _track, int _frame, bool deselectKeyboardFocus=true) {
		// select a frame from the timeline
		cancelTextEditting();
		EventLayout.resetIndexMethodInfo();
		if(aData.getCurrentTake().getTrackCount()<=0) return;
		// select frame
		aData.getCurrentTake().selectFrame(_track,_frame,numFramesToRender, isShiftDown, isControlDown);
		// preview frame
		aData.getCurrentTake().previewFrame(_frame);
		// set active object
		if(_track > -1) timelineSelectObjectFor(aData.getCurrentTake().getTrack(_track));
		// deselect keyboard focus
		if (deselectKeyboardFocus)
			GUIUtility.keyboardControl = 0;

		if(customInspector == null)
			customInspector = ScriptableObject.CreateInstance<CustomInspector>();
		customInspector.ShowUtility();
		customInspector.Repaint();
	}
	void timelineSelectObjectFor(AMTrack track) {
		// translation obj
		if (track.GetType() == typeof(AMTranslationTrack))
			Selection.activeObject = (track as AMTranslationTrack).obj;
		// rotation obj
		else if (track.GetType() == typeof(AMRotationTrack))
			Selection.activeObject = (track as AMRotationTrack).obj;
		else if (track.GetType() == typeof(AMAnimationTrack))
			Selection.activeObject = (track as AMAnimationTrack).obj;
	}
	void timelineSelectNextKey() {
		// select next key
		if(aData.getCurrentTake().getTrackCount()<=0) return;
		if(aData.getCurrentTake().getTrack(aData.getCurrentTake().selectedTrack).keys.Count <= 0) return;
		int frame = aData.getCurrentTake().getTrack(aData.getCurrentTake().selectedTrack).getKeyFrameAfterFrame(aData.getCurrentTake().selectedFrame);
		if(frame <= -1) return;
		timelineSelectFrame(aData.getCurrentTake().selectedTrack,frame);
		
	}
	void timelineSelectPrevKey() {
		// select previous key
		if(aData.getCurrentTake().getTrackCount()<=0) return;
		if(aData.getCurrentTake().getTrack(aData.getCurrentTake().selectedTrack).keys.Count <= 0) return;
		int frame = aData.getCurrentTake().getTrack(aData.getCurrentTake().selectedTrack).getKeyFrameBeforeFrame(aData.getCurrentTake().selectedFrame);
		if(frame <= -1) return;
		timelineSelectFrame(aData.getCurrentTake().selectedTrack,frame);
		
	}
	public void selectFrame(int frame) {
		if(aData.getCurrentTake().selectedFrame != frame) {
			timelineSelectFrame(aData.getCurrentTake().selectedTrack,frame,false);	
		}
	}
	void playerTogglePlay() {
		GUIUtility.keyboardControl = 0;
		cancelTextEditting();
		// toggle player off if is playing
		if(isPlaying) {
			isPlaying = false;
			// select where stopped
			timelineSelectFrame(aData.getCurrentTake().selectedTrack,aData.getCurrentTake().selectedFrame);
			aData.getCurrentTake().stopAudio();
			return;
		}
		// set preview player variables
		playerStartTime = Time.realtimeSinceStartup;
		playerStartFrame = aData.getCurrentTake().selectedFrame;
		// start playing
		isPlaying = true;
		// sample audio from current frame
		aData.getCurrentTake().sampleAudio((float)aData.getCurrentTake().selectedFrame,playbackSpeedValue[aData.getCurrentTake().playbackSpeedIndex]);	
	}
	
	#endregion
	
	#region Set/Get
	

	public static void setDirtyKeys(AMTrack track) {
		foreach(AMKey key in track.keys) {
			EditorUtility.SetDirty(key);	
		}
	}
	public static void setDirtyCache(AMTrack track) {
		foreach(AMAction action in track.cache) {
			EditorUtility.SetDirty(action);	
		}
	}
	public static void setDirtyTracks(AMTake take) {
		foreach(AMTrack track in take.trackValues) {
			EditorUtility.SetDirty(track);	
		}	
	}
	void setScrubSpeed() {
		scrubSpeed = ((currentMousePosition.x-startScrubMousePosition.x))/30;
		
	}	
	void setScrollViewValue(float val) {
		val =  Mathf.Clamp(val,0f,maxScrollView());	
		if(val < scrollViewValue.y || val > scrollViewValue.y+position.height-66f)
			scrollViewValue.y = Mathf.Clamp(val,0f,maxScrollView());	
	}
	// timeline action info
	string getInfoTextForAction(AMTrack _track, AMAction _action, bool brief, int clamped) {
		// get text for track type
		#region translation
		if(_action is AMTranslationAction) {
			return easeTypeNames[(_action as AMTranslationAction).easeType];
		#endregion
		#region rotation
		} else if(_action is AMRotationAction) {
			return easeTypeNames[(_action as AMRotationAction).easeType];
		#endregion
		#region animation
		} else if(_action is AMAnimationAction) {
			if(!(_action as AMAnimationAction).amClip) return "Not Set";
			return (_action as AMAnimationAction).amClip.name+"\n"+((WrapMode)(_action as AMAnimationAction).wrapMode).ToString();
		#endregion
		#region audio
		} else if(_action is AMAudioAction) {
			if(!(_action as AMAudioAction).audioClip) return "Not Set";
			return (_action as AMAudioAction).audioClip.name;	
		#endregion
		#region property
		} else if(_action is AMPropertyAction) {
			string info = (_action as AMPropertyAction).getName() + "\n";
			if((_action as AMPropertyAction).targetsAreEqual()) brief = true;
			if(!brief && (_action as AMPropertyAction).endFrame != -1) {
				info += easeTypeNames[(_action as AMPropertyAction).easeType]+": ";
			}
			string detail;
			if((_action as AMPropertyAction).valueType == (int)AMPropertyTrack.ValueType.MorphChannels) {
				//(_track as AMPropertyTrack).methodInfoMorphNames
				string[] channelNames = (_track as AMPropertyTrack).getMorphNames();
				int startMorphNameIndex = (_action as AMPropertyAction).getStartMorphNameIndex(channelNames.Length);
				string startMorphName;
				if(startMorphNameIndex == -2) startMorphName = "None";
				else startMorphName = (startMorphNameIndex < 0 || startMorphNameIndex >= channelNames.Length ? "Mixed" : channelNames[startMorphNameIndex]);
				detail = startMorphName;
				if(!brief && (_action as AMPropertyAction).endFrame != -1) {
					int endMorphNameIndex = (_action as AMPropertyAction).getEndMorphNameIndex(channelNames.Length);
					string endMorphName;
					if(endMorphNameIndex == -2) endMorphName = "None";
					else endMorphName = (endMorphNameIndex < 0 || endMorphNameIndex >= channelNames.Length ? "Mixed" : channelNames[endMorphNameIndex]);
					detail += " -> "+endMorphName;
				}
			} else {
				detail = (_action as AMPropertyAction).getValueString(brief);	// extra details such as integer values ex. 1 -> 12
			}
			if(detail != null) info += detail;
			return info;
		#endregion
		#region event
		}else if(_action is AMEventAction) {
			if((_action as AMEventAction).methodInfo == null) {
				return "Not Set";
			}
			string txtInfoEvent = (_action as AMEventAction).methodName;
			// include parameters
			if((_action as AMEventAction).parameters != null) {
				 txtInfoEvent += "(";
				for(int i=0;i<(_action as AMEventAction).parameters.Count;i++) {
					if((_action as AMEventAction).parameters[i] == null) txtInfoEvent += "";
					else txtInfoEvent += (_action as AMEventAction).parameters[i].getStringValue();
					if(i<(_action as AMEventAction).parameters.Count-1) txtInfoEvent += ", ";
				}
				
				txtInfoEvent += ")";
				return txtInfoEvent;
			}
			return (_action as AMEventAction).methodName;
		#endregion
		#region orientation
		}else if(_action is AMOrientationAction) {
			if(!(_action as AMOrientationAction).startTarget) return "No Target";
			string txtInfoOrientation = null;
			if((_action as AMOrientationAction).isLookFollow()) {
				txtInfoOrientation = (_action as AMOrientationAction).startTarget.gameObject.name;
				return txtInfoOrientation;
			}
			txtInfoOrientation = (_action as AMOrientationAction).startTarget.gameObject.name +
			" -> " + ((_action as AMOrientationAction).endTarget ? (_action as AMOrientationAction).endTarget.gameObject.name : "No Target");
			txtInfoOrientation += "\n"+easeTypeNames[(_action as AMOrientationAction).easeType];
			return txtInfoOrientation;
		#endregion
		#region camera switcher
		}else if(_action is AMCameraSwitcherAction) {
			if(!(_action as AMCameraSwitcherAction).hasStartTarget()) return "None";
			string txtInfoCameraSwitcher = null;
			if((_action as AMCameraSwitcherAction).targetsAreEqual() || clamped != 0) {
				txtInfoCameraSwitcher = (_action as AMCameraSwitcherAction).getStartTargetName();
				return txtInfoCameraSwitcher;
			}
			txtInfoCameraSwitcher = (_action as AMCameraSwitcherAction).getStartTargetName() +
			" -> " + (_action as AMCameraSwitcherAction).getEndTargetName();
			txtInfoCameraSwitcher += "\n"+AMTween.TransitionNamesDict[((_action as AMCameraSwitcherAction).cameraFadeType > AMTween.TransitionNamesDict.Length ? 0 : (_action as AMCameraSwitcherAction).cameraFadeType)];
			if((_action as AMCameraSwitcherAction).cameraFadeType != (int)AMTween.Fade.None) txtInfoCameraSwitcher += ": "+easeTypeNames[(_action as AMCameraSwitcherAction).easeType];
			return txtInfoCameraSwitcher;
		}
		#endregion
		return "Unknown";
	}

	public Texture getTrackIconTexture(AMTrack _track) {
		if(_track is AMAnimationTrack) return texIconAnimation;
		else if(_track is AMEventTrack ) return texIconEvent;
		else if(_track is AMPropertyTrack) return texIconProperty;
		else if(_track is AMTranslationTrack) return texIconTranslation;
		else if(_track is AMAudioTrack) return texIconAudio;
		else if(_track is AMRotationTrack) return texIconRotation;	
		else if(_track is AMOrientationTrack) return texIconOrientation;	
		else if(_track is AMCameraSwitcherTrack) return texIconCameraSwitcher;
		
		Debug.LogWarning("Animator: Icon texture not found for track "+_track.getTrackType());
		return null;
	}
	Vector2 getGlobalMousePosition(Event e) {
		Vector2 convertedGUIPos  = GUIUtility.GUIToScreenPoint(e.mousePosition);
		convertedGUIPos.x -= position.x;
		convertedGUIPos.y -= position.y;
		return convertedGUIPos;
	}
	public static EditorWindow GetMainGameView()
	{
	    System.Type T = System.Type.GetType("UnityEditor.GameView,UnityEditor");
	    System.Reflection.MethodInfo GetMainGameView = T.GetMethod("GetMainGameView",System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static);
	    System.Object Res = GetMainGameView.Invoke(null,null);
	    return (EditorWindow)Res;
	}
	public static Rect GetMainGameViewPosition()
	{
		return GetMainGameView().position;	
	}
	
	#endregion
	
	#region Add/Delete

	public void addTarget(object key, bool withTranslationTrack) {
		AMTrack sTrack = aData.getCurrentTake().getSelectedTrack();
		AMOrientationKey oKey = key as AMOrientationKey;
		// create target
		GameObject target = new GameObject(getNewTargetName());
		target.transform.position = (sTrack as AMOrientationTrack).obj.position + (sTrack as AMOrientationTrack).obj.forward*5f;
		target.AddComponent(typeof(AMTarget));
		// set target
		oKey.setTarget(target.transform);
		// update cache
		sTrack.updateCache();
		// preview new frame
		aData.getCurrentTake().previewFrame(oKey.frame);
		setDirtyKeys(sTrack);
		setDirtyCache(sTrack);
		// refresh component
		refreshGizmos();
		
		// add to translation track
		if(withTranslationTrack) {
			objects_window = new List<GameObject>();	
			objects_window.Add(target);
			AMTimeline.window.addTrack((int)AMTimeline.Track.Translation);
		}
	}
	private static string getNewTargetName() {
		int count = 1;
		while (true) {
			if(GameObject.Find("Target"+count)) count++;
			else break;
		}
		return "Target"+count;
	}

	public void addTrack(object trackType) {
		// add one null GameObject if no GameObject dragged onto timeline (when clicked on new track button)
		if(objects_window.Count <= 0) {
			objects_window.Add(null);
		}
		foreach(GameObject object_window in objects_window) {
			addTrackWithGameObject(trackType, object_window);
		}
		objects_window = new List<GameObject>();
		timelineSelectTrack(aData.getCurrentTake().track_count);
		// move scrollview to last created track
		setScrollViewValue(aData.getCurrentTake().getElementY(aData.getCurrentTake().selectedTrack,height_track,height_track_foldin,height_group));
	}
	
	void addTrackWithGameObject(object trackType, GameObject object_window) {
		// add track based on index
		switch((int) trackType) {
			case (int)Track.Translation:
				aData.getCurrentTake().addTranslationTrack(object_window);
				break;
			case (int)Track.Rotation:
				aData.getCurrentTake().addRotationTrack(object_window);
				break;
			case (int)Track.Orientation:
				aData.getCurrentTake().addOrientationTrack(object_window);
				break;
			case (int)Track.Animation:
				aData.getCurrentTake().addAnimationTrack(object_window);
				break;
			case (int)Track.Audio:
				aData.getCurrentTake().addAudioTrack(object_window);
				break;
			case (int)Track.Property:
				aData.getCurrentTake().addPropertyTrack(object_window);
				break;
			case (int)Track.Event:
				aData.getCurrentTake().addEventTrack(object_window);
				break;
			case (int)Track.CameraSwitcher:
				if(object_window == null && aData.getCurrentTake().cameraSwitcher) {
					// already exists
					EditorUtility.DisplayDialog("Camera Switcher Already Exists", "You can only have one Camera Switcher track. Transition between cameras by adding keyframes to the track.","Okay");
				} else {
					// add track
					aData.getCurrentTake().addCameraSwitcherTrack(object_window);
					// preview selected frame
					if(object_window != null) {
						aData.getCurrentTake().previewFrame(aData.getCurrentTake().selectedFrame);
					}
				}
				break;
			default:
				break;
		}	
	}
	void addKeyToFrame(int frame) {
		// add key if there are tracks
		if(aData.getCurrentTake().getTrackCount()>0) {
			// add a key
			addKey(aData.getCurrentTake().selectedTrack,frame);
			// preview current frame
			//aData.getCurrentTake().previewFrame(aData.getCurrentTake().selectedFrame);
			// save data
			setDirtyKeys(aData.getCurrentTake().getTrack(aData.getCurrentTake().selectedTrack));
			setDirtyCache(aData.getCurrentTake().getTrack(aData.getCurrentTake().selectedTrack));
			// refresh component
			refreshGizmos();
		}
		timelineSelectFrame(aData.getCurrentTake().selectedTrack,aData.getCurrentTake().selectedFrame,false);	
	}
	void addKeyToSelectedFrame() {
		// add key if there are tracks
		if(aData.getCurrentTake().getTrackCount()>0) {
			// add a key
			addKey(aData.getCurrentTake().selectedTrack,aData.getCurrentTake().selectedFrame);
			// preview current frame
			aData.getCurrentTake().previewFrame(aData.getCurrentTake().selectedFrame);
			// save data
			setDirtyKeys(aData.getCurrentTake().getTrack(aData.getCurrentTake().selectedTrack));
			setDirtyCache(aData.getCurrentTake().getTrack(aData.getCurrentTake().selectedTrack));
			// refresh component
			refreshGizmos();
		}
		timelineSelectFrame(aData.getCurrentTake().selectedTrack,aData.getCurrentTake().selectedFrame,false);
	}
	void addKey(int _track, int _frame) {
		// add a key to the track number and frame, used in OnGUI. Needs to be updated for every track type.
		AMTrack amTrack = aData.getCurrentTake().getTrack (_track);
		// translation
		if(amTrack is AMTranslationTrack) {
			// if missing object, return
			if(!(amTrack as AMTranslationTrack).obj){
				showAlertMissingObjectType("Transform");
				return;
			}
			(amTrack as AMTranslationTrack).addKey(_frame,(amTrack as AMTranslationTrack).obj.position);
		}else if(amTrack is AMRotationTrack) {
			// rotation
			
			// if missing object, return
			if(!(amTrack as AMRotationTrack).obj){
				showAlertMissingObjectType("Transform");
				return;
			}
			// add key to rotation track
			(amTrack as AMRotationTrack).addKey (_frame,(amTrack as AMRotationTrack).obj.rotation);
		}else if(amTrack is AMOrientationTrack) {
			// orientation
			
			// if missing object, return
			if(!(amTrack as AMOrientationTrack).obj){
				showAlertMissingObjectType("Transform");
				return;
			}
			// add key to orientation track
			Transform last_target = null;
			int last_key = (amTrack as AMOrientationTrack).getKeyFrameBeforeFrame(_frame,false);
			if(last_key == -1) last_key = (amTrack as AMOrientationTrack).getKeyFrameAfterFrame(_frame,false);
			if(last_key != -1) {
				AMOrientationKey _oKey = ((amTrack as AMOrientationTrack).getKeyOnFrame(last_key) as AMOrientationKey);
				last_target = _oKey.target;
			}
			(amTrack as AMOrientationTrack).addKey (_frame,last_target);
		}else if(amTrack is AMAnimationTrack) {
			// animation
			
			// if missing object, return
			if(!(amTrack as AMAnimationTrack).obj){
				showAlertMissingObjectType("GameObject");
				return;
			}
			// add key to animation track
			(amTrack as AMAnimationTrack).addKey (_frame,(amTrack as AMAnimationTrack).obj.animation.clip,WrapMode.Once);
		} else if(amTrack is AMAudioTrack) {
			// audio
			
			// if missing object, return
			if(!(amTrack as AMAudioTrack).audioSource){
				showAlertMissingObjectType("AudioSource");
				return;
			}
			// add key to animation track
			(amTrack as AMAudioTrack).addKey (_frame,null,false);
			
		} else if(amTrack is AMPropertyTrack) {
			// property
			
			// if missing object, return
			if(!(amTrack as AMPropertyTrack).obj){
				showAlertMissingObjectType("GameObject");
				return;
			}
			// if missing property, return
			if(!(amTrack as AMPropertyTrack).isPropertySet()){
				EditorUtility.DisplayDialog("Property Not Set","You must set the track property before you can add keys.","Okay");
				return;
			}
			(amTrack as AMPropertyTrack).addKey (_frame);
		} else if(amTrack is AMEventTrack) {
			// event
			
			// if missing object, return
			if(!(amTrack as AMEventTrack).obj){
				showAlertMissingObjectType("GameObject");
				return;
			}
			// add key to event track
			(amTrack as AMEventTrack).addKey (_frame);
		} else if(amTrack is AMCameraSwitcherTrack) {
			// camera switcher
			AMCameraSwitcherKey _cKey = null;
			int last_key = (amTrack as AMCameraSwitcherTrack).getKeyFrameBeforeFrame(_frame,false);
			if(last_key == -1) last_key = (amTrack as AMCameraSwitcherTrack).getKeyFrameAfterFrame(_frame,false);
			if(last_key != -1) {
				_cKey = ((amTrack as AMCameraSwitcherTrack).getKeyOnFrame(last_key) as AMCameraSwitcherKey);
			}
			// add key to camera switcher
			(amTrack as AMCameraSwitcherTrack).addKey (_frame, null, _cKey);
		}
	}
	void deleteKeyFromSelectedFrame() {
		aData.getCurrentTake().getSelectedTrack().deleteKeyOnFrame(aData.getCurrentTake().selectedFrame);
		aData.getCurrentTake().getSelectedTrack().updateCache();
		// save data
		setDirtyKeys(aData.getCurrentTake().getSelectedTrack());
		setDirtyCache (aData.getCurrentTake().getSelectedTrack());
		// select current frame
		timelineSelectFrame(aData.getCurrentTake().selectedTrack,aData.getCurrentTake().selectedFrame);
		// refresh gizmos
		refreshGizmos();	
	}
	void deleteSelectedKeys(bool showWarning) {
		bool shouldClearFrames = true;
		if(showWarning) {
			if(aData.getCurrentTake().contextSelectionTracks.Count > 1) {
				if(!EditorUtility.DisplayDialog("Clear From Multiple Tracks?","Are you sure you want to clear the selected frames from all of the selected tracks?","Clear Frames","Cancel")) {
					shouldClearFrames = false;
				}
			}
		}
		if(shouldClearFrames) {
			foreach(int track_id in aData.getCurrentTake().contextSelectionTracks) {
				aData.getCurrentTake().deleteSelectedKeysFromTrack(track_id);	
			}
		}
		//aData.getCurrentTake().deleteSelectedKeys();
		// save data
		setDirtyKeys(aData.getCurrentTake().getSelectedTrack());
		setDirtyCache (aData.getCurrentTake().getSelectedTrack());
		// select current frame
		timelineSelectFrame(aData.getCurrentTake().selectedTrack,aData.getCurrentTake().selectedFrame,false);
		// refresh gizmos
		refreshGizmos();
	}
	
	#endregion
	
	#region Menus/Context
	
	void addTrackFromMenu(object type) {
		addTrack((int)type);	
	}
	void buildAddTrackMenu() {
		menu.AddItem(new GUIContent("Translation"),false,addTrackFromMenu,(int)Track.Translation);
		menu.AddItem(new GUIContent("Rotation"),false,addTrackFromMenu,(int)Track.Rotation);
		menu.AddItem(new GUIContent("Orientation"),false,addTrackFromMenu,(int)Track.Orientation);
		menu.AddItem(new GUIContent("Animation"),false,addTrackFromMenu,(int)Track.Animation);
		menu.AddItem(new GUIContent("Audio"),false,addTrackFromMenu,(int)Track.Audio);
		menu.AddItem(new GUIContent("Property"),false,addTrackFromMenu,(int)Track.Property);
		menu.AddItem(new GUIContent("Event"),false,addTrackFromMenu,(int)Track.Event);
		menu.AddItem(new GUIContent("Camera Switcher"),false,addTrackFromMenu,(int)Track.CameraSwitcher);	
	}
	void buildAddTrackMenu_Drag() {
		bool hasTransform = true;
		bool hasAnimation = true;
		bool hasAudioSource = true;
		bool hasCamera = true;
		
		foreach(GameObject g in objects_window) {
			// break loop when all variables are false
			if(!hasTransform && !hasAnimation && !hasAudioSource && !hasCamera) break;
			
			if(hasTransform && !g.GetComponent(typeof(Transform))) {
				hasTransform = false;	
			}
			if(hasAnimation && !g.GetComponent(typeof(Animation))) {
				hasAnimation = false;	
			}
			if(hasAudioSource && !g.GetComponent(typeof(AudioSource))) {
				hasAudioSource = false;	
			}
			if(hasCamera && !g.GetComponent(typeof(Camera))) {
				hasCamera = false;	
			}
		}
		// add track menu
		menu_drag = new GenericMenu();
	
		// Translation
		if(hasTransform) menu_drag.AddItem(new GUIContent("Translation"),false,addTrackFromMenu,(int)Track.Translation);
		else menu_drag.AddDisabledItem(new GUIContent("Translation"));
		// Rotation
		if(hasTransform) menu_drag.AddItem(new GUIContent("Rotation"),false,addTrackFromMenu,(int)Track.Rotation);
		else menu_drag.AddDisabledItem(new GUIContent("Rotation"));
		// Orientation
		if(hasTransform) menu_drag.AddItem(new GUIContent("Orientation"),false,addTrackFromMenu,(int)Track.Orientation);
		else menu_drag.AddDisabledItem(new GUIContent("Orientation"));
		// Animation
		if(hasAnimation) menu_drag.AddItem(new GUIContent("Animation"),false,addTrackFromMenu,(int)Track.Animation);
		else menu_drag.AddDisabledItem(new GUIContent("Animation"));
		// Audio
		if(hasAudioSource) menu_drag.AddItem(new GUIContent("Audio"),false,addTrackFromMenu,(int)Track.Audio);
		else menu_drag.AddDisabledItem(new GUIContent("Audio"));
		// Property
		menu_drag.AddItem(new GUIContent("Property"),false,addTrackFromMenu,(int)Track.Property);
		// Event
		menu_drag.AddItem(new GUIContent("Event"),false,addTrackFromMenu,(int)Track.Event);
		// Camera Switcher
		if(hasCamera) menu_drag.AddItem(new GUIContent("Camera Switcher"+(aData.getCurrentTake().cameraSwitcher != null ? " (Key)" : "")),false,addTrackFromMenu,(int)Track.CameraSwitcher);
		else menu_drag.AddDisabledItem(new GUIContent("Camera Switcher"+(aData.getCurrentTake().cameraSwitcher != null ? " (Key)" : "")));
	}
	bool canQuickAddCombo(List<int> combo, bool hasTransform, bool hasAnimation, bool hasAudioSource, bool hasCamera) {
		foreach(int _track in combo) {
			if(!hasTransform && (_track == (int)Track.Translation || _track == (int)Track.Rotation|| _track == (int)Track.Orientation))
				return false;
			else if(!hasAnimation && _track == (int)Track.Animation)
				return false;
			else if(!hasAudioSource && _track == (int)Track.Audio)
				return false;
			else if(!hasCamera && _track == (int)Track.CameraSwitcher)
				return false;
		}
		return true;
	}
	void buildContextMenu(int frame) {
		contextMenuFrame = frame;
		contextMenu = new GenericMenu();
		bool selectionHasKeys = aData.getCurrentTake().contextSelectionTracks.Count > 1 || aData.getCurrentTake().contextSelectionHasKeys();
		bool copyBufferNotEmpty = (contextSelectionKeysBuffer.Count>0);
		bool canPaste = false;
		bool singleTrack = contextSelectionKeysBuffer.Count == 1;
		AMTrack selectedTrack = aData.getCurrentTake().getSelectedTrack();
		if(copyBufferNotEmpty) {
			if(singleTrack) {
				// if origin is property track
				if(selectedTrack is AMPropertyTrack) {
					// if pasting into property track
					if(contextSelectionTracksBuffer[0] is AMPropertyTrack) {
						// if property tracks have the same property
						if ((selectedTrack as AMPropertyTrack).hasSamePropertyAs((contextSelectionTracksBuffer[0] as AMPropertyTrack))) {
							canPaste = true;
						}
					}
				// if origin is event track
				}else if(selectedTrack is AMEventTrack) {
					// if pasting into event track
					if(contextSelectionTracksBuffer[0] is AMEventTrack) {
						// if event tracks are compaitable
						if ((selectedTrack as AMEventTrack).hasSameEventsAs((contextSelectionTracksBuffer[0] as AMEventTrack))) {
							canPaste = true;
						}
					}
				} else {
					if(selectedTrack.getTrackType() == contextSelectionTracksBuffer[0].getTrackType()) {
						canPaste = true;
					}	
				}
			} else {
				// to do
				if(contextSelectionTracksBuffer.Contains(selectedTrack)) canPaste = true;
			}
		}
		contextMenu.AddItem(new GUIContent("Insert Keyframe"),false,invokeContextMenuItem,0);	
		contextMenu.AddSeparator("");
		if(selectionHasKeys) {
			contextMenu.AddItem(new GUIContent("Cut Frames"),false,invokeContextMenuItem,1);
			contextMenu.AddItem(new GUIContent("Copy Frames"),false,invokeContextMenuItem,2);
			if(canPaste) contextMenu.AddItem(new GUIContent("Paste Frames"),false,invokeContextMenuItem,3);
			else contextMenu.AddDisabledItem(new GUIContent("Paste Frames"));
			contextMenu.AddItem(new GUIContent("Clear Frames"),false,invokeContextMenuItem,4);
		} else {
			contextMenu.AddDisabledItem(new GUIContent("Cut Frames"));
			contextMenu.AddDisabledItem(new GUIContent("Copy Frames"));
			if(canPaste) contextMenu.AddItem(new GUIContent("Paste Frames"),false,invokeContextMenuItem,3);
			else contextMenu.AddDisabledItem(new GUIContent("Paste Frames"));
			contextMenu.AddDisabledItem(new GUIContent("Clear Frames"));
		}
		contextMenu.AddItem(new GUIContent("Select All Frames"),false,invokeContextMenuItem,5);
	}
	void invokeContextMenuItem(object _index) {
		int index = (int) _index;
		// insert keyframe
		if(index == 0)	{
			addKeyToFrame(contextMenuFrame);
			selectFrame(contextMenuFrame);
		}
		else if(index == 1) contextCutKeys();
		else if(index == 2) contextCopyFrames();
		else if(index == 3)	contextPasteKeys();
		else if(index == 4) deleteSelectedKeys(true);
		else if(index == 5) contextSelectAllFrames();
	}
	void contextCutKeys() {
		contextCopyFrames();
		deleteSelectedKeys(false);
	}
	void contextPasteKeys() {
		if(contextSelectionKeysBuffer == null || contextSelectionKeysBuffer.Count < 0) return;
		
		bool singleTrack = contextSelectionKeysBuffer.Count == 1;
		int offset = (int)contextSelectionRange.y-(int)contextSelectionRange.x+1;
		
		if(singleTrack) {
			aData.getCurrentTake().getSelectedTrack().offsetKeysFromBy(contextMenuFrame,offset);
			// add buffer keys to track
			foreach(AMKey a in contextSelectionKeysBuffer[0]) {
				// offset keys based on selection range
				a.frame += (contextMenuFrame-(int)contextSelectionRange.x);
				aData.getCurrentTake().getSelectedTrack().keys.Add(a);
				//a.destroy();
			}
		} else {
			for(int i=0;i<contextSelectionTracksBuffer.Count;i++) {
				// offset all keys beyond paste
				contextSelectionTracksBuffer[i].offsetKeysFromBy(contextMenuFrame,offset);
				// add buffer keys to track
				foreach(AMKey a in contextSelectionKeysBuffer[i]) {
					// offset keys based on selection range
					a.frame += (contextMenuFrame-(int)contextSelectionRange.x);
					contextSelectionTracksBuffer[i].keys.Add(a);
					//a.destroy();
				}
			}
		}
		
		// show message if there are out of bounds keys
		checkForOutOfBoundsFramesOnSelectedTrack();
		// update cache
		if(singleTrack) {
			aData.getCurrentTake().getSelectedTrack().updateCache();
		} else {
			for(int i=0;i<contextSelectionTracksBuffer.Count;i++) {
				contextSelectionTracksBuffer[i].updateCache();
			}
		}
		// clear buffer
		contextSelectionKeysBuffer = new List<List<AMKey>>();
		contextSelectionTracksBuffer = new List<AMTrack>();
		// update selection
		//   retrieve cached context selection 
		aData.getCurrentTake().contextSelection = new List<int>();
		foreach(int frame in cachedContextSelection) {
			aData.getCurrentTake().contextSelection.Add(frame);	
		}
		// offset selection
		for(int i = 0;i<aData.getCurrentTake().contextSelection.Count;i++) {
			aData.getCurrentTake().contextSelection[i] += (contextMenuFrame-(int)contextSelectionRange.x);
			
		}
		// copy again for multiple pastes
		contextCopyFrames();
	}
	
	void contextSaveKeysToBuffer() {
		if(aData.getCurrentTake().contextSelection.Count<=0) return;
		// sort
		aData.getCurrentTake().contextSelection.Sort();
		// set selection range
		contextSelectionRange.x = aData.getCurrentTake().contextSelection[0];
		contextSelectionRange.y = aData.getCurrentTake().contextSelection[aData.getCurrentTake().contextSelection.Count-1];
		// set selection track
		//contextSelectionTrack = aData.getCurrentTake().selectedTrack;
		
		if(contextSelectionKeysBuffer != null) {
			foreach(List<AMKey> ls in contextSelectionKeysBuffer) {
				foreach(AMKey key in ls) {
					if(key == null) continue;
					key.destroy();
				}
			}
		}
		contextSelectionKeysBuffer = new List<List<AMKey>>();
		aData.getCurrentTake().contextSelectionTracks.Sort();
		contextSelectionTracksBuffer = new List<AMTrack>();

		foreach(int track_id in aData.getCurrentTake().contextSelectionTracks) {
			contextSelectionTracksBuffer.Add(aData.getCurrentTake().getTrack(track_id));	
		}
		foreach(AMTrack track in contextSelectionTracksBuffer) {
			contextSelectionKeysBuffer.Add(new List<AMKey>());
			foreach(AMKey key in aData.getCurrentTake().getContextSelectionKeysForTrack(track)) {
				AMKey a = key.CreateClone();
				contextSelectionKeysBuffer[contextSelectionKeysBuffer.Count-1].Add(a);
			}
		}
	}
	
	void contextCopyFrames() {
		cachedContextSelection = new List<int>();
		// cache context selection
		foreach(int frame in aData.getCurrentTake().contextSelection) {
			cachedContextSelection.Add(frame);	
		}
		// save keys
		contextSaveKeysToBuffer();
	}
	void contextSelectAllFrames() {
		aData.getCurrentTake().contextSelectAllFrames();
	}
	public void contextSelectFrame(int frame, int prevFrame) {		
		// select range if shift down
		if(isShiftDown) {
			// if control is down, toggle
			aData.getCurrentTake().contextSelectFrameRange(prevFrame,frame);
			return;	
		}
		// clear context selection if control is not down
		if(!isControlDown) aData.getCurrentTake().contextSelection = new List<int>();
		// select single, toggle if control is down
		aData.getCurrentTake().contextSelectFrame(frame,isControlDown);
		//contextSelectFrameRange(frame,frame);
	}
	public void contextSelectFrameRange(int startFrame, int endFrame) {
		// clear context selection if control is not down
		if(isShiftDown) {
			aData.getCurrentTake().contextSelectFrameRange(aData.getCurrentTake().selectedFrame,endFrame);
			return;
		}
		if(!isControlDown) aData.getCurrentTake().contextSelection = new List<int>();

		aData.getCurrentTake().contextSelectFrameRange(startFrame,endFrame);
	}
	public void clearContextSelection() {
		aData.getCurrentTake().contextSelection = new List<int>();
	}
	
	#endregion
	
	#region Other Fns
	
	public int findWidthFontSize(float width, GUIStyle style, GUIContent content, int min = 8, int max = 15) {
		// finds the largest font size that can fit in the given style. max 15
		style.fontSize = max;
		while(style.CalcSize(content).x > width) {
			style.fontSize--;
			if(style.fontSize <= min) break;
		}
		return style.fontSize;
	}
	public int findHeightFontSize(float height, GUIStyle style, GUIContent content, int min = 8, int max = 15) {
		style.fontSize = max;
		while(style.CalcSize(content).y > height) {
			style.fontSize--;
			if(style.fontSize <= min) break;
		}
		return style.fontSize;
	}
	public bool rectContainsMouse(Rect rect, Vector2 mousePosition) {
		if(mousePosition.x < rect.x || mousePosition.x > rect.x + rect.width) return false;
		if(mousePosition.y < rect.y || mousePosition.y > rect.y + rect.height) return false;
		return true;
	}
	public bool didDoubleClick(string elementID) {
		if(doubleClickElementID != elementID) {
			doubleClickCachedTime = EditorApplication.timeSinceStartup;
			doubleClickElementID = elementID;
			return false;
		}
		if(EditorApplication.timeSinceStartup-doubleClickCachedTime <= doubleClickTime) {
			doubleClickElementID = null;
			return true;
		} else {
			doubleClickCachedTime = EditorApplication.timeSinceStartup;
			return false;
		}
	}

	void checkForOutOfBoundsFramesOnSelectedTrack() {
		List<AMTrack> selectedTracks = new List<AMTrack>();
		int shift = 1;
		AMTrack _track_shift = null;
		int increase = 0;
		AMTrack _track_increase = null;
		foreach(int track_id in aData.getCurrentTake().contextSelectionTracks) {
			AMTrack track = aData.getCurrentTake().getTrack(track_id);
			selectedTracks.Add(track);
			if(track.keys.Count >= 1) {
				if(track.keys[0].frame < shift) {
					shift =  track.keys[0].frame;
					_track_shift = track;
				}
				if(track.keys[track.keys.Count-1].frame-aData.getCurrentTake().numFrames > increase) {
					increase = track.keys[track.keys.Count-1].frame-aData.getCurrentTake().numFrames;
					_track_increase = track;
				}
			}
		}
		if(_track_shift != null) {
			if(EditorUtility.DisplayDialog("Shift Frames?", "Keyframes have been moved out of bounds before the first frame. Some data will be lost if frames are not shifted.","Shift","No")) {	
				aData.getCurrentTake().shiftOutOfBoundsKeysOnTrack(_track_shift);
			} else {
				// delete all keys beyond last frame
				aData.getCurrentTake().deleteKeysBefore(1);
			}
		}
		if(_track_increase != null) {
			if(EditorUtility.DisplayDialog("Increase Number of Frames?", "Keyframes have been pushed out of bounds beyond the last frame. Some data will be lost if the number of frames is not increased.","Increase","No")) {
				aData.getCurrentTake().numFrames = _track_increase.keys[_track_increase.keys.Count-1].frame;
			} else {
				// delete all keys beyond last frame
				foreach(AMTrack track in selectedTracks)
					track.deleteKeysAfter(aData.getCurrentTake().numFrames);
			}	
		}
	}
	void checkForOutOfBoundsFramesOnTrack(int track_id) {
		AMTrack _track = aData.getCurrentTake().getTrack(track_id);
		if(_track.keys.Count<=0) return;
		bool needsShift = false;
		bool needsIncrease = false;
		_track.sortKeys();
		// if first key less than 1
		if(_track.keys.Count >= 1 && _track.keys[0].frame < 1) {
			needsShift = true;
		}
		if(needsShift) {
			if(EditorUtility.DisplayDialog("Shift Frames?", "Keyframes have been moved out of bounds before the first frame. Some data will be lost if frames are not shifted.","Shift","No")) {	
				aData.getCurrentTake().shiftOutOfBoundsKeysOnSelectedTrack();
			} else {
				// delete all keys beyond last frame
				aData.getCurrentTake().deleteKeysBefore(1);
			}
		}
		// if last key is beyond last frame
		if(_track.keys.Count >=1 && _track.keys[_track.keys.Count-1].frame > aData.getCurrentTake().numFrames) {
			needsIncrease = true;
		}
		if(needsIncrease) {
			if(EditorUtility.DisplayDialog("Increase Number of Frames?", "Keyframes have been pushed out of bounds beyond the last frame. Some data will be lost if the number of frames is not increased.","Increase","No")) {
				aData.getCurrentTake().numFrames = _track.keys[_track.keys.Count-1].frame;
			} else {
				// delete all keys beyond last frame
				_track.deleteKeysAfter(aData.getCurrentTake().numFrames);
			}
		}
	}
	void resetPreview() {
		// reset all object transforms to frame 1
		aData.getCurrentTake().previewFrame(1f);
	}
	public void refreshGizmos() {
		EditorUtility.SetDirty(aData);
	}
	void cancelTextEditting(bool toggleIsRenamingTake = false) {
		if(!isChangingTimeControl && !isChangingFrameControl) {
			try {
			if(GUIUtility.keyboardControl!=0) GUIUtility.keyboardControl = 0;
			} catch {
				
			}
		}
		if(isRenamingTrack != -1) {
			isRenamingTrack = -1;
		}
	}

	string trimString(string _str, int max_chars) {
		if(_str.Length<=max_chars) return _str;
		return _str.Substring(0,max_chars)+"...";
	}

	float maxScrollView() {
		return height_all_tracks;
	}
	#endregion
	
	#endregion
}