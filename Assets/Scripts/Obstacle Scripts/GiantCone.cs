using UnityEngine;

// Script to handle the behaviour of the Giant Cone.
public class GiantCone : MonoBehaviour {

    void Start() {
        // set Position somewhere in the middle of the map. 
        transform.position = new Vector3(Random.Range(0, 31), 20f, Random.Range(0, 31));
    }
}
