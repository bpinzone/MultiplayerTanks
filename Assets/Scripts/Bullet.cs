using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using System.Collections.Generic; //for lists
using System.Linq; //for the .ToList function

[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(Collider))]
public class Bullet : NetworkBehaviour {

	Rigidbody m_rigidbody;
	Collider m_collider;

	public int m_speed = 100;

	//flying particles sytem
	List<ParticleSystem> m_allParticles;

	public float m_lifetime = 5f;
	//explosion particle system
	public ParticleSystem m_explosionFX;

	//changed certain objects to have tag of bounce. To track bouncing destruction.
	public List<string> m_bounceTags;
	public int m_bounces = 2;

	// Use this for initialization
	void Start () {

		m_allParticles = GetComponentsInChildren<ParticleSystem> ().ToList (); //VERY USEFUL. The GetComponentsInChildren<type> funtion!!!
	
		m_rigidbody = GetComponent<Rigidbody> ();
		m_collider = GetComponent<Collider> ();

		StartCoroutine ("SelfDestruct");
	}

	IEnumerator SelfDestruct(){

		yield return new WaitForSeconds (m_lifetime);

		Explode ();




	}

	void Explode ()
	{
		m_collider.enabled = false;
		m_rigidbody.velocity = Vector3.zero;
		m_rigidbody.Sleep ();
		//so we dont have to handle more physics calculations.
		foreach (MeshRenderer m in GetComponentsInChildren<MeshRenderer> ()) {
			m.enabled = false;
		}
		foreach (ParticleSystem ps in m_allParticles) {
			ps.Stop ();
		}
		if (m_explosionFX != null) {
			m_explosionFX.transform.parent = null;
			//important. so it plays even after bullet destruction.
			m_explosionFX.Play ();
		}
		Destroy (gameObject);
	}
	
	// Update is called once per frame
	void Update () {
		
	}

	//runs when a collision ends.
	void OnCollisionExit(Collision collision){

		//fix bullet orientation.
		if(m_rigidbody.velocity != Vector3.zero){
			transform.rotation = Quaternion.LookRotation (m_rigidbody.velocity);
		}
	}

	void OnCollisionEnter(Collision collision){

		//if the list of tags contains the objects tag.
		if(m_bounceTags.Contains(collision.gameObject.tag)){

			if(m_bounces <= 0){
				Explode ();
			}

			m_bounces--;
		}
	}

}
