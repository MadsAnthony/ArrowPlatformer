using UnityEngine;
using System.Collections;
using System;

public enum StickDir {None, Left, Right};
public class HeroScript : MonoBehaviour {

	public float speed; //10 option a consider 9 			/ option b consider 10 		/ option c consider 10
	public float jumpSpeed; //13 // option a consider 4 	/ option b consider 7		/ option c consider 10
	public float jumpSpeed2; // option a consider 2.5 	  	/ option b consider 2.5		/ option c consider 1.7
	public float gravityAcc; // 80 // optiona consider 40 	/ option b consider 65		/ option c consider 65
	public float forceFall;
	public AnimationCurve curve;
	public AnimationCurve doubleJumpCurve;
	float gravity = 0;
	float gap = 0.05f;
	float extraFall;
	float jumpFrames = 0;
	StickDir wallSticking;
	Vector3 dir = Vector3.zero;
	bool isOnGround;
	bool canPressJump = true;
	bool gravityIsOn = true;
	public Renderer rendererBox;
	public Rigidbody rigidbody;
	void Start () {
	
	}

	void LateUpdate () {
		if (gravityIsOn) {
			gravity += gravityAcc * Time.deltaTime;
		}
		//dir = Vector3.zero;
		if (Input.GetKey (KeyCode.RightArrow)) {
			dir += Vector3.right*speed;
		}
		if (Input.GetKey (KeyCode.LeftArrow)) {
			dir += Vector3.left*speed;
		}
		if (Input.GetKeyDown(KeyCode.UpArrow)) {
			if (isOnGround) {
				StopCoroutine("Jump");
				StartCoroutine("Jump");
			}
			if (wallSticking != StickDir.None) {
				StopCoroutine("WallJump");
				StartCoroutine("WallJump");
			}
		}

		if (wallSticking == StickDir.Right) {
			dir += Vector3.right*speed;
		}
		if (wallSticking == StickDir.Left) {
			dir += Vector3.left*speed;
		}

		if (Input.GetKey (KeyCode.DownArrow)) {
			extraFall = forceFall;
			gravity += extraFall;
		} else {
			extraFall = 0;
		}

		gravity = Mathf.Clamp (gravity, -35, 15+extraFall);
		dir *= Time.deltaTime;
		MoveInDir (Vector3.up*-gravity*Time.deltaTime,true);
		MoveInDir (dir);

		if (Input.GetKey (KeyCode.R)) {
			Reset();
		}
	}

	void MoveInDir(Vector3 dir, bool isCeil = false) {
		RaycastHit[] hits = rigidbody.SweepTestAll (dir.normalized,dir.magnitude+gap);
		if (hits.Length == 0) {
			transform.position += dir;
			if (Mathf.Abs(Vector3.Dot(dir,Vector3.up))>0) {
				isOnGround = false;
			}
			return;
		}
		float moveD = 0;
		Vector3 p = new Vector3(0,0,0);
		Array.Sort(hits, delegate(RaycastHit x, RaycastHit y) { return x.distance.CompareTo(y.distance); });
		foreach (RaycastHit hit in hits)  {
			Entity entity = hit.transform.gameObject.GetComponent<Entity>();
			if (entity != null) {
				if (entity.type == EntityType.Spike && isCeil) {
					Reset ();
					break;
				}
			}
			moveD = hit.distance-gap;
			if (gravity>2 && Vector3.Dot(dir,Vector3.right)>0) {
				wallSticking = StickDir.Right;
				gravity = 0;
			}
			if (gravity>2 && Vector3.Dot(dir,Vector3.left)>0) {
				wallSticking = StickDir.Left;
				gravity = 0;
			}
			if (gravity<0 && Mathf.Abs(Vector3.Dot(dir,Vector3.up))>0) {
				gravity = -1;
			}
			break;
		}
		if (Vector3.Dot(dir,Vector3.up)!=0 && moveD != 0) {
			wallSticking = StickDir.None;
		}
		if (Vector3.Dot(dir,Vector3.down)>0 && moveD>0) {
			isOnGround = true;
		}
		//transform.position = p;
		transform.position += dir.normalized * moveD;
	}

	void Reset() {
		Application.LoadLevel (Application.loadedLevel);
	}

	IEnumerator Fade() {
		if (rendererBox != null) {
			for (var f = 1.0; f >= 0; f -= 0.01) {
				var c = rendererBox.material.color;
				c.a = (float)f;
				if (Input.GetKey("f")) {
					rendererBox.material.color = c;
				}
				yield return null;
			}
		}
	}

	IEnumerator Jump() {
		gravityIsOn = false;
		gravity = -10;
		yield return null;
		float jumpFrames = 0;
		while (Input.GetKey (KeyCode.UpArrow)) {
			jumpFrames += 1;
			gravity = -10;
			//gravity = curve.Evaluate (jumpFrames);
			if (jumpFrames>12) {
				gravityIsOn = true;
				break;
			}
			gravityIsOn = true;
			yield return null;
		}
	}

	IEnumerator WallJump() {
		float jumpFrames = 0;
		Vector3 myDir = Vector3.zero;
		if (wallSticking == StickDir.Right) {
			myDir = Vector3.left*speed;
		}
		if (wallSticking == StickDir.Left) {
			myDir = -Vector3.left*speed;
		}
		StopCoroutine("Jump");
		StartCoroutine("Jump");
		wallSticking = StickDir.None;
		float extraJump = 1;
		float t = 0;
		while (true) {
			t += 0.04f;
			if (Input.GetKey (KeyCode.UpArrow)) {
				extraJump += 0.2f;
			}
			Vector3 dirSpeed = extraJump*myDir*doubleJumpCurve.Evaluate (t);
			dir = dirSpeed;
			if (t>1) {
				break;
			}
			yield return null;
		}
	}
}
