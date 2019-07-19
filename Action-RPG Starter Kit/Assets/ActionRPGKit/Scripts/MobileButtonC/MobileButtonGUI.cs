using UnityEngine;
using System.Collections;

public class MobileButtonGUIC : MonoBehaviour {

	public Texture2D attackIcon;
	public Texture2D skillIcon1;
	public Texture2D skillIcon2;
	public Texture2D skillIcon3;
	
	void Start() {
		
	}
	
	void Update() {
		
	}
	
	void OnGUI() {
		if (GUI.RepeatButton(new Rect(Screen.width -185 ,Screen.height -180,120,120),attackIcon)){
			GetComponent<AttackTriggerC>().TriggerAttack();
		}
		if (GUI.RepeatButton(new Rect(Screen.width -250 ,Screen.height -265,80,80),skillIcon1)){
			GetComponent<AttackTriggerC>().TriggerSkill(0);
		}
		if (GUI.RepeatButton(new Rect(Screen.width -170 ,Screen.height -265,80,80),skillIcon2)){
			GetComponent<AttackTriggerC>().TriggerSkill(1);
		}
		if (GUI.RepeatButton(new Rect(Screen.width -90 ,Screen.height -265,80,80),skillIcon3)){
			GetComponent<AttackTriggerC>().TriggerSkill(2);
		}
		
	}
}
