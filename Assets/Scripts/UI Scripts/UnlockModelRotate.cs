using UnityEngine;

public class UnlockModelRotate : MonoBehaviour {

    private readonly float speed = 2f;

    void FixedUpdate() { transform.Rotate(new Vector3(0f, speed, 0f)); }
}
