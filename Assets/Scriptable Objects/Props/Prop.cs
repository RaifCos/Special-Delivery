using UnityEngine;

public class Prop : MonoBehaviour {
    public Prop_SO so;

    bool beganFading = false;
    
    private void OnCollisionEnter(Collision collision) {
        GameObject collisionGO = collision.gameObject;
        // If stackable, ignore Collisions with other Props of the same type.
        if (so.stackable && collisionGO.name == gameObject.name) { return; }
        // Check if Collisions with the Level Enviornment Count
        if ((!collisionGO.CompareTag("Level")) && !beganFading) {
            // Shrink and Delete Object Shortly After Collision.
            StartCoroutine(GameManager.obstacleManager.ShrinkAndDestroy(gameObject, true));
            beganFading = true;
            if (GameManager.instance.GetDifficulty() != 0 && collisionGO.CompareTag("Player")) { GameManager.dataManager.AddPropEncounter(so.internalName); }
        }
    }
}