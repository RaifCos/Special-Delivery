using TMPro;
using UnityEngine;

// Script to handle objectives (Parcels and Delivery Spots)
public class BossParcelObjective : MonoBehaviour {
    [Header ("Gameplay Elements")]
    [SerializeField] private GameObject parcelObj; 
    [SerializeField] private GameObject deliveryObjPlayer; 
    [SerializeField] private GameObject deliveryObjBoss; 
    [SerializeField] private int bossDeliveryTime; 
    [SerializeField] private GameObject boss; 

    [Header ("UI Elements")]
    [SerializeField] private TMP_Text playerScoreText;
    [SerializeField] private TMP_Text bossScoreText;
    [SerializeField] private Animator playerScoreAnimator;
    [SerializeField] private Animator bossScoreAnimator;
    private static readonly int ScoreAnimHash = Animator.StringToHash("scoreAnim");

    private int phase, playerScore, bossScore;
    private bool isParcel;
    private DeliveryManager dm;

    // Start is called before the first frame update
    void Start() {
        dm = GameManager.deliveryManager;
        SetScore(0, 0, null);
        phase = 0;
        isParcel = false;
        parcelObj.GetComponent<Rigidbody>().AddTorque(new Vector3(0, 50, 0));
        ChangeState(true);
        boss.SetActive(true);
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
        deliveryObjBoss.SetActive(phase == 2);

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
        if (phase == 1) SetScore(playerScore + 1, bossScore, playerScoreAnimator); 
        if (phase == 2) SetScore(playerScore, bossScore + 1, bossScoreAnimator); 
        Debug.Log(playerScore + " - " + bossScore);

        GameManager.gameplayManager.ResetBossTimer();
        ChangeState(true);
    }

    private void SetScore(int pS, int bS, Animator animator) {
        playerScore = pS;
        bossScore = bS;

        playerScoreText.text = playerScore.ToString();
        bossScoreText.text = bossScore.ToString();

        if (animator != null) { TriggerScoreAnimation(animator); } 
        if (playerScore == 5) { GameManager.gameplayManager.GameOver(); } // Replace with Win Function.
        else if (bossScore == 5) { GameManager.gameplayManager.GameOver(); } // Replace with Lose Function.
    }

    private void TriggerScoreAnimation(Animator animator) => animator.SetTrigger(ScoreAnimHash);

}
