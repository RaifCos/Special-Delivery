using UnityEngine;

// Script to handle collisions for Props (Signs, Cones, Bins, etc.)
public class PropCollision : MonoBehaviour {
    bool beganFading = false;
    
    private void OnCollisionEnter(Collision collision) {
        // Check if Collisions with the Level Enviornment Count
        if (!beganFading) {
            // Shrink and Delete Object Shortly After Collision.
            StartCoroutine(GameManager.obstacleManager.ShrinkAndDestroy(gameObject, false));
            beganFading = true;
        }
    }
}
