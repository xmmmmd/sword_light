﻿using UnityEngine;
using System.Collections;

[RequireComponent (typeof(GUITexture))]

public class MobileJoyStickC : MonoBehaviour {

	//////////////////////////////////////////////////////////////
	// Joystick.js
	// Modify from Penelope iPhone Tutorial
	//
	// Joystick creates a movable joystick (via GUITexture) that
	// handles touch input, taps, and phases. Dead zones can control
	// where the joystick input gets picked up and can be normalized.
	//
	// Optionally, you can enable the touchPad property from the editor
	// to treat this Joystick as a TouchPad. A TouchPad allows the finger
	// to touch down at any point and it tracks the movement relatively
	// without moving the graphic
	//////////////////////////////////////////////////////////////
			
	public GameObject player;
	// A simple class for bounding how far the GUITexture will move
	[System.Serializable]
	public class Boundary {
		public Vector2 min = Vector2.zero;
		public Vector2 max = Vector2.zero;
	}
	
	static private MobileJoyStickC[] joysticks; // A static collection of all joysticks
	static private bool enumeratedJoysticks = false;
	static private float tapTimeDelta = 0.3f; // Time allowed between taps
	
	public bool touchPad; // Is this a TouchPad?
	public Rect touchZone;
	public Vector2 deadZone = Vector2.zero; // Control when position is output
	public bool normalize = false; // Normalize output after the dead-zone?
	public Vector2 position; // [-1, 1] in x,y
	public int tapCount; // Current tap count

	private int lastFingerId = -1; // Finger last used for this joystick
	private float tapTimeWindow; // How much time there is left for a tap to occur
	private Vector2 fingerDownPos;
	//private float fingerDownTime;
	//private float firstDeltaTime = 0.5f;
	
	private GUITexture gui; // Joystick graphic
	private Rect defaultRect; // Default position / extents of the joystick graphic
	private Boundary guiBoundary = new Boundary(); // Boundary for joystick graphic
	private Vector2 guiTouchOffset; // Offset to apply to touch input
	private Vector2 guiCenter; // Center of joystick
	public Transform joyBackground;
	public bool alignRightScreen = false;
	
	void Start() {
		if(!player){
			player = GameObject.FindWithTag("Player");
		}
		// Cache this component at startup instead of looking up every frame
		gui = GetComponent<GUITexture>();
		//gameObject.tag = "JoyStick";
		//DontDestroyOnLoad (transform.gameObject);
		Vector2 originalInset = new Vector2(gui.pixelInset.x, gui.pixelInset.y);
		Rect insetRect = gui.pixelInset;

		insetRect.x = originalInset.x;
		insetRect.y = originalInset.y;

		if(alignRightScreen){
			insetRect.x = Screen.width - gui.pixelInset.x - gui.pixelInset.width;
			gui.pixelInset = insetRect;
		}

		//Reset GUI Pixel Inset to original position
		gui.pixelInset = insetRect;

		transform.parent = null;
		Vector3 tempPosition = transform.position;

		tempPosition.x = 0.0f;
		tempPosition.y = 0.0f;
		transform.position = tempPosition;

		if(joyBackground){
			joyBackground.parent = null;
			DontDestroyOnLoad (joyBackground.gameObject);
			tempPosition.x = 0.0f;
			tempPosition.y = 0.0f;
			tempPosition.z = transform.position.z - 2;
			joyBackground.position = tempPosition;

			if(alignRightScreen){
				GUITexture jgui = joyBackground.GetComponent<GUITexture>();
				Rect jRect = jgui.pixelInset;
				jRect.x = Screen.width - jgui.pixelInset.x - jgui.pixelInset.width;
				jgui.pixelInset = jRect;
			}
		}
		DontDestroyOnLoad (transform.gameObject);
		// Store the default rect for the gui, so we can snap back to it
		defaultRect = gui.pixelInset;
		defaultRect.x += transform.position.x * Screen.width;// + gui.pixelInset.x; // - Screen.width * 0.5;
		defaultRect.y += transform.position.y * Screen.height;// - Screen.height * 0.5;
		
		if(touchPad){
			// If a texture has been assigned, then use the rect ferom the gui as our touchZone
			if(gui.texture)
				touchZone = defaultRect;
		}else{
			// This is an offset for touch input to match with the top left
			// corner of the GUI
			guiTouchOffset.x = defaultRect.width * 0.5f;
			guiTouchOffset.y = defaultRect.height * 0.5f;
			
			// Cache the center of the GUI, since it doesn't change
			guiCenter.x = defaultRect.x + guiTouchOffset.x;
			guiCenter.y = defaultRect.y + guiTouchOffset.y;
			
			// Let's build the GUI boundary, so we can clamp joystick movement
			guiBoundary.min.x = defaultRect.x - guiTouchOffset.x;
			guiBoundary.max.x = defaultRect.x + guiTouchOffset.x;
			guiBoundary.min.y = defaultRect.y - guiTouchOffset.y;
			guiBoundary.max.y = defaultRect.y + guiTouchOffset.y;
		}
	}
	
