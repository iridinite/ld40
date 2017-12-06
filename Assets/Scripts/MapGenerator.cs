using UnityEngine;

public class MapGenerator : MonoBehaviour {

	public GameObject BackWallPrefab;
	public GameObject TilePrefab;
	public GameObject MinePrefab;

	private Player m_Player;
	private Transform m_PlayerTransform;
	private Transform m_Container;

	private int m_GeneratedUpTo;

	private void Start() {
		m_Container = new GameObject("Map Container").transform;
		m_Player = FindObjectOfType<Player>();
		m_PlayerTransform = m_Player.GetComponent<Transform>();
		m_GeneratedUpTo = 0;

		Noise.Octaves = 4;
		Noise.Frequency = 0.45;
		Noise.Amplitude = 2.2;

		// place the ground level
		for (int x = -40; x < Tuning.MAPWIDTH + 40; x++) {
			// instantiate prefab
			var newobj = Instantiate(TilePrefab, m_Container);
			newobj.transform.position = new Vector3(x, 0f, 0f);

			// tiles outside playable area should be marked as indestructible
			if (x < 0 || x >= Tuning.MAPWIDTH)
				newobj.GetComponent<Tile>().Border = true;
		}
		// back wall
		var bwall = Instantiate(BackWallPrefab, m_Container);
		bwall.transform.position = new Vector3(0f, 0f, 0f);
	}

	private void Update() {
		// generate the map up to a few blocks under the player
		int shouldGenerateUpTo = (int)m_PlayerTransform.position.y - 15;
		while (m_GeneratedUpTo >= shouldGenerateUpTo) {
			m_GeneratedUpTo--;

			// back wall
			var bwall = Instantiate(BackWallPrefab, m_Container);
			bwall.transform.position = new Vector3(0f, m_GeneratedUpTo, 0f);

			// place a row of tiles
			for (int x = 0; x < Tuning.MAPWIDTH; x++) {
				// noise sample
				var noise = Noise.Value2D(x, m_GeneratedUpTo);
				if (noise < 0.5) continue;
				// instantiate prefab
				var newobj = Instantiate(Random.value < GetMineChance() ? MinePrefab : TilePrefab, m_Container);
				newobj.transform.position = new Vector3(x, m_GeneratedUpTo, 0f);
			}

			// and two border tiles
			var b1 = Instantiate(TilePrefab, m_Container);
			b1.GetComponent<Tile>().Border = true;
			b1.transform.position = new Vector3(-1, m_GeneratedUpTo, 0f);
			var b2 = Instantiate(TilePrefab, m_Container);
			b2.GetComponent<Tile>().Border = true;
			b2.transform.position = new Vector3(Tuning.MAPWIDTH, m_GeneratedUpTo, 0f);
		}
	}

	private float GetMineChance() {
		// new version: if the player has a high power level, generate more mines
		return Tuning.MINE_CHANCE_BASE + GameController.Inst.GetPowerFactor() * Tuning.MINE_CHANCE_DELTA;
		//Mathf.Clamp01(Mathf.Abs(y) / 400f) * 0.1f;
	}

}
