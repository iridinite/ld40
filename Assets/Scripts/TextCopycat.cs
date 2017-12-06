using UnityEngine;
using UnityEngine.UI;

public class TextCopycat : MonoBehaviour {

	public Text Master;
	public bool CopyAlpha = false;
	private Text Self;

	private void Start() {
		Self = GetComponent<Text>();
	}

	private void Update() {
		Self.text = Master.text;

		if (CopyAlpha)
			Self.color = new Color(Self.color.r, Self.color.g, Self.color.b, Master.color.a);
	}

}
