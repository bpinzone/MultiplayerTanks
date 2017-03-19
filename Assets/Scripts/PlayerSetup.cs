using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Networking;

//remember, this is run on EVERY player in the scene. local or not. The server has authorty of ALL player colors and names.
public class PlayerSetup : NetworkBehaviour {

	[SyncVar(hook = "UpdateColor")]
	public Color m_playerColor; //note: this variable type matches the type of the argument in the hooked funcion.

	public string m_basename = "Player";

	[SyncVar(hook = "UpdateName")]
	public int m_playerNum = 1; //note: this variable type matches the type of the argument in the hooked funcion.

	public Text m_playerNameText;


	void Start(){
		if (!isLocalPlayer) {

			UpdateName (m_playerNum);
			UpdateColor (m_playerColor);

		}
	}

	//OnStartClient runs just before OnStartLocalPlayer
	public override void OnStartClient(){

		//run original method you are overriding
		base.OnStartClient ();

		//disable the next field by default.
		if(m_playerNameText != null){

			m_playerNameText.enabled = false;

		}

	}

	void UpdateColor (Color pColor){
		
		MeshRenderer[] meshes = GetComponentsInChildren<MeshRenderer> ();
		foreach (MeshRenderer r in meshes) {
			r.material.color = pColor;
		}
	}

	void UpdateName (int pNum){
		if (m_playerNameText != null) {
			m_playerNameText.enabled = true;
			m_playerNameText.text = m_basename + pNum.ToString ();
		}
	}

	//need override  keyword
	public override void OnStartLocalPlayer(){

		//run original method you are overriding
		base.OnStartLocalPlayer ();
		Cmd_SetupPlayer ();



	}

	[Command]
	void Cmd_SetupPlayer(){
		//runs on server.
		//never directly change these on clients.
		GameManager.Instance.AddPlayer (this);
		GameManager.Instance.m_playerCount++;
	}
}
