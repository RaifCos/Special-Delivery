using System.Collections;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class MainMenuGlide : MonoBehaviour {
    private static WaitForSeconds _waitForSeconds0_5 = new WaitForSeconds(0.5f);
    public float setSpeed;
    private Rigidbody rb;
    public TrafficNode startingNode;
    private TrafficNode currNode, prevNode;
    private Vector3 currPos;
    private float speed;

    void Start() {
        rb = GetComponent<Rigidbody>();
        prevNode = startingNode;
        currNode = prevNode.GetNextNode(prevNode);
        currPos = currNode.GetPos();
        StartCoroutine(BeginMoving());
    }

    void FixedUpdate() {
        if (Vector3.Distance(rb.position, currPos) > 3f) {
            rb.MovePosition(Vector3.MoveTowards(transform.position, currPos, speed * Time.deltaTime));
        } else {
            TrafficNode tempNode = currNode;
            currNode = tempNode.GetNextNode(prevNode);
            currPos = currNode.GetPos();
            prevNode = tempNode;
        }
    }
    
    IEnumerator BeginMoving() {
        int i = 0;
        
        while (i < 30) {
            i++;
            rb.AddTorque(new Vector3(0f, 7.5f/30, 0f));
            speed += setSpeed / 30;
            yield return _waitForSeconds0_5;
        }
    }
    
}
