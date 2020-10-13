using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class LevelManager : MonoBehaviour {
    ReportGenerator reportGenerator;
    public bool moderatorActive;
    public bool doCountDown;
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


    private void Awake() {
        reportGenerator = GameObject.Find("ReportGenerator").GetComponent<ReportGenerator>();
        if(doCountDown) {
            timer = GameObject.Find("Timer").GetComponent<Text>();
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
                LoadSurvey();
                //LoadEndScreen();
            }

            timer.text = Mathf.RoundToInt(countdown) > 9 ? 
                         Mathf.RoundToInt(countdown).ToString() : 
                         "0" + Mathf.RoundToInt(countdown).ToString();
        }
        if (doScore) {
            score = Mathf.Max(0, score);
            scoreBoard.text = score.ToString() + " SCORE";
            reportGenerator.currentPlaySession.score = score;
        }
    }

    public void LoadLevel(string name) {
        if (moderatorActive) {
            if (Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl)) {
                SceneManager.LoadScene(name);
            }
        }
        if (!moderatorActive || (moderatorActive && name == "Tutorial")) {
            SceneManager.LoadScene(name);
        }
    }

    public void LoadSurvey() {
        if (reportGenerator.sessionNumber > 1) {
            SceneManager.LoadScene("Survey");
        } else {
            LoadEndScreen();
        }
    }

    public void LoadEndScreen() {
        if (reportGenerator.allSessionNumber > reportGenerator.sessionNumber) {
            SceneManager.LoadScene("EndLevel");
        } else {
            SceneManager.LoadScene("EndGame");
        }
    }

    public void LoadNextLevel() {
        //Prevents subjects accidentally starting the next playsession
        // Hold CTRL to activate "Next Level" button
        if (moderatorActive) {
            if (Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl)) {
                int thisLevel = int.Parse(reportGenerator.currentPlaySession.levelID.Replace("Level - ", ""));
                SceneManager.LoadScene("Level - " + GetRandomLevel());
            }
        } 

        if (!moderatorActive) {
            int thisLevel = int.Parse(reportGenerator.currentPlaySession.levelID.Replace("Level - ", ""));
            SceneManager.LoadScene("Level - " + GetRandomLevel());
        }
    }

    public void LoadRandomLevel() {
        SceneManager.LoadScene("Level - " + GetRandomLevel());
    }

    public void QuitRequest(string name) {
        Application.Quit();
    }

    int GetRandomLevel() {
        int randomIndex = new System.Random().Next(0, reportGenerator.possibleLevels.Count);
        int nextLevel = reportGenerator.possibleLevels[randomIndex];
        reportGenerator.possibleLevels.RemoveAt(randomIndex);
        return nextLevel;
    }
}