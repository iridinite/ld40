using UnityEngine;
using UnityEngine.UI;

public class TextShadow : MonoBehaviour {

	public int Offset = 2;

	private Text m_Self;
	private Text m_Shadow;

	private void Start() {
		// clone the object and attach it to the canvas
		var container = Instantiate(gameObject); //new GameObject();
		container.name = gameObject.name + " (Shadow)";
		container.transform.SetParent(transform.parent, false);
		// no infinite loops please
		var scripts = container.GetComponents<MonoBehaviour>();
		foreach (var script in scripts)
			if (!(script is Text))
				Destroy(script);

		// set shadow color
		m_Self = GetComponent<Text>();
		m_Shadow = container.GetComponent<Text>();
		m_Shadow.color = new Color(0.15f, 0.15f, 0.15f, 1f);

		// apply parenting
		transform.SetParent(container.transform, false);

		// offset the colored text
		var myRect = GetComponent<RectTransform>();
		myRect.anchoredPosition = new Vector2(-Offset, Offset);
		myRect.localRotation = Quaternion.identity;
	}

	private void Update() {
		m_Shadow.text = m_Self.text;
	}

}
