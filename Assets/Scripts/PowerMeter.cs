using UnityEngine;
using UnityEngine.UI;

public class PowerMeter : MonoBehaviour {

	public Image BasePip;
	public Text PowerOverwhelming;

	private static readonly Color blinkColor1 = new Color(1f, 0.5f, 0.2f);
	private static readonly Color blinkColor2 = Color.yellow;

	private const int NUM_PIPS = 10;
	private Image[] m_Pips;

	private void Start() {
		m_Pips = new Image[NUM_PIPS];
		m_Pips[0] = BasePip;
		for (int i = 1; i < NUM_PIPS; i++) {
			var pip = Instantiate(BasePip.gameObject);
			pip.transform.SetParent(transform, false);
			pip.GetComponent<RectTransform>().anchoredPosition = new Vector2(0f, i * -48f);
			m_Pips[i] = pip.GetComponent<Image>();
		}
	}

	private void Update() {
		var colornormal = Color.white;
		var blinkopacity = Mathf.Clamp(GameController.Inst.GetPowerFactor() - 0.5f, 0f, 0.5f) * 2f; // start showing blink at 50% power

		for (int i = 0; i < NUM_PIPS; i++) {
			var colorblink = Color.Lerp(blinkColor1, blinkColor2, Mathf.Sin(-Time.time * 5f + i) * 0.5f + 0.5f); // do the wave!

			float opacity = Mathf.Clamp01(GameController.Inst.GetPowerFactor() * NUM_PIPS - i);
			m_Pips[i].color = Color.Lerp(colornormal, colorblink, blinkopacity) * ((opacity * 0.85f) + 0.15f);
		}

		// power max text
		PowerOverwhelming.color = Color.Lerp(blinkColor1, blinkColor2, Mathf.Sin(-Time.time * 8f) * 0.5f + 0.5f);
		PowerOverwhelming.transform.parent.gameObject.SetActive(GameController.Inst.GetPowerFactor() >= 1f); // hierarchy hack, we know a TextShadow is being used
	}

}
