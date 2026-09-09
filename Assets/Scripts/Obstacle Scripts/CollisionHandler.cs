using UnityEngine;

// Script to handle collisions for Props (Signs, Cones, Bins, etc.)
public class CollisionHandler : MonoBehaviour {

    [SerializeField] private bool destroy;
    [SerializeField] private bool excludeGround;
    bool beganFading;

    private void OnEnable() { beganFading = false; }

    private void OnCollisionEnter(Collision collision) {
        if (!beganFading && !(collision.gameObject.CompareTag("Level") && !excludeGround)) {
            // Shrink and Delete Object Shortly After Collision.
            StartCoroutine(GameManager.obstacleManager.ShrinkAndDestroy(gameObject, destroy, true));
            beganFading = true;
        }
    }
}
