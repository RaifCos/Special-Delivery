using UnityEngine;
using System.Collections;
using UnityEngine.InputSystem;

enum GliderStates {
    closed,
    animating,
    opened
}

[RequireComponent(typeof(Rigidbody))]
public class PlayerGliderControl : MonoBehaviour {

    [Header("Player Input")]
    [SerializeField] private InputAction vanGlide;

    [Header("Gliding")]
    [SerializeField] private float glideGravityMultiplier = 0.3f;
    [SerializeField] private float glideLiftCoefficient = 0.02f;
    [SerializeField] private float airPitchTorque = 6f;
    [SerializeField] private float airRollTorque = 6f;
    [SerializeField] private float airYawTorque = 4f;
    [SerializeField] private float airStabilizeTorque = 2f;
    [SerializeField] private float airAngularDamping = 1.5f;
    [SerializeField] private float glideMaxLiftSpeed  = 20f;

    [Header("Glider Effects")]
    [SerializeField] private Transform gliderObject;
    [SerializeField] private AudioClip gliderOpenSound;

    private Rigidbody rb;
    private bool isGlidingLocked = false;
    private GliderStates state = GliderStates.closed;
    private bool glidePressQueued = false;

    void Start() { 
        rb = GetComponent<Rigidbody>();
        if (isGlidingLocked) {
            enabled = false; return; 
        }
    }

    void OnEnable() {
        vanGlide.Enable();
        vanGlide.performed += OnGlidePerformed;
    }

    void OnDisable() {
        vanGlide.performed -= OnGlidePerformed;
        vanGlide.Disable();
    }

    // Queue Glider calls to avoid input delays.
    private void OnGlidePerformed(InputAction.CallbackContext ctx) { glidePressQueued = true; }

    public void GliderUpdate(bool vanGrounded, float vInput, float hInput) {
        if (state == GliderStates.animating) return;

        if (vanGrounded) {
            if (state != GliderStates.closed) {
                StartCoroutine(GliderAnimation(false));
            } glidePressQueued = false;
            return;
        }

        if (glidePressQueued && state != GliderStates.animating) {
            glidePressQueued = false;
            StartCoroutine(GliderAnimation(state == GliderStates.closed));
            return;
        }

        if (state == GliderStates.opened) Glide(vInput, hInput);
    }

    private void Glide(float vInput, float hInput) {
        rb.AddForce((glideGravityMultiplier - 1f) * rb.mass * Physics.gravity, ForceMode.Force);

        float forwardSpeed = Vector3.Dot(transform.forward, rb.linearVelocity);
        float liftSpeed = Mathf.Min(Mathf.Abs(forwardSpeed), glideMaxLiftSpeed);

        float lift = liftSpeed * liftSpeed * glideLiftCoefficient * rb.mass;
        rb.AddForce(transform.up * lift, ForceMode.Force);
        rb.AddTorque(-vInput * airPitchTorque * rb.mass * transform.right, ForceMode.Force);
        rb.AddTorque(-hInput * airRollTorque * rb.mass * transform.forward, ForceMode.Force);
        rb.AddTorque(hInput * airYawTorque * rb.mass * transform.up, ForceMode.Force);
        rb.AddTorque(airAngularDamping * rb.mass * -rb.angularVelocity, ForceMode.Force);

        // Level Rotation
        Vector3 rollAxis = Vector3.Cross(transform.up, Vector3.up);
        rb.AddTorque(airStabilizeTorque * rb.mass * rollAxis, ForceMode.Force);
    }

    public void GliderCrash() {
        if (state == GliderStates.opened) StartCoroutine(GliderAnimation(false));
    }

    private IEnumerator GliderAnimation(bool opening) {
        state = GliderStates.animating;

        GameManager.audioManager.PlaySpatialSoundEffect(gliderOpenSound, transform.position, true);

        float start = opening ? 0.35f : 1f;
        float target = opening ? 1f : 0.35f;
        float rate = opening ? 0.05f : -0.05f;

        gliderObject.localScale = new(start, 1f, 1f);

        while (!Mathf.Approximately(gliderObject.localScale.x, target)) {
            gliderObject.localScale += Vector3.right * rate;
            yield return null;
        } state = opening ? GliderStates.opened : GliderStates.closed;
    }

    public bool IsGliding() => state == GliderStates.opened;
}