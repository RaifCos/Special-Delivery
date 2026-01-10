using System.Linq;
using UnityEngine;

    public struct Obstacle {
        public string intenalName, externalName, description, headline;
        public int count, limit;
        public bool isPermanent;

        public Obstacle(string iN, string eN, bool iP, int l, string d, string h) {
            intenalName = iN;
            externalName = eN;
            isPermanent = iP;
            limit = l;
            count = 0;
            description = d;
            headline = h;
        }

        public void SetCount(int input) { count = input; }
        public void IncrementCount() { count++; }
        public readonly bool CheckLimit() { return count < limit; }
    }


// Script to handle all the obstacles on stage.
public class ObstacleData : MonoBehaviour
{
    private readonly Obstacle[] obstacles = {
        new("carRed", "car", true, 3,
        "cars navigate the roads of parcel city, honking at everything in their path.\n\nthe drivers of parcel city love honking so much they specially imported oversea cars built with two horns. Nothing else in these models really work, but haven't seemed to notice.",
        "efficient delivery driver somehow causes surge in traffic."),

        new("carBlue", "slowmobile", true, 3,
        "slowmobiles are slower, heavier cars that start fierce traffic.\n\ndespite their name, slowmobiles are actually one of the fastest car brands worldwide. The drivers just like to hold up traffic. Jerks.",
        "traffic backed up on every major route due to gameplay mechanics."),

        new("carGreen", "slick speedster", true, 3,
        "slick speeders are faster, lighter cars that'll push you off the road if you're not careful.\n\nthese drivers have absolutely no respect for the road! well, neither do you (clearly), but at least you have an excuse. sorta.",
        "\"where do these cars keep coming from??\" - cries frustrated commuter."),

        new("carToy", "toy car", true, 2,
        "toy cars are incredibly light, but cause chaos with their extreme speed.\n\nthese little terrors were originally found in homes and schools across the globe, but took to the streets en masse after they had enough of terrorizing children. now they focus on postal workers.",
        "toy car voted \"cutest road hazard\" by drivers."),

        new("carBig", "monster car", true, 1,
        "monster cars dominate the road like a giant moving wall.\n\ndespite it's name, the monster car is still only about half the size of a hummer.",
        "there have been reports downtown of... \"a really big car\"?.. seriously?"),

        new("ufo", "ufo", true, 1,
        "ufos hover from above, pulling objects (and you!) up with their tractor beam.\n\nyou may think the occupants of this ship are trying to abduct you, thankfully they just go around beaming stuff for the love of the game. be honest, wouldn't you do the same if you had one?",
        "ufo spotted downtown as writer struggles to come up with funny headline."),

        new("boomerang", "boomerang", true, 2,
        "boomerangs spin across the stage, whacking anything in their path.\n\n\"full disclosure: boomerangs are absolutely broken, they basically break the physics of anything they touch. However, this is funny, so i'm keeping them in the game.\" - the developer",
        "\"the dangers of boomerangs\" reaches top of nyt bestsellers list after initial low reviews."),

        new("giantCone", "giant Cone", true, 3,
        "giant invincible cones that come crashing down from the sky into the middle of the stage.\n\nif these cones are invincible then how can i see them?",
        "local artist unveils touching \"tribute to cones everywhere\", whatever that means."),

        new("boulder", "boulder", false, 1,
        "boulders roll across the stage causing destruction and mayhem.\n\nnobody knows where the boulder came from, or where they're going, all they are is what they are in the moment. we have a lot to learn from the boulder.",
        "local museum carries out search party for misplaced boulder: \"no stone left unturned.\""),

        new("boulderSnow", "snowball", false, 1,
        "snowballs roll across the stage, shrinking but speeding up along the way.\n\nthese snowballs don't leave behind a trail of snow, but rather a path destruction and various debris- which isn't as fun :(",
        "\"world's largest snowball fight\" being held downtown today, despite complete lack of snow."),

        new("dumpster", "dumpster", false, 1,
        "dumpsters come tumbling across the stage, speeding up along the way.\n\ndumpster diving? no this is dumpster driving- dumpster driving you crazy!",
        "\"i've heard of garbage collection but this is just ridiculous!\" - says bystander following reports of runaway dumpsters."),

        new("anvil", "anvil", false, 1,
        "anvils come crashing down from the sky right onto the delivery van.\n\nit may not be cats and dogs, but it's certainly raining... something.",
        "weather forecast - sunny with slight overcast. yellow weather warning for falling anvils."),

        new("piano", "piano", false, 1,
         "pianos decrescendo from above right onto the delivery van.\n\ndropping pianos from great heights is all part of the latest music craze called \"breakcore\", and- oh? what's that? ah, i've just been informed that is not what breakcore is.",
        "experts advise you should \"see sharp\" or else you might \"be flat\", regarding falling pianos."),

        new("fakeParcels", "fake parcels", false, 1,
        "imposter parcels fall from the sky to trick and confuse.\n\nthese imposter parcels are quite deceiving... suspicious even...\n\namongus.",
        "delivery drivers have been warned to look out for faux parcels, because they exist. apparently."),

        new("boxingGlove", "boxing glove", false, 1,
        "boxing gloves sprout of the ground to suckerpunch the delivery van.\n\nrumours of where these mysterious subterranean fists keep coming from all circle back to some sort of underground fighting club. I'm told we're not allowed to discuss it though.",
        "underground boxing club canceled after excessive uppercuts resulted in ceiling damage.")
    };

