using UnityEngine;
using System.Collections;

public class ArrowScript : MonoBehaviour {
	public Vector3 dir;
	public Vector3 speedDir;
	float tmpGravity;
	Vector3 lineStartPos = Vector3.zero;
	// Use this for initialization
	void Start () {
	}

	public void Init(Vector3 speedDir) {
		this.speedDir = speedDir;
		tmpGravity = 0;
	}
	// Update is called once per frame
	void Update () {
		tmpGravity += 0.0003f;
		speedDir += new Vector3(0,-tmpGravity,0);
		this.transform.position += speedDir;
		transform.eulerAngles = new Vector3(0,0,Mathf.Atan2(speedDir.y, speedDir.x)*Mathf.Rad2Deg);
	}
}
