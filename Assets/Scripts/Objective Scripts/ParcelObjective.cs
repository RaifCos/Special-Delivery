using TMPro;
using UnityEngine;

// Script to handle objectives (Parcels and Delivery Spots)
public class ParcelObjective : MonoBehaviour {

    [Header ("Gameplay Elements")]
    [SerializeField] private GameObject parcelObj; 
    [SerializeField] private GameObject deliveryObj; 
    [SerializeField] private int timeEarned; 

    [Header ("UI Elements")]
    [SerializeField] private TMP_Text scoreText;
    [SerializeField] private Animator scoreAnimator;
    private static readonly int ScoreAnimHash = Animator.StringToHash("scoreAnim");

    private int completeDeliveries, difficulty;
    private bool isParcel;
    private DeliveryManager dm;

    void Start() { 
        dm = GameManager.deliveryManager;
        difficulty = GameManager.instance.GetDifficulty();
        isParcel = false;
        parcelObj.GetComponent<Rigidbody>().AddTorque(new Vector3(0, 50, 0));
        ChangeState(true);
    }

    private void OnTriggerEnter(Collider other) {
        // Only React if the Colliding Object is the Player.
        if (other.gameObject.CompareTag("Player")) {
            if (!isParcel) {
                SetScore(1, true);
                scoreAnimator.SetTrigger(ScoreAnimHash);
                if (GameManager.instance.GetDifficulty() != 0) { DeliveryCompleted(); }
            }
            GameManager.audioManager.PlayParcelSound(isParcel);
            ChangeState(!isParcel);
        }   
    }

    public void ChangeState(bool input) {
        isParcel = input;
        parcelObj.SetActive(isParcel);
        deliveryObj.SetActive(!isParcel);
        if (isParcel) {
            float x = Random.Range(1.4f, 2f);
            float y = Random.Range(1.4f, 2f);
            float z = Random.Range(1.4f, 2f);
            parcelObj.transform.localScale = new Vector3(x, y, z);
            transform.position = dm.GetParcelPos();
        } else { transform.position = dm.GetDeliverySpot(); }
    }

    private void DeliveryCompleted() {
        GameManager.dataManager.IncreaseProgress(0);
        GameManager.obstacleManager.SpawnObstacle(completeDeliveries % 2 == 0);
        GameManager.gameplayManager.SetTime(timeEarned, true);
    }

    private void SetScore (int value, bool addingScore) {
        if (addingScore) {
            completeDeliveries++;
            if (difficulty != 0) {
                if (completeDeliveries == 10) { GameManager.dataManager.CompleteAchievement("score10"); }
                if (completeDeliveries == 50) { GameManager.dataManager.CompleteAchievement("score50"); }
                if (completeDeliveries > GameManager.dataManager.GetBestScore()) { GameManager.dataManager.SetBestScore(completeDeliveries); }
            }
        } else { completeDeliveries = value; }
        scoreText.text = completeDeliveries.ToString();
    }
}
