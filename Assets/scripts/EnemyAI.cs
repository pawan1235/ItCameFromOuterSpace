﻿using UnityEngine;
using Pathfinding;
using System.Collections;

/*
 *EnemyAI find path for the Enemy to move to the Player.
 * @author Theeruth Borisuth 
 */
[RequireComponent (typeof (Rigidbody2D))]
[RequireComponent (typeof (Seeker))]
public class EnemyAI : MonoBehaviour {

	//What to chase?
	public Transform target;

	//How many times each second we will update our path
	public float updateRate = 2f;

	//Caching
	private Seeker seeker;
	private Rigidbody2D rb;

	//The calculated path.
	public Path path;

	//The Ai's speed per second.
	public float speed = 300f;
	public ForceMode2D fMode;

	[HideInInspector]
	public bool pathIsEnded = false;

	//The max distance from the AI to a waypoint for it to continue to the next waypoint
	public float nextWaypointDistance = 3;

	//The waypoint we are current;y moving towards.
	private int currentWayPoint = 0 ;

	private bool searchingForPlayer = false;

	void Start () {
		seeker = GetComponent<Seeker>();
		rb = GetComponent<Rigidbody2D>();

		if (target == null) {
			if (!searchingForPlayer){
				searchingForPlayer = true ;
				StartCoroutine (SearchForPlayer());
			}
			return ;
		}
		// Start a new path to the target position, return the result.
		seeker.StartPath (transform.position, target.position, OnPathComplete);
        
		StartCoroutine(UpdatePath ());
	}

    /*
     *Find any GameObject that have tag "Player". 
     */
	IEnumerator SearchForPlayer (){
		GameObject sResult = GameObject.FindGameObjectWithTag ("Player");

        if (sResult == null) {
			yield return new WaitForSeconds (0.5f);
			StartCoroutine (SearchForPlayer ());
		} else {
			target = sResult.transform;
			searchingForPlayer = false;
			StartCoroutine (UpdatePath ());
			yield break;
		}
	}

	IEnumerator UpdatePath (){
        if (target == null) {
			if (!searchingForPlayer){
				searchingForPlayer = true ;
				StartCoroutine (SearchForPlayer());
			}
            yield return false;
        }

        //Start a new path to the target position, return the result to the OnPathComplete method.
        if(target != null) seeker.StartPath (transform.position, target.position, OnPathComplete);
		yield return new WaitForSeconds (1f/ updateRate);
		StartCoroutine (UpdatePath ());
	}

	public void OnPathComplete(Path p){
		Debug.Log ("We got a path. Did it have an error? " + p.error);
		if (!p.error) {
			path = p;
			currentWayPoint = 0 ;
		}
	}

	void FixedUpdate(){
		if (target == null) {
			if (!searchingForPlayer){
				searchingForPlayer = true ;
				StartCoroutine (SearchForPlayer());
			}
            return ;
		}

			//TODO: Always look at player?

		if (path == null)
			return;

		if (currentWayPoint >= path.vectorPath.Count) {
			if(pathIsEnded) 
				return;

			Debug.Log ("End of the path reached.");
			pathIsEnded = true;
			return;
		}
		pathIsEnded = false;	

		//Direction to the next waypoint
		Vector3 dir = (path.vectorPath[currentWayPoint] - transform.position).normalized ;
		dir *= speed * Time.fixedDeltaTime;

		//Move the AI
		rb.AddForce (dir, fMode);

		float dist = Vector3.Distance (transform.position, path.vectorPath[currentWayPoint]);
		if (dist < nextWaypointDistance) {
			currentWayPoint++;
			return;
		}
	}

}
 