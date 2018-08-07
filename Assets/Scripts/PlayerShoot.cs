using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

//all scripts run on all tanks in scene

/*
 * Remove actions. aka remove procedure call (rpc)
 * 	funtion that can be run over the network
 * Command- type of RPC invoked on Client, run on server.
 * 	//mark with:  [Command]    and, must begin with Cmd
 * 
 */

public class PlayerShoot : NetworkBehaviour {

	public Rigidbody m_bulletPrefab;

	public Transform m_bulletSpawn;

	public int m_shotsPerBurst = 2;

	int m_shotsLeft;

	bool m_isReloading;

	public float m_reloadTime = 1f;

	public ParticleSystem m_misfireEffect;

	public LayerMask m_obstacleMask;

	bool m_canShoot = false;


	// Use this for initialization
	void Start () {

		m_shotsLeft = m_shotsPerBurst;
		m_isReloading = false;
	}

	public void Enable(){
		m_canShoot = true;
	}

	public void Disable(){
		m_canShoot = false;
	}
	
	// Update is called once per frame
	void Update () {


	}

	public void Shoot(){

		if(m_isReloading || m_bulletPrefab == null || !m_canShoot){
			return;
		}



		RaycastHit hit;

		Vector3 center = new Vector3 (transform.position.x, m_bulletSpawn.position.y, transform.position.z);

		Vector3 dir = (m_bulletSpawn.position - center).normalized;

		if (Physics.SphereCast (center, 0.30f, dir, out hit, 3.0f, m_obstacleMask, QueryTriggerInteraction.Ignore)) {

			if(m_misfireEffect != null){
				ParticleSystem effect = Instantiate (m_misfireEffect, hit.point, Quaternion.identity) as ParticleSystem;
				effect.Stop ();
				effect.Play ();
				Destroy (effect.gameObject, 3f);
				 
			}

		} 
		else {

			Cmd_Shoot ();



			m_shotsLeft--;

			if (m_shotsLeft <= 0) {
				StartCoroutine ("Reload");  //this looks like its about to be useful
			}
		}


	}

	//called from client, run on server.
	[Command]
	void Cmd_Shoot ()
	{
		Bullet bullet = null;
		//bullet = m_bulletPrefab.GetComponent<Bullet> ();
		Rigidbody rbody = Instantiate (m_bulletPrefab, m_bulletSpawn.position, m_bulletSpawn.rotation) as Rigidbody;
		bullet = rbody.gameObject.GetComponent<Bullet> ();
		if (rbody != null) {
			rbody.velocity = bullet.m_speed * m_bulletSpawn.transform.forward;
			bullet.m_owner = GetComponent<PlayerManager> ();

			NetworkServer.Spawn (rbody.gameObject); //IMPORTANT

		}
	}

	//see line ~54 with:   StartCoroutine("Reload");
	//this is sick.
	IEnumerator Reload(){

		m_shotsLeft = m_shotsPerBurst;
		m_isReloading = true;
		yield return new WaitForSeconds (m_reloadTime);
		m_isReloading = false;
	}
}
