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
    private bool isBoosting;

    void Start() {
        if( !GameManager.dataManager.IsUpgraded("booster") ) { enabled = false; return; }
        maxFuel = defaultMaxFuel;
        maxFuel += GameManager.dataManager.IsUpgraded("boosterFuel_I")? 50: 0;
        maxFuel += GameManager.dataManager.IsUpgraded("boosterFuel_II")? 50: 0;
        SetFuel(maxFuel);
        StartCoroutine(FuelAdjustment());
        fuelMeterSlider.GetComponent<Slider>().maxValue = maxFuel;
        fuelMeterSlider.gameObject.SetActive(false);
        fuelTank.SetActive(false);
    }

    void FixedUpdate() {
        if (Input.GetKey(KeyCode.Space) && fuel > 0f) { StartBoost(); } 
        else { StopBoost(); }
    }

    void StartBoost() {
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