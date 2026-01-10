using UnityEngine;

public class ObstaclePerm : MonoBehaviour {

    public ObstaclePerm_SO so;
    int count;

    public int GetCount() { return count; }
    public void SetCount(int input) { count = input; }
    public void IncrementCount() { count++; }
    public bool CheckLimit() => count < so.limit;
}