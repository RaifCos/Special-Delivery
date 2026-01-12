using UnityEngine;

    public struct ShopUpgrade {
        public string internalName, externalName, description;
        public int cost, state;

        // State = 0 (Locked)
        // State = 1 (Unlocked, Unpurchased)
        // State = 2 (Purchased)

        public ShopUpgrade(string iN, string eN, int c, int defaultState, string d ) {
            internalName = iN;
            externalName = eN;
            cost = c;
            description = d;
            state = defaultState;
        }

        public readonly int GetCost() { return cost; }

        public void SetState(int input) { state = input; }
    }


// Script to handle all the obstacles on stage.
public class ShopData : MonoBehaviour
{
    private readonly ShopUpgrade[] upgrades = {
        new("booster", "booster", 250, 1,
        "equips a rocket booster to the van. press SPACE to accelerate to top-notch speeds!"),

        new("fuel-I", "bigger tank", 500, 0,
        "increases the maximum amount of fuel in the booster tank."),

        new("consumption-I", "fuel efficient", 500, 0,
        "decreases the rate at which fuel depltes from the booster tank."),
    };

    void Awake() { }

    void Start() {
        for (int i = 0; i < upgrades.Length; i++) {
            upgrades[i].SetState(PlayerPrefs.GetInt("Upgrade" + i, 0));
        }
    }

    public ShopUpgrade[] GetUpgrades() { return upgrades; }
    
    public void BuyUpgrade(int id) {
        ShopUpgrade u = upgrades[id];
        u.SetState(2);
    }
}
