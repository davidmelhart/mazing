using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using UnityEngine;
using UnityEngine.SceneManagement;

public class ReportGenerator : MonoBehaviour {
    GameObject player;
    GameObject agent;
    GameObject cursor;
    LevelManager levelManager;
    Vector3 previousPlayerPosition;
    Vector3 previousAgentPosition;

    string playerID;
    public Queue<string> gameplayData;
    public Queue<List<string>> reportData;
    public int additionalItems = 0;
    public int sessionNumber = 1;
    public int allSessionNumber = 2;
    public GamePlaySession currentPlaySession;
    public string dataFolder;
    DateTime epochStart = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);

    public List<int> possibleLevels = new List<int>{1, 2, 3};

    bool recordGameplay = false;

    private void OnLevelWasLoaded(int level) {
        if (level == 0 || level == 1) {
            recordGameplay = false;
        }
        if (level == 2 || level == 3 || level == 4 || level == 5) {
            player = GameObject.Find("PlayerController");
            cursor = GameObject.Find("GunControls");
            agent = GameObject.Find("Monster");
            levelManager = GameObject.Find("LevelManager").GetComponent<LevelManager>();
            previousPlayerPosition = player.GetComponent<PlayerController>().transform.position;
        
            recordGameplay = true;
            currentPlaySession = new GamePlaySession();
            reportData = new Queue<List<string>>();

            FrustrationComponent agentFrustration = agent.GetComponent<FrustrationComponent>();
            currentPlaySession.startOfSession = DateTime.Now;
            currentPlaySession.frustration = agentFrustration.frustrationIsActive;
            currentPlaySession.frustrationLocked = agentFrustration.lockFrustration;
            currentPlaySession.frustrationRange = (agentFrustration.minFrustration + "-" + agentFrustration.maxFrustration);
            currentPlaySession.agentAvgFrustration = new List<float>();
            currentPlaySession.agentMoreFrustrated = 0;
            currentPlaySession.agentLessFrustrated = 0;
            currentPlaySession.agentBothFrustrated = 0;
            currentPlaySession.playerMoreFrustrated = 0;
            currentPlaySession.playerLessFrustrated = 0;
            currentPlaySession.playerBothFrustrated = 0;

            currentPlaySession.startByEpoch = (long)(DateTime.UtcNow - epochStart).TotalSeconds;
            currentPlaySession.levelID = SceneManager.GetActiveScene().name;
        }
        if (level == 6 || level == 7 || level == 8) {
            recordGameplay = false;
            currentPlaySession.endOfSession = DateTime.Now;            
        }
    }

    private void Awake() {
        DontDestroyOnLoad(transform.gameObject);
        playerID = CreateGuid();
        gameplayData = new Queue<string>();

        if (!Directory.Exists(AppDomain.CurrentDomain + "/../RecordedData")) { 
            Directory.CreateDirectory(AppDomain.CurrentDomain + "/../RecordedData");
            Debug.Log("Data Folder created at " + AppDomain.CurrentDomain + "/../RecordedData");
        }

        dataFolder = AppDomain.CurrentDomain + "/../RecordedData";
        Debug.Log("Data Folder accessed at " + dataFolder);
    }

    public void CompileReport () {
        string data = JsonUtility.ToJson(currentPlaySession, true);
        String fileName = String.Format("{0:yyyy_MMMM_dd_h_mm_ss}_GamePlayData.json", DateTime.Now);
        fileName = Path.Combine(dataFolder, fileName);
        File.WriteAllText(fileName, data);
    }

    [Serializable]
    public struct GamePlaySession {
        public string levelID;
        public bool frustration;
        public bool frustrationLocked;
        public string frustrationRange;
        public long startByEpoch;
        public DateTime startOfSession;
        public DateTime endOfSession;
        public int score;
        public int shotsFired;
        public int bombsDropped;
        public int healthLost;
        public float distanceTraveled;
        public int numberOfDashes;
        public float agentDistanceTraveled;
        public int agentDetectedPlayer;
        public int agentLostPlayer;
        public float timePlayerWasInSight;
        public float timeAgentWasAgressive;
        public List<float> agentAvgFrustration;
        public int agentHealthLost;
        public int agentDied;
        public int playerDied;
        public bool wonTheMatch; //depreciated
        public int agentMoreFrustrated;
        public int agentLessFrustrated;
        public int agentBothFrustrated;
        public int playerMoreFrustrated;
        public int playerLessFrustrated;
        public int playerBothFrustrated;
    }
    
    //Functions writing the gamedata files !!! Make sure that the order of the tags are matching the order of data points in the next function

    //Appends a session to the gameplayData queue but does not write it to file
        //This way multiple playsessions are gathered in the same file on a per player basis
    public void AppendSessionData() {
        List<string> rowData = new List<string>();
        rowData.Add(playerID);
        rowData.Add(currentPlaySession.levelID);
        rowData.Add(currentPlaySession.frustration.ToString());
        rowData.Add(currentPlaySession.frustrationRange.ToString());
        rowData.Add(currentPlaySession.startOfSession.ToString());
        rowData.Add(currentPlaySession.endOfSession.ToString());
        rowData.Add(currentPlaySession.endOfSession.Subtract(currentPlaySession.startOfSession).ToString());
        rowData.Add(currentPlaySession.score.ToString());
        rowData.Add(currentPlaySession.distanceTraveled.ToString());
        rowData.Add(currentPlaySession.shotsFired.ToString());
        rowData.Add(currentPlaySession.bombsDropped.ToString());
        rowData.Add(currentPlaySession.healthLost.ToString());
        rowData.Add(currentPlaySession.numberOfDashes.ToString());
        rowData.Add(currentPlaySession.playerDied.ToString());
        rowData.Add(currentPlaySession.agentDistanceTraveled.ToString());
        rowData.Add(currentPlaySession.agentDetectedPlayer.ToString());
        rowData.Add(currentPlaySession.agentLostPlayer.ToString());
        rowData.Add(currentPlaySession.timePlayerWasInSight.ToString());
        rowData.Add(currentPlaySession.timeAgentWasAgressive.ToString());
        rowData.Add(currentPlaySession.agentAvgFrustration.Average().ToString());
        rowData.Add(currentPlaySession.agentHealthLost.ToString());
        rowData.Add(currentPlaySession.agentDied.ToString());
        rowData.Add(currentPlaySession.agentMoreFrustrated.ToString());
        rowData.Add(currentPlaySession.agentLessFrustrated.ToString());
        rowData.Add(currentPlaySession.agentBothFrustrated.ToString());
        rowData.Add(currentPlaySession.playerMoreFrustrated.ToString());
        rowData.Add(currentPlaySession.playerLessFrustrated.ToString());
        rowData.Add(currentPlaySession.playerBothFrustrated.ToString());

        string dataLine = string.Join(",", rowData.ToArray());
        gameplayData.Enqueue(dataLine);
    }

    public void WriteGamePlayData() {
        AppendSessionData();

        StringBuilder finalData = new StringBuilder();

        List<string> titleLine = new List<string>();
        titleLine.Add("PlayerID");
        titleLine.Add("LevelID");
        titleLine.Add("FrustrationActive");
        titleLine.Add("FrustrationRange");
        titleLine.Add("StartOfSession");
        titleLine.Add("EndOfSession");
        titleLine.Add("PlayTime");
        titleLine.Add("Score");
        titleLine.Add("PlayerDistanceTraveled");
        titleLine.Add("ShotsFired");
        titleLine.Add("BombsDropped");
        titleLine.Add("PlayerHealthLost");
        titleLine.Add("PlayerNumberOfDashes");
        titleLine.Add("PlayerDied");
        titleLine.Add("AgentDistanceTraveled");
        titleLine.Add("PlayerDetected");
        titleLine.Add("PlayerLost");
        titleLine.Add("TimePlayerWasInSight");
        titleLine.Add("TimeSpentChasingPlayer(ms)");
        titleLine.Add("AgentAvgFrustration");
        titleLine.Add("AgentHealthLost");
        titleLine.Add("AgentDied");
        titleLine.Add("Agent-MoreFrustratedThanPrevious");
        titleLine.Add("Agent-LessFrustratedThenPrevious");
        titleLine.Add("Agent-BothFrustrated");
        titleLine.Add("Player-MoreFrustratedThanPrevious");
        titleLine.Add("Player-LessFrustratedThenPrevious");
        titleLine.Add("Player-BothFrustrated");
        string firstLine = string.Join(",", titleLine.ToArray());
        finalData.AppendLine(firstLine);

        while(gameplayData.Count > 0) {
            string sessionData = gameplayData.Dequeue();
            finalData.AppendLine(sessionData);
        }
        String fileName = String.Format(playerID + "_{0:yyyy-MM-dd}_GamePlaySummary.csv", DateTime.Now);
        fileName = Path.Combine(dataFolder, fileName);

        File.WriteAllText(fileName, finalData.ToString());
    }

    //Functions writing the gamedata files !!! Make sure that the order of the tags are matching the order of data points in the next function

    //Update function for monitoring and logging the gamestate
        //Contains helper lines for logging the summarized gameplay data
    private void FixedUpdate() {
        if (recordGameplay) {
            //Logs distance traveled for the gameplay summary
            currentPlaySession.distanceTraveled += (player.GetComponent<PlayerController>().transform.position - previousPlayerPosition).magnitude;
            previousPlayerPosition = player.GetComponent<PlayerController>().transform.position;

            currentPlaySession.agentDistanceTraveled += (agent.transform.position - previousAgentPosition).magnitude;
            previousAgentPosition = agent.transform.position;

            //Logs time spent on chasing the player
            bool agentInChase = false;
            bool targetSeen = false;
            if (!agent.Equals(null)) {
                targetSeen = (agent.GetComponent<ProximityDetector>().playerDetected || agent.GetComponent<FieldOfView>().targetDetected);
                agentInChase = agent.GetComponent<Movement>().targetLastSeen;
                if (agentInChase) {
                    currentPlaySession.timeAgentWasAgressive += Time.deltaTime;
                }
                if (targetSeen) {
                    currentPlaySession.timePlayerWasInSight += Time.deltaTime;
                }
            }            
            
            //Construct row for report data
            List<string> rowData = new List<string>();
            long epochTime = currentPlaySession.startByEpoch * 1000;
            int timeStep = Mathf.RoundToInt(Time.timeSinceLevelLoad * 1000);

            rowData.Add((epochTime + timeStep).ToString());                                             //TimeStamp
            rowData.Add(levelManager.score.ToString());                                                 //CurrentScore
            if (!agent.Equals(null)) {
                rowData.Add(agent.transform.position.x.ToString());                                     //AgentPositionX
                rowData.Add(agent.transform.position.y.ToString());                                     //AgentPositionY
                rowData.Add(agent.transform.eulerAngles.z.ToString());                                  //AgentRotation
                rowData.Add(agent.GetComponent<Movement>().speed.ToString());                           //AgentSpeed
                rowData.Add(agent.GetComponent<Movement>().rotationSpeed.ToString());                   //AgentRotationSpeed
                rowData.Add(agent.GetComponent<FieldOfView>().viewAngle.ToString());                    //AgentViewAngle
                rowData.Add(agent.GetComponent<FieldOfView>().viewRadius.ToString());                   //AgentViewRadius
                rowData.Add((agent.GetComponent<Movement>().isWaiting ? 1 : 0).ToString());             //AgentSearching
                rowData.Add(agent.GetComponent<Movement>().lookAroundCycle.ToString());                 //AgentSearchLength(turn#)
                rowData.Add(agent.GetComponent<ProximityDetector>().hearingRadius.ToString());          //AgentHearingRadius
                rowData.Add(agent.GetComponent<ProximityDetector>().detectionChance.ToString());        //AgentHearingProbability
                rowData.Add(agent.GetComponent<Health>().health.ToString());                            //AgentHealth
                rowData.Add(agent.GetComponent<FrustrationComponent>().levelOfFrustration.ToString());  //AgentFrustration
                rowData.Add(agent.GetComponent<Movement>().riskTakingFactor.ToString());                //AgentRiskTakingFactor
                rowData.Add((agent.GetComponent<Movement>().takingRiskyPath ? 1 : 0).ToString());       //AgentTakingRiskyPath
                rowData.Add((targetSeen ? 1 : 0).ToString());                                           //AgentSeeingPlayer
                rowData.Add((agentInChase ? 1 : 0).ToString());                                         //AgentChasingPlayer
            } else {
                for (int i=0; i < 17; i++) {
                    rowData.Add("%AD%");    //Tag for "Agent Down". Still records the last actions of the Player before end level
                }
            }
            if (!agent.Equals(null) && !player.Equals(null)) {                                          //AgentDistanceFromPlayer
                rowData.Add((new Vector2(agent.transform.position.x, agent.transform.position.y) - 
                             new Vector2(player.transform.position.x, player.transform.position.y)).magnitude.ToString());
            } else {
                rowData.Add("%GO%");        //Tag for "Game Over". Still records inputs from the Player before end level
            }
            if (!player.Equals(null)) {
                rowData.Add(player.GetComponent<PlayerController>().transform.position.x.ToString());   //PlayerPositionX
                rowData.Add(player.GetComponent<PlayerController>().transform.position.y.ToString());   //PlayerPositionY
                rowData.Add(player.transform.eulerAngles.z.ToString());                                 //PlayerRotation
                rowData.Add(player.GetComponent<PlayerHealth>().health.ToString());                     //PlayerHealth
                rowData.Add((player.GetComponent<PlayerController>().dashing ? 1 : 0).ToString());      //PlayerIsDashing
                rowData.Add((((Input.GetKeyDown(KeyCode.LeftShift) ||
                          Input.GetKeyDown(KeyCode.RightShift) ||
                          Input.GetKeyDown(KeyCode.Space)) && 
                          player.GetComponent<PlayerController>().dashOnCD) ? 1 : 0).ToString());       //PlayerTriesDashOnCD

            } else {
                for (int i = 0; i < 5; i++) {
                    rowData.Add("%PD%");    //Tag for "Player Down". Still records inputs from the Player before end level
                }
            }
            rowData.Add(((Input.GetKeyDown(KeyCode.LeftShift) || 
                          Input.GetKeyDown(KeyCode.RightShift) || 
                          Input.GetKeyDown(KeyCode.Space)) ? 1 : 0).ToString());                        //DashPressed
            rowData.Add(cursor.transform.position.x.ToString());                                        //CursorPositionX
            rowData.Add(cursor.transform.position.y.ToString());                                        //CursorPositionY
            rowData.Add((Input.GetMouseButton(0) &&
                         cursor.GetComponent<GunControls>().projectileCount == 0 ? 1 : 0).ToString());  //PlayerTriesToFireOnCD
            rowData.Add((Input.GetMouseButtonUp(1) &&
                         cursor.GetComponent<GunControls>().bombCount == 0 ? 1 : 0).ToString());        //PlayerTriesToBombOnCD
            rowData.Add((Input.GetMouseButton(0) && 
                         !cursor.GetComponent<GunControls>().reloading ? 1 : 0).ToString());            //ShotsFired
            rowData.Add((Input.GetMouseButtonUp(1) && 
                         !cursor.GetComponent<GunControls>().bombReloading ? 1 : 0).ToString());        //BombsDropped

            GameObject[] currentFires = GameObject.FindGameObjectsWithTag("fire");
            int additonalItemsSpawned = currentFires.Length;

            if (additonalItemsSpawned > additionalItems) {
                additionalItems = additonalItemsSpawned;
            }

            rowData.Add(currentFires.Length.ToString());                                                //NumberOfActiveFires

            for (int i = 0; i < currentFires.Length; i++) {                                             //PositionOfActiveFires
                rowData.Add(currentFires[i].transform.position.x.ToString());
                rowData.Add(currentFires[i].transform.position.y.ToString());
            }

            reportData.Enqueue(rowData);
        }  
    }

    public void WriteGamePlayRecording() {
        if (reportData.Count > 0) {
            StringBuilder finalData = new StringBuilder();

            List<string> titleLine = new List<string>();
            titleLine.Add("TimeStamp");
            titleLine.Add("Score");
            titleLine.Add("AgentPositionX");
            titleLine.Add("AgentPositionY");
            titleLine.Add("AgentRotation");
            titleLine.Add("AgentSpeed");
            titleLine.Add("AgentRotationSpeed");
            titleLine.Add("AgentViewAngle");
            titleLine.Add("AgentViewRadius");
            titleLine.Add("AgentSearching");
            titleLine.Add("AgentSearchLength(turn No)");
            titleLine.Add("AgentHearingRadius");
            titleLine.Add("AgentHearingProbablility");
            titleLine.Add("AgentHealth");
            titleLine.Add("AgentFrustration");
            titleLine.Add("AgentRiskTakingFactor");
            titleLine.Add("AgentTakingRiskyPath");
            titleLine.Add("AgentSeeingPlayer");
            titleLine.Add("AgentChasingPlayer");
            titleLine.Add("AgentDistanceFromPlayer");
            titleLine.Add("PlayerPositionX");
            titleLine.Add("PlayerPositionY");
            titleLine.Add("PlayerRotation");
            titleLine.Add("PlayerHealth");
            titleLine.Add("PlayerIsDashing");
            titleLine.Add("PlayerTriesDashOnCD");
            titleLine.Add("DashPressed");
            titleLine.Add("CursorPositionX");
            titleLine.Add("CursorPositionY");
            titleLine.Add("PlayerTriesToFireOnCD");
            titleLine.Add("PlayerTriesToBombOnCD");
            titleLine.Add("ShotFired");
            titleLine.Add("BombDropped");
            titleLine.Add("NumberOfActiveFires");

            for (int i = 0; i < additionalItems; i++) {
                titleLine.Add("ActiveFire#" + (i + 1) + " PositionX");
                titleLine.Add("ActiveFire#" + (i + 1) + " PositionY");
            }

            string epochStamp = currentPlaySession.startByEpoch.ToString();
            finalData.AppendLine(epochStamp + ',' + currentPlaySession.frustration + "," + currentPlaySession.frustrationRange);

            string firstLine = string.Join(",", titleLine.ToArray());
            finalData.AppendLine(firstLine);

            while (reportData.Count > 0) {
                List<string> rowData = reportData.Dequeue();
                string newLine = string.Join(",", rowData.ToArray());
                finalData.AppendLine(newLine);
            }

            String folderName = String.Format(playerID + "_{0:yyyy-MM-dd}_Raw", currentPlaySession.startOfSession);
            Directory.CreateDirectory(AppDomain.CurrentDomain + "/../RecordedData/" + folderName);
            Debug.Log("Folder created at " + AppDomain.CurrentDomain + "/../RecordedData" + folderName);
            string currentDataFolder = AppDomain.CurrentDomain + "/../RecordedData/"  + folderName;

            //String fileName = String.Format(playerID + "_{0:yyyy-MM-dd}_GamePlayRecording" + sessionNumber + "_frustration-" + currentPlaySession.frustration + "_" + currentPlaySession.frustrationRange + ".csv", currentPlaySession.startOfSession);
            String fileName = String.Format("GamePlay" + sessionNumber + ".csv");
            fileName = Path.Combine(currentDataFolder, fileName);
            sessionNumber++;

            File.WriteAllText(fileName, finalData.ToString());
        }
    }

    //Creates a relatively short GUID (not really global)
    public string CreateGuid() {
        Guid guid = Guid.Empty;
        while (Guid.Empty == guid) {
            guid = Guid.NewGuid();
        }
        
        string id = Convert.ToBase64String(guid.ToByteArray());
        id.Remove(id.Length - 2, 2);
        id = Regex.Replace(id, @"[^0-9a-zA-Z]+", "0");

        return id;
    }   
}
