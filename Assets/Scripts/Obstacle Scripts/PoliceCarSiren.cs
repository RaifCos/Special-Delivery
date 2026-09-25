using UnityEngine;
using System.Collections;

public class PoliceCarSiren : MonoBehaviour {

    [Header ("Siren Display")]
    [SerializeField] private MeshRenderer blueSiren;
    [SerializeField] private MeshRenderer redSiren;
    [SerializeField] private Material litMat = null;
    [SerializeField] private Material unlitMat = null;
    [SerializeField] private float interval = 0.3f;

    [Header ("Siren Light")]
    [SerializeField] private Light light;
    [SerializeField] private Color blueCol;
    [SerializeField] private Color redCol;

    private void OnEnable() => StartCoroutine(FlashRoutine()); 

    private void OnDisable() => StopAllCoroutines();

    private IEnumerator FlashRoutine() {
        bool blueLit = true;
        var wait = new WaitForSeconds(interval);

        while (true) {
            blueSiren.material = blueLit ? litMat : unlitMat;
            redSiren.material = !blueLit ? litMat : unlitMat;
            light.color = blueLit ? blueCol : redCol;
            blueLit = !blueLit;
            yield return wait;
        }
    }
}