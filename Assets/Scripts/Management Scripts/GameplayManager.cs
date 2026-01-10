using System.Collections;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

// Script to handle main game functionality.
public class GameplayManager : MonoBehaviour
{
    private static readonly WaitForSeconds _waitForSeconds0_01 = new(0.01f);
    private static readonly WaitForSeconds _waitForSeconds1 = new(1);
    public GameObject gameUI, endUI, pauseUI, confirmUI;
    public GameObject playerVan, directionArrow;
    public bool isPlaying = false;
    private bool isGamePaused = false;

    private int completeDeliveries, timeLeft, difficulty; //, confirmationUIID;
    private Animator scoreAnimator, timeAnimator;

    void Awake() { GameManager.gameplayManager = this; }

    // Start is called before the first frame update.
    void Start() {
        Time.timeScale = 1;
        difficulty = GameManager.instance.GetDifficulty();

        // Set Difficulty based on user selection, hide the timer UI in the tutorial
        gameUI.transform.GetChild(2).GetComponent<Image>().enabled = difficulty != 0;
        gameUI.transform.GetChild(4).GetComponent<TMP_Text>().enabled = difficulty != 0;

        // Set up game UI and score/timer values.
        AlternateGameMenus(0);
        SetScore(0, false);
        SetTime(60, false);

        // Initialize the player van.
        playerVan.GetComponent<PlayerControl>().SetState(true);

        // Spawn starting obstacles into the city (if not in the tutorial).
        if (difficulty != 0) { GameManager.obstacleManager.SpawnStartingObstacles(); }

        // Start Music
        StartCoroutine(GameManager.audioManager.StartGameMusic());

        // Start News Text UI
        GameManager.newsTextScroller.StartNews();

        // Set UI for Score Animation
        scoreAnimator = gameUI.transform.GetChild(1).gameObject.GetComponent<Animator>();
        timeAnimator = gameUI.transform.GetChild(2).gameObject.GetComponent<Animator>();

        // Start timer and begin game. 
        isPlaying = true;
        if (difficulty != 0) { StartCoroutine(GameTimer()); }
    }

    // Update is called once per frame.
    void Update() {
        // Only run when a game is in session.
        if (isPlaying) {
            // Pause game if the Escape key is pressed.
            if (Input.GetKeyDown(KeyCode.Escape)) { 
                if (!isGamePaused) { PauseGame(); } 
                else { ResumeGame(); }
            }

            // Rotate Directional Arrow to point towards the current objective, relative to the player's position.
            Vector3 direction = GameManager.deliveryManager.GetCurrentPosition() - playerVan.transform.position;
            Quaternion targetRotation = Quaternion.LookRotation(direction, Vector3.up);
            directionArrow.transform.rotation = Quaternion.Slerp(directionArrow.transform.rotation, targetRotation, Time.deltaTime * 5f);
        }
    }

    /*
     * ======================
     *  GAME-STATE FUNCTIONS
     * ======================
     */

    // Function for when the player runs out of time.
    public void GameOver() {
        // Stop the game.
        StopGameloop();

        // Display game over screen.
        AlternateGameMenus(1);
        StartCoroutine(GameOverFade());

        // Stop game music and play game over music.
        StopCoroutine(GameManager.audioManager.StartGameMusic());
        StartCoroutine(GameManager.audioManager.EndGameMusic());
    }

    // Function to stop gameplay when the game ends.
    public void StopGameloop() {
        // Stop gameplay loop and timer.
        isPlaying = false;

        // Stop van controls.
        playerVan.GetComponent<PlayerControl>().SetState(false);

        // Stop News Text UI
        GameManager.newsTextScroller.StopNews();
    }

    /*
    * ======================
    *  GAMEPLAY FUNCTIONS
    * ======================
    */

    // Coroutine to decrement the game timer every second.
    private IEnumerator GameTimer() {
        // Only decrement while there is time left, and a game is in session.
        while (timeLeft > 0 && isPlaying) {
            yield return _waitForSeconds1; // Wait one second before decrementing time.
            SetTime(-1, true);
        }
    }

