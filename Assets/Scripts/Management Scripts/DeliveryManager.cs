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

    void Awake() => GameManager.deliveryManager = this;

    void Start() {
        difficulty = GameManager.instance.GetDifficulty();

        if(difficulty != 2) { objectiveObj = standardObjective; }
        else { objectiveObj = bossObjective; }
        objectiveObj.SetActive(true);

        currPosition = parcelPos;
        parcelPos = parcelNode.transform.position;
        for (int x = 0; x < deliveryNodes.transform.childCount; x++) { nodePositions.Add(deliveryNodes.transform.GetChild(x).transform.position); }
    }

    public void ChangeState(bool input) {
        if (difficulty != 2) { objectiveObj.GetComponent<ParcelObjective>().ChangeState(input); }
        else { objectiveObj.GetComponent<BossParcelObjective>().ChangeState(input); }
    }

    public Vector3 GetParcelPos() {
        currPosition = parcelPos;
        return currPosition;
    }

    public Vector3 GetDeliverySpot() {
        int currIndex = nodePositions.IndexOf(currPosition);
        int newIndex;

        do { newIndex = Random.Range(0, nodePositions.Count);
        } while (newIndex == currIndex);

        currPosition = nodePositions[newIndex];
        return currPosition;
    }

    public Vector3 GetCurrentPosition() => currPosition;
}
