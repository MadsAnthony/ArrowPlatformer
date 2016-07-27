using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class UIManager : MonoBehaviour {
	public Text lifeLabel;

	public void SetLife(int lives) {
		lifeLabel.text = lives.ToString();
	}
}