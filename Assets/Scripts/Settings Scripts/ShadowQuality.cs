using UnityEngine;

[RequireComponent(typeof(Light))]
public class ShadowQuality : MonoBehaviour {
    void Start() {
        Light light = GetComponent<Light>();
        Debug.Log(GameManager.instance.GetShadowQuality());
        light.shadows = GameManager.instance.GetShadowQuality() ? LightShadows.Soft : LightShadows.Hard;
    }
}
