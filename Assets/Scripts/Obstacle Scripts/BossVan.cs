using UnityEngine;

[RequireComponent(typeof(CarTraversal))]
[RequireComponent(typeof(CarMovement))]
public class BossVan : MonoBehaviour {

    [SerializeField] int regularSpeed;
    [SerializeField] int chasingSpeed;
    private CarTraversal vanTraversal;
    private CarMovement vanMovement;
    private GameObject player, deliveryObjective;
    private int phase;

    void Awake() { 
        vanTraversal = GetComponent<CarTraversal>();
        vanMovement = GetComponent<CarMovement>();
    }

    public void Initialise() {
        vanTraversal = GetComponent<CarTraversal>();
        player = GameManager.gameplayManager.GetPlayer();
        deliveryObjective = GameManager.deliveryManager.GetDeliveryObjective();
        ChangePhase(0);
    }

    public void ChangePhase(int input) {
        phase = input;
        switch (phase) {
            case 0: { // Drive towards the Parcel.
                vanTraversal.ChangeTarget(deliveryObjective); 
                vanMovement.ChangeTopSpeed(regularSpeed);
                break;       
            } case 1: { // Chase the Player.
                vanTraversal.ChangeTarget(player); 
                vanMovement.ChangeTopSpeed(chasingSpeed);
                break;       
            } case 2: { // Drive towards the Delivery Spot.
                vanTraversal.ChangeTarget(deliveryObjective); 
                vanMovement.ChangeTopSpeed(regularSpeed);
                break;       
            }
        }
    }
}
