using System.Collections;
using UnityEngine;

public class Cannon : MonoBehaviour {
    
    [SerializeField] private float firePower;
    [SerializeField] private float minTime, maxTime;
    [SerializeField] private GameObject cannonBall, loadingPointObj;
    private Vector3 loadingPoint;

    void Start() {
        Transform trans = GameManager.obstacleManager.GetSideNode();
        transform.SetPositionAndRotation(trans.position, trans.rotation);
        loadingPoint = loadingPointObj.transform.position;
        StartCoroutine(CannonCoroutine());
    }

    private void FireCannon() {
        GameObject obj = Instantiate(cannonBall, loadingPoint, transform.rotation);
        Rigidbody rb = obj.GetComponent<Rigidbody>();
        rb.AddForce(firePower * transform.right, ForceMode.Impulse);
    }

    private IEnumerator CannonCoroutine() {
        while(true) {
            yield return new WaitForSeconds(Random.Range(minTime, maxTime));
            FireCannon();
        }
    }
}
