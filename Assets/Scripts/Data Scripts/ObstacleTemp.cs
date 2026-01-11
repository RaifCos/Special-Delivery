using UnityEngine;

public class ObstacleTemp : MonoBehaviour {

    public ObstacleTemp_SO so;
    
    void Start() {
        GameManager.obstacleData.AddTempObstacleEncounter(so.intenalName);
    }

}