using UnityEngine;
using UnityEngine.Networking;

public class EnemySpawner : NetworkBehaviour {

	public GameObject enemyPrefab;
	public int numberOfEnemies;

	public override void OnStartServer()
	{
		for (int i=0; i < numberOfEnemies; i++)
		{
			Vector3 spawnPosition = new Vector3 (
				Random.Range (-24.0f, 24.0f),
				0.0f,
				Random.Range (-8.0f, 4.0f));
			if (i % 3 == 1) {
				spawnPosition = new Vector3 (
					Random.Range (-24.0f, 24.0f),
					0.0f,
					Random.Range (36.0f, 46.0f));
			} else if (i % 3 == 2) {
				spawnPosition = new Vector3 (
					Random.Range (-3.0f, 24.0f),
					0.0f,
					Random.Range (21.0f, 26.0f));
			}
			var spawnRotation = Quaternion.Euler( 
				0.0f, 
				Random.Range(0,180), 
				0.0f);

			var enemy = (GameObject)Instantiate(enemyPrefab, spawnPosition, spawnRotation);
			enemy.tag = "Enemy";
			NetworkServer.Spawn(enemy);
		}
	}

	public void SpawnEnemy()
	{
		int rand = Random.Range (0, 2);
		Vector3 spawnPosition = new Vector3 (
			Random.Range (-24.0f, 24.0f),
			0.0f,
			Random.Range (-8.0f, 4.0f));
		if (rand % 3 == 1) {
			spawnPosition = new Vector3 (
				Random.Range (-24.0f, 24.0f),
				0.0f,
				Random.Range (36.0f, 46.0f));
		} else if (rand % 3 == 2) {
			spawnPosition = new Vector3 (
				Random.Range (-3.0f, 24.0f),
				0.0f,
				Random.Range (21.0f, 26.0f));
		}
		var spawnRotation = Quaternion.Euler( 
			0.0f, 
			Random.Range(0,180), 
			0.0f);

		var enemy = (GameObject)Instantiate(enemyPrefab, spawnPosition, spawnRotation);
		enemy.tag = "Enemy";
		NetworkServer.Spawn(enemy);
	}
}