using UnityEngine;

// Script to handle collisions for Props (Signs, Cones, Bins, etc.)
public class CollisionHandler : MonoBehaviour {

    [SerializeField] private bool wait = true;
    [SerializeField] private bool destroy;
    [SerializeField] private bool excludeGround;
    [SerializeField] private bool slipTarget;
    bool beganFading;

    private void OnEnable() { beganFading = false; }

    private void OnCollisionEnter(Collision collision) {
        bool touchedGrass = collision.gameObject.CompareTag("Level");
        if (!beganFading && !(touchedGrass && excludeGround)) {

            // Slip Target if it isn't the ground.
            if (slipTarget && !touchedGrass) SlipTarget(collision.rigidbody);

            // Shrink and Delete Object Shortly After Collision.
            StartCoroutine(GameManager.obstacleManager.ShrinkAndDestroy(gameObject, destroy, wait));
            beganFading = true;
        }
    }

    private void SlipTarget(Rigidbody rb) {
        if (rb == null) return;
        Debug.Log("Whoosh!");
        rb.AddTorque(Vector3.up * 90000f, ForceMode.Impulse);
    }
}
