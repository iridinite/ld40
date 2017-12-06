using UnityEngine;

public class FollowCam : MonoBehaviour {

	private Transform m_Transform;

	public Transform Target;
	public float ZOffset;

	private void Start() {
		m_Transform = GetComponent<Transform>();
		m_Transform.position = Target.position + Vector3.back * ZOffset;
	}

	private void FixedUpdate() {
		// when above ground, show space above player, otherwise focus on below
		var ybonus = Target.position.y >= 0f ? 0f : -3.5f;
		var zbonus = GameController.Inst.GetPowerFactor() * 2f;
		var targetpos = new Vector3(Mathf.Clamp(Target.position.x, Tuning.MAPCENTER - 4f, Tuning.MAPCENTER + 4f), Target.position.y + ybonus, -ZOffset - zbonus);
		var diff = targetpos - m_Transform.position;
		//float diff = Target.position.y - m_Transform.position.y;

		//m_Transform.localPosition +=;
		m_Transform.position += diff * 8f * Time.fixedDeltaTime;
	}

}
