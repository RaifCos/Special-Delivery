using UnityEngine;

public class Obstacle : MonoBehaviour {

    public Obstacle_SO so;

    void Start() {
        GameManager.obstacleData.AddObstacleEncounter(so.internalName);
    }
}