using UnityEngine;

public class Stamp : MonoBehaviour {

    [SerializeField] private int stampNumber;
    private bool collected = false;

    private GameObject stampObject;

    void Start() {
        stampObject = transform.GetChild(0).gameObject;
        // collected = GameManager.DataManager.IsStampCollected(stampNumber);
    }

    void ActivateStamp() { if (!collected) stampObject.SetActive(true); }

    void DeactivateStamp() { stampObject.SetActive(false); }

    void OnTriggerEnter(Collider other) {
        GameObject triggerGO = other.gameObject;

        if (!triggerGO.CompareTag("Player") || collected) return; // Ignore Non-Player Triggers
        collected = true;
        // GameManager.DataManager.StampCollected(stampNumber);
        DeactivateStamp();
    }
}
