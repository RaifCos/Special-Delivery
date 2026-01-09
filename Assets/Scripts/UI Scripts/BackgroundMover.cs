using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Image))]
public class BackgroundMover : MonoBehaviour
{

    public float speed;
    Vector2 direction = new(-1, -1);
    Image img;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start() {
        img = GetComponent<Image>();
    }

    // Update is called once per frame
    void Update() {
        img.material.mainTextureOffset += -direction.normalized * Time.deltaTime * speed;
    }
}
