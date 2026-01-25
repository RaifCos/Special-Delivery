using UnityEngine;

[RequireComponent(typeof(Light))]
public class ShadowQuality : MonoBehaviour {
    void Start() {
        Light light = GetComponent<Light>();
        if(GameManager.instance.GetShadowQuality()) {
            light.shadows = LightShadows.Soft;
        } else {
            light.shadows = LightShadows.Hard;
        }
    }
}
