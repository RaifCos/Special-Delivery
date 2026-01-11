using UnityEngine;

public class Prop : MonoBehaviour {
    public Prop_SO so;

    bool beganFading = false;
    
    private void OnCollisionEnter(Collision collision) {
        // Check if Collisions with the Level Enviornment Count
        if ((!collision.gameObject.CompareTag("Level") || so.includeGround) && !beganFading) {
            // Shrink and Delete Object Shortly After Collision.
            StartCoroutine(GameManager.obstacleManager.ShrinkAndDestroy(gameObject, false));
            beganFading = true;
            if (GameManager.instance.GetDifficulty() != 0) { GameManager.obstacleData.AddPropEncounter(so.intenalName); }
        }
    }
}