using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;
using UnityEngine.EventSystems;
using TMPro;
using System;

// Script to handle main game functionality.
public class GameplayManager : MonoBehaviour {
    #region Variables

    [Header ("Player Input")]
    [SerializeField] private InputAction pauseAction;

    [Header ("Game Variables")]
    [SerializeField] private int startingTime; 
    private int timeLeft, deliveryTime, difficulty, deliveryPayment, moneyEarnt;
    private float penaltyMult, incomeMult;

    [Header ("Game Objects")] 
    [SerializeField] private GameObject directionArrow;
    [SerializeField] private GameObject moneyText;
    private GameObject player;

    [Header ("Sound Effects")]
    [SerializeField] private AudioClip countSound;
    [SerializeField] private AudioClip overtimeSound;

    [Header ("UI Canvases")]
    [SerializeField] private GameObject mainUI;
    [SerializeField] private GameObject regularUI;
    [SerializeField] private GameObject regularEndUI;
    [SerializeField] private GameObject bossUI;
    [SerializeField] private GameObject bossEndUI;
    [SerializeField] private GameObject pauseUI;
    [SerializeField] private GameObject confirmUI;
    [SerializeField] private GameObject pauseStartSelect;
    [SerializeField] private GameObject confirmStartSelect;
    private GameObject gameUI, endUI;

    [Header ("UI Elements")]
    [SerializeField] private Image timerImage;
    [SerializeField] private TMP_Text timerText;
    [SerializeField] private Animator timeAnimator;
    [SerializeField] private TMP_Text confirmText;

    private bool isPlaying = false;
    private bool isGamePaused = false;
    private bool secondLife = false; 
    private EventSystem eventSystem;

    private static readonly int HighTimeHash = Animator.StringToHash("highTime");
    private static readonly int LowTimeHash = Animator.StringToHash("lowTime");
    private static readonly WaitForSeconds _waitForSeconds1 = new(1);
    private static readonly WaitForSeconds _waitForSeconds001 = new(0.01f);
    private static readonly WaitForSeconds _waitForSeconds0001 = new(0.001f);
    private Coroutine bossTimerCoroutine;

    #endregion

    #region Handler Functions

    void OnEnable() { 
        pauseAction.Enable();
        eventSystem = EventSystem.current;    
    }

    void OnDisable() { pauseAction.Disable(); }

    void Awake() { GameManager.gameplayManager = this; }

