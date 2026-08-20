using System.Collections;
using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class Perfume : MonoBehaviour {
    
    [SerializeField] private float minTime, maxTime;
    [SerializeField] private float nozzleSpeed;
    private AudioSource audioSource;
    private Transform nozzleTransform;
    private ParticleSystem spray;
    private Vector3 nozzleUp, nozzleDown;

    void Awake() { 
        GameObject nozzle = transform.GetChild(0).gameObject;
        nozzleTransform = nozzle.transform;
        spray = nozzle.GetComponent<ParticleSystem>(); 
        audioSource = GetComponent<AudioSource>();
    }

    void OnEnable() {
        Transform trans = GameManager.obstacleManager.GetSideNode();
        transform.SetPositionAndRotation(trans.position, trans.rotation);
        nozzleUp = nozzleTransform.localPosition;
        nozzleDown = new Vector3(nozzleUp.x, nozzleUp.y - 0.75f, nozzleUp.z);
        StartCoroutine(PerfumeCoroutine());
    }

    private void FixedUpdate() {
        if (spray.isPlaying) nozzleTransform.localPosition = Vector3.MoveTowards(nozzleTransform.localPosition, nozzleDown, nozzleSpeed * Time.deltaTime);
        else nozzleTransform.localPosition = Vector3.MoveTowards(nozzleTransform.localPosition, nozzleUp, nozzleSpeed * 2 * Time.deltaTime);
    }

    private IEnumerator PerfumeCoroutine() {
        while(true) {
            yield return new WaitForSeconds(Random.Range(minTime, maxTime));
            spray.Play();
            audioSource.Play();
        }
    }
}
