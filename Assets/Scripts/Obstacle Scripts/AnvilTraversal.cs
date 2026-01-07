using UnityEngine;

// Script to handle the behaviour of the Anvil, Piano, and Fake Parcels.
public class AnvilTraversal : MonoBehaviour {
    public float startHeight;

    // Set initial height based on public variable.
    void Start() { transform.position = new Vector3(0f, startHeight, 0f);  }

    // While object is still falling, move position to fall in front of the camera (where the player is). 
    void FixedUpdate() {
        Vector3 target = Camera.main.transform.position + (Camera.main.transform.forward * 40);
        if (transform.position.y > 20) {
            transform.position = Vector3.MoveTowards(transform.position, new Vector3(target.x, transform.position.y, target.z), 20f);
        }
    }
}
