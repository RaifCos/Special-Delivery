using UnityEngine;

// Script to handle objectives (Parcels and Delivery Spots)
public class BossParcelObjective : MonoBehaviour {
    [SerializeField] private GameObject parcelObj; 
    [SerializeField] private GameObject deliveryObjPlayer; 
    [SerializeField] private GameObject deliveryObjBoss; 
    [SerializeField] private int bossDeliveryTime; 
    private int phase, playerScore, bossScore;
    private bool isParcel;
    private DeliveryManager dm;

    // Start is called before the first frame update
    void Start() {
        dm = GameManager.deliveryManager;
        playerScore = 0;
        bossScore = 0;
        phase = 0;
        isParcel = false;
        parcelObj.GetComponent<Rigidbody>().AddTorque(new Vector3(0, 50, 0));
        ChangeState(true);
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
        else { GameManager.gameplayManager.StartBossTimer(bossDeliveryTime); }
        parcelObj.SetActive(isParcel);

        deliveryObjPlayer.SetActive(phase == 1);
        deliveryObjBoss.SetActive(phase == 1);

        if (isParcel) {
            float x = Random.Range(1.4f, 2f);
            float y = Random.Range(1.4f, 2f);
            float z = Random.Range(1.4f, 2f);
            parcelObj.transform.localScale = new Vector3(x, y, z);
            transform.position = dm.GetParcelPos();
        } else { transform.position = dm.GetDeliverySpot(); }
    }

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
