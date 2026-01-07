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
    private int lastStory;
    public readonly List<string> newsQueue = new(); 

    // Collections of standard news headlines.
    private readonly string[] genericHeadlines = {
    "tip: the red arrow at the top of the screen points to where you need to go.",
    "tip: reversing is much slower than driving forward. only reverse when you need to.",
    "tip: slow down before taking tight turns!",
    "tip: more obstacles will pop up as you complete more deliveries, so look out!",
    "tip: there are shortcuts hidden around the city to help you get places quicker.",
    "tip: pay attention to these news headlines, some of them tell you what obstacles are coming your way!",
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

    void Awake() {
        GameManager.newsTextScroller = this;
    }

    void Start() {
        newsText = newsUI.GetComponent<TMP_Text>();
        lastStory = -1;
    }

    // Function to begin the scrolling text 
    public void StartNews(int difficulty) {
        // Clear Queue
        newsQueue.Clear();

        // Add starting headline and additional text if the player is in the tutorial.
        if (difficulty == 0) {
            newsQueue.Add("welcome to special delivery! use WASD or the arrow keys to drive, and hold SPACE to break.");
            newsQueue.Add("follow the red arrow to start delivering parcels!");
            newsQueue.Add("once you have the hang of things, press ESCAPE to return to the menu and start playing!");
        } else { newsQueue.Add("BREAKING NEWS - local delivery service hires new driver, somehow a headlining story."); }

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

    // Coroutine to take text from the queue and move it along the news bar.
    IEnumerator ScrollText() {
        // Only scroll text while a game is in session.
        while (isPlaying) {
            // If there are no headlines in the queue, add a random generic string.
            if (newsQueue.Count == 0) { AddGenericHeadline(); }
            // Take headline at top of the queue.
            newsText.text = newsQueue[0];
            newsQueue.RemoveAt(0);

            // Move the text along the top of the screen until it reaches the end. 
            Vector2 preferredValues = newsText.GetPreferredValues(newsText.text);
            newsText.rectTransform.sizeDelta = new Vector2(preferredValues.x, newsText.rectTransform.sizeDelta.y);
            newsText.rectTransform.anchoredPosition = new Vector2(1010f, 5);
            while (newsText.rectTransform.anchoredPosition.x > -1010f) {
                newsText.rectTransform.anchoredPosition += Vector2.left * scrollSpeed * Time.deltaTime;
                yield return null;
            }
        }
    }
}
