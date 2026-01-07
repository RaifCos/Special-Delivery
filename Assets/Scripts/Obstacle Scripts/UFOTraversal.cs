using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]

// Script to handle the behaviour of the UFO.
public class UFOTraversal : MonoBehaviour
{

    public float speed, height;
    private Rigidbody rb;
    GameObject currNode, prevNode;
    Vector3 currPos;

    void Start() {
        rb = GetComponent<Rigidbody>();

        prevNode = GameManager.instance.GetComponent<ObstacleManager>().GetStartingNode(1);
        currNode = prevNode.GetComponent<TrafficNode>().GetNextNode(prevNode);
        currPos = currNode.transform.position + (Vector3.up * 25f);
        transform.position = prevNode.transform.position + (Vector3.up * 25f);
    }

    void FixedUpdate() {
        if (Vector3.Distance(transform.position, currPos) > 3f) {
            Vector3 direction = (currPos - transform.position).normalized;
            rb.linearVelocity = direction * speed;
            SpinRotation();
        }
        else {
            GameObject tempNode = currNode;
            currNode = tempNode.GetComponent<TrafficNode>().GetNextNode(prevNode);
            prevNode = tempNode;
            currPos = currNode.transform.position + (Vector3.up * 25f);
        }
    }

    // Function to constantly rotate the UFO.
    public void SpinRotation() { transform.Rotate(Vector3.up, -(speed * 10) * Time.deltaTime, Space.World); }
}
