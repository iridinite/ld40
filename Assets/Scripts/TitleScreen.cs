using System;
using System.Globalization;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class TitleScreen : MonoBehaviour {

	public Image Background;
	public Image Fader;
	public Text Hiscore;
	public GameObject XSoundPrefab;
	public AudioClip ConfirmSound;

	private bool m_FadingOut;
	private float m_FaderOpacity = 1f;
	private float m_BgMovement;

	private void Start() {
		Background.rectTransform.sizeDelta = new Vector2(Screen.width, Mathf.Max(Screen.height * 2, 1280f * 2f));

		Hiscore.text = "Hi-Score: " + PlayerPrefs.GetFloat("highScore").ToString("##,##0", CultureInfo.InvariantCulture);
		// just hide the text if there's no hiscore yet
		if (PlayerPrefs.GetFloat("highScore") <= 0f)
			Hiscore.text = String.Empty;

		Time.timeScale = 1f;
	}

	private void Update() {

		Vector2 pos1 = Background.rectTransform.offsetMin;
		m_BgMovement += Time.unscaledDeltaTime * 64f;
		pos1.y += Time.unscaledDeltaTime * 64f;
		if (m_BgMovement >= 256f) {
			m_BgMovement -= 256f;
			pos1.y -= 256f;
		}

		Background.rectTransform.offsetMin = pos1;

		Fader.color = Color.black * m_FaderOpacity;
		m_FaderOpacity = m_FadingOut
			? Mathf.Clamp01(m_FaderOpacity + Time.unscaledDeltaTime)
			: Mathf.Clamp01(m_FaderOpacity - Time.unscaledDeltaTime);
		if (m_FadingOut) {
			if (m_FaderOpacity >= 1f)
				SceneManager.LoadScene("Game");
			return;
		}

		// exit to desktop
		if (Input.GetButtonDown("Cancel")) {
			Debug.Log("Exit");
			Application.Quit();
			return;
		}

		// start fade to game
		if (Input.anyKeyDown) {
			var gobj = Instantiate(XSoundPrefab);
			gobj.GetComponent<CrossSceneSound>().MyClip = ConfirmSound;

			m_FadingOut = true;
		}
	}

}
