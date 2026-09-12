using UnityEngine;

public class CollisionExplosion : MonoBehaviour {

    [SerializeField] private float forceSensitivity; 
    [SerializeField] private float explosionPower; 
    [SerializeField] private float explosionRadius;
    [SerializeField] private bool destoryOnExplosion;

    private void OnCollisionEnter(Collision collision) { if (collision.relativeVelocity.magnitude > forceSensitivity) Explode(); }

    private void Explode() {
        Vector3 explosionPos = transform.position;
        Collider[] colliders = Physics.OverlapSphere(explosionPos, explosionRadius);
        foreach (Collider hit in colliders) {
            if (hit.attachedRigidbody != null)
                hit.attachedRigidbody.AddExplosionForce(explosionPower, explosionPos, explosionRadius, 3.0f, ForceMode.Impulse);
        } GameManager.obstacleManager.ExplodeAndDestory(gameObject, destoryOnExplosion, false);
    }
}
