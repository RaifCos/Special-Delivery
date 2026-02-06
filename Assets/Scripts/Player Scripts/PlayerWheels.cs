using UnityEngine;

[RequireComponent(typeof(WheelCollider))]
public class WheelControl : MonoBehaviour {
    public Transform wheelModel;

    public bool steerable;

    private Vector3 position;
    private Quaternion rotation;

    private WheelCollider WheelCollider;

    void Start() {
        WheelCollider = this.GetComponent<WheelCollider>();
    }

    void Update() {
        WheelCollider.GetWorldPose(out position, out rotation);
        wheelModel.transform.SetPositionAndRotation(position, rotation);
    }

    public void SetMotorTorque(float input) => WheelCollider.motorTorque = input; 

    public void SetBrakeTorque(float input) => WheelCollider.brakeTorque = input;

    public void SetSteerAngle(float input) => WheelCollider.steerAngle = input;
}
