using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

/*Singleton Patter
 * allows global access from any other component. 
 * 	Means anything inside this component will be accessible to anyone. Flies in the face of OOP. 
 * Dont use unless you have very carefully considered it. 
 * 
 * refer to GameManager via     GameManager.Instance
 * only permits one component of GameManager to exist per Scene. Hence "Singleton"
 * 
 * 
 */

//not adding "persist on load" because we only have one scene in this game. See "music player" in laser defender for this usage.

//NetworkLobbyManager is a thing
public class GameManager : NetworkBehaviour {
	/*Main game loop. 
	 * If you want this to happen in order, odds are you want to use a Coroutine to create a sequence of game states.
	 * One main coroutine, then 3 sub coroutines inside of this. 
	 * 	EX:	
	 * 		yield return new WaitForSeconds(3f);
	 * 		yield return null;
	 * 		yield return StartCoroutine("someCoroutineName"); //you can yield to another coroutine!
	 * //learn about these keywords.
	 * 
	 */



	public Text m_messageText;

	int m_minPlayers = 2;
	int m_maxPlayers = 4;

	[SyncVar] //all syncvars have server authority.
	public int m_playerCount = 0;

	//Had to make this static to get it to work. Got array index out of bounds, even though I gave it 0. LOOK.
	public static Color[] m_playerColors = {Color.red, Color.blue, Color.green, Color.magenta};

	static GameManager instance;

	//property. A property is a field with a get function and a set function. Leaving off "set" here though.
	public static GameManager Instance{

		get{
			if(instance == null){
				instance = GameObject.FindObjectOfType<GameManager> ();
				if(instance == null){
					instance = new GameObject ().AddComponent<GameManager> ();
				}
			}
			return instance;
		}
			

	}


	//look into Awake
	void Awake(){

		if(instance == null){
			instance = this;
		}
		else{
			Destroy (gameObject); //one other already exists, destroy ourselves.
		}

	}

	void Start(){
		StartCoroutine ("GameLoopRoutine");
	}

	IEnumerator GameLoopRoutine(){
		//IEnumerators MUST have a yield statement inside

		yield return StartCoroutine ("EnterLobby");
		yield return StartCoroutine ("PlayGame");
		yield return StartCoroutine ("EndGame");


	}

	IEnumerator EnterLobby(){

		if(m_messageText != null){
			m_messageText.gameObject.SetActive (true);
			m_messageText.text = "Waiting for players...";
		}

		while(m_playerCount < m_minPlayers){
			DisablePlayers ();
			yield return null;
		}

	}
	IEnumerator PlayGame(){

		EnablePlayers ();
		if(m_messageText != null){
			m_messageText.gameObject.SetActive (false);

		}


		yield return null;
	}
	IEnumerator EndGame(){
		yield return null;
	}

	void SetPlayerState(bool state){

		PlayerController[] allPlayers = GameObject.FindObjectsOfType<PlayerController>();
		foreach(PlayerController p in allPlayers){
			p.enabled = state; //looks like enabled is not a function. High level thing.
		}


	}

	void EnablePlayers(){
		SetPlayerState (true);

	}

	void DisablePlayers(){
		SetPlayerState (false);
	}

	public void AddPlayer(PlayerSetup pSetup){

		if(m_playerCount < m_maxPlayers){
			Debug.Log (m_playerCount);
			pSetup.m_playerColor = m_playerColors [m_playerCount];
			pSetup.m_playerNum = m_playerCount + 1;
		}

	}
}
