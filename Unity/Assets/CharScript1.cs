using UnityEngine;
using System.Collections;
using System;

//public enum StickDir {None, Left, Right};
public class CharScript1 : MonoBehaviour {

	private int lives;
	public int Lives { 
		get
		{
			return lives;
		}
		set 
		{
			lives = value;
			Director.Instance.UIManager.SetLife(lives);
			if (lives<=0) {
				Invoke("Reset",0.1f);
			}
		}
	}
	public float speed;
	public float gravityAcc;
	public float forceFall;
	public AnimationCurve curve;
	float gravity = 0;
	float gap = 0.05f;
	float extraFall;
	StickDir wallSticking;
	Vector3 dir = Vector3.zero;
	bool isOnGround;

	public Rigidbody rigidbody;
	public Animator anim;
	public CameraScript camera;
	public ParticleSystem particleTrail;
	bool enableGravity = true;
	Vector3 startScale;
	bool canMoveHorizontal = true;
	bool vulnerable = true;
	public GameObject cursor;
	public LineRenderer line;

	public bool Vulnerable { 
		get { return vulnerable;}
		set { vulnerable = value;
			if (vulnerable) {
				visualHero.GetComponent<Renderer> ().material.color = new Color(163/255f,255/255f,109/255f,1); // green
			} else {
				visualHero.GetComponent<Renderer> ().material.color = new Color(255/255f,101/255f,101/255f,1); // red
			}
		}
	}

	public GameObject visualHero;
	public GameObject arrow;
	bool pauseAll;

	void Start () {
		Lives = 3;
		startScale = anim.transform.localScale;
		cursor.SetActive (false);
	}

