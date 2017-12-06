using UnityEngine;

public class Boulder : MonoBehaviour {

	public AudioClip SpawnSound;

	private void Start() {
		GameController.Inst.PlaySoundAt(SpawnSound, transform.position);
		Destroy(gameObject, 10f);
	}

	private void OnCollisionEnter(Collision collision) {
		var tile = collision.gameObject.GetComponent<Tile>();
		if (tile != null)
			tile.Break(false);

		// mines are also hit, but this is handled in Mine script
	}

}
