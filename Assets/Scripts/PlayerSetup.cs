using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Networking;

//remember, this is run on EVERY player in the scene. local or not. The server has authorty of ALL player colors and names.
public class PlayerSetup : NetworkBehaviour {

	//recall: [Command]   before a method. 


	//property is synchronized across server. UpdateColor function is run whenever it changes.
	[SyncVar(hook = "UpdateColor")]
	public Color m_playerColor; //note: this variable type matches the type of the argument in the hooked funcion.

	[SyncVar(hook = "UpdateName")]
	public string m_name = "Player";




	public Text m_playerNameText;



	//OnStartClient runs just before OnStartLocalPlayer
	//Invoked after clients have connected
	public override void OnStartClient(){

		//run original method you are overriding
		base.OnStartClient ();

		if (!isServer) {
			PlayerManager pManager = GetComponent<PlayerManager> ();
			if (pManager != null) {
				GameManager.m_allPlayers.Add (pManager);
			}
		}

		UpdateName (m_name);
		UpdateColor (m_playerColor);


	}

	void UpdateColor (Color pColor){
		
		MeshRenderer[] meshes = GetComponentsInChildren<MeshRenderer> ();
		foreach (MeshRenderer r in meshes) {
			r.material.color = pColor;
		}
	}

	void UpdateName (string name){
		if (m_playerNameText != null) {
			m_playerNameText.enabled = true;
			m_playerNameText.text = m_name;
		}
	}


}
