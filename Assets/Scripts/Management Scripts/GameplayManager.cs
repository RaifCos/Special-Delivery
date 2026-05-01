using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;
using UnityEngine.EventSystems;
using TMPro;
using System;

// Script to handle main game functionality.
public class GameplayManager : MonoBehaviour
{
    private static readonly WaitForSeconds _waitForSeconds1 = new(1);
    private static readonly WaitForSeconds _waitForSeconds001 = new(0.01f);
    private static readonly WaitForSeconds _waitForSeconds0001 = new(0.001f);
    [SerializeField] private int startingTime; 
    [SerializeField] private InputAction pauseAction;
    [SerializeField] private AudioClip countSound, overtimeSound;
    [SerializeField] private GameObject gameUI, endUI, pauseUI, confirmUI;
    [SerializeField] private GameObject playerVan, directionArrow, moneyText; 
    [SerializeField] private GameObject pauseStartSelect, endStartSelect, confirmStartSelect;
    public bool isPlaying = false;
    private bool isGamePaused = false;
    private bool secondLife = false; 

    private int completeDeliveries, timeLeft, deliveryTime, difficulty, deliveryPayment, moneyEarnt;
    private float penaltyMult, incomeMult;
    private Animator scoreAnimator, timeAnimator;
    private EventSystem eventSystem;

    void OnEnable() { 
        pauseAction.Enable();
        eventSystem = EventSystem.current;    
    }

    void OnDisable() { pauseAction.Disable(); }

    void Awake() { GameManager.gameplayManager = this; }

    // Start is called before the first frame update.
    void Start() {
        penaltyMult = GameManager.dataManager.IsUpgraded("noPenalty")? 0f: 1f;
        incomeMult = GameManager.dataManager.IsUpgraded("moreMoney")? 1.5f: 1f;
        secondLife = GameManager.dataManager.IsUpgraded("secondLife");
        moneyEarnt = 0;
        Time.timeScale = 1;
        difficulty = GameManager.instance.GetDifficulty();

        // Set Difficulty based on user selection, hide the timer UI in the tutorial
        gameUI.transform.GetChild(2).GetComponent<Image>().enabled = difficulty != 0;
        gameUI.transform.GetChild(4).GetComponent<TMP_Text>().enabled = difficulty != 0;

        // Set up game UI and score/timer values.
        AlternateGameMenus(0);
        SetScore(0, false);

        int startTime = startingTime;
        startTime += GameManager.dataManager.IsUpgraded("moreTime")? 20: 0;
        SetTime(startTime, false);

        // Initialize the player van.
        playerVan.GetComponent<PlayerControl>().SetState(true);

        // Spawn starting obstacles into the city (if not in the tutorial).
        if (difficulty != 0) { GameManager.obstacleManager.SpawnStartingObstacles(); }

        // Start Music
        GameManager.audioManager.StartGameMusic();

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
            if (pauseAction.WasPressedThisFrame()) { 
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
            deliveryTime++;
        }
    }

    // Setter Method for the timer, also updates the UI and checks if time has ran out.
    public void SetTime(int value, bool addingTime) {
        if (addingTime) {
            if (value < 0 && timeLeft == 1 && secondLife) { 
                GameManager.audioManager.PlaySoundEffect(overtimeSound, false);
                secondLife = false; value = 30;
            }
            timeLeft += value;
            if (value > 0 && timeLeft >= 120) { GameManager.dataManager.CompleteAchievement("timer120"); }
            if (value > 0 && timeLeft >= 10) { TimerAnimation("highTime"); }
            if (value < 0 && timeLeft <= 10) { TimerAnimation("lowTime"); }
            if (timeLeft == 0) { GameOver(); }
        } else { timeLeft = value; }
        gameUI.transform.GetChild(4).gameObject.GetComponent<TMP_Text>().text = timeLeft.ToString();
    }

