using System.Collections;
using UnityEngine;

[RequireComponent(typeof(ParticleSystem))]
public class Cannon : MonoBehaviour {
    
    [SerializeField] private float firePower;
    [SerializeField] private float minTime, maxTime;
    [SerializeField] private GameObject cannonBall, loadingPointObj;
    [SerializeField] private AudioSource audioSource;
    private ParticleSystem cannonParticles;
    private Vector3 loadingPoint;

    void Awake() { cannonParticles = GetComponent<ParticleSystem>(); }

    void OnEnable() {
        Transform trans = GameManager.obstacleManager.GetSideNode();
        transform.SetPositionAndRotation(trans.position, trans.rotation);
        loadingPoint = loadingPointObj.transform.position;
        StartCoroutine(CannonCoroutine());
    }

    private void FireCannon() {
        audioSource.Play();
        cannonParticles.Play();
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
