using System.Globalization;
using UnityEngine;
using UnityEngine.UI;

public class ScoreIndicator : MonoBehaviour {

	private Text m_Text;

	private float m_CurrentDisplay;

	private void Start() {
		m_Text = GetComponent<Text>();
		m_CurrentDisplay = 0;
	}

	private void Update() {
		if (GameController.Inst.Score > m_CurrentDisplay) {
			float delta = Mathf.Max(1, (GameController.Inst.Score - m_CurrentDisplay) / 16f);
			m_CurrentDisplay += delta;
		} else if (GameController.Inst.Score < m_CurrentDisplay) {
			m_CurrentDisplay = GameController.Inst.Score;
		}

		m_Text.text = m_CurrentDisplay.ToString("##,##0", CultureInfo.InvariantCulture);
		if (GameController.Inst.HasHighScore())
			m_Text.text += " !";
	}

}
