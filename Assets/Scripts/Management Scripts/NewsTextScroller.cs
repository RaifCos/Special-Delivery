using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using System.Linq;

// Script to control the scrolling text news bar during gameplay
public class NewsTextScroller : MonoBehaviour
{
    public GameObject newsUI;
    public float scrollSpeed;
    TMP_Text newsText;
    private bool isPlaying;
    private int lastStory, difficulty;
    public readonly List<string> newsQueue = new(); 

    // Collections of standard news headlines.
    private readonly string[] genericHeadlines = {
    "tip: the red arrow at the top of the screen points to where you need to go.",
    "tip: reversing is much slower than driving forward. only reverse when you need to.",
    "tip: slow down before taking tight turns!",
    "tip: more obstacles will pop up as you complete more deliveries, so look out!",
    "tip: there are shortcuts hidden around the city to help you get places quicker.",
    "tip: pay attention to these news headlines, some of them tell you what obstacles are coming your way!",
    "tip: using your boost will make it harder to turn, so save your fuel for long, straight stretches of road.",
    "\"please stop running over the stop signs\" pleads city council.",
    "mass confusion caused after most street signs knocked down by wreckless delivery driver.",
    "local resident creates website to track the number of cones knocked over by delivery drivers - \"leavetheconesalone.org\"",
    "local rodent community celebrates destruction of bins.",
    "council spends \u20AC9.8 million on new benches across the city, hopefully nobody knocks them over or anything.",
    "fire department switches to water balloons after shortage of hydrants - \"it makes fires way more exciting\".",
    "\"there's no knocking down my trees\", boasts park gardener.",
    "fountain in city centre turned off so developer doesn't have to program flowing water.",
    "\"kfg ipsiufbgpu989-jnu\", says cat who walked across writer's keyboard.",
    "mail delivery driver breaks record for most road traffic violations.",
    "scientists attempt to explain the high volume of glowing yellow spots around the city.",
    "top ten numbers from one to ten (number four will surprise you!)",
    "the average cosumer now expects their package to be delivered in thirty seconds.",
    "study concludes in-universe news mechanic serves \"no real use, honestly.\"",
    "diamond heist at local bank \"sounds like a much better idea for a video game than whatever this nonsense is\".",
    "psa: if a car magically appears in front of you, don't worry, it (probaly) isn't a bug.",
    "\"it's a feature\" says game developer in response to what is clearly a bug.",
    };

    void Start() {
        GameManager.newsTextScroller = this;
        difficulty = GameManager.instance.GetDifficulty();
        newsText = newsUI.GetComponent<TMP_Text>();
        lastStory = -1;
    }

    // Function to begin the scrolling text 
    public void StartNews() {
        // Clear Queue
        newsQueue.Clear();

        // Add starting headline and additional text if the player is in the tutorial.
        if (difficulty == 0) { AddTutorialHeadlines(); }
        else { newsQueue.Add("BREAKING NEWS - local delivery service hires new driver, somehow a headlining story."); }

        // Set position of text component and start scrolling.
        newsText.rectTransform.position = new Vector3(1010f, 5, 0);
        isPlaying = true;
        StartCoroutine(ScrollText());
    }

    // Function to end the scrolling text when the game ends.
    public void StopNews() { 
        isPlaying = false;
        StopAllCoroutines();
    }

    // Function to "queue up" a random headline if no other headlines are currently in the queue.
    private void AddGenericHeadline() {
        // Generate random headline that's different from the previous.
        int i;
        do { i = Random.Range(0, genericHeadlines.Count()); } while (i == lastStory);
        newsQueue.Add(genericHeadlines[i]);
        lastStory = i;
    }

    // Function to add headline corresponding to an achievement being earned.
    public void AddAchievementHeadline(string achievementName) {
        if (isPlaying) {
            string res = "BREAKING NEWS - you've completed the achievement " + achievementName + "!";
            newsQueue.Add(res);
        }
    }

    public void AddTutorialHeadlines() {
        newsQueue.Add("welcome to special delivery! a game all about delivering parcels as fast as you can. use WASD or the arrow keys to drive.");
        newsQueue.Add("delivering parcels is the name of the game. follow the tracker arrow at the top of your screen to find a parcel.");
        newsQueue.Add("once you've collected a parcel, follow the tracker arrow again to its glowing yellow delivery spot.");
        newsQueue.Add("press SPACE to use your booster and go faster! boosting does take up fuel though, so use it wisely.");
        newsQueue.Add("as you complete more deliveries, obstacles will begin to fill the stage, making each delivery harder than the last. try to deliver as many parcels as you can before time runs out.");
        newsQueue.Add("this is just the tutorial, so no need to worry about the timer or obstacles. Just get used to the controls!");
        newsQueue.Add("once you're ready to play, press ESCAPE to return to the menu. happy delivering!");
    }

    // Coroutine to take text from the queue and move it along the news bar.
    IEnumerator ScrollText() {
        // Only scroll text while a game is in session.
        while (isPlaying) {
            // If there are no headlines in the queue, add a random generic string.
            if (newsQueue.Count == 0) { 
                if(difficulty == 0) { AddTutorialHeadlines(); }
                if(difficulty == 1) { AddGenericHeadline(); }
            }
            // Take headline at top of the queue.
            newsText.text = newsQueue[0];
            newsQueue.RemoveAt(0);

            RectTransform textRect = newsText.rectTransform;
            RectTransform parentRect = textRect.parent as RectTransform;

            Vector2 preferredValues = newsText.GetPreferredValues(newsText.text);
            newsText.rectTransform.sizeDelta = new Vector2(preferredValues.x, newsText.rectTransform.sizeDelta.y);

            // Start just outside the right edge
            float startX = parentRect.rect.width / 2f + textRect.rect.width / 2f;

            // End just outside the left edge
            float endX = -parentRect.rect.width / 2f - textRect.rect.width / 2f;

            textRect.anchoredPosition = new Vector2(startX, 0);

            while (textRect.anchoredPosition.x > endX) {
                textRect.anchoredPosition += Vector2.left * scrollSpeed * Time.deltaTime;
                yield return null;
            }
        }
    }
}