using UnityEngine;

public class CrossSceneSound : MonoBehaviour {

	public AudioClip MyClip;

	private void Start() {
		DontDestroyOnLoad(gameObject);
		GetComponent<AudioSource>().PlayOneShot(MyClip);
		Destroy(gameObject, MyClip.length + 1f);
	}

}
