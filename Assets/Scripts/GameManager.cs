using System.Collections;
//using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;
using System.Collections.Generic; //for lists. Its a generic thing.
using Prototype.NetworkLobby;

/* Singleton Pattern
 * allows global access from any other component. 
 * Means anything inside this component will be accessible to anyone. Flies in the face of OOP. 
 * Dont use unless you have very carefully considered it. 
 * 
 * refer to GameManager via: GameManager.Instance
 * only permits one component of GameManager to exist per Scene. Hence "Singleton"
 */

//not adding "persist on load" because only have one scene in this game. See "music player" in laser defender for this usage.

public class GameManager : NetworkBehaviour {
	/*Main game loop. 
	 * If you want this to happen in order, odds are you want to use a Coroutine to create a sequence of game states.
	 * One main coroutine, then 3 sub coroutines inside of this. 
	 * 	EX:	
	 * 		yield return new WaitForSeconds(3f);
	 * 		yield return null;
	 * 		yield return StartCoroutine("someCoroutineName"); //you can yield to another coroutine!
	 * 
	 */

	public Text m_messageText;

	static GameManager instance;

	public static List<PlayerManager> m_allPlayers = new List<PlayerManager> ();

	public List<Text> m_playerNameText;
	public List<Text> m_playerScoreText;

	public int m_maxScore = 3;

	[SyncVar]
	bool m_gameOver = false;

	PlayerManager m_winner;

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

	[Server] //this method will only run on the server. 
	void Start(){
		StartCoroutine ("GameLoopRoutine");
	}

	IEnumerator GameLoopRoutine(){
		//IEnumerators MUST have a yield statement inside

		LobbyManager lobbyManager = LobbyManager.s_Singleton;

		if(lobbyManager != null){

			while(m_allPlayers.Count < lobbyManager._playerNumber){
				yield return null; //waits one frame?
			}

			yield return new WaitForSeconds (2f);

			//these correspond to methods below. 
			yield return StartCoroutine ("StartGame");
			yield return StartCoroutine ("PlayGame");
			yield return StartCoroutine ("EndGame");

			StartCoroutine ("GameLoopRoutine");
		}
		else{
			Debug.LogWarning ("GAME MANAGER WARNING: Launch game from lobby scene only!!!");
		}
	}

	[ClientRpc]  //server invokes
	void RpcStartGame () {
		UpdateMessage ("FIGHT");
		DisablePlayers();
	}

	IEnumerator StartGame(){

		Reset ();
		RpcStartGame ();
		UpdateScoreBoard ();  
		yield return new WaitForSeconds(3f);


	}

	[ClientRpc]
	void RpcPlayGame (){
		EnablePlayers ();
   		UpdateMessage ("");
	}

	IEnumerator PlayGame(){
		
		yield return new WaitForSeconds(1f);
		RpcPlayGame ();
		// PlayerManager winner = null;
		while (m_gameOver == false) {
			CheckScores ();
			yield return null;
		}
	}

	[ClientRpc] //invoked on server, run on client. Remote procedure call. 
	void RpcEndGame (){
		DisablePlayers ();
	}

	IEnumerator EndGame(){
		RpcEndGame ();
		RpcUpdateMessage ("GAME OVER\n" + m_winner.m_pSetup.m_name + " wins!"); //fill out string (winner) properly on the server, then pass it along to clients through the rpc. 
		yield return new WaitForSeconds(3f);
		Reset ();
		LobbyManager.s_Singleton._playerNumber = 0;
		LobbyManager.s_Singleton.SendReturnToLobby ();
	}

	void EnablePlayers(){
		for(int i = 0; i < m_allPlayers.Count; i++){
			if (m_allPlayers[i] != null) {
				m_allPlayers [i].EnableControls ();
			}
		}
	}

	void DisablePlayers(){
		for(int i = 0; i < m_allPlayers.Count; i++){
			if (m_allPlayers != null) {
				m_allPlayers [i].DisableControls ();
			}
		}
	}

	//RPC are invoked on server run on clients.
	//As opposed to commands, which are invoked on the clients, run on the server.
	[ClientRpc] 
	void Rpc_UpdateScoreboard(string[] playerNames, int[] playerScores){ //data type of arguments are restricted in RPC methods.

		for(int i = 0; i < m_allPlayers.Count; i++){
			if(playerNames[i] != null){
				m_playerNameText [i].text = playerNames [i];
			}
			if (playerScores [i] != null) {
				m_playerScoreText [i].text = playerScores [i].ToString();
			}
		}
	}

	[Server] //only runs on server
	public void UpdateScoreBoard(){

		// string[] pNames = new string[m_allPlayers.Count];
		// int[] pScores = new int[m_allPlayers.Count];
		// for(int i = 0; i < m_allPlayers.Count; i++){
		// 	if(m_allPlayers[i] != null){
		// 		pNames [i] = m_allPlayers [i].GetComponent<PlayerSetup> ().m_name;
		// 		pScores [i] = m_allPlayers [i].m_score;
		// 	}
		// }
		// Rpc_UpdateScoreboard (pNames, pScores);
	}

	[ClientRpc]
	void RpcUpdateMessage (string msg){
		UpdateMessage (msg);
	}

	public void UpdateMessage(string msg){
		if (m_messageText != null) {
			m_messageText.gameObject.SetActive (true);
			m_messageText.text = msg;
		}
	}

	public void CheckScores(){

		m_winner = GetWinner ();
		if(m_winner != null){
			m_gameOver = true;
		}
	}

	PlayerManager GetWinner(){

		for (int i = 0; i < m_allPlayers.Count; i++) {
			if (m_allPlayers [i].m_score >= m_maxScore) {
				return m_allPlayers [i];
			}
		}
		return null;
	}

	void Reset(){

		for(int i = 0; i < m_allPlayers.Count; i++){
			PlayerHealth pHealth = m_allPlayers [i].GetComponent<PlayerHealth> ();
			pHealth.Reset ();

			m_allPlayers[i].m_score = 0;
		}

	}

}
