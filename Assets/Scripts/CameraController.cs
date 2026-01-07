using UnityEngine;

// Script to handle the Main Camera's movement in respect to the player.
public class CameraController : MonoBehaviour
{
    public float distance, height, smoothTime;
    Vector3 lookTarget, lookTargetVelocity, currentVelocity;

    private void LateUpdate()
    {
        if (GameManager.gameplayManager.isPlaying) {
            // Set the Camera's Target Position based on the Player's Position.
            Vector3 hPos = transform.position + (-transform.forward * distance);
            Vector3 vPos = Vector3.up * height;

            Vector3 target = Vector3.SmoothDamp(Camera.main.transform.position, hPos + vPos, ref currentVelocity, smoothTime);
            target.y = Mathf.Max(target.y, 4);

            // Update the camera's position with the clamped value
            Camera.main.transform.position = target;

            // Set Camera to Look at Player.
            lookTarget = Vector3.SmoothDamp(lookTarget, transform.position + vPos, ref lookTargetVelocity, smoothTime);
            Camera.main.transform.LookAt(lookTarget);
        }
    }
}
