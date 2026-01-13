using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class PlayerBoosterControl : MonoBehaviour {

     [Header("Fuel Values")]
    public float defaultMaxFuel, comsumptionRate, replenishRate, cooldownTime;
    private float maxFuel;

    [Header("Boost Effects")]
    public AudioSource boosterSound; 
    public ParticleSystem boostParticle; 
    public Slider fuelMeterSlider;
    public Image fuelSliderImage;
    public GameObject fuelTank;
    [SerializeField] private Gradient fuelGradient;

    private float fuel;
    private bool wantsToBoost, canBoost, isBoosting, isBoostUnlocked;

    void Start() {
        isBoostUnlocked = PlayerPrefs.GetInt("Upgrade_booster", 0) == 1;
        maxFuel = defaultMaxFuel;
        maxFuel += PlayerPrefs.GetInt("Upgrade_boosterFuel_I", 0) == 1? 50: 0;
        maxFuel += PlayerPrefs.GetInt("Upgrade_boosterFuel_II", 0) == 1? 50: 0;
        SetFuel(maxFuel);
        StartCoroutine(FuelAdjustment());
        if(!isBoostUnlocked) {
            fuelMeterSlider.gameObject.SetActive(false);
            fuelTank.SetActive(false);
        }
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

    IEnumerator FuelAdjustment() {
        while (true) {
            if (isBoosting) { SetFuel(fuel - comsumptionRate * Time.deltaTime); }
            else if (fuel < maxFuel) { SetFuel(fuel + replenishRate * Time.deltaTime); }
            yield return null;
        }
    }
}