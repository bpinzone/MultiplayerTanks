using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Utility : MonoBehaviour {

	public static Vector3 GetWorldPointFromScreenPoint(Vector3 screenPoint, float height){

		// Check API
		Ray ray = Camera.main.ScreenPointToRay (screenPoint);//ray from camera to screen point
		Plane plane = new Plane (Vector3.up, new Vector3 (0, height, 0));
		float distance;

		//out keyword...
		if(plane.Raycast(ray, out distance)){
			return ray.GetPoint (distance);
		}
		return Vector3.zero;
	}
}
