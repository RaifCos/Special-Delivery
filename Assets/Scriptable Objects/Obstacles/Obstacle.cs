using UnityEngine;

public class Obstacle : MonoBehaviour {

    public Obstacle_SO so;

    void Start() {
        GameManager.dataManager.AddObstacleEncounter(so.internalName);
    }
}