using System;
using System.Collections.Generic;
using System.Globalization;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Random = UnityEngine.Random;

public enum PowerupType {
	None,
	Shield,
	Laser,
	RapidFire,
	Boulder
}

public class GameController : MonoBehaviour {

	public static GameController Inst { get; private set; }

	public GameObject ScoreTextPrefab;
	public GameObject XSoundPrefab;
	public AudioClip PowerUpSound;
	public AudioClip ShieldSound;
	public AudioClip HighScoreSound;
	public AudioClip ConfirmSound;
	public Canvas TheCanvas;

	public GameObject HUDPanel;
	public GameObject GameOverPanel;
	public GameObject HiscorePanel;
	public GameObject PausedPanel;
	public Text GameOverResultText;
	public Text GameOverAgainText;
	public Text TimerText;
	public Text PowerupText;
	public Image Fader;

	public int Ammo { get; set; }
	public int Health { get; set; }

	public float Score { get; private set; }
	public float Power { get; set; }
	public PowerupType Powerup { get; set; }

	public bool Paused { get; private set; }
	//public bool ControllerMode { get; private set; }

	private TimeSpan m_Timer;
	private float m_NextExtend;
	private float m_InvulnTime;
	private float m_RapidTime;
	private float m_HighScore;
	private float m_MessageShowTime;
	private float m_FaderOpacity;
	private Queue<string> m_MessageQueue;
	private bool m_HasHighScore;
	private bool m_FadingOut;
	private bool m_FadingToMenu;

	private void Awake() {
		Inst = this;
	}

	private void Start() {
		m_MessageQueue = new Queue<string>();
		Ammo = Tuning.PLAYER_MAX_AMMO;
		Health = Tuning.PLAYER_HEALTH_START;
		m_NextExtend = Tuning.SCORE_EXTEND;
		m_HighScore = PlayerPrefs.GetFloat("highScore");
		Powerup = PowerupType.None;
		m_FaderOpacity = 1f;
		Time.timeScale = 1f;
	}

	private void Update() {
		// are we feeling like using a controller today?
		// i suppose that if a controller is connected, they might want to use it?
		//var thing = Input.GetJoystickNames();
		//ControllerMode = Input.GetJoystickNames().Length > 0;

		// fade screen
		Fader.color = Color.black * m_FaderOpacity;
		m_FaderOpacity = m_FadingOut
			? Mathf.Clamp01(m_FaderOpacity + Time.unscaledDeltaTime)
			: Mathf.Clamp01(m_FaderOpacity - Time.unscaledDeltaTime);

		if (m_FadingOut) {
			if (m_FaderOpacity >= 1f)
				SceneManager.LoadScene(m_FadingToMenu ? "Title" : "Game");
			// no input while animating
			return;
		}

		if (Health <= 0) {
			// ded
			if (HUDPanel.activeSelf) {
				// do this only once, for performance (many string concats)
				Paused = false;
				HUDPanel.SetActive(false);
				GameOverPanel.SetActive(true);
				GameOverResultText.text = "Score: " + Score.ToString("##,##0", CultureInfo.InvariantCulture) + "  //  Play Time: " +
					m_Timer.TotalMinutes.ToString("##00") + ":" + m_Timer.Seconds.ToString("00");
				HiscorePanel.SetActive(m_HasHighScore);
				//GameOverAgainText.text = ControllerMode ? "Press A or Start to play again  //  Press B or Back to exit" : "Press Enter to play again  //  Press Esc to exit";

				// save any new highscores
				PlayerPrefs.Save();
			}
			// input handling
			if (Input.GetButtonDown("Submit") || Input.GetKeyDown("joystick button 7")) {
				m_FadingOut = true;
				m_FadingToMenu = false;
				PlaySound(ConfirmSound);
			} else if (Input.GetButtonDown("Cancel")) {
				m_FadingOut = true;
				m_FadingToMenu = true;
				PlaySound(ConfirmSound);
			}
			return;
		}

		// pause panel handling
		PausedPanel.SetActive(Paused);
		if (!Paused && Input.GetButtonDown("Pause")) {
			Paused = true;
			Time.timeScale = 0f;
			PlaySound(ConfirmSound);
		} else if (Paused) {
			if (Input.GetButtonDown("Submit") || Input.GetKeyDown("joystick button 7")) {
				Paused = false;
				Time.timeScale = 1f;
				PlaySound(ConfirmSound);
			} else if (Input.GetButtonDown("Cancel")) {
				m_FadingOut = true;
				m_FadingToMenu = true;
				PlaySound(ConfirmSound);
			}
		}

		PowerupText.text = Powerup == PowerupType.None
			? String.Empty
			: Powerup.ToString().ToUpper();

		// game timer.  this if-block is a bit of a cheap hack. if the player has any points at all, then we know they must have
		// broken a block at the surface. therefore, the game timer starts counting.
		if (Score > 0)
			m_Timer += TimeSpan.FromSeconds(Time.deltaTime);
		TimerText.text = m_Timer.TotalMinutes.ToString("##00") + ":" + m_Timer.Seconds.ToString("00");
		TimerText.transform.parent.gameObject.SetActive(m_Timer > TimeSpan.Zero);

		// apply power caps
		// note that we allow a 10% buffer to allow the bar to fill up and make it look like it stays full
		Power = Mathf.Clamp(Power, 0f, Tuning.POWER_MAX * 1.1f);

		// show messages at regular interrvals
		m_MessageShowTime -= Time.deltaTime;
		if (m_MessageQueue.Count > 0 && m_MessageShowTime <= 0f) {
			m_MessageShowTime = 0.25f;
			ShowScoreMessage(m_MessageQueue.Dequeue(), false);
		}

		// decrease i-frames
		m_InvulnTime -= Time.deltaTime;
		if (m_InvulnTime <= 0f && Powerup == PowerupType.Shield)
			Powerup = PowerupType.None;

		m_RapidTime -= Time.deltaTime;
		if (m_RapidTime <= 0f && Powerup == PowerupType.RapidFire)
			Powerup = PowerupType.None;
	}

