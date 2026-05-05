using UnityEngine;

[RequireComponent(typeof(CarTraversal))]
public class BossVan : MonoBehaviour {

    [SerializeField] int regularSpeed;
    [SerializeField] int chasingSpeed;
    private CarTraversal vanTraversal;
    private GameObject player, deliveryObjective;
    private int phase;

    void Awake() { 
        vanTraversal = GetComponent<CarTraversal>();
    }

    void Start() => ChangeState(0); 

    public void Initialise() {
        vanTraversal = GetComponent<CarTraversal>();
        player = GameManager.gameplayManager.GetPlayer();
        deliveryObjective = GameManager.deliveryManager.GetDeliveryObjective();
    }

    public void ChangeState(int input) {
        phase = input;
        switch (phase) {
            case 0: { // Drive towards the Parcel.
                vanTraversal.ChangeTarget(deliveryObjective); 
                vanTraversal.ChangeTopSpeed(regularSpeed);
                Debug.Log("Collecting Parcel");
                break;       
            } case 1: { // Chase the Player.
                vanTraversal.ChangeTarget(player); 
                vanTraversal.ChangeTopSpeed(chasingSpeed);
                Debug.Log("Chasing Player");
                break;       
            } case 2: { // Drive towards the Delivery Spot.
                vanTraversal.ChangeTarget(deliveryObjective); 
                vanTraversal.ChangeTopSpeed(regularSpeed);
                Debug.Log("Delivering Parcel");
                break;       
            }
        }
    }
}
