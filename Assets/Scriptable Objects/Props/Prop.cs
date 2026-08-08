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
        // Check if Collisions with the Level Enviornment Count
        if ((!collisionGO.CompareTag("Level")) && !beganFading) {
            if (so.suspended) { GetComponent<Rigidbody>().isKinematic = false; } // Enable Physics if suspended prop is hit.
            if (so.isLit) { BreakLight(); } // Break Light if prop is a light source.
            // Shrink and Delete Object Shortly After Collision.
            StartCoroutine(GameManager.obstacleManager.ShrinkAndDestroy(gameObject, true, true));
            beganFading = true;
            if (GameManager.instance.GetDifficulty() != 0 && collisionGO.CompareTag("Player")) { GameManager.dataManager.AddPropEncounter(so.internalName); }
        }
    }
}