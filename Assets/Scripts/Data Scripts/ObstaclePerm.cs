using UnityEngine;

public class ObstaclePerm : MonoBehaviour {

    public ObstaclePerm_SO so;

    void Start() {
        GameManager.obstacleData.AddPermObstacleEncounter(so.intenalName);
    }
}