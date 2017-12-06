using UnityEngine;

public class Player : MonoBehaviour {

	public GameObject BulletPrefab;
	public GameObject LaserPrefab;
	public GameObject BombPrefab;
	public GameObject TileEraserPrefab;
	public GameObject GibsPrefab;
	public GameObject DustPrefab;
	public GameObject Shield;
	public Transform PlayerModel;
	public Renderer PlayerModelInner;
	public TrailRenderer PlayerTrail;

	public AudioClip JumpSound;
	public AudioClip LandSound;
	public AudioClip LaserSound;

	private GameController m_Ctl;
	private Transform m_Transform;
	private Rigidbody m_Rigidbody;
	private SphereCollider m_Collider;
	private Vector3 m_Movement;

	private float m_ShootCooldown;
	private float m_JumpCooldown;
	private float m_DustCooldown;
	private float m_AmmoCooldown;
	private float m_AmmoRecharge;

	private float m_TimeAirborne;
	private float m_InvulnBlinkTime;
	private bool m_WasGrounded;

	private void Start() {
		m_Ctl = GameController.Inst;
		m_Rigidbody = GetComponent<Rigidbody>();
		m_Transform = GetComponent<Transform>();
		m_Collider = GetComponent<SphereCollider>();
		m_Transform.localPosition = new Vector3(Tuning.MAPCENTER, 2f, 0f);
	}

	private void Update() {
		// ignore all input if the game has been paused
		if (m_Ctl.Paused) return;

		if (m_Ctl.Health <= 0) {
			gameObject.SetActive(false);
			Instantiate(GibsPrefab, m_Transform.position, m_Transform.rotation);
			return;
		}

		m_Movement = new Vector3(Input.GetAxis("Horizontal"), 0, 0)
			* Tuning.PLAYER_MOVE_SPEED
			* (m_Ctl.GetPowerFactor() * 0.35f + 1f); // LEEEROOOYYY

		// get ammo
		if (m_AmmoCooldown > 0f) {
			m_AmmoCooldown -= Time.deltaTime;
		} else if (m_Ctl.Ammo < Tuning.PLAYER_MAX_AMMO) {
			if (m_AmmoRecharge > 0f)
				m_AmmoRecharge -= Time.deltaTime;
			else {
				m_AmmoRecharge = Tuning.PLAYER_RECHARGE_INTERVAL;
				m_Ctl.Ammo++;
			}
		}

		// blink the model if invulnerable
		if (m_Ctl.GetInvulnerableTime() > 0f) {
			m_InvulnBlinkTime -= Time.deltaTime;
			if (m_InvulnBlinkTime <= 0f) {
				m_InvulnBlinkTime = Mathf.Clamp(m_Ctl.GetInvulnerableTime() * 0.05f, 0.02f, 0.08f);
				PlayerModel.gameObject.SetActive(!PlayerModel.gameObject.activeSelf);
			}
		} else {
			PlayerModel.gameObject.SetActive(true);
		}
		// show shield bubble
		Shield.SetActive(m_Ctl.Powerup == PowerupType.Shield);

		// fire bullets
		m_ShootCooldown -= Time.deltaTime;
		if ((Input.GetButton("Fire1") || Input.GetButton("Fire2")) && m_ShootCooldown <= 0f) {
			// no spam pls
			m_ShootCooldown = Tuning.PLAYER_SHOOT_COOLDOWN;

			// fire a thing
			if (m_Ctl.Powerup == PowerupType.Laser) {
				// remove powerup
				m_Ctl.Powerup = PowerupType.None;
				m_Ctl.PlaySoundAt(LaserSound, m_Transform.position);
				// bonus recoil
				m_Rigidbody.AddForce(new Vector3(0f, Tuning.PLAYER_RECOIL_FORCE_LASER, 0f), ForceMode.VelocityChange);
				// spawn a deadly lazor
				Destroy(Instantiate(LaserPrefab, m_Transform.position + Vector3.down * 6f, Quaternion.identity), 0.25f);
				for (int i = 0; i < 12; i++) {
					var eraser = Instantiate(TileEraserPrefab, m_Transform.position + Vector3.down * i, Quaternion.identity);
					eraser.GetComponent<TileEraser>().Radius = 0.5f;
				}

			} else if (m_Ctl.Powerup == PowerupType.Boulder) {
				// remove powerup
				m_Ctl.Powerup = PowerupType.None;
				var bomb = Instantiate(BombPrefab, m_Transform.position + Vector3.down, Quaternion.identity);
				bomb.GetComponent<Rigidbody>().AddForce(Random.value * 4f - 2f, 0f, 0f, ForceMode.VelocityChange);
				// hard recoil
				m_Rigidbody.AddForce(new Vector3(0f, Tuning.PLAYER_RECOIL_FORCE_BOMB, 0f), ForceMode.VelocityChange);

			} else if (m_Ctl.Powerup == PowerupType.RapidFire) {
				// spawn bullet
				m_ShootCooldown = 0.04f;
				Instantiate(BulletPrefab, m_Transform.position, Quaternion.identity);
				// very small amount of recoil (because there's a crapton of bullets)
				m_Rigidbody.AddForce(new Vector3(0f, Tuning.PLAYER_RECOIL_FORCE * 0.15f, 0f), ForceMode.VelocityChange);

			} else if (m_Ctl.Ammo > 0) {
				// set cooldown
				m_AmmoRecharge = Tuning.PLAYER_RECHARGE_INTERVAL;
				m_AmmoCooldown = Tuning.PLAYER_RECHARGE_DELAY;
				// spawn bullet
				m_Ctl.Ammo--;
				Instantiate(BulletPrefab, m_Transform.position, Quaternion.identity);
				// recooooooooooooiiiiiiiiiiiiiiiil
				m_Rigidbody.AddForce(new Vector3(0f, Tuning.PLAYER_RECOIL_FORCE, 0f), ForceMode.VelocityChange);
			}
		}

		bool grounded = Physics.Raycast(m_Transform.position, Vector3.down, 0.75f, -1, QueryTriggerInteraction.Ignore);

		if (grounded && !m_WasGrounded) {
			// we just landed
			MakeDust();
			m_Ctl.PlaySoundAt(LandSound, m_Transform.position);
		}
		m_WasGrounded = grounded;

		if (grounded)
			ResetAirTime();
		else
			m_TimeAirborne += Time.deltaTime;

		m_Ctl.Power += (m_TimeAirborne * Tuning.POWER_ADD_AIRBORNE) * Time.deltaTime;

		m_JumpCooldown -= Time.deltaTime;
		if (Input.GetButtonDown("Jump") && m_JumpCooldown <= 0f && grounded) {
			m_Ctl.PlaySoundAt(JumpSound, m_Transform.position);
			m_JumpCooldown = 0.15f; // prevent spam before leaving ground
			m_Rigidbody.AddForce(new Vector3(0, Tuning.PLAYER_JUMP_FORCE, 0), ForceMode.VelocityChange);
		}

		// drain power if player is on the ground
		if (grounded)
			m_Ctl.Power = Mathf.Max(0f, m_Ctl.Power - Time.deltaTime * Tuning.POWER_DRAIN_GROUNDED);

		// spin the model!
		PlayerModel.localEulerAngles += Time.deltaTime * Vector3.back * m_Rigidbody.velocity.x * 64f;
		//PlayerModelInner.material.
		PlayerModelInner.material.color = Color.red * m_Ctl.GetPowerFactor();
		PlayerTrail.startColor = Color.red * m_Ctl.GetPowerFactor();
	}

