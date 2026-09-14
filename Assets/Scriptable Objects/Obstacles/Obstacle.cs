using UnityEngine;

public class Obstacle : MonoBehaviour {

    public Obstacle_SO so;

    void OnEnable() => GameManager.dataManager.AddObstacleEncounter(so.internalName);
}