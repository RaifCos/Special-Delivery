using UnityEngine;

[RequireComponent(typeof(CarTraversal))]
public class BossVan : MonoBehaviour {

    [SerializeField] int regularSpeed;
    [SerializeField] int chasingSpeed;
    private CarTraversal vanTraversal;
    private GameObject player, deliveryObjective;
    private int phase;

    void Awake() { vanTraversal = GetComponent<CarTraversal>(); }

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
                vanTraversal.ChangeTopSpeed(regularSpeed);
                break;       
            } case 1: { // Chase the Player.
                vanTraversal.ChangeTarget(player); 
                vanTraversal.ChangeTopSpeed(chasingSpeed);
                break;       
            } case 2: { // Drive towards the Delivery Spot.
                vanTraversal.ChangeTarget(deliveryObjective); 
                vanTraversal.ChangeTopSpeed(regularSpeed);
                break;       
            }
        }
    }
}
