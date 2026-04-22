using UnityEngine;

// Script to handle collisions for Props (Signs, Cones, Bins, etc.)
public class CollisionHandler : MonoBehaviour {

    [SerializeField] bool destroy;
    bool beganFading = false;
    
    private void OnCollisionEnter(Collision collision) {
        // Check if Collisions with the Level Enviornment Count
        if (!beganFading) {
            // Shrink and Delete Object Shortly After Collision.
            StartCoroutine(GameManager.obstacleManager.ShrinkAndDestroy(gameObject, destroy));
            beganFading = true;
        }
    }
}
