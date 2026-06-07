using UnityEngine;

public enum StickmanState { Running, Jumping, DoubleJumping, Sliding, Dead }

/// <summary>
/// Active-ragdoll locomotion controller.
/// Each limb is driven by a RagdollMuscle (torque P-controller).
/// Target angles oscillate sinusoidally to produce a running cycle.
/// </summary>
public class StickmanController : MonoBehaviour
{
    // ── Body parts (assigned by StickmanBuilder) ──────────────────────────
    [HideInInspector] public Rigidbody2D torso;
    [HideInInspector] public Rigidbody2D lUpperArm, lForearm;
    [HideInInspector] public Rigidbody2D rUpperArm, rForearm;
    [HideInInspector] public Rigidbody2D lThigh, lShin;
    [HideInInspector] public Rigidbody2D rThigh, rShin;

    // ── Config ─────────────────────────────────────────────────────────────
    [Header("Running")]
    public float strideFreq   = 2.8f;
    public float hipAmplitude = 45f;
    public float kneeMax      = 100f;
    public float armAmplitude = 35f;
    public float forwardForce = 20f;
    public float balanceForce = 80f;

    [Header("Jump")]
    public float jumpImpulse       = 12f;
    public float doubleJumpImpulse = 9f;

    [Header("Slide")]
    public float slideDuration   = 0.6f;
    public float slideTorsoAngle = -75f;

    // ── State ──────────────────────────────────────────────────────────────
    public StickmanState State { get; private set; } = StickmanState.Running;

    private float _phase;
    private float _jumpTimer;
    private float _slideTimer;
    private bool  _grounded;
    private int   _groundContactCount;
    private bool  _dead;

    // Muscles
    private RagdollMuscle _mTorso;
    private RagdollMuscle _mLHip, _mLKnee, _mRHip, _mRKnee;
    private RagdollMuscle _mLShoulder, _mLElbow, _mRShoulder, _mRElbow;

    // ── Lifecycle ──────────────────────────────────────────────────────────

    private void Start()
    {
        if (torso == null) return;

        _mTorso     = new RagdollMuscle(torso,     strength: balanceForce, maxTorque: 300f);
        _mLHip      = new RagdollMuscle(lThigh,    strength: 55f, maxTorque: 180f);
        _mLKnee     = new RagdollMuscle(lShin,     strength: 40f, maxTorque: 120f);
        _mRHip      = new RagdollMuscle(rThigh,    strength: 55f, maxTorque: 180f);
        _mRKnee     = new RagdollMuscle(rShin,     strength: 40f, maxTorque: 120f);
        _mLShoulder = new RagdollMuscle(lUpperArm, strength: 25f, maxTorque: 80f);
        _mLElbow    = new RagdollMuscle(lForearm,  strength: 15f, maxTorque: 50f);
        _mRShoulder = new RagdollMuscle(rUpperArm, strength: 25f, maxTorque: 80f);
        _mRElbow    = new RagdollMuscle(rForearm,  strength: 15f, maxTorque: 50f);
    }

    private void FixedUpdate()
    {
        if (_dead || torso == null) return;

        UpdateGrounded();
        UpdateTimers();
        DriveForce();
        DriveMuscles();
    }

    // ── Ground detection ──────────────────────────────────────────────────

    private void UpdateGrounded()
    {
        _grounded = _groundContactCount > 0 || torso.position.y < GroundBuilder.Y + 1.5f;
    }

    public void OnChildCollisionEnter(GameObject other)
    {
        if (other.CompareTag("Ground") || other.CompareTag("Obstacle"))
            _groundContactCount++;
    }

    public void OnChildCollisionExit(GameObject other)
    {
        if (other.CompareTag("Ground") || other.CompareTag("Obstacle"))
            _groundContactCount = Mathf.Max(0, _groundContactCount - 1);
    }

    // ── Input API ─────────────────────────────────────────────────────────

    public void Jump()
    {
        if (_dead || State == StickmanState.Sliding) return;

        if (_grounded || State == StickmanState.Running)
        {
            DoJump(jumpImpulse, StickmanState.Jumping);
        }
        else if (State == StickmanState.Jumping)
        {
            DoJump(doubleJumpImpulse, StickmanState.DoubleJumping);
        }
    }

