using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;

public class PlayerBoosterControl : MonoBehaviour {

    [Header("Player Input")]
    [SerializeField] private InputAction vanBoost;

    [Header("Fuel Values")]
    [SerializeField] private float defaultMaxFuel;
    [SerializeField] private float comsumptionRate;
    [SerializeField] private float replenishRate;
    [SerializeField] private float cooldownTime;
    private float maxFuel;

    [Header("Boost Effects")]
    [SerializeField] private AudioSource boosterSound; 
    [SerializeField] private ParticleSystem boostParticle; 
    [SerializeField] private Slider fuelMeterSlider;
    [SerializeField] private Image fuelSliderImage;
    [SerializeField] private GameObject fuelTank;
    [SerializeField] private Gradient fuelGradient;
    [SerializeField] private GameObject exhaustPipe;

    private float fuel;
    private bool isBoosting;

    void OnEnable() { vanBoost.Enable(); }

    public void Disable() { vanBoost.Disable(); }

    void Start() {
        if( !GameManager.dataManager.IsUpgraded("booster") ) { 
            fuelMeterSlider.gameObject.SetActive(false);
            fuelTank.SetActive(false);
            exhaustPipe.SetActive(false);
            enabled = false; return; 
        }
        maxFuel = defaultMaxFuel;
        maxFuel += GameManager.dataManager.IsUpgraded("boosterFuel_I")? 50: 0;
        maxFuel += GameManager.dataManager.IsUpgraded("boosterFuel_II")? 50: 0;
        fuelMeterSlider.GetComponent<Slider>().maxValue = maxFuel;
        SetFuel(maxFuel);
        StartCoroutine(FuelAdjustment());
    }

    void FixedUpdate() {
        if (vanBoost.IsPressed() && fuel > 0f) { StartBoost(); } 
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