	void LateUpdate () {
		if (pauseAll) return;
		if (anim == null) return;
		if (enableGravity) {
			gravity += gravityAcc * Time.deltaTime;
		}
		if (canMoveHorizontal) {
			if (Input.GetKey (KeyCode.RightArrow)) {
				dir += Vector3.right * speed;
				anim.transform.localScale = startScale;
				anim.SetTrigger ("Run");
			}
			if (Input.GetKey (KeyCode.LeftArrow)) {
				dir += Vector3.left * speed;
				anim.transform.localScale = new Vector3 (startScale.x * -1, startScale.y, startScale.z);
				anim.SetTrigger ("Run");
			}
		}
		if (!Input.GetKey (KeyCode.RightArrow) && !Input.GetKey (KeyCode.LeftArrow)) {
			anim.SetTrigger ("Idle");
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

		if (Input.GetKey (KeyCode.DownArrow)) {
			extraFall = forceFall;
			gravity += extraFall;
		} else {
			extraFall = 0;
		}

		if (Input.GetKeyDown (KeyCode.A)) {
			StopCoroutine("ShootArrow");
			StartCoroutine("ShootArrow");
		}
		gravity = Mathf.Clamp (gravity, -35, 15+extraFall);
		MoveInDir (Vector3.up*-gravity*Time.deltaTime,true);
		dir *= Time.deltaTime;
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
		Vector3 hitPoint = Vector3.zero;
		foreach (RaycastHit hit in hits)  {
			moveD = hit.distance-gap;
			hitPoint = hit.point;
			Entity entity = hit.transform.gameObject.GetComponent<Entity>();
			if (entity != null) {
				if (entity.type == EntityType.Spike && isCeil) {
					if (Vulnerable) {
						StartCoroutine(Hurt());
					}
					//Reset ();
					break;
				}
			}
			if (gravity>0 && Vector3.Dot(dir,Vector3.right)>0) {
				wallSticking = StickDir.Right;
				gravity = 0;
			}
			if (gravity>0 && Vector3.Dot(dir,Vector3.left)>0) {
				wallSticking = StickDir.Left;
				gravity = 0;
			}
			if (gravity<0 && Mathf.Abs(Vector3.Dot(dir,Vector3.up))>0) {
				enableGravity = true;
				gravity = 0;
			}
			break;
		}
		if (Vector3.Dot(dir,Vector3.up)!=0 && moveD != 0) {
			wallSticking = StickDir.None;
		}
		if (Vector3.Dot(dir,Vector3.down)>0 && moveD>0) {
			if (!isOnGround && gravity>20) {
				Director.Instance.SoundsDatabase.PlaySound(Director.Instance.SoundsDatabase.crashSound);
				Director.Instance.CameraScript.Shake();
				Director.Instance.EffectsManager.InsertEffect(hitPoint);
			}
			isOnGround = true;
		}
		transform.position += dir.normalized * moveD;
	}

	void Reset() {
		Application.LoadLevel (Application.loadedLevel);
	}

	IEnumerator Hurt() {
		Vulnerable = false;
		Director.Instance.SoundsDatabase.PlaySound(Director.Instance.SoundsDatabase.splatSound);
		Lives -= 1;
		if (gravity < 0) {
			gravity = 1;
		} else {
			gravity = -8;
		}
		/*canMoveHorizontal = false;
		float t = 0;
		if (gravity < 0) {
			gravity = 1;
		} else {
			gravity = -10;
		}
		float dirSign = Mathf.Sign(dir.x);
		if (dirSign == 0) {
			dirSign = 1;
		}
		Lives -= 1;
		while(true) {
			t += Time.deltaTime;
			dir += Vector3.left*speed*dirSign;
			if (t>2) break;
			if (isOnGround) break;
			if (wallSticking != StickDir.None) break;
			yield return null;
		}
		canMoveHorizontal = true;*/
		yield return new WaitForSeconds (2);
		Vulnerable = true;
	}

	IEnumerator Jump() {
		enableGravity = false;
		Director.Instance.SoundsDatabase.PlaySound(Director.Instance.SoundsDatabase.jumpSound);
		float jumpFrames = 0;
		while (Input.GetKey (KeyCode.UpArrow)) {
			if (pauseAll) yield return null;
			gravity = curve.Evaluate (jumpFrames);
			jumpFrames += 0.1f*Time.deltaTime*50;
			if (jumpFrames>=0.8f || enableGravity) {
				enableGravity = true;
				break;
			}
			yield return null;
		}
		enableGravity = true;
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
		//gravity = -15;
		wallSticking = StickDir.None;
		float extraJump = 1;
		float t = 0;
		float windForce = 0;
		canMoveHorizontal = false;
		while (true) {
			if (pauseAll) yield return null;
			if (Input.GetKey (KeyCode.UpArrow)) {
				if (jumpFrames<=1) {
					jumpFrames += Time.deltaTime*25;//0.2f;
					windForce = curve.Evaluate (jumpFrames)*5;
				}
			}
			windForce += gravityAcc*0.01f;
			if (gravity>10) {
				if (Input.GetKey (KeyCode.RightArrow) || Input.GetKey (KeyCode.LeftArrow)) {
					break;
				}
			}
			dir = -windForce*myDir*0.02f;
			if (isOnGround || wallSticking != StickDir.None || Input.GetKeyDown (KeyCode.RightArrow) || Input.GetKeyDown (KeyCode.LeftArrow)) {
				break;
			}
			yield return null;
		}
		canMoveHorizontal = true;
	}

	IEnumerator InsertEffect(Vector3 pos) {
		var effect = Instantiate (Resources.Load("JumpEffect"), pos, Quaternion.identity);
		yield return new WaitForSeconds (2f);
		Destroy (effect);
	}

	IEnumerator ShootArrow() {
		pauseAll = true;
		cursor.SetActive (true);
		Vector3 cursorDir;
		float cursorSpeed = 10;
		//cursor.transform.localPosition = new Vector3 (0,2,0);
		float arrowSpeed = 10;
		while (true) {
			cursorDir = Vector3.zero;
			if (Input.GetKey (KeyCode.RightArrow)) {
				cursorDir += Vector3.right*cursorSpeed;
			}
			if (Input.GetKey (KeyCode.LeftArrow)) {
				cursorDir += Vector3.left*cursorSpeed;
			}
			if (Input.GetKey (KeyCode.UpArrow)) {
				cursorDir += Vector3.up*cursorSpeed;
			}
			if (Input.GetKey (KeyCode.DownArrow)) {
				cursorDir += Vector3.down*cursorSpeed;
			}
			cursor.transform.localPosition += cursorDir*Time.deltaTime;
			cursor.transform.localPosition = cursor.transform.localPosition.normalized*Mathf.Clamp(cursor.transform.localPosition.magnitude,1,2.5f);

			line.material.mainTextureScale = new Vector2 (0.20f*100, line.material.mainTextureScale.y);
			//line.material.mainTextureOffset = new Vector2(xTextureOffSet,0);
			line.SetVertexCount (100);
			Vector3 speedVector = cursor.transform.localPosition*0.1f;
			float tmpGravity = 0;

			//speedVector *= 0.8f;

			Vector3 velocityDir = speedVector;
			Vector3 tmpPoint = transform.position;
			line.SetPosition (0, transform.position);
			for (int i = 1; i<100; i++) {
				tmpGravity += 0.0003f;
				velocityDir += new Vector3 (0, -tmpGravity,0);
				tmpPoint += velocityDir;
				line.SetPosition (i, tmpPoint);
			}

			if (Input.GetKeyUp (KeyCode.A)) {
				GameObject objectArrow = (GameObject)Instantiate(arrow, transform.position, Quaternion.identity);
				objectArrow.transform.eulerAngles = new Vector3(0,0,Mathf.Atan2(cursor.transform.localPosition.y,cursor.transform.localPosition.x)*Mathf.Rad2Deg);
				objectArrow.GetComponent<ArrowScript>().Init(speedVector);
				Director.Instance.SoundsDatabase.PlaySound(Director.Instance.SoundsDatabase.arrowShootSound);
				break;
			}
			yield return null;
		}
		pauseAll = false;
		cursor.SetActive (false);
	}
}