    public void Slide()
    {
        if (_dead || !_grounded || State == StickmanState.Sliding) return;
        State       = StickmanState.Sliding;
        _slideTimer = slideDuration;
    }

    public void Die()
    {
        if (_dead) return;
        _dead = true;
        State = StickmanState.Dead;
        GameManager.Instance?.TriggerGameOver();
    }

    // ── Internal ──────────────────────────────────────────────────────────

    private void DoJump(float impulse, StickmanState next)
    {
        // Unity 2022 API: rb.velocity (not linearVelocity)
        torso.velocity = new Vector2(torso.velocity.x, 0f);
        torso.AddForce(Vector2.up * impulse, ForceMode2D.Impulse);
        State              = next;
        _jumpTimer         = 0.25f;
        _groundContactCount = 0;
    }

    private void UpdateTimers()
    {
        float dt = Time.fixedDeltaTime;

        if (_jumpTimer > 0f)
        {
            _jumpTimer -= dt;
            if (_jumpTimer <= 0f && _grounded)
                State = StickmanState.Running;
        }

        if (_slideTimer > 0f)
        {
            _slideTimer -= dt;
            if (_slideTimer <= 0f)
                State = StickmanState.Running;
        }

        if (_grounded && (State == StickmanState.Jumping || State == StickmanState.DoubleJumping))
            State = StickmanState.Running;
    }

    private void DriveForce()
    {
        float targetVX = GameManager.Instance != null ? GameManager.Instance.Speed : 6f;
        float curVX    = torso.velocity.x;   // Unity 2022: .velocity
        float mult     = State == StickmanState.Sliding ? 0.4f : 1f;

        if (curVX < targetVX)
            torso.AddForce(new Vector2((targetVX - curVX) * forwardForce * mult, 0f));
    }

    private void DriveMuscles()
    {
        if (_mTorso == null) return;   // not yet initialized

        _phase += strideFreq * Mathf.PI * 2f * Time.fixedDeltaTime;
        float s = Mathf.Sin(_phase);

        // Balance torso
        float torsoTarget   = State == StickmanState.Sliding ? slideTorsoAngle : -5f;
        _mTorso.targetAngle = torsoTarget;
        _mTorso.Activate();

        switch (State)
        {
            case StickmanState.Running:         RunMuscles(s);   break;
            case StickmanState.Jumping:
            case StickmanState.DoubleJumping:   JumpMuscles();   break;
            case StickmanState.Sliding:         SlideMuscles();  break;
        }
    }

    // ── Muscle poses ──────────────────────────────────────────────────────

    private void RunMuscles(float s)
    {
        _mLHip.targetAngle  =  s * hipAmplitude;
        _mRHip.targetAngle  = -s * hipAmplitude;
        _mLKnee.targetAngle = Mathf.Max(0f,  s) * kneeMax;
        _mRKnee.targetAngle = Mathf.Max(0f, -s) * kneeMax;

        _mLShoulder.targetAngle = -s * armAmplitude;
        _mRShoulder.targetAngle =  s * armAmplitude;
        _mLElbow.targetAngle    = Mathf.Abs(s) * 25f;
        _mRElbow.targetAngle    = Mathf.Abs(s) * 25f;

        ActivateAll();
    }

    private void JumpMuscles()
    {
        _mLHip.targetAngle = -20f;  _mRHip.targetAngle = -20f;
        _mLKnee.targetAngle = 60f;  _mRKnee.targetAngle = 60f;
        _mLShoulder.targetAngle = 35f;  _mRShoulder.targetAngle = -35f;
        _mLElbow.targetAngle = 30f;     _mRElbow.targetAngle = 30f;
        ActivateAll();
    }

    private void SlideMuscles()
    {
        _mLHip.targetAngle = 60f;   _mRHip.targetAngle = 60f;
        _mLKnee.targetAngle = 10f;  _mRKnee.targetAngle = 10f;
        _mLShoulder.targetAngle = -60f; _mRShoulder.targetAngle = 60f;
        _mLElbow.targetAngle = 15f;     _mRElbow.targetAngle = 15f;
        ActivateAll();
    }

    private void ActivateAll()
    {
        _mLHip.Activate();      _mRHip.Activate();
        _mLKnee.Activate();     _mRKnee.Activate();
        _mLShoulder.Activate(); _mRShoulder.Activate();
        _mLElbow.Activate();    _mRElbow.Activate();
    }
}
