using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI; //for the player health bar
using UnityEngine.Networking;

//the server is going to have authority over all player healths.


//In UI, child objects are show ON TOP of parents. So the green health bar will be shown above the red background!
	//made health bar a child of background, changed pivot of health bar with shift+cmd so its on the left, and then will scale the width. sick.

public class PlayerHealth : NetworkBehaviour {

	/*syncvars
	 * [SyncVar] attribute
	 * syntchornied betwork variable (server authority)
	 * 
	 */

	/*
	Syncvar hook
	a function that runs every time a syncvar changes.
	modify the previous attribute, now:
	[SyncVar(hook="function name")]
		variable is passed into method as an argument by default.
			in this case, we named it "value" in the UpdateHealthBar method.

	*/

	/*
	ClientRPC is a remote action (rpc) invoked on server but run on client. 
	marked with [ClientRPC] attribute
	method name must begin with "Rpc" 

		sort of like the opposite of commands. commands are invoked on client but run on server.



	*/
	//in order to synchronize health, and do something when it changes.
	[SyncVar(hook = "UpdateHealthBar")]
	float m_currentHealth;
	public float m_maxHealth = 3;

	public GameObject m_deathPrefab;

	[SyncVar]
	public bool m_isDead = false;

	public RectTransform m_healthBar;

	public PlayerManager m_lastAttacker;

	// Use this for initialization
	void Start () {


		Reset ();

		//StartCoroutine ("CountDown");


	}

	IEnumerator CountDown(){

		yield return new WaitForSeconds (1f);
		Damage (1f);
		UpdateHealthBar (m_currentHealth);

		yield return new WaitForSeconds (1f);
		Damage (1f);
		UpdateHealthBar (m_currentHealth);

		yield return new WaitForSeconds (1f);
		Damage (1f);
		UpdateHealthBar (m_currentHealth);
	}
	


	void UpdateHealthBar(float value){

		if(m_healthBar != null){
			//150 is hardcoded. look into api. RectTransform and sizeDelta
			m_healthBar.sizeDelta = new Vector2 (value / m_maxHealth * 150f, m_healthBar.sizeDelta.y);
		}
	}


	public void Damage(float damage, PlayerManager pc = null){ //optional argument.

		if(!isServer){
			return;
		}

		if(pc != null && pc != this.GetComponent<PlayerManager>()){
			m_lastAttacker = pc;
		}

		m_currentHealth -= damage;

		if(m_currentHealth <= 0 && !m_isDead){

			if(m_lastAttacker != null){

				m_lastAttacker.m_score++;
				m_lastAttacker = null;
			}

			GameManager.Instance.UpdateScoreBoard ();
			m_isDead = true;
			Rpc_Die ();
		}
	}

	//in order to synchronize deaths across clients
	//called on server, run on client.
	[ClientRpc]
	void Rpc_Die(){

		if(m_deathPrefab != null){

			GameObject deathFX = Instantiate (m_deathPrefab, transform.position + Vector3.up * 0.5f, Quaternion.identity) as GameObject;
			GameObject.Destroy (deathFX, 3f);
		}

		SetActiveState (false); //dont want to destoy player, it will be holding data. Just disable it. 

		gameObject.SendMessage ("Respawn"); //sends a message to other components on the player tank looking for a METHOD named Respawn. Invokes if its found
	
	}

	void SetActiveState( bool state){ //sets active state of tank.

		foreach(Collider c in GetComponentsInChildren<Collider>()){  //why get components in children? this for whole tank?
			c.enabled = state;
		}

		foreach(Canvas c in GetComponentsInChildren<Canvas>()){
			c.enabled = state;
		}

		//this is how you could make someone invisible. turn off their renderer only on certain clients :D 
		foreach(Renderer r in GetComponentsInChildren<Renderer>()){
			r.enabled = state;
		}
	}

	public void Reset(){

		m_currentHealth = m_maxHealth;
		SetActiveState (true);

		m_isDead = false;
	}

}
