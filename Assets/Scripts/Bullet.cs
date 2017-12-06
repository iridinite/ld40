using UnityEngine;

public class Bullet : MonoBehaviour {

	public AudioClip[] SoundSpawn;
	public AudioClip SoundHit;

	private Transform m_Transform;
	private float m_Life;
	private bool m_Hit;

	private void Start() {
		m_Transform = GetComponent<Transform>();
		m_Life = 10f;
		m_Hit = false;

		// pew pew pew
		GameController.Inst.PlaySoundAt(SoundSpawn[Random.Range(0, SoundSpawn.Length)], m_Transform.position);

		// random color
		var myColor = Color.Lerp(Color.cyan, Color.green, Random.value);
		GetComponent<Renderer>().material.color = myColor;
		GetComponentInChildren<TrailRenderer>().startColor = myColor;
	}

	private void FixedUpdate() {
		m_Transform.localPosition += Vector3.down * Tuning.BULLET_MOVE_SPEED * Time.fixedDeltaTime;

		m_Life -= Time.fixedDeltaTime;
		if (m_Life < 0f) {
			m_Hit = true;
			Destroy(gameObject);
		}
	}

	private void OnTriggerEnter(Collider other) {
		if (m_Hit) return;

		var mine = other.GetComponent<MineBeepBoop>();
		if (mine != null) {
			mine.Explode(null);
			GameController.Inst.AddScore(Tuning.SCORE_BREAK_MINE); // can't do in Explode() because that's also called when colliding with mine
			Remove();
			return;
		}

		var tile = other.GetComponent<Tile>();
		if (tile != null) {
			tile.Break(true);
			Remove();
		}
	}

	private void Remove() {
		GameController.Inst.PlaySoundAt(SoundHit, m_Transform.position);

		// detach the trail renderer so the line doens't immediately disappear
		transform.DetachChildren();
		// remove self
		m_Hit = true;
		Destroy(gameObject);
	}

}
