using UnityEngine;
using UnityEngine.UI;

public class HealthMeter : MonoBehaviour {

	public Image BasePip;

	private Image[] m_Pips;

	private bool m_blinkMode;
	private float m_blinkTime;

	private void Start() {
		m_Pips = new Image[Tuning.PLAYER_HEALTH_MAX];
		m_Pips[0] = BasePip;
		for (int i = 1; i < Tuning.PLAYER_HEALTH_MAX; i++) {
			var pip = Instantiate(BasePip.gameObject);
			pip.transform.SetParent(transform, false);
			pip.GetComponent<RectTransform>().anchoredPosition = new Vector2(i * -48f, 0f);
			m_Pips[i] = pip.GetComponent<Image>();
		}
	}

	private void Update() {
		// blink timer, for last heart
		m_blinkTime -= Time.deltaTime;
		if (m_blinkTime < 0f) {
			m_blinkTime = 0.25f;
			m_blinkMode = !m_blinkMode;
		}

		// color / grey out pips based on ammo counter
		for (int i = 0; i < Tuning.PLAYER_HEALTH_MAX; i++) {
			// split this into an if block because too many ternary operators
			if (i == 0 && GameController.Inst.Health == 1) {
				m_Pips[i].color = m_blinkMode ? Color.red : Color.white;
			} else {
				m_Pips[i].color = (GameController.Inst.Health > i // visible or not?
					? Color.white
					: Color.clear);
			}
		}
	}

}
