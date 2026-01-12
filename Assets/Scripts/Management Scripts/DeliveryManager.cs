using System.Threading;
using UnityEngine;

// Script to handle objectives (Parcels and Delivery Spots)
public class DeliveryManager : MonoBehaviour {
    public AudioSource sound;
    public AudioClip parcelClip, spotClip;
    public GameObject parcelNodes;
    private bool isParcel;
    private GameObject parcel, psA, psB;
    private Vector3 currPos = Vector3.zero;
    private readonly Vector3[] nodePositions = new Vector3[16];

    void Awake() {
        GameManager.deliveryManager = this;
    }

    // Start is called before the first frame update
    void Start() {
        // Retrieve Node Positions (Used for parcels and obstacles)
        for (int x = 0; x < 16; x++) { nodePositions[x] = parcelNodes.transform.GetChild(x).transform.position; }
        parcel = transform.GetChild(0).gameObject;
        psA = transform.GetChild(1).gameObject;
        psB = transform.GetChild(2).gameObject;
        GeneratePos();
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
            GeneratePos();
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
        }
    }

    public Vector3 GetCurrentPosition() { return currPos; }
    
    public void GeneratePos() {
        Vector3 res;
        // Do-While loop ensures the generated position can't be the same as prevPos is canBePrevPos is false. 
        do { res = nodePositions[Random.Range(0, 16)];
        } while (res == currPos);
        currPos = res;
        transform.position = currPos;
    }

    // Function used when the player completes a delivery.
    public void DeliveryCompleted() {
        // Increment score and lifetime score.
        GameManager.dataManager.IncreaseProgress(0);
        GameManager.obstacleManager.SpawnObstacle(GameManager.gameplayManager.GetScore() % 2 == 0);
        GameManager.gameplayManager.SetTime(30, true);
    }
}
