using UnityEngine;
using System.Collections;

public class SkillButtonC : MonoBehaviour {

	public int skillID = 0;
	public GameObject player;
	public float posX = 20.0f;
	public float posY = 20.0f;
	public float size = 120.0f;
	
	public Texture2D downTexture;
	[HideInInspector]
	public Texture originalTexture;
	private bool onMobile = false;
	
	void Start () {
		if(!player){
			player = GameObject.FindWithTag("Player");
		}
		this.transform.parent = null;
		this.transform.position = new Vector3(0,1,0);
		DontDestroyOnLoad (transform.gameObject);
		originalTexture = GetComponent<GUITexture>().texture;
		GetComponent<GUITexture>().pixelInset = new Rect (Screen.width -size - posX, -Screen.height +posY, size, size);
		CheckPlatform();
	}
	
	public void CheckPlatform() {
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
			//if(guiTexture.HitTest(touch.position) && touch.phase == TouchPhase.Began){
			//if(GetComponent<GUITexture>().HitTest(touch.position)){
			if(GetComponent<GUITexture>().HitTest(touch.position)){
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
		//Push Attack Button
		player.GetComponent<AttackTriggerC>().TriggerSkill(skillID);
		if(downTexture){
			GetComponent<GUITexture>().texture = downTexture;
			yield return new WaitForSeconds(0.1f);
			GetComponent<GUITexture>().texture = originalTexture;
		}
		
	}
}