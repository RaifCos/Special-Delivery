using UnityEngine;
[RequireComponent(typeof(Rigidbody))]

[RequireComponent(typeof(MeshRenderer))]
public class Prop : MonoBehaviour {
    public Prop_SO so;
    bool beganFading = false;

    private void Awake() { if(so.suspended) GetComponent<Rigidbody>().isKinematic = true; }

    private void BreakLight() {
        GetComponent<MeshRenderer>().material = GameManager.instance.GetPalette();
        transform.GetChild(0).gameObject.SetActive(false);
    }

    private void OnCollisionEnter(Collision collision) {
        GameObject collisionGO = collision.gameObject;
        
        // If stackable, ignore Collisions with other Props of the same type.
        if (so.stackable && collisionGO.name == gameObject.name) { return; }
        if (beganFading) return;
        
        // Ignore Collisions with the Level Enviornment
        if (!collisionGO.CompareTag("Level")) {
            if (so.suspended) { GetComponent<Rigidbody>().isKinematic = false; } // Enable Physics if suspended prop is hit.
            if (so.isLit) { BreakLight(); } // Break Light if prop is a light source.
            if (!so.isInvincible) StartCoroutine(GameManager.obstacleManager.ShrinkAndDestroy(gameObject, true, true)); // Shrink and Delete if not invincible.
            if (GameManager.instance.GetDifficulty() != 0 && collisionGO.CompareTag("Player")) { GameManager.dataManager.AddPropEncounter(so.internalName); }
            beganFading = true;
        }
    }
}