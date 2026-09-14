using UnityEngine;

[RequireComponent(typeof(Light))]
public class ShadowQuality : MonoBehaviour {
    void Start() {
        Light light = GetComponent<Light>();
        light.shadows = GameManager.instance.GetShadowQuality() ? LightShadows.Soft : LightShadows.Hard;
    }
}
