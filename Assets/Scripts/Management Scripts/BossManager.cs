using System.Collections.Generic;
using UnityEngine;

// Script to handle objectives (Parcels and Delivery Spots)
public class BossManager : MonoBehaviour {
    public GameObject parcelNode;
    public GameObject deliveryNodes;
    private bool isParcel;
    private GameObject parcel, parcelPSA, parcelPSB, bossPSA, bossPSB;
    private Vector3 currPos = Vector3.zero;
    private Vector3 parcelPos;
    private readonly List<Vector3> nodePositions = new();
    private int phase; // 0 (Parcel not Collected), 1 (Player Delivering), 2 (Boss Delivering) 
    private int playerScore, bossScore;

    void Awake() { GameManager.bossManager = this; }

    // Start is called before the first frame update
    void Start() {
        // Retrieve Node Positions (Used for parcels and obstacles)
        parcelPos = parcelNode.transform.position;
        for (int x = 0; x < deliveryNodes.transform.childCount; x++) { nodePositions.Add(deliveryNodes.transform.GetChild(x).transform.position); }
        parcel = transform.GetChild(0).gameObject;
        parcelPSA = transform.GetChild(1).gameObject;
        parcelPSB = transform.GetChild(2).gameObject;
        bossPSA = transform.GetChild(3).gameObject;
        bossPSB =transform.GetChild(4).gameObject;
        phase = 0;
        playerScore = 0;
        bossScore = 0;
        ChangeState(true);
        parcel.GetComponent<Rigidbody>().AddTorque(new Vector3(0, 50, 0));
    }

    private void OnTriggerEnter(Collider other) {
        bool playerHit = other.gameObject.CompareTag("Player");
        bool bossHit = other.gameObject.CompareTag("Boss");

        if (playerHit) {
            if (isParcel) { 
                phase = 1;
                GameManager.newsTextScroller.AddBossHeadline(true);
                ChangeState(false);
            } else if (phase == 1) { DeliveryCompleted(); }
        }

        if (bossHit) { 
            if (isParcel) { 
                phase = 2; 
                GameManager.newsTextScroller.AddBossHeadline(false);
                ChangeState(false);
            } else if (phase == 2) { DeliveryCompleted(); }
        }
    }

    // TODO: Start Timer when Parcel Collected
    // TODO: News Text to indicate whether player should rush to Deliver Parcel or stop Boss. 

    public void ChangeState(bool input) {
        isParcel = input;
        if(isParcel) { phase = 0; }
        else { GameManager.gameplayManager.StartBossTimer(); }
        parcel.SetActive(isParcel);

        parcelPSA.SetActive(phase == 1);
        parcelPSB.SetActive(phase == 1);
        bossPSA.SetActive(phase == 2);
        bossPSB.SetActive(phase == 2);

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

    public void DeliveryCompleted() {
        GameManager.obstacleManager.SpawnObstacle(true);

        if (phase == 1) {
            playerScore++;
            if (playerScore == 5) { GameManager.gameplayManager.GameOver(); } // Replace with Win Function.
        }

        if (phase == 2) {
            bossScore++;
            if (bossScore == 5) { GameManager.gameplayManager.GameOver(); } // Replace with Lose Function.
        }

        Debug.Log(playerScore + " - " + bossScore);

        GameManager.gameplayManager.ResetBossTimer();
        ChangeState(true);
        // TODO: UpdateUI 
    }
}
