using System.Collections;
using UnityEngine;

public class TextBillboard : MonoBehaviour {

	// Update is called once per frame
	void Update () {
		//transform.LookAt(Camera.main.transform);
		transform.rotation = Quaternion.LookRotation(transform.position - Camera.main.transform.position);
	}
}