    // Start is called before the first frame update.
    void Start() {
        // Check for Upgrades.
        penaltyMult = GameManager.dataManager.IsUpgraded("noPenalty")? 0f: 1f;
        incomeMult = GameManager.dataManager.IsUpgraded("moreMoney")? 1.5f: 1f;
        secondLife = GameManager.dataManager.IsUpgraded("secondLife");

        // Get Player Game Object.
        player = GameObject.FindWithTag("Player");

        moneyEarnt = 0;

        // Set Difficulty based on user selection, hide the timer UI in the tutorial
        difficulty = GameManager.instance.GetDifficulty();
        timerImage.enabled = difficulty != 0;
        timerText.enabled = difficulty != 0;

        switch (difficulty) {
            case 0:
                gameUI = regularUI;
                endUI = regularEndUI;
                timerImage.enabled = false;
                timerText.enabled = false;
                break;
            case 1:
                gameUI = regularUI;
                endUI = regularEndUI;
                timerImage.enabled = true;
                timerText.enabled = true;
                GameManager.obstacleManager.SpawnStartingObstacles();
                break;
            case 2:
                gameUI = bossUI;
                endUI = bossEndUI;
                GameManager.obstacleManager.SpawnStartingObstacles();
                break;
        }

        AlternateGameMenus(0);

        // Initialize the player van.
        player.GetComponent<PlayerControl>().SetState(true);

        // Start News Text
        GameManager.newsTextScroller.StartNews();

        // Start timer and begin game. 
        Time.timeScale = 1;
        isPlaying = true;
        if (difficulty == 1) { 
            int startTime = startingTime;
            startTime += GameManager.dataManager.IsUpgraded("moreTime")? 20: 0;
            SetTime(startTime, false);
            StartCoroutine(GameTimer());
        }
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
            Vector3 direction = GameManager.deliveryManager.GetCurrentPosition() - player.transform.position; 
            Quaternion targetRotation = Quaternion.LookRotation(direction, Vector3.up);
            directionArrow.transform.rotation = Quaternion.Slerp(directionArrow.transform.rotation, targetRotation, Time.deltaTime * 5f);
        }
    }

    #endregion

    #region Game State Functions

    // Function for when the player runs out of time.
    public void GameOver() {
        // Stop the game.
        StopGameloop();

        // Display game over screen.
        AlternateGameMenus(1);
        StartCoroutine(GameOverFade());

        // Stop game music and play game over music.
        StartCoroutine(GameManager.audioManager.EndGameMusic());

        endUI.transform.Find("Menu Button").gameObject.SetActive(true);
    }

    public void BossGameOver(int winner) {
        // Stop the game.
        StopGameloop();
        ResetBossTimer();

        // Display game over screen.
        AlternateGameMenus(1);
        endUI.transform.Find("Win (TMP)").gameObject.SetActive(winner == 0);
        endUI.transform.Find("Lose (TMP)").gameObject.SetActive(winner == 1);
        StartCoroutine(GameOverFade());

        // Stop game music and play game over music.
        StartCoroutine(GameManager.audioManager.EndBossMusic(winner));

        endUI.transform.Find("Menu Button").gameObject.SetActive(true);
    }

    // Function to stop gameplay when the game ends.
    public void StopGameloop() {
        // Stop gameplay loop and timer.
        isPlaying = false;

        // Stop van controls.
        player.GetComponent<PlayerControl>().SetState(false);

        // Stop News Text UI
        GameManager.newsTextScroller.StopNews();
    }

    public bool IsPlaying() => isPlaying;

    #endregion

    #region Game Loop Functions

    // Coroutine to decrement the game timer every second.
    private IEnumerator GameTimer() {
        // Only decrement while there is time left, and a game is in session.
        while (timeLeft > 0 && isPlaying) {
            yield return _waitForSeconds1; // Wait one second before decrementing time.
            SetTime(-1, true);
            deliveryTime++;
        }
    }

    private IEnumerator BossGameTimer(int bossDeliveryTime) {
        SetTime(bossDeliveryTime, false);
        while (timeLeft > 0) {
            yield return _waitForSeconds1; // Wait one second before decrementing time.
            SetTime(-1, true);
        }
    }

    public void StartBossTimer(int bossDeliveryTime) {
        if (bossTimerCoroutine != null) { StopCoroutine(bossTimerCoroutine); }
        bossTimerCoroutine = StartCoroutine(BossGameTimer(bossDeliveryTime));
    }

    public void ResetBossTimer() {
        if (bossTimerCoroutine != null) {
            StopCoroutine(bossTimerCoroutine);
            bossTimerCoroutine = null;
        } SetTime(0, false);
    }

    // Setter Method for the timer, also updates the UI and checks if time has ran out.
    public void SetTime(int value, bool addingTime) {
        if (addingTime) {
            if (value < 0 && timeLeft == 1 && secondLife) { 
                GameManager.audioManager.PlaySoundEffect(overtimeSound);
                secondLife = false;
                value = 30;
            }
            
            timeLeft += value;

            // Only in Regular Game Mode.
            if (difficulty == 1) {
                // "Time to Spare" Achievement
                if (value > 0 && timeLeft >= 120) { GameManager.dataManager.CompleteAchievement("timer120"); }

                // Exit "Almost out of Time" Phase.
                if (value > 0 && timeLeft >= 10) { 
                    GameManager.audioManager.SetMusicPitch(1f);
                    TimerAnimation("highTime"); 
                }

                // "Almost out of Time" Phase.
                if (value < 0 && timeLeft <= 10) { 
                    GameManager.audioManager.SetMusicPitch(1f + (11f - timeLeft)/10f);
                    TimerAnimation("lowTime"); 
                }

                if (timeLeft == 0) { GameOver(); }
            }

            // Player/Boss didn't deliver Parcel in Time. 
            else if (difficulty == 2 && timeLeft == 0) { 
                GameManager.deliveryManager.ChangeState(0); 
                timerText.text = "";
            }
        } else { timeLeft = value; }
        timerText.text = timeLeft.ToString();
        if (difficulty == 2 && timeLeft == 0) { timerText.text = ""; }
    }

    public void MoneyScore(int completeDeliveries) {
        CalculateEarnings(completeDeliveries);
        StartCoroutine(MoneyDisplay(deliveryPayment));
    }

    private void CalculateEarnings(int completeDeliveries) {
        int income = 29 + (int) Math.Pow(1.2, completeDeliveries);
        double timePenalty = Math.Min(0.25, deliveryTime/100.0) * income;
        deliveryPayment = (int) (income * incomeMult) - (int) (timePenalty * penaltyMult);
        moneyEarnt += deliveryPayment;
        deliveryTime = 0;
    }

    public GameObject GetPlayer() => player;

    public Vector3 FindPlayer() => player.transform.position;

    #endregion

    #region UI Functions


    public void TimerAnimation(string trigger) {
        timeAnimator.ResetTrigger(LowTimeHash);
        timeAnimator.ResetTrigger(HighTimeHash);
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
        if (difficulty == 1) StartCoroutine(MoneyCount());
    }

    private IEnumerator MoneyCount() {
        GameObject counter = endUI.transform.Find("Counter").gameObject;
        counter.SetActive(true);
        TMP_Text counterText = counter.transform.Find("Amount").GetComponent<TMP_Text>();
        int display = 0;
        while (display < moneyEarnt) {
            display++; 
            counterText.text = display.ToString(); 
            if (display % 10 == 0) { GameManager.audioManager.PlaySoundEffect(countSound); }
            yield return _waitForSeconds0001;
        } yield return _waitForSeconds1;
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
        player.GetComponent<PlayerControl>().SetState(false);
        GameManager.audioManager.TogglePause(true);
        Time.timeScale = 0;
        AlternateGameMenus(2);
    }

    // Function to resume the game from the pause menu.
    public void ResumeGame() {
        isGamePaused = false;
        player.GetComponent<PlayerControl>().SetState(true);
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
                mainUI.SetActive(true);
                gameUI.SetActive(true);
                pauseUI.SetActive(false);
                confirmUI.SetActive(false);
                break; }
            case 1: { // Game Over Screen
                endUI.SetActive(true);
                eventSystem.SetSelectedGameObject(endUI.transform.Find("Menu Button").gameObject);
                break; }
            case 2: { // Pause Menu
                pauseUI.SetActive(true);
                eventSystem.SetSelectedGameObject(pauseStartSelect);
                break; }
            case 3: { // Loading Screen
                mainUI.SetActive(false);
                gameUI.SetActive(false);
                endUI.SetActive(false);
                pauseUI.SetActive(false);
                Instantiate(Resources.Load<GameObject>("LoadingScreen"));
                break; }    
        }
    }

    // Function to ask the user to confirm their choice on an important UI choice.
    public void MenuConfirmationMessage(int cID) {
        //confirmationUIID = cID
        pauseUI.SetActive(false);
        if (difficulty == 0) { confirmText.text = "end the tutorial\nand return to the menu?"; }
        else { confirmText.text = "end the game\nand return to menu?"; }
        confirmUI.SetActive(true);
        eventSystem.SetSelectedGameObject(confirmStartSelect);
    }
    
    // Funciton to carry out the appropiate UI response based on the confirmation response.
    public void MenuConfirmationResponse(bool response) {
        confirmUI.SetActive(false);
        if (!response) { AlternateGameMenus(2); }
        else { QuitGame(); }
    }

    #endregion
}