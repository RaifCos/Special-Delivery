using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Image))]
public class BackgroundMover : MonoBehaviour {

    public float speed;
    Vector2 direction = new(-1, -1);
    Image img;

    void Start() => img = GetComponent<Image>();

    void Update() => img.material.mainTextureOffset += speed * Time.deltaTime * -direction.normalized;
}
