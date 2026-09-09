using UnityEngine;

public class BowlingBall : MonoBehaviour {
    [SerializeField] private GameObject ball;
    [SerializeField] private GameObject pins;
    private Vector3[] pinLocalTransforms = new Vector3[6];

    void Awake() { for(int i=0; i<pins.transform.childCount; i++) pinLocalTransforms[i] = pins.transform.GetChild(i).localPosition; }

    void OnEnable() => ball.SetActive(true);

    void OnDisable() => ball.SetActive(false);

    void LateUpdate() {
        if (ball.activeInHierarchy) return;
        gameObject.SetActive(false);
    }

    public void PlacePins(Vector3 ballPos, Vector3 pinPos) {
        pins.SetActive(true);
        pins.transform.position = pinPos;
        pins.transform.LookAt(ballPos);
        GameObject currPin;
        for (int i = 0; i < pins.transform.childCount; i++) {
            currPin = pins.transform.GetChild(i).gameObject;
            currPin.GetComponent<Rigidbody>().linearVelocity = new Vector3(0, 0, 0);
            currPin.GetComponent<Rigidbody>().angularVelocity = new Vector3(0, 0, 0);
            currPin.transform.localPosition = pinLocalTransforms[i];
            currPin.transform.rotation = Quaternion.Euler(Vector3.zero);
            currPin.transform.localScale = Vector3.one;
            currPin.SetActive(true);
        }
    }

}