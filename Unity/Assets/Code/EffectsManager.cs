using UnityEngine;
using System.Collections;

public class EffectsManager : MonoBehaviour {
	[SerializeField]
	static GameObject jumpEffect;

	public void InsertEffect(Vector3 pos) {
		StartCoroutine(InsertEffectAnim(pos));
	}
	IEnumerator InsertEffectAnim(Vector3 pos) {
		var effect = Instantiate (Resources.Load("JumpEffect"), pos, Quaternion.identity);
		yield return new WaitForSeconds (2f);
		Destroy (effect);
	}
}