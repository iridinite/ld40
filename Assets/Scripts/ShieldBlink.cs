using UnityEngine;

public class ShieldBlink : MonoBehaviour {

	public Color ColorA = Color.white;
	public Color ColorB = Color.white;

	private Renderer m_Renderer;

	private void Start() {
		m_Renderer = GetComponent<Renderer>();
	}

	private void Update() {
		m_Renderer.material.color = Color.Lerp(ColorA, ColorB, Mathf.Sin(Time.time * 8f) * 0.5f + 0.5f) * 0.5f;
	}

}
