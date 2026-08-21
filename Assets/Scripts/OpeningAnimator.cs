using System.Collections;
using UnityEngine;

[RequireComponent(typeof(RectTransform))]
public class OpeningAnimator : MonoBehaviour {
    public enum Direction { Left, Right, Top, Bottom }

    [Header("Setup")]
    [SerializeField] private Direction offscreenDirection = Direction.Left;
    [SerializeField] private float extraPadding = 100f;

    [Header("Animation")]
    [SerializeField] private float duration = 0.6f;
    [SerializeField] private float delay = 0f;
    [SerializeField] private AnimationCurve ease = AnimationCurve.EaseInOut(0, 0, 1, 1);

    private RectTransform rt;
    private Vector2 onScreenPos, offScreenPos;
    private Coroutine activeRoutine;
    private bool alreadyAnimated = false;

    private Animator animator;

    private void Awake() {
        rt = GetComponent<RectTransform>();
        animator = GetComponent<Animator>();
        onScreenPos = rt.anchoredPosition;
    }

    private void OnEnable() { 
        if (!alreadyAnimated) { 
            if (animator != null) animator.enabled = false;
            SnapOffscreen(); 
            AnimateIn();
            alreadyAnimated = true;
        }
    }

    private void SnapOffscreen() {
        Vector2 offset = Vector2.zero;
        float width = rt.rect.width;
        float height = rt.rect.height;

        switch (offscreenDirection) {
            case Direction.Left:
                offset = new Vector2(-(width + extraPadding), 0);
                break;
            case Direction.Right:
                offset = new Vector2(width + extraPadding, 0);
                break;
            case Direction.Top:
                offset = new Vector2(0, height + extraPadding);
                break;
            case Direction.Bottom:
                offset = new Vector2(0, -(height + extraPadding));
                break;
        } offScreenPos = onScreenPos + offset;
        rt.anchoredPosition = offScreenPos;
    }

    public void AnimateIn() {
        if (activeRoutine != null) StopCoroutine(activeRoutine);
        activeRoutine = StartCoroutine(AnimateRoutine());
    }

    private IEnumerator AnimateRoutine() {
        if (delay > 0f) yield return new WaitForSeconds(delay);

        float t = 0f;
        Vector2 start = rt.anchoredPosition;

        while (t < duration) {
            t += Time.deltaTime;
            float normalized = Mathf.Clamp01(t / duration);
            float eased = ease.Evaluate(normalized);
            rt.anchoredPosition = Vector2.LerpUnclamped(start, onScreenPos, eased);
            yield return null;
        } rt.anchoredPosition = onScreenPos;
        if (animator != null) animator.enabled = true;
    }
}