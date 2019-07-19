using UnityEngine;
using System.Collections;

public class QuestTriggerC : MonoBehaviour {
	//This Script use for multiple quests in 1 NPC
	public GameObject[] questClients = new GameObject[2];
	public int questStep = 0;
	private bool enter = false;
	//public Texture2D button;
	private GameObject player;
	private GameObject questData;

	/*void Update(){
		if(Input.GetKeyDown("e") && enter){
			bool  q = questClients[questStep].GetComponent<QuestClientC>().ActivateQuest(player);
			if(q && questStep < questClients.Length){
				questClients[questStep].GetComponent<QuestClientC>().enter = false; //Reset Enter Variable of last client
				questStep++;
				if(questStep >= questClients.Length){
					questStep = questClients.Length -1;
					return;
				}
				questClients[questStep].GetComponent<QuestClientC>().s = 0;
				enter = true;
				questClients[questStep].GetComponent<QuestClientC>().enter = true;
			}
		}
	}*/

	public void Talking(){
		bool q = questClients[questStep].GetComponent<QuestClientC>().ActivateQuest(player);
		if(q && questStep < questClients.Length){
			questClients[questStep].GetComponent<QuestClientC>().enter = false; //Reset Enter Variable of last client
			questStep++;
			if(questStep >= questClients.Length){
				questStep = questClients.Length -1;
				return;
			}
			questClients[questStep].GetComponent<QuestClientC>().s = 0;
			enter = true;
			questClients[questStep].GetComponent<QuestClientC>().enter = true;
		}
	}
	
	/*void OnGUI(){
		if(!player){
			return;
		}
		if(enter && !GlobalConditionC.interacting && !GlobalConditionC.freezeAll){
			//GUI.DrawTexture( new Rect(Screen.width / 2 - 130, Screen.height - 120, 260, 80), button);
			if(GUI.Button(new Rect(Screen.width / 2 - 130, Screen.height - 180, 260, 80), button)){
				bool  q = questClients[questStep].GetComponent<QuestClientC>().ActivateQuest(player);
				if(q && questStep < questClients.Length){
					questClients[questStep].GetComponent<QuestClientC>().enter = false; //Reset Enter Variable of last client
					questStep++;
					if(questStep >= questClients.Length){
						questStep = questClients.Length -1;
						return;
					}
					questClients[questStep].GetComponent<QuestClientC>().s = 0;
					enter = true;
					questClients[questStep].GetComponent<QuestClientC>().enter = true;
				}
			}
		}
	}*/
	
	void OnTriggerEnter(Collider other){
		if(other.tag == "Player"){
			player = other.gameObject;
			CheckQuestSequence();
			
			questClients[questStep].GetComponent<QuestClientC>().s = 0;
			enter = true;
			questClients[questStep].GetComponent<QuestClientC>().enter = true;

			if(player.GetComponent<AttackTriggerC>())
				player.GetComponent<AttackTriggerC>().GetActivator(this.gameObject , "Talking" , "Talk");
		}
	}
	
	void OnTriggerExit(Collider other){
		if(other.tag == "Player"){
			questClients[questStep].GetComponent<QuestClientC>().s = 0;
			enter = false;
			questClients[questStep].GetComponent<QuestClientC>().enter = false;

			if(player.GetComponent<AttackTriggerC>())
				player.GetComponent<AttackTriggerC>().RemoveActivator(this.gameObject);
		}
	}
	
	public void CheckQuestSequence(){
		bool  c = true;
		while(c == true){
			int id = questClients[questStep].GetComponent<QuestClientC>().questId;
			questData = questClients[questStep].GetComponent<QuestClientC>().questData;
			int qprogress = player.GetComponent<QuestStatC>().questProgress[id]; //Check Queststep
			int finish =	questData.GetComponent<QuestDataC>().questData[id].finishProgress;
			if(qprogress >= finish + 9){ 
				questStep++;
				if(questStep >= questClients.Length){
					questStep = questClients.Length -1;
					c = false; // End Loop
				}
			}else{
				c = false; // End Loop
			}
		}
		//print("Quest Sequence = " + questStep);
	}
}