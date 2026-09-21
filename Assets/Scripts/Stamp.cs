using UnityEngine;

public class Stamp : MonoBehaviour {

    [SerializeField] private int stampNumber;
    private bool collected = false;

    void Start() => collected = GameManager.dataManager.IsStampCollected(stampNumber);

    public void ActivateStamp() => gameObject.SetActive(!collected); 

    public void DeactivateStamp() => gameObject.SetActive(false); 

    public int GetStampNumber() => stampNumber;

    public void SetCollected(bool input) => collected = input; 

    void OnTriggerEnter(Collider other) {
        GameObject triggerGO = other.gameObject;
        if (!triggerGO.CompareTag("Player") || collected) return; // Ignore Non-Player Triggers
        GameManager.gameplayManager.SetCurrentStamp(stampNumber); // Hide all Stamps once one is collected.
    }
}
