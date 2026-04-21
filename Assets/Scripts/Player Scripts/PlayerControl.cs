using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(PlayerBoosterControl))]
public class PlayerControl : MonoBehaviour {
    [Header("Mail Van Properties")]
    public float motorTorque;
    public float brakeTorque;
    public float maxSpeed;
    public float defaultBoostPower;
    public float steeringRange;
    public float steeringRangeAtMaxSpeed;
    private float boostPower;
    private float currentSteerInput = 0f;
    private WheelControl[] wheels;

    [Header("Player Input")]
    public InputAction vanDrive;
    public InputAction vanSteer;

    [Header("Flip Recovery")]
    private readonly float flipRecoveryTorque = 15f;
    private readonly float flipRecoveryDelay = 1.5f;
    private readonly float flipAngleThreshold = 140f;

    private float flippedTimer = 0f;

    [Header("Audio Handler")]
    public AudioSource engineSound;
    
    private PlayerBoosterControl pbc;
    private Rigidbody rb;
    private bool isPlaying = false;

    void OnEnable() { 
        vanDrive.Enable(); 
        vanSteer.Enable();
    }

    void OnDisable() { 
        vanDrive.Disable(); 
        vanSteer.Disable();
    }

    void Start() {
        rb = GetComponent<Rigidbody>();
        pbc = GetComponent<PlayerBoosterControl>();

        // Adjust center of mass to improve stability and prevent rolling
        rb.centerOfMass = new Vector3(0f, -1.5f, -0.3f);

        // Get all wheel components attached to the car
        wheels = GetComponentsInChildren<WheelControl>();

        boostPower = defaultBoostPower;
        boostPower += GameManager.dataManager.IsUpgraded("boosterPower_I")? 250f: 0f;
        boostPower += GameManager.dataManager.IsUpgraded("boosterPower_II")? 250f: 0f;    
    }

    void FixedUpdate() {
        if (isPlaying) {
            // Make sure the Mail Van isn't stuck upside down.
            FlipRecovery();
            
            // Get player input for acceleration and steering.
            float vInput = vanDrive.ReadValue<float>(); // Forward/backward input
            float hInput = vanSteer.ReadValue<float>(); // Steering input

            // Calculate current speed along the car's forward axis
            float forwardSpeed = Vector3.Dot(transform.forward, rb.linearVelocity);
            float speedFactor = Mathf.InverseLerp(0, maxSpeed, Mathf.Abs(forwardSpeed));

            currentSteerInput = Mathf.MoveTowards(currentSteerInput, hInput, Time.fixedDeltaTime * 5f);

            if (pbc.IsBoosting() && forwardSpeed < maxSpeed) { 
                rb.AddForce(boostPower * Time.fixedDeltaTime * transform.forward, ForceMode.Acceleration);
            }

            // Reduce motor torque and steering at high speeds for better handling
            float currentMotorTorque;
            currentMotorTorque = Mathf.Lerp(motorTorque, 0, speedFactor);
            float currentSteerRange = Mathf.Lerp(steeringRange, steeringRangeAtMaxSpeed, speedFactor);
            bool isAccelerating = Mathf.Sign(vInput) == Mathf.Sign(forwardSpeed);

            float turnStrength = Mathf.Lerp(5f, 3f, speedFactor);
            rb.AddTorque(currentSteerInput * rb.mass * turnStrength * transform.up);


            foreach (var wheel in wheels) {
                // Apply steering to wheels that support steering
                if (wheel.steerable) { wheel.SetSteerAngle( currentSteerInput * currentSteerRange); }

                if (isAccelerating) {
                    // Apply torque to motorized wheels.
                    wheel.SetMotorTorque(vInput * currentMotorTorque);
                    // Release brakes when accelerating.
                    wheel.SetBrakeTorque(0f);
                } else {
                    // Apply brakes when reversing direction
                    wheel.SetMotorTorque(0f);
                    wheel.SetBrakeTorque(Mathf.Abs(vInput) * brakeTorque);
                }
            }
            engineSound.pitch = 1f + (forwardSpeed / 10); // Adjust pitch of engine sound based on speed.
        } else { engineSound.Stop(); StopVan(); } // Stop engine sound when game is over. 
    }

    private void FlipRecovery() {
    float uprightAngle = Vector3.Angle(transform.up, Vector3.up);
    if (uprightAngle > flipAngleThreshold) {
        flippedTimer += Time.fixedDeltaTime;
        if (flippedTimer >= flipRecoveryDelay) {
            Vector3 rollDirection = Vector3.Cross(transform.up, Vector3.up);
            rb.AddTorque(flipRecoveryTorque * rb.mass * rollDirection.normalized, ForceMode.Force);
        }
    } else { flippedTimer = 0f; }
}
    
    // Completely stop all van movements.
    public void StopVan() {
        // Reset Wheel Collider values and set breaks.
        foreach (var wheel in wheels) {
            wheel.SetMotorTorque(0f);
        }

        // Reset any velocity on the player.
        rb.linearVelocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;
    }

    // Function to between game state (used to stop player movement at the pause and game over screens).
    public void SetState(bool state) {
        isPlaying = state;
        if (!state) { 
            engineSound.Stop(); 
        } else { 
            engineSound.Play();
        }
    }
}