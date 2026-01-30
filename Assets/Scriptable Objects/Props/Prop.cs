using UnityEngine;

public class Prop : MonoBehaviour {
    public Prop_SO so;

    bool beganFading = false;
    
    private void OnCollisionEnter(Collision collision) {
        GameObject collisionGO = collision.gameObject;
        // Check if Collisions with the Level Enviornment Count
        if ((!collisionGO.CompareTag("Level") || so.includeGround) && !beganFading) {
            // Shrink and Delete Object Shortly After Collision.
            StartCoroutine(GameManager.obstacleManager.ShrinkAndDestroy(gameObject, false));
            beganFading = true;
            if (GameManager.instance.GetDifficulty() != 0 && collisionGO.CompareTag("Player")) { GameManager.dataManager.AddPropEncounter(so.internalName); }
        }
    }
}