using UnityEngine;

// Script to handle collisions for Props (Signs, Cones, Bins, etc.)
public class CollisionHandler : MonoBehaviour {

    [SerializeField] readonly bool destroy;
    bool beganFading = false;
    
    private void OnCollisionEnter(Collision collision) {
        if (!beganFading) {
            // Shrink and Delete Object Shortly After Collision.
            StartCoroutine(GameManager.obstacleManager.ShrinkAndDestroy(gameObject, destroy));
            beganFading = true;
        }
    }
}