	private void FixedUpdate() {
		m_DustCooldown -= Time.fixedDeltaTime;

		// apply user input
		if (m_Movement.sqrMagnitude > 0f) {
			RaycastHit hit;
			// use a spherecast to see if the player will hit a wall. if so, disallow the input, because if the player can push into a wall,
			// the physics sim will let them 'hang in place' w/o gravity
			if (!Physics.SphereCast(m_Transform.position, m_Collider.radius * 0.5f, m_Movement.normalized, out hit, m_Collider.radius * 0.75f)) {
				// looks pretty
				if (m_WasGrounded && m_DustCooldown <= 0f) {
					m_DustCooldown = 0.1f;
					MakeDust();
				}
				// accelerate
				m_Rigidbody.AddForce(m_Movement * Tuning.PLAYER_MOVE_SPEED, ForceMode.Acceleration);
			}
		}

		// calculate target position, and clamp it to within field
		var clamppos = m_Rigidbody.position;
		clamppos.x = Mathf.Clamp(clamppos.x, 0f, Tuning.MAPWIDTH - 1);
		m_Rigidbody.position = clamppos;

		// limit velocities to reasonable amounts
		m_Rigidbody.velocity = new Vector3(
			Mathf.Clamp(m_Rigidbody.velocity.x, -Tuning.PLAYER_MOVE_SPEED_MAX, Tuning.PLAYER_MOVE_SPEED_MAX),
			Mathf.Clamp(m_Rigidbody.velocity.y, GetMaxFallSpeed(), Tuning.PLAYER_MOVE_SPEED_MAX * 10f),
			0f);
	}

	public float GetMaxFallSpeed() {
		// here's where the theme comes in i guess
		return -(Tuning.PLAYER_FALL_SPEED_BASE + Mathf.Clamp01(m_Ctl.GetPowerFactor()) * Tuning.PLAYER_FALL_SPEED_DELTA);
	}

	public void ResetAirTime() {
		m_TimeAirborne = 0f;
	}

	public void MakeDust() {
		Instantiate(DustPrefab, m_Transform.position + Vector3.down * 0.5f + Vector3.back * 0.5f, Quaternion.identity);
	}

	private void OnCollisionEnter(Collision collision) {
		// lose power
		m_Ctl.Power -= Tuning.POWER_DRAIN_BUMP;

		// if we hit a block with enough speed, break through the block, because that looks awesome
		if (Mathf.Abs(m_Rigidbody.velocity.y) >= Tuning.PLAYER_MOVE_SPEED_MAX * 0.5f) {
			var tile = collision.transform.GetComponent<Tile>();
			if (tile != null)
				tile.Break(true);
		}

		MakeDust();
	}

}
