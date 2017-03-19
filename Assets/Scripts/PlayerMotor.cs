using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

[RequireComponent(typeof(Rigidbody))]
public class PlayerMotor : NetworkBehaviour {

	private Rigidbody m_rigidbody;

	public Transform m_chassis;
	public Transform m_turret;

	public float m_moveSpeed = 100f;

	public float m_chassisRotateSpeed = 1f;
	public float m_turretRotateSpeed = 3f;



	// Use this for initialization
	void Start () {

		m_rigidbody = GetComponent<Rigidbody>();

	}


	public void MovePlayer(Vector3 dir){

		Vector3 moveDirection = dir * m_moveSpeed * Time.deltaTime;
		m_rigidbody.velocity = moveDirection;

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

		FaceDirection (m_chassis, dir, m_chassisRotateSpeed);
	}

	public void RotateTurrent(Vector3 dir){

		FaceDirection (m_turret, dir, m_turretRotateSpeed);
	}


		
}
