using UnityEngine;

public enum TileType {
	Normal,
	Gem,
	Gem2,
	PowerS,
	PowerL,
	PowerR,
	PowerB
}

public class Tile : MonoBehaviour {

	public Material MaterialNormal, MaterialGem, MaterialGem2;
	public Material MaterialPowS, MaterialPowL, MaterialPowR, MaterialPowB;

	public GameObject BreakParticles;
	public AudioClip BreakSound;

	public bool Border;
	public TileType Type;

	private Transform m_Transform;

	private void Start() {
		m_Transform = GetComponent<Transform>();

		// random rotation!
		m_Transform.localEulerAngles = new Vector3(0, 0, Random.Range(0, 3) * 90f);

		// pick something to do
		if (Border) {
			Type = TileType.Normal;
		} else if (Random.value < 0.00075f) {
			// throwback, feelsgoodman
			Type = TileType.Gem2;
		} else if (Random.value < 0.05f + GameController.Inst.GetPowerFactor() * 0.05f) {
			Type = TileType.Gem;
		} else if (Random.value < 0.017f) {
			Type = (TileType)Random.Range((int)TileType.PowerS, (int)TileType.PowerB + 1);
		} else {
			Type = TileType.Normal;
		}

		// apply the correct material. it might be worth stuffing them into an array?
		var rend = GetComponentInChildren<Renderer>();
		switch (Type) {
			case TileType.Normal:
				rend.material = MaterialNormal;
				break;
			case TileType.Gem:
				rend.material = MaterialGem;
				break;
			case TileType.Gem2:
				rend.material = MaterialGem2;
				break;
			case TileType.PowerS:
				rend.material = MaterialPowS;
				break;
			case TileType.PowerL:
				rend.material = MaterialPowL;
				break;
			case TileType.PowerR:
				rend.material = MaterialPowR;
				break;
			case TileType.PowerB:
				rend.material = MaterialPowB;
				break;
		}
	}

	public void Break(bool byPlayer) {
		// can't remove border tiles
		if (Border) return;

		if (byPlayer) {
			switch (Type) {
				case TileType.Normal:
					GameController.Inst.AddScore(Tuning.SCORE_BREAK_BLOCK);
					break;
				case TileType.Gem:
					GameController.Inst.AddScore(Tuning.SCORE_BREAK_GEM);
					break;
				case TileType.Gem2:
					GameController.Inst.AddScore(Tuning.SCORE_BREAK_GEM * 10f);
					break;
				case TileType.PowerS:
				case TileType.PowerL:
				case TileType.PowerR:
				case TileType.PowerB:
					//GameController.Inst.AddScore(Tuning.SCORE_BREAK_POWERUP);
					GameController.Inst.GivePowerup((PowerupType)(Type - TileType.PowerS + 1));
					break;
			}

			GameController.Inst.Power += Tuning.POWER_ADD_BLOCK;
		}

		GameController.Inst.PlaySoundAt(BreakSound, m_Transform.position);

		Instantiate(BreakParticles, m_Transform.position, Quaternion.identity);
		ScreenShaker.AddShake(0.12f, 0.22f);

		Destroy(gameObject);
	}

}
