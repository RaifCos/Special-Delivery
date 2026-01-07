using System.Collections;
using UnityEngine;

// Script to handle collisions for Props (Signs, Cones, Bins, etc.)
public class PropCollision : MonoBehaviour {
    public int id;
    public bool includeGround;
    bool beganFading = false;
    

    private void OnCollisionEnter(Collision collision) {
        // Check if Collisions with the Level Enviornment Count
        if ((!collision.gameObject.CompareTag("Level") || includeGround) && !beganFading) {
            // Shrink and Delete Object Shortly After Collision.
            StartCoroutine(GameManager.obstacleManager.ShrinkAndDestroy(gameObject, false));
            beganFading = true;
            if (GameManager.instance.GetDifficulty() != 0) { GameManager.obstacleData.AddPropEncounter(id); }
        }
    }
}
