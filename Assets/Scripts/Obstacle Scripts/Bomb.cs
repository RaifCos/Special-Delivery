using System.Collections;
using UnityEngine;
[RequireComponent(typeof(ParticleSystem))]
[RequireComponent(typeof(AudioSource))]
[RequireComponent(typeof(MeshRenderer))]
public class Bomb : MonoBehaviour {
    [SerializeField] private float fuseTime;
    [SerializeField] private float radius = 40.0f;
    [SerializeField] private float power = 20000.0f;
    private WaitForSeconds fuseTimer;

    void Awake() => fuseTimer = new(fuseTime);

    void OnEnable() { StartCoroutine(BombTimer()); }

    private IEnumerator BombTimer() {
        yield return fuseTimer;
        Explode();
    }

    private void Explode() {  
        Vector3 explosionPos = transform.position;
        Collider[] colliders = Physics.OverlapSphere(explosionPos, radius);
        foreach (Collider hit in colliders) {
            if (hit.TryGetComponent<Rigidbody>(out var rb))
                rb.AddExplosionForce(power, explosionPos, radius, 3.0f, ForceMode.Impulse);
        } GameManager.obstacleManager.ExplodeAndDestory(gameObject, false, true);
    }
}