    // Setter Method for the delivery score, also updates the UI.
    public void SetScore(int value, bool addingScore) {
        if (addingScore) {
            completeDeliveries++;
            if (difficulty != 0) {
                CalculateEarnings();
                StartCoroutine(MoneyDisplay(deliveryPayment));
                if (completeDeliveries == 10) { GameManager.dataManager.CompleteAchievement("score10"); }
                if (completeDeliveries == 50) { GameManager.dataManager.CompleteAchievement("score50"); }
                if (completeDeliveries > GameManager.dataManager.GetBestScore()) { GameManager.dataManager.SetBestScore(completeDeliveries); }
            }
        }
        else { completeDeliveries = value; }
        gameUI.transform.GetChild(3).gameObject.GetComponent<TMP_Text>().text = completeDeliveries.ToString();
    }

    public int GetScore() { return completeDeliveries; }

    private void CalculateEarnings() {
        int income = 29 + (int) Math.Pow(1.2, completeDeliveries);
        double timePenalty = Math.Min(0.25, deliveryTime/100.0) * income;
        deliveryPayment = (int) (income * incomeMult) - (int) (timePenalty * penaltyMult);
        moneyEarnt += deliveryPayment;
        deliveryTime = 0;
    }

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
        while (endUI.GetComponent<CanvasGroup>().alpha < 1) {
            yield return _waitForSeconds001;
            endUI.GetComponent<CanvasGroup>().alpha += 0.05f;
        } yield return _waitForSeconds1;
        StartCoroutine(MoneyCount());
    }

    private IEnumerator MoneyCount() {
        GameObject counter = endUI.transform.Find("Counter").gameObject;
        counter.SetActive(true);
        TMP_Text counterText = counter.transform.Find("Amount").GetComponent<TMP_Text>();
        int display = 0;
        do { display++; 
            counterText.text = display.ToString(); 
            if (display % 10 == 0) { GameManager.audioManager.PlaySoundEffect(countSound, false); }
            yield return _waitForSeconds0001;
        } while (display < moneyEarnt);
        yield return _waitForSeconds1;
        endUI.transform.Find("Menu Button").gameObject.SetActive(true);
    }

    private IEnumerator MoneyDisplay(int amount) {
        TextMeshProUGUI moneyTMP = moneyText.GetComponent<TextMeshProUGUI>();
        RectTransform rect = moneyText.GetComponent<RectTransform>();
        moneyTMP.text = "+" + amount;
        rect.anchoredPosition = Vector2.up * 400f;
        byte alp = 0;
        while(alp < 255) {
            alp += 5;
            moneyTMP.color = new Color32(255, 227, 0, alp);
            yield return _waitForSeconds0001;
        } while(alp > 0) {
            alp -= 3;
            rect.anchoredPosition += Vector2.up * 2f;
            moneyTMP.color = new Color32(255, 227, 0, alp);
            yield return _waitForSeconds0001;
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
        GameManager.audioManager.ConfirmVolumeChange();
        Time.timeScale = 1;
        AlternateGameMenus(0);
    }

    // Function to quit the current round and return to the main menu.
    public void QuitGame() {
        if(difficulty > 0) { GameManager.dataManager.CashTransaction(moneyEarnt); }
        GameManager.audioManager.ConfirmVolumeChange();
        StopGameloop();
        Time.timeScale = 1;
        AlternateGameMenus(3);
        GameManager.dataManager.SaveData();
        StartCoroutine(GameManager.instance.LoadAsyncScene("MainMenu"));
    }
    
    public void AlternateGameMenus(int menu) {
        switch (menu) {
            case 0: { // Game UI
                gameUI.SetActive(true);
                pauseUI.SetActive(false);
                confirmUI.SetActive(false);
                break; }
            case 1: { // Game Over Screen
                endUI.SetActive(true);
                eventSystem.SetSelectedGameObject(endStartSelect);
                break; }
            case 2: { // Pause Menu
                pauseUI.SetActive(true);
                eventSystem.SetSelectedGameObject(pauseStartSelect);
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
        eventSystem.SetSelectedGameObject(confirmStartSelect);
    }
    
    // Funciton to carry out the appropiate UI response based on the confirmation response.
    public void MenuConfirmationResponse(bool response) {
        confirmUI.SetActive(false);
        if (!response) { AlternateGameMenus(2); }
        else { QuitGame(); }
    }
}