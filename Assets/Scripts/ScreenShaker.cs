using UnityEngine;

public class ScreenShaker : MonoBehaviour {

	private static float shakeintensity;
	private static float shaketime;
	private static float shaketimemax;

	private Transform m_Transform;

	static ScreenShaker() {
		Reset();
	}

	public static void Reset() {
		shakeintensity = 0f;
		shaketime = 0f;
		shaketimemax = 0f;
	}

	public static void AddShake(float time, float intensity) {
		shaketime = time;
		shaketimemax = time;
		shakeintensity = intensity;
	}

	private void Awake() {
		m_Transform = GetComponent<Transform>();
	}

	private void Update() {
		if (shaketime <= 0f) {
			// if timer expired, make sure to reset cam
			m_Transform.localPosition = Vector3.zero;
			return;
		}

		shaketime -= Time.unscaledDeltaTime;

		// get random delta, multiply by time and intensity
		var power = Mathf.Max(shaketime / shaketimemax, 0f) * shakeintensity;
		var delta = Random.insideUnitSphere;

		// apply to camera
		m_Transform.localPosition = delta * power;
	}

}
