using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(PlayerBoosterControl))]
public class PlayerControl : MonoBehaviour {
    [Header("Mail Van Properties")]
    public float motorTorque, brakeTorque, maxSpeed, defaultBoostPower, steeringRange, steeringRangeAtMaxSpeed;
    private float boostPower;
    private WheelControl[] wheels;

    [Header("Audio Handler")]
    public AudioSource engineSound;
    
    private PlayerBoosterControl pbc;
    private Rigidbody rb;
    private bool isPlaying = false;

    // Start is called before the first frame update
    void Start() {
        rb = GetComponent<Rigidbody>();
        pbc = GetComponent<PlayerBoosterControl>();

        // Adjust center of mass to improve stability and prevent rolling
        rb.centerOfMass += new Vector3(0f, -2f, -1f);

        // Get all wheel components attached to the car
        wheels = GetComponentsInChildren<WheelControl>();

        boostPower = defaultBoostPower;
        boostPower += PlayerPrefs.GetInt("Upgrade_boosterPower_I", 0) == 1? 2.5f: 0f;
        boostPower += PlayerPrefs.GetInt("Upgrade_boosterPower_II", 0) == 1? 2.5f: 0f;        
    }

    // FixedUpdate is called at a fixed time interval 
    void FixedUpdate() {
        if (isPlaying) {
            if (Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.LeftShift)) {
                Quaternion newRotation = Quaternion.Euler(0f, rb.rotation.eulerAngles.y, 0f);
                rb.MoveRotation(newRotation);
            }

            // Get player input for acceleration and steering
            float vInput = Input.GetAxis("Vertical"); // Forward/backward input
            float hInput = Input.GetAxis("Horizontal"); // Steering input

            // Calculate current speed along the car's forward axis
            float forwardSpeed = Vector3.Dot(transform.forward, rb.linearVelocity);
            float speedFactor = Mathf.InverseLerp(0, maxSpeed, Mathf.Abs(forwardSpeed));

            if (pbc.IsBoosting() && forwardSpeed < maxSpeed) { 
                rb.AddForce(boostPower * Time.fixedDeltaTime * transform.forward, ForceMode.Acceleration);
            }

            // Reduce motor torque and steering at high speeds for better handling
            float currentMotorTorque;
            currentMotorTorque = Mathf.Lerp(motorTorque, 0, speedFactor);
            float currentSteerRange = Mathf.Lerp(steeringRange, steeringRangeAtMaxSpeed, speedFactor);
            bool isAccelerating = Mathf.Sign(vInput) == Mathf.Sign(forwardSpeed);

            foreach (var wheel in wheels) {
                // Apply steering to wheels that support steering
                if (wheel.steerable) { wheel.WheelCollider.steerAngle = hInput * currentSteerRange; }

                if (isAccelerating) {
                    // Apply torque to motorized wheels
                    if (wheel.motorized) { wheel.WheelCollider.motorTorque = vInput * currentMotorTorque; }
                    // Release brakes when accelerating
                    wheel.WheelCollider.brakeTorque = 0f;
                } else {
                    // Apply brakes when reversing direction
                    wheel.WheelCollider.motorTorque = 0f;
                    wheel.WheelCollider.brakeTorque = Mathf.Abs(vInput) * brakeTorque;
                }
            }
            engineSound.pitch = 1f + (forwardSpeed / 10); // Adjust pitch of engine sound based on speed.
        } else { engineSound.Stop(); StopVan(); } // Stop engine sound when game is over. 
    }
    
    // Completely stop all van movements.
    public void StopVan() {
        // Reset Wheel Collider values and set breaks.
        foreach (var wheel in wheels) {
            wheel.WheelCollider.motorTorque = 0f;
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