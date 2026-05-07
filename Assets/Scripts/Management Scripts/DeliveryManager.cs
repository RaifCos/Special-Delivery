using System.Collections.Generic;
using UnityEngine;

// Script to handle objectives (Parcels and Delivery Spots)
public class DeliveryManager : MonoBehaviour {
    [Header ("Objective Objects")]
    [SerializeField] private GameObject standardObjective;
    [SerializeField] private GameObject bossObjective;
    private GameObject objectiveObj;

    [Header ("Node Data")]
    [SerializeField] private GameObject parcelNode;
    [SerializeField] private GameObject deliveryNodes;
    private readonly List<Vector3> nodePositions = new();
    private Vector3 parcelPos, currPosition;
    private int difficulty;

    void Awake() {
        GameManager.deliveryManager = this;
        difficulty = GameManager.instance.GetDifficulty();
        objectiveObj = (difficulty != 2) ? standardObjective : bossObjective;
        objectiveObj.SetActive(true);
    }

    void Start() {
        parcelPos = parcelNode.transform.position;
        currPosition = parcelPos;
        for (int x = 0; x < deliveryNodes.transform.childCount; x++) {
            nodePositions.Add(deliveryNodes.transform.GetChild(x).transform.position);
        }
    }

    public void ChangeState(int phase) {
        if (difficulty != 2) { objectiveObj.GetComponent<ParcelObjective>().ChangeState(phase == 0); }
        else { objectiveObj.GetComponent<BossParcelObjective>().ChangeState(phase); }
    }

    public Vector3 GetParcelPos() {
        currPosition = parcelPos;
        return currPosition;
    }

    public Vector3 GetDeliverySpot() {
        currPosition = nodePositions[0];
        int newIndex;
        if (nodePositions.Count <= 1) {
            newIndex = 0;
        } else {
            int currIndex = nodePositions.IndexOf(currPosition);
            newIndex = Random.Range(0, nodePositions.Count);
            if (newIndex == currIndex) {
                newIndex = (newIndex + 1) % nodePositions.Count;
            }
        }
        currPosition = nodePositions[newIndex];
        return currPosition;
    }

    public Vector3 GetCurrentPosition() => currPosition;

    public GameObject GetDeliveryObjective() => objectiveObj;

}
