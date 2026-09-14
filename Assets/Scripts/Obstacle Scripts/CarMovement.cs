using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(CarTraversal))]
public class CarMovement : MonoBehaviour {

    [Header("Car Variables")]
    [SerializeField] private float topSpeed;
    [SerializeField] private float turnSpeed;
    [SerializeField] private float grip;
    private float actTopSpeed;

    [Header("Collision Reaction")]
    [SerializeField] private bool playerStunOnly;
    [SerializeField] private bool ignoreStun; 
    [SerializeField] private float minImpactForce = 5f;    
    [SerializeField] private float stunDuration = 0.6f;       
    [SerializeField] private float bounceForceMultiplier = 0.02f;
    [SerializeField] private float maxBounceForce = 12f;
    [SerializeField] private float spinTorque = 6f;
    [SerializeField] private float collisionCooldown = 0.15f;

    [Header("Stuck Recovery")]
    [SerializeField] private float stuckSpeedThreshold = 0.5f;
    [SerializeField] private float stuckTimeThreshold = 1.5f;   
    [SerializeField] private float unstuckReverseDuration = 1.2f;
    [SerializeField] private float unstuckReverseSpeed = 6f;
    [SerializeField] private float unstuckTurnTorque = 8f;

    private float stuckTimer = 0f;
    private float unstuckTimer = 0f;
    private float unstuckTurnSide = 1f;

    public bool IsUnstucking => unstuckTimer > 0f;

    private float stunTimer;
    private float collisionCooldownTimer = 0f;
    public bool IsStunned => stunTimer > 0f;
    private Rigidbody rb;
    private CarTraversal cT;

    void Awake() {
        rb = GetComponent<Rigidbody>();
        cT = GetComponent<CarTraversal>();
        rb.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
    }

    private void OnCollisionEnter(Collision collision) {
        // Ignore non-player collisions if car can only be stunned by Player. 
        if (playerStunOnly && !collision.gameObject.CompareTag("Player")) return; 

        // Ignore collisions that are too weak, are with the ground, or are before the cooldown is finished.
        if (collision.gameObject.CompareTag("Level")
        || collisionCooldownTimer > 0f
        || collision.relativeVelocity.magnitude < minImpactForce
        || ignoreStun
        ) return; 

        float impactForce = collision.impulse.magnitude / Time.fixedDeltaTime;
        if (impactForce < minImpactForce) return;

        collisionCooldownTimer = collisionCooldown;
        stunTimer = stunDuration;   

        ContactPoint contact = collision.GetContact(0);

        Vector3 pushDir = contact.normal;
        pushDir.y = 0f;
        if (pushDir.sqrMagnitude > 0.0001f) {
            float pushForce = Mathf.Min(impactForce * bounceForceMultiplier, maxBounceForce);
            rb.AddForce(pushDir.normalized * pushForce, ForceMode.Impulse);
        }

        float side = Vector3.Dot(transform.right, contact.point - transform.position) >= 0f ? -1f : 1f;
        float torqueScale = Mathf.Clamp01(impactForce / (minImpactForce * 4f));
        rb.AddTorque(side * spinTorque * torqueScale * Vector3.up, ForceMode.Impulse);
    }

    public void DriveToward(Vector3 surfaceNormal, Vector3 targetPosition) {
        if (unstuckTimer > 0f) {
            Reverse(surfaceNormal);
            unstuckTimer -= Time.fixedDeltaTime;
            return;
        }

        LookRotation(surfaceNormal, targetPosition);
        float forwardSpeed = Vector3.Dot(rb.rotation * Vector3.forward, rb.linearVelocity);

        bool tryingToMove = Mathf.Abs(actTopSpeed) > 0.01f;
        bool barelyMoving = Mathf.Abs(forwardSpeed) < stuckSpeedThreshold;

        if (tryingToMove && barelyMoving && !IsStunned) {
            stuckTimer += Time.fixedDeltaTime;
            if (stuckTimer >= stuckTimeThreshold) Unstuck();
        } else {  stuckTimer = 0f; }

        if (actTopSpeed > 0f && forwardSpeed < actTopSpeed || actTopSpeed < 0f && forwardSpeed > actTopSpeed) {
            Vector3 right = rb.rotation * Vector3.right;
            float lateralVel = Vector3.Dot(rb.linearVelocity, right);
            Vector3 lateralCorrection = grip * lateralVel * -right;
            rb.AddForce(lateralCorrection, ForceMode.Acceleration);
            rb.AddForce(actTopSpeed * 3f * transform.forward, ForceMode.Acceleration);
        }
    }

    private void Unstuck() {
        unstuckTimer = unstuckReverseDuration;
        stuckTimer = 0f;

        float lateralVel = Vector3.Dot(rb.linearVelocity, rb.rotation * Vector3.right);
        unstuckTurnSide = Mathf.Abs(lateralVel) > 0.05f
            ? (lateralVel >= 0f ? 1f : -1f)
            : (Random.value > 0.5f ? 1f : -1f);
    }

    private void Reverse(Vector3 surfaceNormal) {
        Vector3 forward = rb.rotation * Vector3.forward;
        rb.AddForce(-forward * unstuckReverseSpeed, ForceMode.Acceleration);
        rb.AddTorque(unstuckTurnSide * unstuckTurnTorque * surfaceNormal, ForceMode.Acceleration);
    }

    private void LookRotation(Vector3 surfaceNormal, Vector3 targetPosition) {
        Vector3 direction = (targetPosition - rb.position).normalized;
        if (direction.sqrMagnitude <= 0.001f) return;

        Vector3 surfaceForward = Vector3.ProjectOnPlane(direction, surfaceNormal).normalized;
        if (surfaceForward.sqrMagnitude <= 0.001f) return;

        Quaternion targetRotation = Quaternion.LookRotation(surfaceForward, surfaceNormal);
        Quaternion smoothedRotation = Quaternion.RotateTowards(rb.rotation, targetRotation, turnSpeed * Time.fixedDeltaTime);
        rb.MoveRotation(smoothedRotation);
    }

    public void ChangeTopSpeed(int input) => topSpeed = input;

    public void AdjustTopSpeed() => actTopSpeed = topSpeed;

    public void AdjustTopSpeed(float hitDist) => actTopSpeed = Mathf.Lerp(-topSpeed / 1.5f, topSpeed, hitDist / 25f);

    public void DecreaseCollisionTimer() {  if (collisionCooldownTimer > 0f) collisionCooldownTimer -= Time.fixedDeltaTime; }

    public void DecreaseStunTimer() {

    if (stunTimer > 0f) {
        stunTimer -= Time.fixedDeltaTime;
        if (stunTimer <= 0f) cT.ReattachToNodeSystem();
    }
}

}