using UnityEngine;

// Script to handle objectives (Parcels and Delivery Spots)
public class ParcelObjective : MonoBehaviour {
    [SerializeField] private GameObject parcelObj; 
    [SerializeField] private GameObject deliveryObj; 
    [SerializeField] private int timeEarned; 
    private bool isParcel;
    private DeliveryManager dm;

    void Start() { 
        dm = GameManager.deliveryManager;
        isParcel = false;
        parcelObj.GetComponent<Rigidbody>().AddTorque(new Vector3(0, 50, 0));
        ChangeState(true);
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

    public void DeliveryCompleted() {
        // Increment score and lifetime score.
        GameManager.dataManager.IncreaseProgress(0);
        GameManager.obstacleManager.SpawnObstacle(GameManager.gameplayManager.GetScore() % 2 == 0);
        GameManager.gameplayManager.SetTime(timeEarned, true);
    }
}
