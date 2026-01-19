using UnityEngine;

public class Magnet : MonoBehaviour {
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start() {
        Transform trans = GameManager.obstacleManager.GetSideNode();
        transform.SetPositionAndRotation(trans.position, trans.rotation);
    }
}
