using System.Collections;
//using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;
using System.Collections.Generic; //for lists. Its a generic thing.

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

	public int m_minPlayers = 1;
	int m_maxPlayers = 4;

	[SyncVar] //all syncvars have server authority.
	public int m_playerCount = 0;

	//Had to make this static to get it to work. Got array index out of bounds, even though I gave it 0. LOOK.
	public static Color[] m_playerColors = {Color.red, Color.blue, Color.green, Color.magenta};

	static GameManager instance;

	public List<PlayerController> m_allPlayers;

	public List<Text> m_nameLabelText;
	public List<Text> m_playerScoreText;

	public int m_maxScore = 3;

	[SyncVar]
	bool m_gameOver = false;

	PlayerController m_winner;

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

		StartCoroutine ("GameLoopRoutine");

	}

	IEnumerator EnterLobby(){

		DisablePlayers ();

		while(m_playerCount < m_minPlayers){
			UpdateMessage ("Waiting for players...");

			yield return null;
		}

	}
	IEnumerator PlayGame(){
		DisablePlayers ();
		yield return new WaitForSeconds(1f);
		UpdateMessage ("3");
		yield return new WaitForSeconds(1f);
		UpdateMessage ("2");
		yield return new WaitForSeconds(1f);
		UpdateMessage ("1");
		yield return new WaitForSeconds(1f);
		UpdateMessage ("FIGHT");

		EnablePlayers ();
		UpdateScoreboard ();
		yield return new WaitForSeconds(1f);
		UpdateMessage ("");

		PlayerController winner = null;
		//loops and yields?
		while (m_gameOver == false) {
			yield return null;
		}
	}
	IEnumerator EndGame(){
		DisablePlayers ();
		UpdateMessage ("GAME OVER\n" + m_winner.m_pSetup.m_playerNameText.text + " wins!");
		Reset ();
		yield return new WaitForSeconds(3f);
		UpdateMessage ("Restarting...");
		yield return new WaitForSeconds(3f);
	}

	[ClientRpc]
	void Rpc_SetPlayerState(bool state){

		PlayerController[] allPlayers = GameObject.FindObjectsOfType<PlayerController>();
		foreach(PlayerController p in allPlayers){
			p.enabled = state; //looks like enabled is not a function. High level thing.
		}


	}

	void EnablePlayers(){

		if (isServer) {
			Rpc_SetPlayerState (true);
		}
	}

	void DisablePlayers(){

		if (isServer) {
			Rpc_SetPlayerState (false);
		}
	}

	public void AddPlayer(PlayerSetup pSetup){

		if(m_playerCount < m_maxPlayers){

			m_allPlayers.Add (pSetup.GetComponent<PlayerController> ());
			Debug.Log (m_playerCount);
			pSetup.m_playerColor = m_playerColors [m_playerCount];
			pSetup.m_playerNum = m_playerCount + 1;
		}

	}


	//RPC are invoked on server run on clients.
	//As opposed to commands, which are invoked on the clients, run on the server.
	[ClientRpc]
	void Rpc_UpdateScoreboard(string[] playerNames, int[] playerScores){

		for(int i = 0; i <m_playerCount; i++){
			if(playerNames[i] != null){
				m_nameLabelText [i].text = playerNames [i];

			}
			if (playerScores [i] != null) {

				m_playerScoreText [i].text = playerScores [i].ToString();

			}
				

		}


	}

	public void UpdateScoreboard(){

		if (isServer) {

			m_winner = GetWinner ();
			if(m_winner != null){
				m_gameOver = true;
			}

			string[] names = new string[m_playerCount];
			int[] scores = new int[m_playerCount];

			for(int i = 0; i < m_playerCount; i++){
				names [i] = m_allPlayers [i].GetComponent<PlayerSetup> ().m_playerNameText.text;
				scores [i] = m_allPlayers [i].m_score;
			}

			Rpc_UpdateScoreboard (names, scores);


		}

	}

	[ClientRpc]
	void Rpc_UpdateMessage (string msg)
	{
		if (m_messageText != null) {
			m_messageText.gameObject.SetActive (true);
			m_messageText.text = msg;
		}
	}

	public void UpdateMessage(string msg){
		if(isServer){
			Rpc_UpdateMessage (msg);
		}
	}

	PlayerController GetWinner(){

		if (isServer) {
			for (int i = 0; i < m_playerCount; i++) {

				if (m_allPlayers [i].m_score >= m_maxScore) {
					return m_allPlayers [i];
				}
			}
		}
		return null;
	}

	void Reset(){

		if(isServer){
			Rpc_Reset ();
			UpdateScoreboard ();
			m_winner = null;
			m_gameOver = false;
		}
	}

	[ClientRpc]
	void Rpc_Reset(){
		PlayerController[] allPlayers = GameObject.FindObjectsOfType<PlayerController> ();
		foreach(PlayerController p in allPlayers){
			p.m_score = 0;
		}
		
	}
}
