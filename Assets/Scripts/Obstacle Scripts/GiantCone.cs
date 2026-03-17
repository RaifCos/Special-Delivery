using UnityEngine;

// Script to handle the behaviour of the Giant Cone.
public class GiantCone : MonoBehaviour {

    [SerializeField] private Vector2 topCorner;
    [SerializeField] private Vector2 bottomCorner;
    void Start() {
        // set Position somewhere in the middle of the map. 
        transform.position = new Vector3(Random.Range(topCorner.x, bottomCorner.x), 20f, Random.Range(topCorner.y, bottomCorner.y));
    }
}
