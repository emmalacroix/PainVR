using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

public class PlayerController : NetworkBehaviour {

	public GameObject bulletPrefab;
	public Transform bulletSpawn;
	public Camera cam;
	public Canvas healthBar;
	public Text scoreText;
	public bool competing;
	[SyncVar(hook = "OnChangeScore")]
	private int score;
	[SyncVar(hook = "OnChangeSpeed")]
	private float speed;
	[SyncVar(hook = "OnChangeDamage")]
	private int damage;
	[SyncVar(hook = "OnStun")]
	private bool stunned;

	void Start()
	{
		score = 0;
		speed = 1.0f;
		stunned = false;
		damage = 10;

		if (isLocalPlayer)
		{
			if (competing)
				scoreText.text = "My Score: " + score.ToString ();
			else
				scoreText.text = "Team Score: " + score.ToString ();
			return;
		}

		cam.enabled = false;
		healthBar.enabled = false;

		if (competing)
		{
			scoreText.transform.position = scoreText.transform.position + new Vector3 (150, 0, 0);
			scoreText.text = "Opponent's Score: " + score.ToString ();
		}
		else
			scoreText.enabled = false;
	}

	// Use this for initialization
	public override void OnStartLocalPlayer()
	{
		GetComponent<MeshRenderer> ().material.color = Color.blue;
	}
	
	// Update is called once per frame
	void Update ()
	{
		if (!isLocalPlayer)
		{
			return;
		}

		if (!stunned)
		{
			var x = Input.GetAxis ("Horizontal") * Time.deltaTime * 150.0f;
			var z = Input.GetAxis ("Vertical") * Time.deltaTime * 3.0f * speed;

			transform.Rotate (0, x, 0);
			transform.Translate (0, 0, z);

			if (Input.GetKeyDown (KeyCode.Space))
				CmdFire ();
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
		speed = 3.0f;
		damage = 20;
		Invoke ("RemoveBoost", 5.0f);
	}

	public void Stun()
	{
		stunned = true;
		Invoke ("RemoveBoost", 5.0f);
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
		speed = 1.0f;
		stunned = false;
		damage = 10;
	}
		
}
