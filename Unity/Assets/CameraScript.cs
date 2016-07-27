using UnityEngine;
using System.Collections;

public class CameraScript : MonoBehaviour {
	public AnimationCurve shakeCurve;
	public GameObject followingObject;

	void LateUpdate () {
		Vector3 difV = (transform.position - followingObject.transform.position)*0.05f;
		transform.position -= new Vector3(difV.x,difV.y,0)*Time.deltaTime*50;
	}

	public void Shake() {
		StartCoroutine(ShakeAnim());
	}

	public IEnumerator ShakeAnim() {
		float t = 0;
		Vector3 originalEulerAngles = transform.eulerAngles;
		while (t<3) {
			t += Time.deltaTime*8;
			transform.eulerAngles = new Vector3(0,0,shakeCurve.Evaluate(t))*10;
			yield return null;
		}
		transform.eulerAngles = originalEulerAngles;
	}
}
