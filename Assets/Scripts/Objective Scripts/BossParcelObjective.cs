using TMPro;
using UnityEngine;
using System.Collections;

public class BossParcelObjective : MonoBehaviour {
    [Header("Gameplay Elements")]
    [SerializeField] private GameObject parcelObj;
    [SerializeField] private GameObject deliveryObjPlayer;
    [SerializeField] private GameObject deliveryObjBoss;
    [SerializeField] private int bossDeliveryTime;
    [SerializeField] private GameObject boss;
    private BossVan bossVan;

    [Header("UI Elements")]
    [SerializeField] private TMP_Text playerScoreText;
    [SerializeField] private TMP_Text bossScoreText;
    [SerializeField] private Animator playerScoreAnimator;
    [SerializeField] private Animator bossScoreAnimator;
    private static readonly int ScoreAnimHash = Animator.StringToHash("scoreAnim");

    private int phase, playerScore, bossScore;
    private bool isChangingState = false;
    private DeliveryManager dm;

    void Start() {
        dm = GameManager.deliveryManager;
        boss.SetActive(true);
        bossVan = boss.GetComponent<BossVan>();
        bossVan.Initialise();

        ChangeState(0);
        SetScore(0, 0, null);

        parcelObj.GetComponent<Rigidbody>().AddTorque(new Vector3(0, 50, 0));
    }

    private void OnTriggerEnter(Collider other) {
        if (isChangingState) return;

        bool playerHit = other.gameObject.CompareTag("Player");
        bool bossHit = other.gameObject.CompareTag("Boss");

        if (playerHit) {
            if (phase == 1) DeliveryCompleted();
            else if (phase == 0) ChangeState(1);
        }

        if (bossHit) {
            if (phase == 2) DeliveryCompleted();
            else if (phase == 0) ChangeState(2);
        }
    }

    public void ChangeState(int input) {
        if (isChangingState) return;
        StartCoroutine(ChangeStateRoutine(input));
    }

    // Wrapping state changes in a coroutine lets Unity's physics pipeline
    // fully process the teleport before new trigger events are accepted.
    private IEnumerator ChangeStateRoutine(int input) {
        isChangingState = true;

        phase = input;
        if (phase == 0) {
            GameManager.gameplayManager.ResetBossTimer();
            parcelObj.SetActive(true);
            float x = Random.Range(1.4f, 2f);
            float y = Random.Range(1.4f, 2f);
            float z = Random.Range(1.4f, 2f);
            parcelObj.transform.localScale = new Vector3(x, y, z);
            transform.position = dm.GetParcelPos();
        } else {
            GameManager.newsTextScroller.AddBossHeadline(phase == 1);
            parcelObj.SetActive(false);
            GameManager.gameplayManager.StartBossTimer(bossDeliveryTime);
            transform.position = dm.GetDeliverySpot();
        }

        bossVan.ChangePhase(phase);
        deliveryObjPlayer.SetActive(phase == 1);
        deliveryObjBoss.SetActive(phase == 2);

        // Wait for physics to process the new position before accepting
        // new trigger events — prevents immediate re-entry at the new spot.
        yield return new WaitForFixedUpdate();
        yield return new WaitForFixedUpdate();

        isChangingState = false;
    }

    public void DeliveryCompleted() {
        GameManager.obstacleManager.SpawnObstacle(true);
        if (phase == 1) SetScore(playerScore + 1, bossScore, playerScoreAnimator);
        if (phase == 2) SetScore(playerScore, bossScore + 1, bossScoreAnimator);
        ChangeState(0);
    }

    private void SetScore(int pS, int bS, Animator animator) {
        playerScore = pS;
        bossScore = bS;

        playerScoreText.text = playerScore.ToString();
        bossScoreText.text = bossScore.ToString();

        if (animator != null) TriggerScoreAnimation(animator);
        if (playerScore == 5) GameManager.gameplayManager.BossGameOver(0);
        else if (bossScore == 5) GameManager.gameplayManager.BossGameOver(1);
    }

    private void TriggerScoreAnimation(Animator animator) => animator.SetTrigger(ScoreAnimHash);
}