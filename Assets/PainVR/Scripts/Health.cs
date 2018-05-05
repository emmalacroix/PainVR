using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Networking;
using System.Collections;

public class Health : NetworkBehaviour {

	public const int maxHealth = 100;
	[SyncVar(hook = "OnChangeHealth")]
	public int currentHealth = maxHealth;
	[SyncVar(hook = "OnChangeColor")]
	private Color color;
	public RectTransform healthBar;
	private NetworkStartPosition[] spawnPoints;
	public AudioClip crashSoft;
	private AudioSource source;

	void Start ()
	{
		source = GetComponent<AudioSource>();
		if (isLocalPlayer)
		{
			spawnPoints = FindObjectsOfType<NetworkStartPosition>();
		}
	}

	public void TakeDamage(GameObject shooter)
	{
		if (!isServer)
		{
			return;
		}
		source.PlayOneShot (crashSoft, 0.5F);
		if (gameObject.tag == "Enemy")
		{
			if (shooter.tag == "Player") //a player is shooting an enemy
			{
				PlayerController player = shooter.GetComponent<PlayerController> ();
				currentHealth -= player.GetDamage ();
				if (currentHealth <= 0)
				{
					if (player.IsCompeting ())
					{
						player.IncrementScore ();
					}
					else
					{
						player.IncrementTeamScore ();
					}
					Destroy (gameObject);
					FindObjectOfType<EnemySpawner> ().SpawnEnemy ();
				}
			}
			else //an enemy is shooting another enemy
			{
				//do nothing
			}
		} else if (gameObject.tag == "Player")
		{
			if (shooter.tag == "Player") //a player is shooting another player
			{
				PlayerController playerShooting = shooter.GetComponent<PlayerController> ();
				PlayerController playerBeingShot = gameObject.GetComponent<PlayerController> ();
				if (playerShooting.IsCompeting ())
				{
					currentHealth -= playerShooting.GetDamage ();
					//playerBeingShot.Stun ();
					if (currentHealth <= 0)
					{
						playerBeingShot.ResetScore ();
						currentHealth = maxHealth;
						RpcRespawn ();
					}
					ChangeColor ();
				}
				else
				{
					currentHealth = Mathf.Min(currentHealth+20, 100);
					ChangeColor ();
				}
			}
			else //an enemy is shooting a player
			{
				EnemyController enemyShooting = shooter.GetComponent<EnemyController> ();
				currentHealth -= enemyShooting.GetDamage ();
				if (currentHealth <= 0)
				{
					PlayerController playerBeingShot = gameObject.GetComponent<PlayerController> ();
					if (playerBeingShot.IsCompeting ()) {
						playerBeingShot.ResetScore ();
					} else {
						playerBeingShot.ResetTeamScore ();
					}
					currentHealth = maxHealth;
					RpcRespawn ();
				}
				ChangeColor ();
			}
		}
	}
		
	void OnChangeColor(Color color)
	{
		gameObject.GetComponent<MeshRenderer> ().material.color = color;
	}

	void ChangeColor()
	{
		if (currentHealth == 100) {
			color = Color.green;
		}
		else if (currentHealth <= 90 && currentHealth >= 70) {
			color = Color.white;
		}
		else if (currentHealth <= 60 && currentHealth >= 40) {
			color = Color.yellow;
		}
		else if (currentHealth <= 30 && currentHealth >= 10) {
			color = Color.red;
		}
	}

	void OnChangeHealth (int currentHealth)
	{
		healthBar.sizeDelta = new Vector2 (
			currentHealth,
			healthBar.sizeDelta.y);
	}

	[ClientRpc]
	void RpcRespawn()
	{
		if (isLocalPlayer)
		{
			// Set the spawn point to origin as a default value
			Vector3 spawnPoint = Vector3.zero;

			// If there is a spawn point array and the array is not empty, pick a spawn point at random
			if (spawnPoints != null && spawnPoints.Length > 0)
			{
				spawnPoint = spawnPoints[Random.Range(0, spawnPoints.Length)].transform.position;
			}

			// Set the player’s position to the chosen spawn point
			transform.position = spawnPoint;
			GetComponent<MeshRenderer> ().material.color = Color.white;
		}
	}

}
