using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine.Events;

public class LevelManager : MonoBehaviour {    
    public bool moderatorActive;
    public bool doCountDown;
    public bool silentCountDown;
    public bool warnStart;
    public float timeLimit;
    private float countdown;
    private Text timer;

    public bool doScore;
    [HideInInspector]
    public int score;
    private Text scoreBoard;

    //For Level Reset
    [HideInInspector]
    public Vector3 agentStartPosition;
    [HideInInspector]
    public float agentStartFrustration;
    [HideInInspector]
    public Vector3 playerStartPosition;
    GameObject agent;
    GameObject player;

    //Game Events
    public UnityEvent OnGameStart;
    public UnityEvent OnGameEnd;
    private bool gameEnded = false;
    private bool gameStarted = false;

    //FPS Counter
    private int fps = 0;
    private float fpsTimer;
    public float fpsRefreshRate = 1f;
    public Text fpsText;
    public int fpsLimit;
    private Color fpsIndicatorColor;
    private Color fpsWarningColor = new Color(0.9f, 0, 0.2f, 1);

    private void Awake() {        
        if(doCountDown) {
            if (!silentCountDown) {
                timer = GameObject.Find("Timer").GetComponent<Text>();
            }
            countdown = timeLimit;
        }
        if (doScore) {
            scoreBoard = GameObject.Find("Score").GetComponent<Text>();
        }
        agent = GameObject.Find("Monster");
        player = GameObject.Find("PlayerController");

        if (agent != null) {
            agentStartFrustration = agent.GetComponent<FrustrationComponent>().levelOfFrustration;
        }

        if (player != null) {
            playerStartPosition = player.transform.position;
        }
    }

    private void Start() {
        fpsIndicatorColor = fpsText.color;

        if (!gameStarted) {
            gameStarted = true;
            StartCoroutine(LateStart());
            if (warnStart) {
                StartCoroutine(RemindStart());
            }
        }

        //Make the cursor invisible.
        Cursor.SetCursor(null, Vector2.zero, CursorMode.ForceSoftware);
        Cursor.visible = false;
    }

    IEnumerator LateStart() {
        yield return new WaitForFixedUpdate();
        OnGameStart.Invoke();
        yield return null;
    }

    IEnumerator RemindStart() {
        yield return new WaitForSeconds(30);
        Button startButton = GameObject.Find("Start").GetComponent<Button>();
        ColorBlock cb = startButton.colors;
        cb.normalColor = fpsWarningColor;
        startButton.colors = cb;
        yield return null;
    }

    private void Update() {
        if (Time.unscaledTime > fpsTimer) {
            fps = (int)(1f / Time.unscaledDeltaTime);
            fpsText.text = "FPS: " + fps;
            fpsTimer = Time.unscaledTime + fpsRefreshRate;
            
            // Gives 3 seconds for the game to stabilize
            if (fps < fpsLimit && Time.timeSinceLevelLoad > 3f) {
                fpsText.color = fpsWarningColor;
            } else {
                fpsText.color = fpsIndicatorColor;
            }
        }
    }

    public void ResetStage(int reward) {
        GameObject[] currentBombs = GameObject.FindGameObjectsWithTag("bomb");
        for (int i = 0; i < currentBombs.Length; i++) {
            //Defusing bombs before destroy prevents them spawing fires
            currentBombs[i].GetComponent<BombBehavior>().fuse = false;
            Destroy(currentBombs[i]);
        }
        GameObject[] currentProjectiles = GameObject.FindGameObjectsWithTag("projectile");
        for (int i = 0; i < currentProjectiles.Length; i++) {
            Destroy(currentProjectiles[i]);
        }
        GameObject[] currentFires = GameObject.FindGameObjectsWithTag("fire");
        for (int i = 0; i < currentFires.Length; i++) {
            Destroy(currentFires[i]);
        }

        player.GetComponent<PlayerHealth>().health = 100;
        agent.GetComponent<Health>().health = 100;
        agent.GetComponent<FrustrationComponent>().levelOfFrustration = agentStartFrustration;

        if (GameObject.Find("playerDestroyEffect(Clone)") == null) {
            Instantiate(player.GetComponent<PlayerHealth>().destroyEffect, new Vector3(player.transform.position.x, player.transform.position.y, -1), player.transform.rotation);
            score += reward;
        }

        player.transform.position = playerStartPosition;
        if (GameObject.Find("agentDestroyEffect(Clone)") == null) {
            Instantiate(agent.GetComponent<Health>().destroyEffect, new Vector3(agent.transform.position.x, agent.transform.position.y, -1), agent.transform.rotation);
        }

        agent.transform.position = agentStartPosition;
        Movement agentStatus = agent.GetComponent<Movement>();
        agentStatus.shouldLookAround = true;
        agentStatus.shouldWait = true;
        agentStatus.hasTarget = false;
        agentStatus.targetLastSeen = false;
        agentStatus.hitRegistered = false;
        agentStatus.takingRiskyPath = false;
        agentStatus.breakBeingStuck = false;
        agentStatus.fleeing = false;
        agentStatus.searching = false;
        agent.GetComponent<FieldOfView>().targetDetected = false;
        agent.GetComponent<ProximityDetector>().playerDetected = false;
        agentStatus.lastVisibleTargetPosition = agentStartPosition;
        //Dirty workaround to stop the agent from following the previous path. 
            //For some reason the coroutine started by the same expression was not able to stop, so just call a new path request that instantly exits.
        PathRequestManager.RequestPath(transform.position, agentStartPosition, agentStatus.OnPathFound);
    }

    private void FixedUpdate() {
        if (doCountDown) {
            countdown -= Time.deltaTime;
            if (countdown < 0) {
                if (!gameEnded) {
                    gameEnded = true;
                    OnGameEnd.Invoke();
                }
                
                //LoadSurvey();
                //LoadEndScreen();
            }
            if (!silentCountDown) {
                timer.text = Mathf.RoundToInt(Mathf.Clamp(countdown, 0, timeLimit)) > 9 ?
                             Mathf.RoundToInt(Mathf.Clamp(countdown, 0, timeLimit)).ToString() :
                             "0" + Mathf.RoundToInt(Mathf.Clamp(countdown, 0, timeLimit)).ToString();
            }
        }
        if (doScore) {
            score = Mathf.Max(0, score);
            scoreBoard.text = score.ToString() + " SCORE";

        }
    }

    public void LoadLevel(string name) {
        // Moderator is a research assistant who has to hold down Ctrl for key commands to work.
        if (moderatorActive) {
            if (Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl)) {
                SceneManager.LoadScene(name);
            }
        }
        if (!moderatorActive || (moderatorActive && name == "Tutorial")) {
            SceneManager.LoadScene(name);
        }
    }
}