using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class RespawnHandler : MonoBehaviour {
    private static WaitForSeconds _waitForSeconds1 = new WaitForSeconds(1f);
    [SerializeField] private Image respawnPanel;
    [SerializeField] private Transform[] respawnNodes;
    [SerializeField] private Vector3[] respawnRotations;

    private void OnTriggerEnter(Collider other) {
        GameObject collisionGO = other.gameObject;

        if (collisionGO.CompareTag("Player")) StartCoroutine(RespawnFade(collisionGO));
        else if (collisionGO.CompareTag("Prop")) Destroy(collisionGO);
        else { 
            if (collisionGO.TryGetComponent(out Rigidbody rb)){
                ResetObject(collisionGO); 
                rb.linearVelocity = Vector3.zero;
                rb.angularVelocity = Vector3.zero;
            }
        }
    }

    private IEnumerator RespawnFade(GameObject playerObj) {
        PlayerControl pc = playerObj.GetComponent<PlayerControl>();
        pc.SetState(false);
        pc.StopVan();

        int alpha = 0;
        while (alpha < 255) {
            alpha += 5;
            respawnPanel.color = new Color(0, 0, 0, alpha / 255f);
            yield return null;
        } 
        
        ResetObject(playerObj);
        yield return _waitForSeconds1;

        while (alpha > 0) {
            alpha -= 5;
            respawnPanel.color = new Color(0, 0, 0, alpha / 255f);
            yield return null;
        } // Check game didn't end/pause during Player Respawn before re-enabling control.
        if (GameManager.gameplayManager.IsPlaying()) pc.SetState(true);
    }

    private void ResetObject(GameObject obj) {
        // Identify Closest Respawn Node to Object.
        Vector3 target = respawnNodes[0].position;
        foreach(Transform node in respawnNodes) {
            if (Vector3.Distance(obj.transform.position, node.position) < Vector3.Distance(obj.transform.position, target)) {
                target = node.position;
            }
        }

        // Identify Closest Respawn Rotation to Object.
        Vector3 targetRotation = respawnRotations[0];
        foreach(Vector3 rotation in respawnRotations) {
            if (Vector3.Distance(obj.transform.eulerAngles, rotation) < Vector3.Distance(obj.transform.eulerAngles, targetRotation)) {
                targetRotation = rotation;
            }
        }

        obj.transform.SetPositionAndRotation(target, Quaternion.Euler(targetRotation));
    }
}