    // Setter Method for the timer, also updates the UI and checks if time has ran out.
    public void SetTime(int value, bool addingTime) {
        if (addingTime) {
            timeLeft += value;
            if (value > 0 && timeLeft >= 100) { GameManager.achievementManager.CompleteAchievement(5); }
            if (value > 0 && timeLeft >= 10) { TimerAnimation("highTime"); }
            if (value < 0 && timeLeft <= 10) { TimerAnimation("lowTime"); }
            if (timeLeft == 0) { GameOver(); }
        }
        else { timeLeft = value; }
        gameUI.transform.GetChild(4).gameObject.GetComponent<TMP_Text>().text = timeLeft.ToString();
    }

    // Setter Method for the delivery score, also updates the UI.
    public void SetScore(int value, bool addingScore) {
        if (addingScore) {
            completeDeliveries += value;
            if (difficulty != 0) {
                if (completeDeliveries == 10) { GameManager.achievementManager.CompleteAchievement(0); }
                if (completeDeliveries == 50) { GameManager.achievementManager.CompleteAchievement(2); }
                if (completeDeliveries > GameManager.instance.GetBestScore()) { GameManager.instance.SetBestScore(completeDeliveries); }
            }
        }
        else { completeDeliveries = value; }
        gameUI.transform.GetChild(3).gameObject.GetComponent<TMP_Text>().text = completeDeliveries.ToString();
    }

    public int GetScore() { return completeDeliveries; }

    /*
    * ======================
    *     UI FUNCTIONS
    * ======================
    */

    public void ScoreAnimation() { scoreAnimator.SetTrigger("scoreAnim"); }

    public void TimerAnimation(string trigger) {
        timeAnimator.ResetTrigger("lowTime");
        timeAnimator.ResetTrigger("highTime");
        timeAnimator.SetTrigger(trigger);
    }

    // Coroutine that fades the game over screen into view.
    private IEnumerator GameOverFade() {
        // Set UI to be fully transparant.
        endUI.GetComponent<CanvasGroup>().alpha = 0;
        // Until fully faded in, decrease transparancy a little bit every 1/100 seconds.
        while (endUI.GetComponent<CanvasGroup>().alpha < 1)
        {
            yield return _waitForSeconds0_01;
            endUI.GetComponent<CanvasGroup>().alpha += 0.05f;
        }
    }

    // Function to pause the game and go to the pause menu.
    public void PauseGame() {
        isGamePaused = true;
        playerVan.GetComponent<PlayerControl>().SetState(false);
        GameManager.audioManager.TogglePause(true);
        Time.timeScale = 0;
        AlternateGameMenus(2);
    }

    // Function to resume the game from the pause menu.
    public void ResumeGame() {
        isGamePaused = false;
        playerVan.GetComponent<PlayerControl>().SetState(true);
        GameManager.audioManager.TogglePause(false);
        Time.timeScale = 1;
        AlternateGameMenus(0);
    }

    // Function to quit the current round and return to the main menu.
    public void QuitGame() {
        StopGameloop();
        Time.timeScale = 1;
        AlternateGameMenus(3);
        StartCoroutine(GameManager.instance.LoadAsyncScene("MainMenu"));
    }
    
    public void AlternateGameMenus(int menu) {
        switch (menu) {
            case 0: { // Game UI
                pauseUI.SetActive(false);
                break; }
            case 1: { // Game Over Screen
                endUI.SetActive(true);
                break; }
            case 2: { // Pause Menu
                pauseUI.SetActive(true);
                break; }
            case 3: { // Loading Screen
                gameUI.SetActive(false);
                endUI.SetActive(false);
                pauseUI.SetActive(false);
                Instantiate(Resources.Load<GameObject>("LoadingScreen"));
                break; }    
        }
    }

    // Function to ask the user to confirm their choice on an important UI choice.
    public void MenuConfirmationMessage(int cID) {
        //confirmationUIID = cID;
        TMP_Text message = confirmUI.transform.GetChild(3).GetComponent<TMP_Text>();

        pauseUI.SetActive(false);
        if (difficulty == 0) { message.text = "end the tutorial\nand return to the menu?"; }
        else { message.text = "end the game\nand return to menu?"; }
        confirmUI.SetActive(true);
    }
    
    // Funciton to carry out the appropiate UI response based on the confirmation response.
    public void MenuConfirmationResponse(bool response) {
        if (!response) { confirmUI.SetActive(false); }
        else { QuitGame(); }
    }
}