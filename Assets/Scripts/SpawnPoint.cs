using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpawnPoint : MonoBehaviour {

	public bool m_isOccupied = false;

	//On Trigger enter always takes a collider. This is the collider that has ran into us. 
	void OnTriggerEnter(Collider other){
		if(other.gameObject.tag == "Player"){
			m_isOccupied = true;
		}
	}

	void OnTriggerStay(Collider other){
		if(other.gameObject.tag == "Player"){
			m_isOccupied = true;
		}
	}

	void OnTriggerExit(Collider other){
		if(other.gameObject.tag == "Player"){
			m_isOccupied = false;
		}
	}
}
