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
    [SerializeField] private bool ignoreStun; 
    [SerializeField] private float minImpactForce = 5f;    
    [SerializeField] private float stunDuration = 0.6f;       
    [SerializeField] private float bounceForceMultiplier = 0.02f;
    [SerializeField] private float maxBounceForce = 12f;
    [SerializeField] private float spinTorque = 6f;
    [SerializeField] private float collisionCooldown = 0.15f;

    private float stunTimer;
    private float collisionCooldownTimer;
    public bool IsStunned => stunTimer > 0f;
    private Rigidbody rb;
    private CarTraversal cT;

    void Start() {
        rb = GetComponent<Rigidbody>();
        cT = GetComponent<CarTraversal>();
        rb.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
    }

    private void OnCollisionEnter(Collision collision) {
        if (collision.gameObject.CompareTag("Level")
        || collisionCooldownTimer > 0f
        || collision.relativeVelocity.magnitude < minImpactForce
        || ignoreStun
        ) return; 

        float impactForce = collision.impulse.magnitude / Time.fixedDeltaTime;
        if (impactForce < minImpactForce) return;

        collisionCooldownTimer = collisionCooldown;
        cT.SetStunTimer(stunDuration);

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
        LookRotation(surfaceNormal, targetPosition);
        float forwardSpeed = Vector3.Dot(rb.rotation * Vector3.forward, rb.linearVelocity);
        if (actTopSpeed > 0f && forwardSpeed < actTopSpeed || actTopSpeed < 0f && forwardSpeed > actTopSpeed) {
            Vector3 right = rb.rotation * Vector3.right;
            float lateralVel = Vector3.Dot(rb.linearVelocity, right);
            Vector3 lateralCorrection = grip * lateralVel * -right;
            rb.AddForce(lateralCorrection, ForceMode.Acceleration);
            rb.AddForce(actTopSpeed * 3f * transform.forward, ForceMode.Acceleration);
        } 
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

}