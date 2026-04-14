using UnityEngine;

public class Magnet : MonoBehaviour {
    MagnetPull pull; 

    void OnEnable() {
        Transform trans = GameManager.obstacleManager.GetSideNode();
        transform.SetPositionAndRotation(trans.position, trans.rotation);
        pull = transform.GetChild(0).GetComponent<MagnetPull>();
    }

    void OnCollisionEnter(Collision collision) {
        pull.removeObj(collision.gameObject.GetComponent<Rigidbody>());
    }
}
