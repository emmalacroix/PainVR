using System.Collections;
using UnityEngine;

public class Bullet : MonoBehaviour {

	void OnCollisionEnter(Collision collision)
	{
		Destroy (gameObject);

		var hit = collision.gameObject;
		var health = hit.GetComponent<Health> ();
		if (health != null)
		{
			health.TakeDamage (10);
		}
	}
}
