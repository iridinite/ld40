using UnityEngine;

public class MineBeepBoop : MonoBehaviour {

	public Material MatBlinkOff;
	public Material MatBlinkOn;
	public GameObject TileEraserPrefab;
	public GameObject ExplosionParticles;
	public AudioClip ExplodeSound;

	private Transform m_Transform;
	private Renderer m_Renderer;
	private float m_BlinkTime;
	private bool m_BlinkMode;
	private bool m_Exploded;

	private void Start() {
		m_Renderer = GetComponentInChildren<Renderer>();
		m_Transform = GetComponent<Transform>();
		m_Transform.localRotation = Random.rotationUniform;
		m_BlinkTime = Random.value;
	}

	private void Update() {
		m_Transform.localEulerAngles += new Vector3(0f, Tuning.MINE_ROTATE_SPEED * Time.deltaTime * 0.25f, Tuning.MINE_ROTATE_SPEED * Time.deltaTime);

		m_BlinkTime -= Time.deltaTime;
		if (m_BlinkTime <= 0f) {
			m_BlinkMode = !m_BlinkMode;
			m_BlinkTime = m_BlinkMode ? 0.45f : 1f;

			var mats = m_Renderer.materials; // make a copy because Unity doesn't know how to do reference access
			mats[1] = m_BlinkMode ? MatBlinkOn : MatBlinkOff;
			m_Renderer.materials = mats;
		}
	}

	private void OnTriggerEnter(Collider other) {
		if (other.GetComponent<Boulder>() != null) {
			// boulders cause mines to explode
			Explode(null);
			return;
		}

		if (!other.CompareTag("Player")) return;
		// damage
		GameController.Inst.Power *= 0.5f; // test
		GameController.Inst.TakeDamage();
		FindObjectOfType<Player>().ResetAirTime();
		// effect
		Explode(other.attachedRigidbody);
	}

	public void Explode(Rigidbody pushAway) {
		if (m_Exploded) return;
		// don't fire again
		m_Exploded = true;
		Destroy(gameObject, 0.1f);

		// shoot the player away
		if (pushAway != null) {
			var dir = (pushAway.transform.position - m_Transform.position).normalized;
			pushAway.AddForce(dir * Tuning.MINE_EXPLODE_FORCE, ForceMode.VelocityChange);
		}
		// pretty effects
		Instantiate(ExplosionParticles, m_Transform.position, Quaternion.identity);
		GameController.Inst.PlaySoundAt(ExplodeSound, m_Transform.position);
		ScreenShaker.AddShake(0.45f, 0.55f);
		// eraser object (removes tiles and chain-reacts mines)
		Instantiate(TileEraserPrefab, m_Transform.position, Quaternion.identity);
	}

}
