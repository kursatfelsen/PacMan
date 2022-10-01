using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class GameManager : MonoBehaviour
{
    public GameObject pacman;

    public GameObject LeftWarpNode;
    public GameObject RightWarpNode;
    public GameObject ghostNodeLeft;
    public GameObject ghostNodeRight;
    public GameObject ghostNodeCenter;
    public GameObject ghostNodeStart;

    public AudioSource siren;
    public AudioSource munch1;
    public AudioSource munch2;
    public AudioSource powerPelletAudio;
    public AudioSource respawningAudio;
    public AudioSource ghostEatenAudio;

    public int currentMunch = 0;

    public int score;
    public TextMeshProUGUI scoreText;
    public TextMeshProUGUI livesText;

    public bool newGame;
    public bool clearedLevel;

    public AudioSource startGameAudio;
    public AudioSource death;

    public int lives;
    public int currentLevel;

    public Image blackBackground;

    public TextMeshProUGUI gameOverText;

    public bool isPowerPelletRunning = false;
    public float currentPowerPelletTime = 0;
    public float powerPelletTimer = 8f;
    public int powerPelletMultiplier = 1;

    public enum GhostMode
    {
        chase, scatter
    }

    public GhostMode currentGhostMode;

    public int[] ghostModeTimers = new int[] {7, 20, 7, 20, 5, 20, 5};
    public int ghostModeTimerIndex;
    public float ghostModeTimer = 0;
    public bool runningTimer;
    public bool completedTimer;

    public GameObject blinky;
    public GameObject pinky;
    public GameObject inky;
    public GameObject clyde;

    public EnemyController blinkyController;
    public EnemyController pinkyController;
    public EnemyController inkyController;
    public EnemyController clydeController;

    public int totalPellets;
    public int pelletsLeft;
    public int pelletCollectedOnThisLife;

    public bool hadDeathOnThisLevel = false;

    public bool gameIsRunning;

    public List<NodeController> nodeControllers = new();

    public AudioSource extendAudio;
    public bool isExtendAudioActivated = false;

    // Start is called before the first frame update
    void Awake()
    {
        newGame = true;
        clearedLevel = false;
        blackBackground.enabled = false;
        isPowerPelletRunning = false;

        blinkyController = blinky.GetComponent<EnemyController>();
        pinkyController = pinky.GetComponent<EnemyController>();
        inkyController = inky.GetComponent<EnemyController>();
        clydeController = clyde.GetComponent<EnemyController>();

        ghostNodeStart.GetComponent<NodeController>().isGhostStartingNode = true;

        pacman = GameObject.Find("Player");

        StartCoroutine(Setup());
        
    }

    public IEnumerator Setup()
    {
        ghostModeTimerIndex = 0;
        ghostModeTimer = 0;
        completedTimer = false;
        runningTimer = true;
        gameOverText.enabled = false;
        powerPelletTimer = 8f;
        //If pacman clears a level, a background will appear covering the level, and the game will pause for 0.1 seconds
        if (clearedLevel)
        {
            blackBackground.enabled = true;
            yield return new WaitForSeconds(0.1f);
        }
        blackBackground.enabled = false;

        pelletCollectedOnThisLife = 0;
        currentGhostMode = GhostMode.scatter;
        gameIsRunning = false;
        currentMunch = 0;

        float waitTimer = 1f;

        if (clearedLevel || newGame)
        {
            pelletsLeft = totalPellets;
            waitTimer = 4f;
            //Pellets will respawn when Pacman clears the level or starts a new game
            for (int i = 0; i < nodeControllers.Count; i++)
            {
                nodeControllers[i].RespawnPellet();
            }
        }

        if (newGame)
        {
            startGameAudio.Play();
            score = 0;
            scoreText.text = "Score: " + score.ToString();
            SetLives(3);
            currentLevel = 1;
        }

        pacman.GetComponent<PlayerController>().Setup();

        blinkyController.Setup();
        pinkyController.Setup();
        inkyController.Setup();
        clydeController.Setup();

        newGame = false;
        clearedLevel = false;
        yield return new WaitForSeconds(waitTimer);

        StartGame();
    }

    void SetLives(int newLives)
    {
        lives = newLives;
        livesText.text = "Lives: " + lives.ToString();
    }

    public void StartGame()
    {
        gameIsRunning = true;
        siren.Play();
    }

    public void StopGame()
    {
        gameIsRunning = false;
        siren.Stop();
        powerPelletAudio.Stop();
        respawningAudio.Stop();
        pacman.GetComponent<PlayerController>().Stop();
    }

    public void PauseGame()
    {
        death.Pause();
        siren.Pause();
        munch1.Pause();
        munch2.Pause();
        powerPelletAudio.Pause();
        respawningAudio.Pause();
        ghostEatenAudio.Pause();
        startGameAudio.Pause();
        extendAudio.Pause();
        pacman.GetComponent<PlayerController>().Stop();
        gameIsRunning = false;
    }

    public void ResumeGame()
    {
        death.UnPause();
        siren.UnPause();
        munch1.UnPause();
        munch2.UnPause();
        powerPelletAudio.UnPause();
        respawningAudio.UnPause();
        ghostEatenAudio.UnPause();
        startGameAudio.UnPause();
        extendAudio.UnPause();
        gameIsRunning = true;
    }

    // Update is called once per frame
    void Update()
    {
        if (!gameIsRunning)
        {
            return;
        }

        if (blinkyController.ghostNodeState == EnemyController.GhostNodeState.respawning
            || inkyController.ghostNodeState == EnemyController.GhostNodeState.respawning
            || pinkyController.ghostNodeState == EnemyController.GhostNodeState.respawning
            || clydeController.ghostNodeState == EnemyController.GhostNodeState.respawning)
        {
            if (!respawningAudio.isPlaying)
            {
                respawningAudio.Play();
            }
        }
        else
        {
            if (respawningAudio.isPlaying)
            {
                respawningAudio.Stop();
            }
        }
        if (!completedTimer && runningTimer)
        {
            ghostModeTimer += Time.deltaTime;
            if (ghostModeTimer >= ghostModeTimers[ghostModeTimerIndex])
            {
                ghostModeTimer = 0;
                ghostModeTimerIndex++;
                if (currentGhostMode == GhostMode.chase)
                {
                    currentGhostMode = GhostMode.scatter;
                }
                else
                {
                    currentGhostMode = GhostMode.chase;
                }
                if (ghostModeTimerIndex == ghostModeTimers.Length)
                {
                    completedTimer = true;
                    runningTimer = false;
                    currentGhostMode = GhostMode.chase;
                }
            }
        }

        if (isPowerPelletRunning)
        {
            currentPowerPelletTime += Time.deltaTime;
            if (currentPowerPelletTime >= powerPelletTimer)
            {
                isPowerPelletRunning = false;
                currentPowerPelletTime = 0;
                powerPelletAudio.Stop();
                siren.Play();
                powerPelletMultiplier = 1;
            }
        }
    }

    public void GotPelletFromNodeController(NodeController nodeController)
    {
        nodeControllers.Add(nodeController);
        totalPellets++;
        pelletsLeft++;
    }

    public void AddToScore(int amount)
    {
        score += amount;
        scoreText.text = "Score: " + score.ToString();
        if (!isExtendAudioActivated && score >= 10000)
        {
            extendAudio.Play();
            isExtendAudioActivated = true;
        }
    }

    public IEnumerator CollectedPellet(NodeController nodeController)
    {
        switch (currentMunch)
        {
            case 0:
                munch1.Play();
                currentMunch = 1;
                break;
            case 1:
                munch2.Play();
                currentMunch = 0;
                break;
        }

        pelletsLeft--;
        pelletCollectedOnThisLife++;

        int requiredBluePellets = 0;
        int requiredOrangePellets = 0;

        if (hadDeathOnThisLevel)
        {
            requiredBluePellets = 12;
            requiredOrangePellets = 32;
        }
        else
        {
            requiredBluePellets = 30;
            requiredOrangePellets = 60;
        }

        if (pelletCollectedOnThisLife >= requiredBluePellets && !inky.GetComponent<EnemyController>().leftHomeBefore)
        {
            inky.GetComponent<EnemyController>().readyToLeaveHome = true;
        }
        if (pelletCollectedOnThisLife >= requiredOrangePellets && !clyde.GetComponent<EnemyController>().leftHomeBefore)
        {
            clyde.GetComponent<EnemyController>().readyToLeaveHome = true;
        }


        AddToScore(10);

        //Check if there are any pellets left
        if (pelletsLeft == 0)
        {
            currentLevel++;
            clearedLevel = true;
            StopGame();
            yield return new WaitForSeconds(1);
            StartCoroutine(Setup());
        }

        //Is this a power pellet
        if (nodeController.isPowerPellet)
        {
            siren.Stop();
            powerPelletAudio.Play();
            isPowerPelletRunning = true;
            currentPowerPelletTime = 0;

            blinkyController.SetFrightened(true);
            inkyController.SetFrightened(true);
            pinkyController.SetFrightened(true);
            clydeController.SetFrightened(true);
        }
    }

    public IEnumerator PauseGame(float timeToPause)
    {
        gameIsRunning = false;
        yield return new WaitForSeconds(timeToPause);
        gameIsRunning = true;
    }

    public void GhostEaten()
    {
        //ghostEatenAudio.Play();
        AddToScore(400 * powerPelletMultiplier);
        powerPelletMultiplier += 1;
        StartCoroutine(PauseGame(1));
    }

    public IEnumerator PlayerEaten()
    {
        hadDeathOnThisLevel = true;
        StopGame();
        yield return new WaitForSeconds(1);

        blinkyController.setVisible(false);
        pinkyController.setVisible(false);
        inkyController.setVisible(false);
        clydeController.setVisible(false);

        pacman.GetComponent<PlayerController>().Death();
        death.Play();
        yield return new WaitForSeconds(3);
        SetLives(lives-1);
        if (lives <= 0)
        {
            newGame = true;
            //Display gameover text
            gameOverText.enabled = true;

            yield return new WaitForSeconds(3);
        }
        StartCoroutine(Setup());
    }
}
