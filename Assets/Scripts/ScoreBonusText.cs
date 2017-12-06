using UnityEngine;
using UnityEngine.UI;

public class ScoreBonusText : MonoBehaviour {

	private Text m_Shadow;
	private Text m_Text;
	private RectTransform m_Rect;

	private float m_Life;
	private float m_LifeMax;

	public bool Blink;

	private void Start() {
		m_Rect = GetComponent<RectTransform>();
		m_Shadow = GetComponent<Text>();
		m_Text = transform.GetChild(0).GetComponent<Text>(); // fixme, hardcoded hierarchy
		m_Text.text = m_Shadow.text;
		m_LifeMax = Blink ? 4f : 2f;

		if (Blink) {
			// special text is moved slightly to the right and lasts longer, for readability
			m_Rect.anchoredPosition += Vector2.right * 100f;
		}
	}

	private void Update() {
		var color = Blink ? Color.Lerp(Color.red, Color.yellow, Mathf.Sin(Time.time * 14f) * 0.5f + 0.5f) : Color.white;
		var opacity = (1f - m_Life / m_LifeMax);

		m_Life += Time.deltaTime;
		m_Text.color = color * opacity;
		m_Shadow.color = Color.black * opacity;
		m_Rect.anchoredPosition += Vector2.down * (Blink ? 50f : 100f) * Time.deltaTime;

		if (m_Life > m_LifeMax)
			Destroy(gameObject);
	}

}
