using UnityEngine;
using UnityEngine.UI;
using System.Collections;

[RequireComponent(typeof(Image))]
public class OpeningScreen : MonoBehaviour {
    [SerializeField] private float duration = 1f;
    [SerializeField] private AnimationCurve ease = AnimationCurve.EaseInOut(0, 0, 1, 1);

    private Image panel;
    private Material material;
    private static readonly int RadiusID = Shader.PropertyToID("_Radius");

    void Awake() {
        panel = GetComponent<Image>();
        material = panel.material;
    }

    void Start() { IrisEnter(); } 

    public void IrisEnter() => StartCoroutine(Animate(0f, 1.5f));
    public void IrisExit() => StartCoroutine(Animate(1.5f, 0f));

    private IEnumerator Animate(float from, float to) {
        float t = 0f;
        while (t < duration) {
            t += Time.deltaTime;
            float r = Mathf.Lerp(from, to, ease.Evaluate(Mathf.Clamp01(t / duration)));
            material.SetFloat(RadiusID, r);
            yield return null;
        } material.SetFloat(RadiusID, to);
    }
}