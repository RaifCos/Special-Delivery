using UnityEngine;
[RequireComponent(typeof(ParticleSystem))]

[RequireComponent(typeof(AudioSource))]
[RequireComponent(typeof(MeshRenderer))]
public class Bomb : MonoBehaviour {

    [SerializeField] private float radius = 40.0f;
    [SerializeField] private float power = 20000.0f;
    private bool hasExploded;

    private MeshRenderer meshRenderer;
    private AudioSource boomSound;
    private ParticleSystem boomPS;
    [SerializeField] private ParticleSystem fusePS;

    void Awake() {
        meshRenderer = GetComponent<MeshRenderer>();
        boomSound = GetComponent<AudioSource>();
        boomPS = GetComponent<ParticleSystem>();
    }

    void OnEnable() {
        fusePS.Play();
        hasExploded = false;
        meshRenderer.enabled = true;  
        transform.position = GameManager.obstacleManager.GetNearestNode(1, 0f).transform.position + (Vector3.up * 25f);
    }

    void LateUpdate() {
        if (!fusePS.isPlaying && !hasExploded) {
            boomPS.Play();
            boomSound.Play();
            Explode();
            hasExploded = true;
        } else if(hasExploded && !boomPS.isPlaying) { gameObject.SetActive(false); }
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
