using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI; //for the player health bar
using UnityEngine.Networking;

//In UI, child objects are show ON TOP of parents. So the green health bar will be shown above the red background!
	//made health bar a child of background, changed pivot of health bar with shift+cmd so its on the left, and then will scale the width. sick.

public class PlayerHealth : NetworkBehaviour {



	float m_currentHealth;
	public float m_maxHealth = 3;

	public GameObject m_deathPrefab;

	public bool m_isDead = false;

	public RectTransform m_healthBar;

	// Use this for initialization
	void Start () {


		m_currentHealth = m_maxHealth;

		//StartCoroutine ("CountDown");


	}

	IEnumerator CountDown(){

		yield return new WaitForSeconds (1f);
		Damage (1f);
		UpdatehealthBar (m_currentHealth);

		yield return new WaitForSeconds (1f);
		Damage (1f);
		UpdatehealthBar (m_currentHealth);

		yield return new WaitForSeconds (1f);
		Damage (1f);
		UpdatehealthBar (m_currentHealth);
	}
	
	// Update is called once per frame
	void Update () {
		
	}

	void UpdatehealthBar(float value){

		if(m_healthBar != null){
			//150 is hardcoded. look into api. RectTransform and sizeDelta
			m_healthBar.sizeDelta = new Vector2 (value / m_maxHealth * 150f, m_healthBar.sizeDelta.y);
		}
	}

	public void Damage(float damage){


		m_currentHealth -= damage;
		UpdatehealthBar (m_currentHealth);
		if(m_currentHealth <= 0 && !m_isDead){

			m_isDead = true;
			Die ();
		}
	}

	void Die(){

		if(m_deathPrefab != null){

			GameObject deathFX = Instantiate (m_deathPrefab, transform.position + Vector3.up * 0.5f, Quaternion.identity) as GameObject;
			GameObject.Destroy (deathFX, 3f);
		}

		SetActiveState (false); //dont want to destoy player, it will be holding data. Just disable it. 

		gameObject.SendMessage ("Disable"); //sends a message to other components.

	}

	void SetActiveState( bool state){ //sets active state of tank.

		foreach(Collider c in GetComponentsInChildren<Collider>()){
			c.enabled = state;
		}

		foreach(Canvas c in GetComponentsInChildren<Canvas>()){
			c.enabled = state;
		}

		foreach(Renderer r in GetComponentsInChildren<Renderer>()){
			r.enabled = state;
		}
	}

}
