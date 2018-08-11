using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using Prototype.NetworkLobby;

public class NetworkLobbyHook : LobbyHook {

	//runs when we transition from the lobby scene to the game scene
	//Only runs on the server!!!
	public override void OnLobbyServerSceneLoadedForPlayer(NetworkManager manager, GameObject lobbyPlayer, GameObject gamePlayer) { 
	
		LobbyPlayer lPlayer = lobbyPlayer.GetComponent<LobbyPlayer> ();
		PlayerSetup pSetup = gamePlayer.GetComponent<PlayerSetup> ();

		pSetup.m_name = lPlayer.playerName; //whatever the user types in at the lobby.
		pSetup.m_playerColor = lPlayer.playerColor;

		PlayerManager pManager = gamePlayer.GetComponent<PlayerManager> ();
		if(pManager != null){
			GameManager.m_allPlayers.Add (pManager);
		}
	}
}
