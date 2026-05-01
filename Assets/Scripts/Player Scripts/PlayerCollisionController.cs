using UnityEngine;

// Script to handle control of the Mail Van.
public class PlayerCollisionController : MonoBehaviour {
    
    [SerializeField] private GameObject particleManager;

    private void OnCollisionEnter(Collision collision) {
        // Increase Crash Count for achievement tracking.
        GameManager.dataManager.IncreaseProgress(1);
        
        if (collision.relativeVelocity.magnitude > 5f) {
            Vector3 collisionPos = collision.contacts[0].point;
            
            // Produce Collision Particles.
            particleManager.transform.position = collisionPos;
            particleManager.GetComponent<ParticleSystem>().Play(); 

            // Play the Default Crash sound if the colliding object doesn't have any crash sounds specified.
            if (collision.gameObject.GetComponent<CollisionSounds>() == null) { GameManager.audioManager.DefaultCrashSound(collisionPos); }
        }
    }
}
