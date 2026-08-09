using UnityEngine;

// Script to handle the behaviour of the Anvil, Piano, and Fake Parcels.
public class AnvilTraversal : MonoBehaviour {
    public float startHeight;
    [SerializeField] private AudioClip fallSound;
    [SerializeField] private bool playsFallingSound;
    private bool fallingSoundPlayed;

    // Set initial height based on public variable.
    void OnEnable() { 
        fallingSoundPlayed = !playsFallingSound;
        transform.position = new Vector3(0f, startHeight, 0f);  
        transform.localScale = new Vector3(1f, 1f, 1f);    
    }

    // While object is still falling, move position to fall in front of the camera (where the player is). 
    void FixedUpdate() {
        Vector3 target = Camera.main.transform.position + (Camera.main.transform.forward * 40);
        float y = transform.position.y;

        if (y > 20) {
            transform.position = Vector3.MoveTowards(transform.position, new Vector3(target.x, transform.position.y, target.z), 20f);
        }
        
        if (y < 160 && !fallingSoundPlayed) {
            fallingSoundPlayed = true;
            GameManager.audioManager.PlaySpatialSoundEffect(fallSound, target, 0f, true);
        }
    }
}
