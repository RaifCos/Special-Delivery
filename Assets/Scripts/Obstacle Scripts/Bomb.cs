using UnityEngine;
[RequireComponent(typeof(ParticleSystem))]
[RequireComponent(typeof(AudioSource))]
[RequireComponent(typeof(MeshRenderer))]
public class Bomb : MonoBehaviour {

    [SerializeField] private float radius = 40.0f;
    [SerializeField] private float power = 20000.0f;

    private bool hasExploded;
    private MeshRenderer meshRenderer;
    private GameObject boom;
    private AudioSource boomAS;
    private ParticleSystem fusePS;

    void Awake() {
        meshRenderer = GetComponent<MeshRenderer>();
        fusePS = GetComponent<ParticleSystem>();
        boomAS = GetComponent<AudioSource>();
        boom = transform.GetChild(0).gameObject;
    }

    void OnEnable() {
        boom.SetActive(false);
        fusePS.Play();
        hasExploded = false;
        meshRenderer.enabled = true;  
        transform.position = GameManager.obstacleManager.GetNearestNode(1, 0f).transform.position + (Vector3.up * 25f);
    }

    void LateUpdate() {
        if (!fusePS.isPlaying && !hasExploded) {
            boom.SetActive(true);
            Explode();
            hasExploded = true;
        } else if(hasExploded && !boomAS.isPlaying) { gameObject.SetActive(false); }
    }

    private void Explode() {
        meshRenderer.enabled = false;  
        Vector3 explosionPos = transform.position;
        Collider[] colliders = Physics.OverlapSphere(explosionPos, radius);
        foreach (Collider hit in colliders) {
            if (hit.TryGetComponent<Rigidbody>(out var rb))
                rb.AddExplosionForce(power, explosionPos, radius, 3.0f, ForceMode.Impulse);
        }
    }
}
