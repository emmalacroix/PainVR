using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

public class PlayerController : NetworkBehaviour {

	public GameObject bulletPrefab;
	public Transform bulletSpawn;
	public Camera cam;
	public Canvas healthBar;
	public GameObject visor;
	public GameObject gun;
	public Text scoreText;
	public bool competing;
	public bool leftHanded;

	[SyncVar(hook = "OnChangeScore")]
	private int score;
	[SyncVar(hook = "OnChangeSpeed")]
	private float speed;
	[SyncVar(hook = "OnChangeDamage")]
	private int damage;
	[SyncVar(hook = "OnStun")]
	private bool stunned;
	private bool forwardCollide;
	private bool backwardCollide;
	private Vector3 maxPosition;
	private Vector3 minPosition;
	private bool firstTriggerPress;

	void Start()
	{
		score = 0;
		speed = 1.2f;
		stunned = false;
		forwardCollide = false;
		backwardCollide = false;
		damage = 10;
		var minObj = GameObject.FindGameObjectWithTag ("MinBoundary");
		var maxObj = GameObject.FindGameObjectWithTag ("MaxBoundary");
		minPosition = minObj.transform.position;
		maxPosition = maxObj.transform.position;
		firstTriggerPress = false;
		GetComponent<MeshRenderer> ().material.color = Color.green;

		if (isLocalPlayer)
		{
			if (competing)
			{
				scoreText.text = "My Score: " + score.ToString ();
			}
			else
				scoreText.text = "Team Score: " + score.ToString ();
			if (leftHanded)
			{
				moveGun ();
			}
			visor.GetComponent<MeshRenderer> ().enabled = false;
			return;
		}

		cam.enabled = false;
		healthBar.enabled = false;

		if (competing)
		{
			PlayerController[] players = FindObjectsOfType<PlayerController> ();
			PlayerController otherPlayer = GetComponent<PlayerController>();
			foreach (PlayerController player in players)
			{
				if (player.transform.position != GetComponent<PlayerController>().transform.position)
				{
					Debug.Log ("Other player found");
					otherPlayer = player;
				}
			}
			Vector3 pos = otherPlayer.transform.position;
			Quaternion rot = otherPlayer.transform.rotation;
			otherPlayer.transform.position = GetComponent<PlayerController> ().transform.position;
			otherPlayer.transform.rotation = GetComponent<PlayerController> ().transform.rotation;
			scoreText.transform.rotation = otherPlayer.transform.GetChild (1).GetChild (3).GetChild (0).rotation;
			scoreText.transform.position = otherPlayer.transform.GetChild (1).GetChild (3).GetChild (0).position;
			scoreText.transform.position = scoreText.transform.position + new Vector3 (0.0f, .08f, 0.0f) + GetComponent<PlayerController> ().transform.right * 0.5f;
			scoreText.transform.SetParent (otherPlayer.transform.GetChild (1).GetChild (3));
			scoreText.text = "Opponent's Score: " + score.ToString ();
			otherPlayer.transform.rotation = rot;
			otherPlayer.transform.position = pos;
		}
		else
			scoreText.enabled = false;
		if (leftHanded)
		{
			moveGun ();
		}
	}

	void moveGun()
	{
		Quaternion rot = gameObject.transform.rotation;
		transform.rotation = Quaternion.LookRotation (new Vector3 (0,0,1));
		Vector3 gunPos = gun.transform.position;
		Vector3 bSpawnPos = bulletSpawn.position;
		gun.transform.position = new Vector3(gunPos.x - 1.0f, gunPos.y, gunPos.z);
		bulletSpawn.position = new Vector3(bSpawnPos.x - 1.0f, bSpawnPos.y, bSpawnPos.z);
		transform.rotation = rot;
	}

	// Use this for initialization
//	public override void OnStartLocalPlayer()
//	{
//		GetComponent<MeshRenderer> ().material.color = Color.blue;
//	}
	
