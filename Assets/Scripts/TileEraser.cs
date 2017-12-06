using UnityEngine;

public class TileEraser : MonoBehaviour {

	public float Radius = 1.5f;

	private void Start() {
		GetComponent<SphereCollider>().radius = Radius;
		Destroy(gameObject, 1f);
	}

	private void OnTriggerEnter(Collider other) {
		var tile = other.GetComponent<Tile>();
		if (tile != null)
			tile.Break(false);

		var mine = other.GetComponent<MineBeepBoop>();
		if (mine != null) // chain reactions, let's go!
			mine.Explode(null);
	}

}
