using UnityEngine;
using System.Collections;

public class OpenGuiButtonC : MonoBehaviour {

	public GameObject player;
	public float posX = 20.0f;
	public float posY = 20.0f;
	public float size = 120.0f;
	
	public Texture2D downTexture;
	private Texture originalTexture;
	private bool onMobile = false;
	
	public enum GuiType {
		Inventory = 0,
		Skill = 1,
		Status = 2,
		Quest = 3
	}

	public GuiType open = GuiType.Inventory;
	
	public enum ButtonPos {
		TopLeft = 0,
		TopMiddle = 1,
		TopRight = 2,
		BottomLeft = 3,
		BottomMiddle = 4,
		BottomRight = 5,
		MiddleLeft = 6,
		MiddleRight = 7,
		Middle = 8
	}

	public ButtonPos alignment = ButtonPos.TopLeft;
	
	void Start() {
		if(!player){
			player = GameObject.FindWithTag("Player");
		}
		this.transform.parent = null;	// Set parent to null and Reset Position for GUI Texture
		this.transform.position = new Vector3(0,1,0);
		DontDestroyOnLoad (transform.gameObject);
		originalTexture = GetComponent<GUITexture>().texture;
		SetPosition();
		CheckPlatform();
	}
	
	public void CheckPlatform(){
		if(Application.platform == RuntimePlatform.Android || Application.platform == RuntimePlatform.IPhonePlayer){
			onMobile = true;
		}else{
			onMobile = false;
		}
		
	}
	
	void Update() {
		if(!player){
			Destroy(gameObject);
		}
		
		int count = Input.touchCount;
		
		for(int i = 0;i < count; i++){
			Touch touch = Input.GetTouch(i);
			if(GetComponent<GUITexture>().HitTest(touch.position) && touch.phase == TouchPhase.Began){
				StartCoroutine(Activate());
			}
		}
	}
	
	void OnMouseDown() {
		if(onMobile){
			return;
		}
		//Push Attack Button
		StartCoroutine(Activate());

	}
	
	IEnumerator Activate() {
		//Push Button
		if(open == GuiType.Inventory){
			if(!player.GetComponent<InventoryC>().useLegacyUi && player.GetComponent<UiMasterC>()){
				player.GetComponent<UiMasterC>().OnOffInventoryMenu();
			}else{
				player.GetComponent<InventoryC>().OnOffMenu();
			}
		}else if(open == GuiType.Skill){
			if(!player.GetComponent<SkillWindowC>().useLegacyUi && player.GetComponent<UiMasterC>()){
				player.GetComponent<UiMasterC>().OnOffSkillMenu();
			}else{
				player.GetComponent<SkillWindowC>().OnOffMenu();
			}
		}else if(open == GuiType.Status){
			if(!player.GetComponent<StatusWindowC>() && player.GetComponent<UiMasterC>() || !player.GetComponent<StatusWindowC>().enabled && player.GetComponent<UiMasterC>()){
				player.GetComponent<UiMasterC>().OnOffStatusMenu();
			}else{
				player.GetComponent<StatusWindowC>().OnOffMenu();
			}
		}else if(open == GuiType.Quest){
			player.GetComponent<QuestStatC>().OnOffMenu();
		}
		
		if(downTexture){
			GetComponent<GUITexture>().texture = downTexture;
			yield return new WaitForSeconds(0.1f);
			GetComponent<GUITexture>().texture = originalTexture;
		}
		
	}
	
	public void SetPosition() {
		//Set GUI Texture Position up to Alignment you choose.
		if(alignment == ButtonPos.TopLeft){
			//Top Left
			GetComponent<GUITexture>().pixelInset = new Rect (posX, -size -posY, size, size);
		}else if(alignment == ButtonPos.TopMiddle){
			//Top Middle
			GetComponent<GUITexture>().pixelInset = new Rect (Screen.width /2 -size /2 + posX, -size -posY, size, size);
		}else if(alignment == ButtonPos.TopRight){
			//Top Right
			GetComponent<GUITexture>().pixelInset = new Rect (Screen.width -size - posX, -size -posY, size, size);
		}else if(alignment == ButtonPos.BottomLeft){
			//Buttom Left
			GetComponent<GUITexture>().pixelInset = new Rect (posX, -Screen.height +posY, size, size);
		}else if(alignment == ButtonPos.BottomMiddle){
			//Buttom Middle
			GetComponent<GUITexture>().pixelInset = new Rect (Screen.width /2 -size /2 + posX, -Screen.height +posY, size, size);
		}else if(alignment == ButtonPos.BottomRight){
			//Buttom Right
			GetComponent<GUITexture>().pixelInset = new Rect (Screen.width -size - posX, -Screen.height +posY, size, size);
		}else if(alignment == ButtonPos.MiddleLeft){
			//Middle Left
			GetComponent<GUITexture>().pixelInset = new Rect (posX, -Screen.height /2 -posY, size, size);
		}else if(alignment == ButtonPos.MiddleRight){
			//Middle Right
			GetComponent<GUITexture>().pixelInset = new Rect (Screen.width -size - posX, -Screen.height /2 -posY, size, size);
		}else if(alignment == ButtonPos.Middle){
			//Middle
			GetComponent<GUITexture>().pixelInset = new Rect (Screen.width /2 -size /2 + posX, -Screen.height /2 -posY, size, size);
		}
	}
}