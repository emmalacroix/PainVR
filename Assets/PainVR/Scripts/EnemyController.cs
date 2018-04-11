using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

public class EnemyController : NetworkBehaviour {

	public GameObject bulletPrefab;
	public Transform bulletSpawn;
	private float forwardSpeed;
	private float rotationSpeed;
	private int damage;
	private Vector3 maxPosition;
	private Vector3 minPosition;
	private bool waiting;

	// Use this for initialization
	void Start () {
		forwardSpeed = 1.0f;
		rotationSpeed = 2.0f;
		damage = 10;
		var minObj = GameObject.FindGameObjectWithTag ("MinBoundary");
		var maxObj = GameObject.FindGameObjectWithTag ("MaxBoundary");
		minPosition = minObj.transform.position;
		maxPosition = maxObj.transform.position;
		waiting = false;
	}
	
	// Update is called once per frame
	void Update () {
		
		//Enemy AI shooting goes here
		Vector3 targetDir = new Vector3 (0, 0, 0);
		float dist = Mathf.Infinity;
		PlayerController[] players = FindObjectsOfType<PlayerController> ();
		foreach (PlayerController player in players)
		{
			float distToPlayer = Vector3.Magnitude(player.transform.position - transform.position);
			if (distToPlayer < 5.0f && distToPlayer < dist)
			{
				dist = distToPlayer;
				targetDir = player.transform.position - transform.position;
			}
		}
		if (targetDir != new Vector3 (0, 0, 0))
		{
			if (!waiting)
			{
				//transform.LookAt (target);
				float step = rotationSpeed * Time.deltaTime;
				Vector3 newDir = Vector3.RotateTowards (transform.forward, targetDir, step, 0.0f);
				transform.rotation = Quaternion.LookRotation (newDir);
				if (Vector3.Magnitude(Vector3.Cross(transform.forward, targetDir)) < 0.5f && Vector3.Dot(transform.forward, targetDir) >= 0)
				{
					CmdFire ();
					waiting = true;
					Invoke ("StopWaiting", 1.0f);
				}
			}
		}
		else
		{
			//Enemy AI movement goes here
			var dir = transform.forward;
			dir.Normalize ();
			var z = Mathf.Pow(Mathf.Pow(dir.z, 2)+Mathf.Pow(dir.x, 2), 0.5f);
			if (z < 0)
				z = -z;
			z = z * Time.deltaTime * forwardSpeed;

			transform.Translate (0, 0, z);
			if (OutOfBounds())
			{
				transform.Translate (0, 0, -z);
				transform.Rotate (0, 60, 0);
			}
		}
	}

	bool OutOfBounds()
	{
		if (transform.position.x > maxPosition.x || transform.position.z > maxPosition.z
		    || transform.position.x < minPosition.x || transform.position.z < minPosition.z)
			return true;
		else
			return false;
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

	public void StopWaiting()
	{
		waiting = false;
	}

	void OnTriggerEnter(Collider other)
	{
		//Debug.Log ("Trigger entered");
		transform.Rotate (0, 60, 0);
	}

	public int GetDamage()
	{
		return damage;
	}

}
