using UnityEngine;

// Script to handle the behaviour of the Giant Cone.
public class GiantCone : MonoBehaviour {

    [SerializeField] private Vector2 topCorner;
    [SerializeField] private Vector2 bottomCorner;
    void OnEnable() {
        // set Position somewhere in the middle of the map. 
        transform.position = new Vector3(Random.Range(topCorner.x, bottomCorner.x), 100f, Random.Range(topCorner.y, bottomCorner.y));
    }
}
