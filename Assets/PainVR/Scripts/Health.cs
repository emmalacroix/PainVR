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

	public void TakeDamage(PlayerController shooter)
	{
		if (!isServer)
		{
			return;
		}

		if (gameObject.tag == "Enemy")
		{
			currentHealth -= shooter.GetDamage();
			if (currentHealth <= 0)
			{
				if (shooter.IsCompeting ())
				{
					shooter.IncrementScore ();
				}
				else
				{
					shooter.IncrementTeamScore ();
				}
				Destroy (gameObject);
			}
		} else if (gameObject.tag == "Player")
		{
			PlayerController player = gameObject.GetComponent<PlayerController> ();
			if (shooter.IsCompeting ())
			{
				currentHealth -= shooter.GetDamage();
				player.Stun ();
				if (currentHealth <= 0)
				{
					currentHealth = maxHealth;
					RpcRespawn ();
				}
			}
			else
			{
				player.AddBoost ();
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
