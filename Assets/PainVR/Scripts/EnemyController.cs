﻿using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

public class EnemyController : NetworkBehaviour {

	public GameObject bulletPrefab;
	public Transform bulletSpawn;
	private float speed;
	private int damage;
	private Vector3 maxPosition;
	private Vector3 minPosition;

	// Use this for initialization
	void Start () {
		speed = 3.0f;
		damage = 10;
		maxPosition = new Vector3(11.9f,0.0f,19.9f);
		minPosition = new Vector3(-19.6f,0.0f,-11.4f);
	}
	
	// Update is called once per frame
	void Update () {
		//Use this for enemy AI
		//var x = transform.forward.x * Time.deltaTime * speed;
		var dir = transform.forward;
		dir.Normalize ();
		var z = Mathf.Pow(Mathf.Pow(dir.z, 2)+Mathf.Pow(dir.x, 2), 0.5f);
		if (z < 0)
			z = -z;
		z = z * Time.deltaTime * speed;

		transform.Translate (0, 0, z);
		if (transform.position.x > maxPosition.x || transform.position.z > maxPosition.z
		    || transform.position.x < minPosition.x || transform.position.z < minPosition.z)
		{
			transform.Translate (0, 0, -z);
			transform.Rotate (0, 60, 0);
		}
	}

	[Command] //this code is called on the client but runs on the server!
	void CmdFire()
	{
		// Create the Bullet from the Bullet Prefab
		var bullet = (GameObject)Instantiate (
			bulletPrefab,
			bulletSpawn.position,
			bulletSpawn.rotation,
			gameObject.transform);

		// Add velocity to the bullet
		bullet.GetComponent<Rigidbody>().velocity = bullet.transform.forward * 6;

		//Spawn the bullet on the clients
		NetworkServer.Spawn (bullet);

		// Destroy the bullet after 2 seconds
		Destroy(bullet, 2.0f);
	}
}