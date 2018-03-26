using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class Bullet : MonoBehaviour {

	void OnCollisionEnter(Collision collision)
	{
		//var parent = gameObject.transform.parent.gameObject;
		//var player = parent.GetComponent<PlayerController>();

		//Destroy (parent);
		Destroy (gameObject);

		var hit = collision.gameObject;
		var health = hit.GetComponent<Health> ();

		if (health != null)
		{
			health.TakeDamage (10);//, player);
		}
	}
}
