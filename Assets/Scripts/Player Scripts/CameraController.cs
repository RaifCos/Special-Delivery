using UnityEngine;

public class CameraController : MonoBehaviour {
    public float distance, height, smoothTime;
    public LayerMask collisionMask;
    public float collisionRadius = 0.3f;

    Vector3 lookTarget, lookTargetVelocity, currentVelocity;

    private void LateUpdate() {
        if (GameManager.gameplayManager.isPlaying) {

            Vector3 flatForward = transform.forward;
            flatForward.y = 0;

            if (flatForward.sqrMagnitude < 0.001f) flatForward = Vector3.forward;
            flatForward.Normalize();

            Vector3 hPos = transform.position + (-flatForward * distance);
            Vector3 vPos = Vector3.up * height;
            Vector3 desiredPosition = hPos + vPos;


            Vector3 dirToCamera = (desiredPosition - transform.position).normalized;
            float desiredDistance = Vector3.Distance(transform.position, desiredPosition);

            if (Physics.SphereCast(
                    transform.position,
                    collisionRadius,
                    dirToCamera,
                    out RaycastHit hit,
                    desiredDistance,
                    collisionMask))
            { desiredPosition = transform.position + dirToCamera * (hit.distance - collisionRadius); }

            Vector3 target = Vector3.SmoothDamp(
                Camera.main.transform.position,
                desiredPosition,
                ref currentVelocity,
                smoothTime);

            target.y = Mathf.Max(target.y, 4);
            Camera.main.transform.position = target;

            Vector3 lookOffset = Vector3.up * height;
            lookTarget = Vector3.SmoothDamp(
                lookTarget,
                transform.position + lookOffset,
                ref lookTargetVelocity,
                smoothTime);

            Camera.main.transform.LookAt(lookTarget);
        }
    }
}