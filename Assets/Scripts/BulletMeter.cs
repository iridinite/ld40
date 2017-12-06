using UnityEngine;
using UnityEngine.UI;

public class BulletMeter : MonoBehaviour {

	public Image BasePip;

	private static readonly Color fullColor = new Color(1f, 0.9f, 0.25f);
	private Image[] m_Pips;

	private void Start() {
		m_Pips = new Image[Tuning.PLAYER_MAX_AMMO];
		m_Pips[0] = BasePip;
		for (int i = 1; i < Tuning.PLAYER_MAX_AMMO; i++) {
			var pip = Instantiate(BasePip.gameObject);
			var row = i % 6;
			var col = i / 6;
			pip.transform.SetParent(transform, false);
			pip.GetComponent<RectTransform>().anchoredPosition = new Vector2(col * -32f, row * -48f);
			m_Pips[i] = pip.GetComponent<Image>();
		}
	}

	private void Update() {
		bool full = GameController.Inst.Ammo == Tuning.PLAYER_MAX_AMMO;

		// color / grey out pips based on ammo counter
		for (int i = 0; i < Tuning.PLAYER_MAX_AMMO; i++) {
			m_Pips[i].color = full
				? fullColor
				: (Color.white * (GameController.Inst.Ammo > i ? 1f : 0.25f));
		}
	}

}
