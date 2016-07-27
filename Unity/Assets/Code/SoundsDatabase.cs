using UnityEngine;
using System.Collections;

public class SoundsDatabase : MonoBehaviour {
	public AudioSource audioSource;

	public AudioClip crashSound;
	public AudioClip splatSound;
	public AudioClip jumpSound;
	public AudioClip arrowShootSound;

	public void PlaySound(AudioClip sound) {
		audioSource.PlayOneShot (sound, 2);
	}
}
