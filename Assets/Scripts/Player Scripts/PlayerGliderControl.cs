using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

enum GliderStates {
    closed,
    animating,
    opened
}

public class PlayerGliderControl : MonoBehaviour {
    private static readonly WaitForSeconds animationWait = new(0.1f);

    [Header("Player Input")]
    [SerializeField] private InputAction vanGlide;

    [Header("Glider Effects")]
    [SerializeField] private Transform gliderObject;

    private GliderStates state = GliderStates.closed;

    void OnEnable() { vanGlide.Enable(); }

    public void Disable() { vanGlide.Disable(); }

    private void FixedUpdate() {
        if (vanGlide.IsPressed() && state == GliderStates.closed) { 
            StartCoroutine(GliderAnimation(true));
            state = GliderStates.animating;
        } 

        else if (vanGlide.IsPressed() && state == GliderStates.opened) { 
            StartCoroutine(GliderAnimation(false));
            state = GliderStates.animating;
        }
    }

    private IEnumerator GliderAnimation(bool opening) {
        float start = opening? 0.35f : 1f;
        float target = opening? 1f : 0.35f;
        float rate = opening? 0.05f : -0.05f;

        gliderObject.localScale = new(start, 1f, 1f);

        while (!Mathf.Approximately(gliderObject.localScale.x, target)) {
            gliderObject.localScale += Vector3.right * rate;
            yield return null;
        } state = opening? GliderStates.opened : GliderStates.closed;
    }
}
