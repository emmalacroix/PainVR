using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class Bullet : MonoBehaviour {

	public GameObject shooter;
	//public AudioClip crashSoft;
	//private AudioSource source;

	void Start()
	{
		shooter = gameObject.transform.parent.gameObject;
		gameObject.transform.parent = null;
		//source = GetComponent<AudioSource> ();
	}

	void OnCollisionEnter(Collision collision)
	{
		var hit = collision.gameObject;
		var health = hit.GetComponent<Health> ();

		if (health != null)
		{
			//Debug.Log ("hit");
			//source.PlayOneShot (crashSoft, 0.5F);
			health.TakeDamage (shooter);
		}

		Destroy (gameObject);
	}
}
