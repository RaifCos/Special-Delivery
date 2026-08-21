using System.Collections;
using Unity.VisualScripting;
using UnityEngine;

public class CameraController : MonoBehaviour {
    [SerializeField] float distance = 20;
    [SerializeField] float height = 2.5f;
    [SerializeField] float smoothTime = 0.4f;
    [SerializeField] LayerMask collisionMask;
    [SerializeField] float collisionRadius = 0.3f;
    [SerializeField] float openingTime = 4f;
    private float openingSmoothTime;
    private bool openingFinsihed = false;

    Vector3 lookTarget, lookTargetVelocity, currentVelocity;

    private void Start() {
        openingSmoothTime = smoothTime * 4f;
        StartCoroutine(OpeningCamera());
    }

    private void LateUpdate() {
        if (!openingFinsihed) MoveCamera(openingSmoothTime);
        else if (GameManager.gameplayManager.IsPlaying()) MoveCamera(smoothTime);
        
    }

    private void MoveCamera(float sT) {
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
            sT);

        target.y = Mathf.Max(target.y, 4);
        Camera.main.transform.position = target;

        Vector3 lookOffset = Vector3.up * height;
        lookTarget = Vector3.SmoothDamp(
            lookTarget,
            transform.position + lookOffset,
            ref lookTargetVelocity,
            sT);

        Camera.main.transform.LookAt(lookTarget);
    }

    private IEnumerator OpeningCamera() {
        yield return new WaitForSeconds(openingTime);
        openingFinsihed = true;
    }
}