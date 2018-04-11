using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Networking;
using System.Collections;

public class Health : NetworkBehaviour {

	public const int maxHealth = 100;
	[SyncVar(hook = "OnChangeHealth")]
	public int currentHealth = maxHealth;
	public RectTransform healthBar;
	private NetworkStartPosition[] spawnPoints;

	void Start ()
	{
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
					playerBeingShot.Stun ();
					if (currentHealth <= 0)
					{
						currentHealth = maxHealth;
						RpcRespawn ();
					}
				}
				else
				{
					playerBeingShot.AddBoost ();
				}
			}
			else //an enemy is shooting a player
			{
				EnemyController enemyShooting = shooter.GetComponent<EnemyController> ();
				currentHealth -= enemyShooting.GetDamage ();
				if (currentHealth <= 0)
				{
					currentHealth = maxHealth;
					RpcRespawn ();
				}
			}
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
		}
	}

}
