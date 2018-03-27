using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class Bullet : MonoBehaviour {

	public PlayerController shooter;

	void Start()
	{
		shooter = gameObject.transform.parent.gameObject.GetComponent<PlayerController>();
		gameObject.transform.parent = null;
	}

	void OnCollisionEnter(Collision collision)
	{
		var hit = collision.gameObject;
		var health = hit.GetComponent<Health> ();

		if (health != null)
		{
			health.TakeDamage (10, shooter);
		}

		Destroy (gameObject);
	}
}
