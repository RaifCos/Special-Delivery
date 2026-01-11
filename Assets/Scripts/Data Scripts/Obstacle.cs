using UnityEngine;

public class Obstacle : MonoBehaviour {

    public Obstacle_SO so;

    void Start() {
        GameManager.obstacleData.AddPermObstacleEncounter(so.intenalName);
    }
}