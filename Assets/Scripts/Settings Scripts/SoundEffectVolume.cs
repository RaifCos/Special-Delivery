using UnityEngine;
using System.Collections.Generic;

public class SoundEffectVolume : MonoBehaviour {

    [SerializeField] private List<AudioSource> soundEffectSources;
    readonly Dictionary<AudioSource, float> maxVolumeValues = new();

    void Start() {
        foreach (AudioSource aS in soundEffectSources) maxVolumeValues.Add(aS, aS.volume);
    }

    public void AdjustVolume(float percentage) {
        foreach (AudioSource aS in maxVolumeValues.Keys) {
            aS.volume = maxVolumeValues[aS] * percentage;
        }
    }
}