    private int[] lifetimeObstacleCount, lifetimePropCount;

    void Awake() { GameManager.obstacleData = this; ResetEncounters(); }

    void Start() {
        for (int i = 0; i < lifetimeObstacleCount.Length; i++) {
            lifetimeObstacleCount[i] = PlayerPrefs.GetInt("EncounterO" + i, 0);
        }
        for (int i = 0; i < lifetimePropCount.Length; i++) {
            lifetimePropCount[i] = PlayerPrefs.GetInt("EncounterP" + i, 0);
        }   
    }

    public Obstacle[] GetObstacles() { return obstacles; }
    
    // Function to increment the number of times an obstacle has been "encountered" (for achievement tracking).
    public  void AddObstacleEncounter(int id) {
        lifetimeObstacleCount[id]++;
        PlayerPrefs.SetInt("EncounterO" + id, lifetimeObstacleCount[id]);
        PlayerPrefs.Save();
        if (!lifetimeObstacleCount.Contains(0) && !lifetimePropCount.Contains(0)) { GameManager.achievementData.CompleteAchievement(6); }
    }
    
    public void AddPropEncounter(int id) {
        lifetimePropCount[id]++;
        PlayerPrefs.SetInt("EncounterP" + id, lifetimePropCount[id]);
        PlayerPrefs.Save();
        if (!lifetimeObstacleCount.Contains(0) && !lifetimePropCount.Contains(0)) { GameManager.achievementData.CompleteAchievement(6); }
        CheckProps();
    }
    
    public void ResetEncounters() {
        lifetimeObstacleCount = new int[obstacles.Length];
        lifetimePropCount = new int[8];
    }
    
    // Corountine to check if all props of a certain type have been destoyed (for achievement tracking).
    public void CheckProps() {
        if (GameObject.Find("Stop Sign") == null && GameObject.Find("Street Sign") == null) { GameManager.achievementData.CompleteAchievement(7); }
        if (GameObject.Find("Cone") == null) { GameManager.achievementData.CompleteAchievement(8); }
        if (GameObject.Find("Bin") == null) { GameManager.achievementData.CompleteAchievement(9); }
        if (GameObject.Find("Hydrant") == null) { GameManager.achievementData.CompleteAchievement(10); }
        if (GameObject.Find("Bench") == null) { GameManager.achievementData.CompleteAchievement(11); }
    }
}
