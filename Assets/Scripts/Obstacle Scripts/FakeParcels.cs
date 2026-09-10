using System.Collections;
using UnityEngine;

public class FakeParcels : MonoBehaviour {

    [SerializeField] private GameObject fakeParcel;
    [SerializeField] private int minParcels, maxParcels;
    [SerializeField] private float spawnRate;

    private static WaitForSeconds wait;
    private int parcelCount;

    void Awake() => wait = new(spawnRate);

    // Start is called before the first frame update
    void OnEnable() {
        parcelCount = Random.Range(minParcels, maxParcels);
        StartCoroutine(ParcelGroup());
    }

    void OnDisable() { StopAllCoroutines(); }

    IEnumerator ParcelGroup() {
        for (int i = 0; i < parcelCount; i++) {
            SpawnParcel();
            yield return wait;
        } gameObject.SetActive(false);
    }

    void SpawnParcel() {
        GameObject obj = Instantiate(fakeParcel);
        float x = Random.Range(1.4f, 2f);
        float y = Random.Range(1.4f, 2f);
        float z = Random.Range(1.4f, 2f);
        obj.transform.localScale = new Vector3(x, y, z);
    }
}