	// Update is called once per frame
	void Update ()
	{
		if (!isLocalPlayer)
		{
			return;
		}

		bool buttonPressed = false;
		OVRInput.Update ();
		if (!stunned && !forwardCollide && !backwardCollide)
		{
			var x = Input.GetAxis ("Horizontal") * Time.deltaTime * 150.0f;
			var z = Input.GetAxis ("Vertical") * Time.deltaTime * 3.0f * speed;

			transform.Rotate (0, x, 0);
			transform.Translate (0, 0, z);
			if (OutOfBounds ())
				transform.Translate (0, 0, -z);

			if (Input.GetKeyDown (KeyCode.Space) || OVRInput.Get (OVRInput.RawAxis1D.LIndexTrigger) >= .95f
			    || OVRInput.Get (OVRInput.RawAxis1D.RIndexTrigger) >= .95f)
			{
				if (firstTriggerPress)
				{
					CmdFire ();
					firstTriggerPress = false;
					Invoke ("AllowRepeatFire", 0.4f);
				}
			}
			if (OVRInput.Get (OVRInput.RawAxis1D.LIndexTrigger) < .95f)
			{
				firstTriggerPress = true;
			}
		}
		else if (forwardCollide)
		{
			var z = 0.5f * Time.deltaTime * 3.0f * speed;
			transform.Translate (0, 0, -z);
		}
		else if (backwardCollide)
		{
			var z = 0.5f * Time.deltaTime * 3.0f * speed;
			transform.Translate (0, 0, z);
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

	public void IncrementScore()
	{
		score++;
	}

	public void IncrementTeamScore()
	{
		PlayerController[] players = FindObjectsOfType<PlayerController> ();
		foreach (PlayerController player in players)
		{
			player.IncrementScore ();
		}
	}

	public void ResetScore()
	{
		score = 0;
	}

	public void ResetTeamScore()
	{
		PlayerController[] players = FindObjectsOfType<PlayerController> ();
		foreach (PlayerController player in players)
		{
			player.ResetScore ();
		}
	}

	void OnChangeScore(int new_score)
	{
		if (competing)
		{
			if (isLocalPlayer)
				scoreText.text = "My Score: " + new_score.ToString ();
			else
				scoreText.text = "Opponent's Score: " + new_score.ToString ();
		}
		else
		{
			scoreText.text = "Team Score: " + new_score.ToString ();
		}
	}

	public bool IsCompeting()
	{
		return competing;
	}

	public bool Stunned()
	{
		return stunned;
	}

	public int GetDamage()
	{
		return damage;
	}

	public void AddBoost()
	{
		speed = 2.5f;
		damage = 20;
		Invoke ("RemoveBoost", 20.0f);
	}

	public void Stun()
	{
		stunned = true;
		Invoke ("RemoveBoost", 10.0f);
	}

	void OnChangeSpeed(float new_speed)
	{
		speed = new_speed;
	}

	void OnStun(bool s)
	{
		stunned = s;
	}

	void OnChangeDamage(int new_damage)
	{
		damage = new_damage;
	}

	public void RemoveBoost()
	{
		//return to default speed and damage settings
		speed = 1.2f;
		stunned = false;
		damage = 10;
	}

	void OnTriggerEnter(Collider other)
	{
		Vector3 playerForward = transform.forward;
		Vector3 outwardFromBuilding = transform.position - other.ClosestPointOnBounds(transform.position);
		if (Vector3.Dot(playerForward, outwardFromBuilding) < 0)
		{
			forwardCollide = true;
		}
		else
		{
			backwardCollide = true;
		}
	}

	void OnTriggerExit(Collider other)
	{
		forwardCollide = false;
		backwardCollide = false;
	}

	bool OutOfBounds()
	{
		if (transform.position.x > maxPosition.x || transform.position.z > maxPosition.z
			|| transform.position.x < minPosition.x || transform.position.z < minPosition.z)
			return true;
		else
			return false;
	}

	void AllowRepeatFire()
	{
		firstTriggerPress = true;
	}
}
