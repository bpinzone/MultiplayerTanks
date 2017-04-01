using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

[RequireComponent(typeof(PlayerShoot))]
[RequireComponent(typeof(PlayerHealth))]
[RequireComponent(typeof(PlayerMotor))]
[RequireComponent(typeof(PlayerSetup))]
public class PlayerController : NetworkBehaviour {


	PlayerHealth m_pHealth;
	PlayerMotor m_pMotor;
	public PlayerSetup m_pSetup;
	PlayerShoot m_pShoot;

	Vector3 m_originalPosition; //store the location that the client spawned to when they connected to server.

	NetworkStartPosition[] m_spawnPoints;

	public GameObject m_spawnFx;

	public int m_score;

	// Use this for initialization
	void Start () {
		
		m_pHealth = GetComponent<PlayerHealth> ();
		m_pMotor = GetComponent<PlayerMotor> ();
		m_pSetup = GetComponent<PlayerSetup> ();
		m_pShoot = GetComponent<PlayerShoot> ();

		GameManager gm = GameManager.Instance; //Instance is a public static property. globally accessible. Can do so without a reference. Dont need to find it, or set it up in inspector. Slightly dangerous. dont overuse.

		
	}


	public override void OnStartLocalPlayer(){

		m_spawnPoints = GameObject.FindObjectsOfType<NetworkStartPosition> (); //populate the spawn points list. Look at the assignment type in api. Looks like we are calling static method in GameObject type.
		m_originalPosition = transform.position;
	}
	
	Vector3 GetInput(){

		float h = Input.GetAxis ("Horizontal");
		float v = Input.GetAxis ("Vertical");
		return new Vector3 (h, 0, v);
	}

	//Any method involving physics needs to be invoked inside fixed update
	void FixedUpdate(){

		if(!isLocalPlayer || m_pHealth.m_isDead){
			return;
		}

		//move player
		Vector3 inputDirection = GetInput ();
		m_pMotor.MovePlayer (inputDirection);



	}


	void Update(){

		if(!isLocalPlayer || m_pHealth.m_isDead){
			return;
		}

		if(Input.GetMouseButtonDown(0)){

			m_pShoot.Shoot ();
		}

		Vector3 inputDirection = GetInput ();

		//rotate chassis
		if(inputDirection.sqrMagnitude > 0.25f){
			m_pMotor.RotateChassis (inputDirection);
		}

		//rotate turret
		//see pic on phone concerning this calculation/subtraction.
		Vector3 turretDir = Utility.GetWorldPointFromScreenPoint (Input.mousePosition, m_pMotor.m_turret.position.y) - m_pMotor.m_turret.position;
		m_pMotor.RotateTurrent (turretDir);
	}

	void Disable(){

		StartCoroutine ("RespawnRoutine");

	}

	//spawning is handled by the network manager.
	IEnumerator RespawnRoutine(){

		transform.position = GetRandomSpawnPosition(); //maybe not great, because then a player will always spawn at the same point. 
		m_pMotor.m_rigidbody.velocity = Vector3.zero;
		yield return new WaitForSeconds (3f);
		m_pHealth.Reset ();

		if(m_spawnFx != null){
			GameObject spawnFx = Instantiate (m_spawnFx, transform.position + Vector3.up * 0.5f, Quaternion.identity) as GameObject; //why does this work over the network? because it happened in an RPC a couple funtion calls back.
			Destroy (spawnFx, 3f);
		}
	}

	Vector3 GetRandomSpawnPosition(){

		if (m_spawnPoints != null) {
			if (m_spawnPoints.Length > 0) {

				NetworkStartPosition startPos = m_spawnPoints [Random.Range (0, m_spawnPoints.Length)]; 
				return startPos.transform.position;

			}
		}
		return m_originalPosition;


	}
}
