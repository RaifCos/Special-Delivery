using UnityEngine;

public class ObstacleTemp : MonoBehaviour {

    public ObstacleTemp_SO so;
    int count;

    public int GetCount() { return count; }
    public void SetCount(int input) { count = input; }
    public void IncrementCount() { count++; }
}