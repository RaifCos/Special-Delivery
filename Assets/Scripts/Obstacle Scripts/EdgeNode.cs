using UnityEngine;

public class EdgeNode : MonoBehaviour {
    [SerializeField] private GameObject[] oppositeNodes;

    public GameObject GetOppositeNode() => oppositeNodes[Random.Range(0, oppositeNodes.Length)]; 

    public Vector3[] GetPath() {
        Vector3[] res = new Vector3[2];
        res[0] = transform.position;
        res[1] = GetOppositeNode().transform.position;
        return res;
    }
}