	public float GetPowerFactor() {
		return Mathf.Clamp01(Power / Tuning.POWER_MAX);
	}

	public void AddScore(float addition) {
		float realScore = addition * (GetPowerFactor() * 4f + 1f);
		Score += realScore;
		m_MessageQueue.Enqueue("+ " + Mathf.CeilToInt(realScore));

		if (Score >= m_NextExtend) {
			// add a life, up to the max
			if (Health < Tuning.PLAYER_HEALTH_MAX) {
				Health++;
				ShowScoreMessage("EXTRA LIFE!", true);
				PlaySound(HighScoreSound);
			}
			// next milestone
			m_NextExtend += Tuning.SCORE_EXTEND;
		}

		if (Score > m_HighScore) {
			if (!m_HasHighScore) {
				m_HasHighScore = true;
				if (m_HighScore > 0f) {
					// don't show a message if high score was zero
					ShowScoreMessage("NEW HIGH SCORE!", true);
					PlaySound(HighScoreSound);
				}
			}
			m_HighScore = Score;
			PlayerPrefs.SetFloat("highScore", m_HighScore);
		}
	}

	public void TakeDamage() {
		if (GetInvulnerableTime() > 0f) return;
		m_InvulnTime = Tuning.PLAYER_INVULN_TIME;

		// bubble prevents one hit
		/*if (Powerup == PowerupType.Shield) {
			Powerup = PowerupType.None;
			return;
		}*/

		Health--;
	}

	public float GetInvulnerableTime() {
		return m_InvulnTime;
	}

	public bool HasHighScore() {
		return m_HasHighScore;
	}

	public void PlaySound(AudioClip clip) {
		var gobj = Instantiate(XSoundPrefab);
		gobj.GetComponent<CrossSceneSound>().MyClip = clip;
	}

	public void PlaySoundAt(AudioClip clip, Vector3 pos) {
		var gobj = Instantiate(XSoundPrefab, pos, Quaternion.identity);
		var src = gobj.GetComponent<AudioSource>();
		gobj.GetComponent<CrossSceneSound>().MyClip = clip;
		src.spatialBlend = 0.7f;
		src.minDistance = 6f;
		src.maxDistance = 20f;
		src.pitch = Random.Range(0.85f, 1.15f);
	}

	public void GivePowerup(PowerupType pow) {
		if (Powerup != PowerupType.None) return;

		Powerup = pow;
		PlaySound(PowerUpSound);

		if (pow == PowerupType.Shield) {
			// invulnerable period
			m_InvulnTime = Tuning.PLAYER_INVULN_TIME_SHIELD;
			PlaySound(ShieldSound);
		} else if (pow == PowerupType.RapidFire) {
			m_RapidTime = Tuning.PLAYER_RAPIDFIRE_TIME;
		}

		// and a nice message to go with it
		ShowScoreMessage(pow.ToString().ToUpper(), true);
	}

	public void ShowScoreMessage(string text, bool blink) {
		var messageObject = Instantiate(ScoreTextPrefab, TheCanvas.transform);
		var messageText = text;
		messageObject.GetComponent<Text>().text = messageText;
		messageObject.GetComponent<ScoreBonusText>().Blink = blink;
	}

}
