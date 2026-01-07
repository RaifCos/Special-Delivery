using UnityEngine;

// Script to constantly rotate wheels on NPC Cars.
public class WheelRotation : MonoBehaviour {

    public bool leftWheel;
    private float speed = 8f;

    void FixedUpdate() { transform.Rotate(new Vector3(speed, 0f, 0f)); }
}
