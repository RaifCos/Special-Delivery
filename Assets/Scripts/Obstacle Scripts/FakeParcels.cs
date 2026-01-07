using System.Collections;
using UnityEngine;

public class FakeParcels : MonoBehaviour {
    private static WaitForSeconds _waitForSeconds0_5 = new WaitForSeconds(0.5f);
    private int parcelCount;

    // Start is called before the first frame update
    void Start() {
        parcelCount = Random.Range(4, 8);
        StartCoroutine(ParcelGroup());
    }

    IEnumerator ParcelGroup() {
        for (int i = 0; i < parcelCount; i++) {
            SpawnParcel();
            yield return _waitForSeconds0_5;
        }
        Destroy(gameObject);
    }
    void SpawnParcel() {
        GameObject obj = Instantiate(Resources.Load<GameObject>("Obstacles/fakeParcel"));
        float x = Random.Range(1.4f, 2f);
        float y = Random.Range(1.4f, 2f);
        float z = Random.Range(1.4f, 2f);
        obj.transform.localScale = new Vector3(x, y, z);
        GameManager.obstacleManager.AddObstacle(obj);
    }
}
