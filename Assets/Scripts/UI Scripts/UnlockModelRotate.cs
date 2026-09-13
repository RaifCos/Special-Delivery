using UnityEngine;

public class UnlockModelRotate : MonoBehaviour {

    private readonly float speed = 1.5f;

    void FixedUpdate() { transform.Rotate(new Vector3(0f, speed, 0f)); }
}
