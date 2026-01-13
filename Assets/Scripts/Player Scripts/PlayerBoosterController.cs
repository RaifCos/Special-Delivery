using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class PlayerBoosterControl : MonoBehaviour {

     [Header("Fuel Values")]
    public float maxFuel, comsumptionRate, replenishRate, cooldownTime;

    [Header("Boost Effects")]
    public AudioSource boosterSound; 
    public ParticleSystem boostParticle; 
    public Slider fuelMeterSlider;
    public Image fuelSliderImage;
    [SerializeField] private Gradient fuelGradient;

    private float fuel;
    private bool wantsToBoost, canBoost, isBoosting, isBoostUnlocked;

    void Start() {
        isBoostUnlocked = PlayerPrefs.GetInt("Upgrade_booster", 0) == 1;
        SetFuel(maxFuel);
        StartCoroutine(FuelAdjustment());
    }

    void FixedUpdate() {
        wantsToBoost = Input.GetKey(KeyCode.Space);
        canBoost = isBoostUnlocked && fuel > 0f;

        if (!canBoost) {
            StopBoost();
            return;
        }

        if (wantsToBoost) { StartBoost(); } 
        else { StopBoost(); }
    }

    void StartBoost() {
        if (isBoosting) return;
        isBoosting = true;
        boostParticle.Play();
        if (!boosterSound.isPlaying)
            boosterSound.Play();
    }

    void StopBoost() {
        if (!isBoosting) return;
        isBoosting = false;
        boostParticle.Stop();
        boosterSound.Stop();
    }

    public bool IsBoosting() { return isBoosting; }

    public void SetFuel(float input) {
        fuel = input;
        fuelMeterSlider.value = fuel;
        fuelSliderImage.color = fuelGradient.Evaluate(fuel / maxFuel);
    }

    public IEnumerator FuelAdjustment() {
        while(true) {
            if (isBoosting) { 
                SetFuel(fuel-1); 
                yield return new WaitForSeconds(comsumptionRate);    
            } else if (fuel < maxFuel) {
                yield return new WaitForSeconds(cooldownTime);
                while(fuel < maxFuel && !isBoosting) {
                    SetFuel(fuel+1); 
                    yield return new WaitForSeconds(replenishRate);
                }
            } else { yield return null; }
        }
    }
}