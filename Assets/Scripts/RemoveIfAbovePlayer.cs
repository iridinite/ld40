using UnityEngine;

public class RemoveIfAbovePlayer : MonoBehaviour {

	private static Transform m_PlayerTransform; // optimization, get only once for all insts
	private Transform m_Transform;

	private void Start() {
		m_Transform = GetComponent<Transform>();
		if (m_PlayerTransform == null)
			m_PlayerTransform = FindObjectOfType<Player>().GetComponent<Transform>();
	}

	private void Update() {
		// destroy tiles that the player is far ahead of, to save overhead
		if (m_Transform.localPosition.y >= m_PlayerTransform.position.y + 50) {
			Destroy(gameObject);
		}
	}

}
