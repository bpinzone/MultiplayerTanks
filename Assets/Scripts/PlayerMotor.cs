using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

[RequireComponent(typeof(Rigidbody))]
public class PlayerMotor : NetworkBehaviour {

	public Rigidbody m_rigidbody; //this was previously private. had to change it in sec 3 lec 19 @6:42 in order to use it in Respawn Routine

	public Transform m_chassis;
	public Transform m_turret;

	public float m_moveSpeed = 100f;

	public float m_chassisRotateSpeed = 1f;
	public float m_turretRotateSpeed = 3f;

	bool m_canMove = false;

	public void Enable(){
		m_canMove = true;
	}
	public void Disable(){
		m_canMove = false;
		m_rigidbody.velocity = Vector3.zero;
	}

	// Use this for initialization
	void Start () {

		m_rigidbody = GetComponent<Rigidbody>();

	}


	public void MovePlayer(Vector3 dir){

		if (m_canMove) {
			Vector3 moveDirection = dir * m_moveSpeed * Time.deltaTime;
			m_rigidbody.velocity = moveDirection;
		}

	}

	public void FaceDirection(Transform xform, Vector3 dir, float rotSpeed){

		if (dir != Vector3.zero && xform != null) {

			Quaternion desiredRot = Quaternion.LookRotation (dir);

			//third param = 0 -> result will be first parameter. 
			//third param = 1 -> result will be second parameter.
			xform.rotation = Quaternion.Slerp (xform.rotation, desiredRot, rotSpeed * Time.deltaTime); //LOOK.


		}


	}

	public void RotateChassis(Vector3 dir){

		if (m_canMove) {
			FaceDirection (m_chassis, dir, m_chassisRotateSpeed);
		}
	}

	public void RotateTurrent(Vector3 dir){

		if (m_canMove) {
			FaceDirection (m_turret, dir, m_turretRotateSpeed);
		}
	}


		
}
