using System.Collections;
using UnityEngine;

public class WreckingBall : MonoBehaviour {
    private static readonly WaitForSeconds _waitForSeconds20 = new(12f);

    void OnEnable() {
        transform.localScale = Vector3.one; 

        Transform trans = GameManager.obstacleManager.GetMiddleNode();
        transform.SetPositionAndRotation(trans.position, trans.rotation);
        transform.position += Vector3.up * 36.5f;

        float zRotation = Random.value < 0.5f ? 170f : -170f;
        transform.GetChild(0).rotation = Quaternion.Euler(0f, 0f, zRotation);

        StartCoroutine(BallCountdown());
    }

    void OnDisable() { StopAllCoroutines(); }

    private IEnumerator BallCountdown() {
        yield return _waitForSeconds20;
        StartCoroutine(GameManager.obstacleManager.ShrinkAndDestroy(gameObject, false, true));
    }

}