	void Disable() {
		//gameObject.active = false;
		gameObject.SetActive(false);
		enumeratedJoysticks = false;
	}
	
	public void ResetJoystick() {
		// Release the finger control and set the joystick back to the default position
		gui.pixelInset = defaultRect;
		lastFingerId = -1;
		position = Vector2.zero;
		fingerDownPos = Vector2.zero;
		
		if (touchPad) {
			Color tempColor = gui.color;
			tempColor.a = 0.025f;
			gui.color = tempColor;
		}
	}
	
	public bool IsFingerDown() {
		return (lastFingerId != -1);
	}
	
	public void LatchedFinger(int fingerId) {
		// If another joystick has latched this finger, then we must release it
		if ( lastFingerId == fingerId )
			ResetJoystick();
	}
	
	void Update() {
		if(!player){
			if(joyBackground){
				Destroy(joyBackground.gameObject);
			}
			Destroy(gameObject);
		}
		
		//-----------------------------------------------
		for (var k = 0; k < Input.touchCount; ++k) {
			Touch toucha = Input.GetTouch(k);
			if (toucha.position.x < (Screen.width/2) && toucha.phase == TouchPhase.Ended || toucha.position.x < (Screen.width/2) && toucha.phase == TouchPhase.Canceled ){
				ResetJoystick();
			}
			//--------------
			if (toucha.phase == TouchPhase.Began) {
				if (toucha.position.x > (Screen.width/2) && toucha.phase == TouchPhase.Began) {
					return;
				}
			}
		}
		//-----------------------------------------------
		
		if (!enumeratedJoysticks ){
			// Collect all joysticks in the game, so we can relay finger latching messages
			joysticks = FindObjectsOfType<MobileJoyStickC>();

			enumeratedJoysticks = true;
		}
		int count = 0;
		
		if (Application.platform != RuntimePlatform.IPhonePlayer){
			if(Input.GetMouseButton(0)){
				count = 1;
				//Debug.Log("Mouse button: " + count);
			}
		}else{
			count = Input.touchCount;
		}
		//var count = Input.touchCount;
		// Adjust the tap time window while it still available
		if ( tapTimeWindow > 0 )
			tapTimeWindow -= Time.deltaTime;
		else
			tapCount = 0;
		
		if ( count == 0 )
			ResetJoystick();
		else{
			for(int i = 0;i < count; i++){
				Touch touch = new Touch();
				Vector2 guiTouchPos;
				int fingerID;
				Vector2 touchPosition;
				
				if(Application.platform == RuntimePlatform.Android){
					touch = Input.GetTouch(i);
					guiTouchPos = touch.position - guiTouchOffset;
					fingerID = touch.fingerId;
					touchPosition = touch.position;
				}else if (Application.platform != RuntimePlatform.IPhonePlayer){
					guiTouchPos = new Vector2(Input.mousePosition.x, Input.mousePosition.y) - guiTouchOffset;
					fingerID = 1;
					touchPosition = Input.mousePosition;
				}else{
					touch = Input.GetTouch(i);
					guiTouchPos = touch.position - guiTouchOffset;
					fingerID = touch.fingerId;
					touchPosition = touch.position;
				}
				var shouldLatchFinger = false;
				if ( touchPad ){
					if ( touchZone.Contains( touchPosition ) )
						shouldLatchFinger = true;
				}else if ( gui.HitTest( touchPosition ) ){
					shouldLatchFinger = true;
				}
				
				// Latch the finger if this is a new touch
				if ( shouldLatchFinger && (lastFingerId == -1 || lastFingerId != fingerID)){
					if ( touchPad ){
						Color tempColor = gui.color;
						tempColor.a = 0.15f;
						gui.color = tempColor;
						
						lastFingerId = fingerID;
						fingerDownPos = touchPosition;
						//fingerDownTime = Time.time;
					}
					
					lastFingerId = fingerID;
					
					// Accumulate taps if it is within the time window
					if ( tapTimeWindow > 0 )
						tapCount++;
					else{
						tapCount = 1;
						tapTimeWindow = tapTimeDelta;
					}		
					// Tell other joysticks we've latched this finger
					foreach (MobileJoyStickC j in joysticks ){
						if ( j != this )
							j.LatchedFinger( fingerID );
					}
				}
				
				if ( lastFingerId == fingerID ){
					// Override the tap count with what the iPhone SDK reports if it is greater
					// This is a workaround, since the iPhone SDK does not currently track taps
					// for multiple touches
					if (Application.platform == RuntimePlatform.IPhonePlayer){
						if ( touch.tapCount > tapCount )
							tapCount = touch.tapCount;
					}
					
					if ( touchPad ){
						// For a touchpad, let's just set the position directly based on distance from initial touchdown
						position.x = Mathf.Clamp( ( touchPosition.x - fingerDownPos.x ) / ( touchZone.width / 2 ), -1, 1 );
						position.y = Mathf.Clamp( ( touchPosition.y - fingerDownPos.y ) / ( touchZone.height / 2 ), -1, 1 );
					}else{
						// Change the location of the joystick graphic to match where the touch is
						Rect tempInset = gui.pixelInset;
						tempInset.x = Mathf.Clamp( guiTouchPos.x, guiBoundary.min.x, guiBoundary.max.x );
						tempInset.y = Mathf.Clamp( guiTouchPos.y, guiBoundary.min.y, guiBoundary.max.y );
						gui.pixelInset = tempInset;
					}
					
					if (Application.platform != RuntimePlatform.IPhonePlayer){
						if (!Input.GetMouseButton(0)){
							ResetJoystick();
							Debug.Log("Joystick Reset.");
						}
					}else{
						if ( touch.phase == TouchPhase.Ended || touch.phase == TouchPhase.Canceled ){
							ResetJoystick();
							Debug.Log("Joystick Reset.");
						}
					}
				}
			}
		}
		
		if ( !touchPad ){
			// Get a value between -1 and 1 based on the joystick graphic location
			position.x = ( gui.pixelInset.x + guiTouchOffset.x - guiCenter.x ) / guiTouchOffset.x;
			position.y = ( gui.pixelInset.y + guiTouchOffset.y - guiCenter.y ) / guiTouchOffset.y;
		}
		
		// Adjust for dead zone
		var absoluteX = Mathf.Abs( position.x );
		var absoluteY = Mathf.Abs( position.y );
		
		if ( absoluteX < deadZone.x ){
			// Report the joystick as being at the center if it is within the dead zone
			position.x = 0;
		}else if ( normalize ){
			// Rescale the output after taking the dead zone into account
			position.x = Mathf.Sign( position.x ) * ( absoluteX - deadZone.x ) / ( 1 - deadZone.x );
		}
		
		if ( absoluteY < deadZone.y ){
			// Report the joystick as being at the center if it is within the dead zone
			position.y = 0;
		}else if ( normalize ){
			// Rescale the output after taking the dead zone into account
			position.y = Mathf.Sign( position.y ) * ( absoluteY - deadZone.y ) / ( 1 - deadZone.y );
		}
		/*if(touchPosition.x > Screen.width /2){
			ResetJoystick();
		}*/
		//print(touchPosition);
	}
}