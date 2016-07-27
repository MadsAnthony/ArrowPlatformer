using UnityEngine;
using System.Collections;

public class Director : MonoBehaviour {
	public static Director Instance;

	void Awake() {
		Instance = this;
	}

	public EffectsManager EffectsManager;
	public UIManager UIManager;
	public CameraScript CameraScript;
	public SoundsDatabase SoundsDatabase;
}
