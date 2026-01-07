using System.Collections;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class MainMenuGlide : MonoBehaviour {
    private static WaitForSeconds _waitForSeconds0_5 = new WaitForSeconds(0.5f);
    public float setSpeed;
    private Rigidbody rb;
    public GameObject startingNode;
    private GameObject currNode, prevNode;
    private float speed;

    // Start is called before the first frame update
    void Start() {
        rb = GetComponent<Rigidbody>();
        prevNode = startingNode;
        currNode = prevNode.GetComponent<TrafficNode>().GetNextNode(prevNode);
        StartCoroutine(BeginMoving());
    }

    // Update is called once per frame
    void FixedUpdate() {
        // Check if Car is close enough to its target node.
        if (Vector3.Distance(rb.position, currNode.transform.position) > 3f) {
            rb.MovePosition(Vector3.MoveTowards(transform.position, currNode.transform.position, speed * Time.deltaTime));
        } else {
            // Car is close enough to target, so select new target node.
            GameObject tempNode = currNode;
            currNode = tempNode.GetComponent<TrafficNode>().GetNextNode(prevNode);
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
