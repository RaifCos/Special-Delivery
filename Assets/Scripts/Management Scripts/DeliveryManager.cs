using System.Collections.Generic;
using UnityEngine;

// Script to handle objectives (Parcels and Delivery Spots)
public class DeliveryManager : MonoBehaviour {
    public AudioSource sound;
    public AudioClip parcelClip, spotClip;
    public GameObject parcelNode;
    public GameObject deliveryNodes;
    [SerializeField] private int bonusTime; 
    private bool isParcel;
    private GameObject parcel, psA, psB;
    private Vector3 currPos = Vector3.zero;
    private Vector3 parcelPos;
    private readonly List<Vector3> nodePositions = new();

    void Awake() {
        GameManager.deliveryManager = this;
    }

    // Start is called before the first frame update
    void Start() {
        // Retrieve Node Positions (Used for parcels and obstacles)
        parcelPos = parcelNode.transform.position;
        for (int x = 0; x < deliveryNodes.transform.childCount; x++) { nodePositions.Add(deliveryNodes.transform.GetChild(x).transform.position); }
        parcel = transform.GetChild(0).gameObject;
        psA = transform.GetChild(1).gameObject;
        psB = transform.GetChild(2).gameObject;
        ChangeState(true);
        parcel.GetComponent<Rigidbody>().AddTorque(new Vector3(0, 50, 0));
    }

    private void OnTriggerEnter(Collider other) {
        // Only React if the Colliding Object is the Player.
        if (other.gameObject.CompareTag("Player")) {
            if (!isParcel) {
                GameManager.gameplayManager.SetScore(1, true);
                GameManager.gameplayManager.ScoreAnimation();
                if (GameManager.instance.GetDifficulty() != 0) { DeliveryCompleted(); }
            }
            GameManager.audioManager.PlayParcelSound(isParcel);
            ChangeState(!isParcel);
        }   
    }

    public void ChangeState(bool input) {
        isParcel = input;
        parcel.SetActive(isParcel);
        psA.SetActive(!isParcel);
        psB.SetActive(!isParcel);
        if (isParcel) {
            float x = Random.Range(1.4f, 2f);
            float y = Random.Range(1.4f, 2f);
            float z = Random.Range(1.4f, 2f);
            parcel.transform.localScale = new Vector3(x, y, z);
            transform.position = parcelPos;
        } else { transform.position = nodePositions[Random.Range(0, nodePositions.Count)]; }
        currPos = transform.position;
    }

    public Vector3 GetCurrentPosition() { return currPos; }

    // Function used when the player completes a delivery.
    public void DeliveryCompleted() {
        // Increment score and lifetime score.
        GameManager.dataManager.IncreaseProgress(0);
        GameManager.obstacleManager.SpawnObstacle(GameManager.gameplayManager.GetScore() % 2 == 0);
        GameManager.gameplayManager.SetTime(bonusTime, true);
    }
}
