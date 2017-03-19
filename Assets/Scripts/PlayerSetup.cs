using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Networking;

public class PlayerSetup : NetworkBehaviour {


	public Color m_playerColor;
	public string m_basename = "Player";
	public int m_playerNum = 1;
	public Text m_playerNameText;


	//OnStartClient runs just before OnStartLocalPlayer
	public override void OnStartClient(){

		//run original method you are overriding
		base.OnStartClient ();

		//disable the next field by default.
		if(m_playerNameText != null){

			m_playerNameText.enabled = false;

		}

	}


	//need override  keyword
	public override void OnStartLocalPlayer(){

		//run original method you are overriding
		base.OnStartLocalPlayer ();

		//set color
		MeshRenderer[] meshes = GetComponentsInChildren<MeshRenderer> ();
		foreach(MeshRenderer r in meshes){
			r.material.color = m_playerColor;
		}

		//set name text. (note this only happens if im a local player because im inside this method.
		if(m_playerNameText != null){

			m_playerNameText.enabled = true;
			m_playerNameText.text = m_basename + m_playerNum.ToString ();

		}
	}
}
