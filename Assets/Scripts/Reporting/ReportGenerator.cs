using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.SceneManagement;

public class ReportGenerator : MonoBehaviour {
    private GameObject player;
    private GameObject bot;
    private FrustrationComponent botFrustrationComponent;
    private GameObject cursor;
    private LevelManager levelManager;
    private Vector3 previousPlayerPosition;
    private Vector3 previousBotPosition;
    private Vector3 previousCursorPosition;
    private WWWForm form;

    public string url;
    private float t = 0.0f;
    public float period;

    public bool recordGameplay;
    private bool canRecord = false;

    private int tick;
    private int idleTime;
    private int fullTime;

    private bool botFrustrationActive;
    private bool botFrustrationLocked;
    private string botFrustrationRange;
    private List<int> score;
    private List<float> botDistanceTraveled;
    private List<float> botPositionX;
    private List<float> botPositionY;
    private List<float> botRotation;
    private List<float> botSpeed;
    private List<float> botRotationSpeed;
    private List<float> botViewAngle;
    private List<float> botViewRadius;
    private List<int> botSearching;
    private List<int> botSearchTurns;
    private List<float> botHearingRadius;
    private List<float> botHearingProbability;
    private List<float> botHealth;
    private List<float> botFrustration;
    private List<float> botRiskTakingFactor;
    private List<float> botTakingRiskyPath;
    private List<int> botSeeingPlayer;
    private List<int> botChasingPlayer;
    private List<float> botDistanceFromPlayer;
    private List<float> cursorDistanceFromPlayer;
    private List<float> cursorDistanceFromBot;
    private List<float> playerDistanceTravelled;
    private List<float> playerPositionX;
    private List<float> playerPositionY;
    private List<float> playerRotation;
    private List<float> playerHealth;
    private List<int> playerIsDashing;
    private List<int> playerTriesDashOnCD;

    private List<int> dashPressed;
    private List<float> cursorDistanceTraveled;
    private List<float> cursorPositionX;
    private List<float> cursorPositionY;
    private List<int> playerTriesToFireOnCD;
    private List<int> playerTriesToBombOnCD;
    private List<int> shotsFired;
    private List<int> bombDropped;
    private List<int> gunReloading;
    private List<int> bombReloading;

    private List<int> playerBurning;
    private List<int> playerHealing;
    private List<float> playerDeltaHealth;
    private List<int> playerDied;
    private List<int> botLostPlayer;
    private List<int> botSpottedPlayer;
    private List<int> botBurning;
    private List<float> botDeltaHealth;
    private List<int> botDied;

    private List<int> onScreenFires;
    private List<int> onScreenBullets;

    private List<string> keyPresses;

    private bool recordStarted = false;

    private void Awake() {
        canRecord = false;
        levelManager = GameObject.Find("LevelManager").GetComponent<LevelManager>();
        form = new WWWForm();
        ResetForm();
    }

    private void Start() {
        StartCoroutine(LateStart());
        levelManager.OnGameEnd.AddListener(delegate {
            if (recordGameplay && recordStarted) {
                recordGameplay = false;
                CompressPackage();
                StartCoroutine(SendPackage(url, form));
            }
        });
        levelManager.OnGameStart.AddListener(delegate {
            if (recordGameplay && !recordStarted) {
                Debug.Log("Record Started!");
                ResetForm();
                recordStarted = true;
                StartCoroutine(UpdatePackage(period));
            }
        });
    }

    private void FixedUpdate() {
        tick++;

        if (recordGameplay && canRecord) {
            int botInChase = 0;
            int targetSeen = 0;
            if (!bot.Equals(null)) {
                targetSeen = (bot.GetComponent<ProximityDetector>().playerDetected || bot.GetComponent<FieldOfView>().targetDetected) ? 1 : 0;
                botInChase = bot.GetComponent<Movement>().targetLastSeen ? 1 : 0;

                botFrustrationActive = botFrustrationComponent.frustrationIsActive;
                botFrustrationLocked = botFrustrationComponent.lockFrustration;
                botFrustrationRange = (botFrustrationComponent.minFrustration + "-" + botFrustrationComponent.maxFrustration);
                score.Add(levelManager.score);

                botDistanceTraveled.Add(((bot.transform.position - previousBotPosition).magnitude));
                previousBotPosition = bot.transform.position;
                botPositionX.Add(bot.transform.position.x);
                botPositionY.Add(bot.transform.position.y);
                botRotation.Add(bot.transform.eulerAngles.z);
                botSpeed.Add(bot.GetComponent<Movement>().speed);
                botRotationSpeed.Add(bot.GetComponent<Movement>().rotationSpeed);
                botViewAngle.Add(bot.GetComponent<FieldOfView>().viewAngle);
                botViewRadius.Add(bot.GetComponent<FieldOfView>().viewRadius);
                botSearching.Add((bot.GetComponent<Movement>().isWaiting ? 1 : 0));
                botSearchTurns.Add(bot.GetComponent<Movement>().lookAroundCycle);
                botHearingRadius.Add(bot.GetComponent<ProximityDetector>().hearingRadius);
                botHearingProbability.Add(bot.GetComponent<ProximityDetector>().detectionChance);
                botHealth.Add(bot.GetComponent<Health>().health);
                botFrustration.Add(bot.GetComponent<FrustrationComponent>().levelOfFrustration);
                botRiskTakingFactor.Add(bot.GetComponent<Movement>().riskTakingFactor);
                botTakingRiskyPath.Add((bot.GetComponent<Movement>().takingRiskyPath ? 1 : 0));
                botSeeingPlayer.Add((targetSeen));
                botChasingPlayer.Add((botInChase));
                cursorDistanceFromBot.Add((new Vector2(cursor.transform.position.x, cursor.transform.position.y) -
                   new Vector2(bot.transform.position.x, bot.transform.position.y)).magnitude);
                botDied.Add((bot.GetComponent<Health>().botDied ? 1 : 0));
                botBurning.Add((bot.GetComponent<Health>().isBurning ? 1 : 0));
                botDeltaHealth.Add((bot.GetComponent<Health>().deltaHealth));
            }
            if (!bot.Equals(null) && !player.Equals(null)) {
                botDistanceFromPlayer.Add((new Vector2(bot.transform.position.x, bot.transform.position.y) -
                    new Vector2(player.transform.position.x, player.transform.position.y)).magnitude);
            }

            if (!player.Equals(null)) {
                playerDistanceTravelled.Add((player.GetComponent<PlayerController>().transform.position - previousPlayerPosition).magnitude);
                previousPlayerPosition = player.GetComponent<PlayerController>().transform.position;
                playerPositionX.Add(player.GetComponent<PlayerController>().transform.position.x);
                playerPositionY.Add(player.GetComponent<PlayerController>().transform.position.y);
                playerRotation.Add(player.transform.eulerAngles.z);
                playerHealth.Add(player.GetComponent<PlayerHealth>().health);
                playerIsDashing.Add((player.GetComponent<PlayerController>().dashing ? 1 : 0));
                playerTriesDashOnCD.Add((((Input.GetKeyDown(KeyCode.LeftShift) ||
                    Input.GetKeyDown(KeyCode.RightShift) ||
                    Input.GetKeyDown(KeyCode.Space)) &&
                    player.GetComponent<PlayerController>().dashOnCD) ? 1 : 0));
                cursorDistanceFromPlayer.Add((new Vector2(cursor.transform.position.x, cursor.transform.position.y) -
                   new Vector2(player.transform.position.x, player.transform.position.y)).magnitude);
                playerDied.Add((player.GetComponent<PlayerHealth>().playerDied ? 1 : 0));
                playerBurning.Add((player.GetComponent<PlayerHealth>().isBurning ? 1 : 0));
                playerHealing.Add((player.GetComponent<PlayerHealth>().isHealing ? 1 : 0));
                playerDeltaHealth.Add((player.GetComponent<PlayerHealth>().deltaHealth));
            }

            dashPressed.Add(((Input.GetKeyDown(KeyCode.LeftShift) ||
                Input.GetKeyDown(KeyCode.RightShift) ||
                Input.GetKeyDown(KeyCode.Space)) ? 1 : 0));
            cursorDistanceTraveled.Add((cursor.transform.position - previousCursorPosition).magnitude);
            previousCursorPosition = cursor.transform.position;
            cursorPositionX.Add(cursor.transform.position.x);
            cursorPositionY.Add(cursor.transform.position.y);
            playerTriesToFireOnCD.Add((Input.GetMouseButton(0) &&
                cursor.GetComponent<GunControls>().projectileCount == 0 ? 1 : 0));
            playerTriesToBombOnCD.Add((Input.GetMouseButtonUp(1) &&
                cursor.GetComponent<GunControls>().bombCount == 0 ? 1 : 0));
            shotsFired.Add((Input.GetMouseButton(0) &&
                !cursor.GetComponent<GunControls>().reloading ? 1 : 0));
            bombDropped.Add((Input.GetMouseButtonUp(1) &&
                !cursor.GetComponent<GunControls>().bombReloading ? 1 : 0));
            gunReloading.Add((cursor.GetComponent<GunControls>().reloading ? 1 : 0));
            bombReloading.Add((cursor.GetComponent<GunControls>().bombReloading ? 1 : 0));

            onScreenFires.Add(GameObject.FindGameObjectsWithTag("fire").Length);
            onScreenBullets.Add(GameObject.FindGameObjectsWithTag("projectile").Length);
        }

        foreach (KeyCode kcode in Enum.GetValues(typeof(KeyCode))) {
            if (Input.GetKey(kcode)) {
                keyPresses.Add(kcode.ToString());
            }
        }

        if (!Input.anyKey) {
            //Starts counting when no button is being pressed
            idleTime = idleTime + 1;
        }
        fullTime = fullTime + 1;
    }

    void ResetForm() {
        tick = 0;
        idleTime = 0;
        fullTime = 0;

        score = new List<int>();
        botDistanceTraveled = new List<float>();
        botPositionX = new List<float>();
        botPositionY = new List<float>();
        botRotation = new List<float>();
        botSpeed = new List<float>();
        botRotationSpeed = new List<float>();
        botViewAngle = new List<float>();
        botViewRadius = new List<float>();
        botSearching = new List<int>();
        botSearchTurns = new List<int>();
        botHearingRadius = new List<float>();
        botHearingProbability = new List<float>();
        botHealth = new List<float>();
        botFrustration = new List<float>();
        botRiskTakingFactor = new List<float>();
        botTakingRiskyPath = new List<float>();
        botSeeingPlayer = new List<int>();
        botChasingPlayer = new List<int>();
        botDistanceFromPlayer = new List<float>();
        cursorDistanceFromPlayer = new List<float>();
        cursorDistanceFromBot = new List<float>();
        playerDistanceTravelled = new List<float>();
        playerPositionX = new List<float>();
        playerPositionY = new List<float>();
        playerRotation = new List<float>();
        playerHealth = new List<float>();
        playerIsDashing = new List<int>();
        playerTriesDashOnCD = new List<int>();

        dashPressed = new List<int>();
        cursorDistanceTraveled = new List<float>();
        cursorPositionX = new List<float>();
        cursorPositionY = new List<float>();
        playerTriesToFireOnCD = new List<int>();
        playerTriesToBombOnCD = new List<int>();
        shotsFired = new List<int>();
        bombDropped = new List<int>();
        gunReloading = new List<int>();
        bombReloading = new List<int>();

        playerBurning = new List<int>();
        playerHealing = new List<int>();
        playerDeltaHealth = new List<float>();
        playerDied = new List<int>();
        botLostPlayer = new List<int>();
        botSpottedPlayer = new List<int>();
        botBurning = new List<int>();
        botDeltaHealth = new List<float>();
        botDied = new List<int>();

        onScreenFires = new List<int>();
        onScreenBullets = new List<int>();

        keyPresses = new List<string>();
    }

    void CompressPackage() {
        form.AddField("epoch", ReturnEpochStamp());
        form.AddField("timeStamp", Time.realtimeSinceStartup.ToString());
        form.AddField("tick", tick.ToString());
        form.AddField("keyPressCount", keyPresses.Count.ToString());
        form.AddField("keyPresses", String.Join("|", keyPresses.ToArray()));
        form.AddField("idleTime", ((float)idleTime / (float)fullTime).ToString());
        form.AddField("botFrustrationActive", botFrustrationActive.ToString());
        form.AddField("botFrustrationLocked", botFrustrationLocked.ToString());
        form.AddField("botFrustrationRange", botFrustrationRange.ToString());
        form.AddField("score", score.DefaultIfEmpty(0).Average().ToString());
        form.AddField("botDistanceTraveled", botDistanceTraveled.DefaultIfEmpty(0).Average().ToString());
        form.AddField("botPositionX", botPositionX.DefaultIfEmpty(0).Average().ToString());
        form.AddField("botPositionY", botPositionY.DefaultIfEmpty(0).Average().ToString());
        form.AddField("botRotation", botRotation.DefaultIfEmpty(0).Average().ToString());
        form.AddField("botSpeed", botSpeed.DefaultIfEmpty(0).Average().ToString());
        form.AddField("botRotationSpeed", botRotationSpeed.DefaultIfEmpty(0).Average().ToString());
        form.AddField("botViewAngle", botViewAngle.DefaultIfEmpty(0).Average().ToString());
        form.AddField("botViewRadius", botViewRadius.DefaultIfEmpty(0).Average().ToString());
        form.AddField("botSearching", botSearching.DefaultIfEmpty(0).Average().ToString());
        form.AddField("botSearchTurns", botSearchTurns.DefaultIfEmpty(0).Average().ToString());
        form.AddField("botHearingRadius", botHearingRadius.DefaultIfEmpty(0).Average().ToString());
        form.AddField("botHearingProbability", botHearingProbability.DefaultIfEmpty(0).Average().ToString());
        form.AddField("botHealth", botHealth.DefaultIfEmpty(0).Average().ToString());
        form.AddField("botFrustration", botFrustration.DefaultIfEmpty(0).Average().ToString());
        form.AddField("botRiskTakingFactor", botRiskTakingFactor.DefaultIfEmpty(0).Average().ToString());
        form.AddField("botTakingRiskyPath", botTakingRiskyPath.DefaultIfEmpty(0).Average().ToString());
        form.AddField("botSeeingPlayer", botSeeingPlayer.DefaultIfEmpty(0).Average().ToString());
        form.AddField("botChasingPlayer", botChasingPlayer.DefaultIfEmpty(0).Average().ToString());
        form.AddField("botDistanceFromPlayer", botDistanceFromPlayer.DefaultIfEmpty(0).Average().ToString());
        form.AddField("cursorDistanceFromPlayer", cursorDistanceFromPlayer.DefaultIfEmpty(0).Average().ToString());
        form.AddField("cursorDistanceFromBot", cursorDistanceFromBot.DefaultIfEmpty(0).Average().ToString());
        form.AddField("playerDistanceTravelled", playerDistanceTravelled.DefaultIfEmpty(0).Average().ToString());
        form.AddField("playerPositionX", playerPositionX.DefaultIfEmpty(0).Average().ToString());
        form.AddField("playerPositionY", playerPositionY.DefaultIfEmpty(0).Average().ToString());
        form.AddField("playerRotation", playerRotation.DefaultIfEmpty(0).Average().ToString());
        form.AddField("playerHealth", playerHealth.DefaultIfEmpty(0).Average().ToString());
        form.AddField("playerIsDashing", playerIsDashing.DefaultIfEmpty(0).Average().ToString());
        form.AddField("playerTriesDashOnCD", playerTriesDashOnCD.DefaultIfEmpty(0).Average().ToString());
        form.AddField("dashPressed", dashPressed.DefaultIfEmpty(0).Average().ToString());
        form.AddField("cursorDistanceTraveled", cursorDistanceTraveled.DefaultIfEmpty(0).Average().ToString());
        form.AddField("cursorPositionX", cursorPositionX.DefaultIfEmpty(0).Average().ToString());
        form.AddField("cursorPositionY", cursorPositionY.DefaultIfEmpty(0).Average().ToString());
        form.AddField("playerTriesToFireOnCD", playerTriesToFireOnCD.DefaultIfEmpty(0).Average().ToString());
        form.AddField("playerTriesToBombOnCD", playerTriesToBombOnCD.DefaultIfEmpty(0).Average().ToString());
        form.AddField("shotsFired", shotsFired.DefaultIfEmpty(0).Average().ToString());
        form.AddField("bombDropped", bombDropped.DefaultIfEmpty(0).Average().ToString());
        form.AddField("gunReloading", gunReloading.DefaultIfEmpty(0).Average().ToString());
        form.AddField("bombReloading", bombReloading.DefaultIfEmpty(0).Average().ToString());
        form.AddField("playerBurning", playerBurning.DefaultIfEmpty(0).Average().ToString());
        form.AddField("playerHealing", playerHealing.DefaultIfEmpty(0).Average().ToString());
        form.AddField("playerDeltaHealth", playerDeltaHealth.DefaultIfEmpty(0).Average().ToString());
        form.AddField("playerDied", playerDied.DefaultIfEmpty(0).Average().ToString());
        form.AddField("botLostPlayer", botLostPlayer.DefaultIfEmpty(0).Average().ToString());
        form.AddField("botSpottedPlayer", botSpottedPlayer.DefaultIfEmpty(0).Average().ToString());
        form.AddField("botBurning", botBurning.DefaultIfEmpty(0).Average().ToString());
        form.AddField("botDeltaHealth", botDeltaHealth.DefaultIfEmpty(0).Average().ToString());
        form.AddField("botDied", botDied.DefaultIfEmpty(0).Average().ToString());
        form.AddField("onScreenFires", onScreenFires.DefaultIfEmpty(0).Average().ToString());
        form.AddField("onScreenBullets", onScreenBullets.DefaultIfEmpty(0).Average().ToString());
    }

    IEnumerator UpdatePackage(float period) {
        while (recordGameplay) {
            yield return new WaitForSeconds(period);
            CompressPackage();
            StartCoroutine(SendPackage(url, form));
            form = new WWWForm();
            ResetForm();
        }
    }

    private string ReturnEpochStamp() {
        DateTime epochStart = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
        string currentEpochTime = ((long)(DateTime.UtcNow - epochStart).TotalMilliseconds).ToString();
        return currentEpochTime;
    }

    IEnumerator SendPackage(string destination, WWWForm package) {
        if (package.data.Count() > 0) {
            UnityWebRequest www = UnityWebRequest.Post(destination, package);
            yield return www.SendWebRequest();
        } else {
            yield return null;
        }
    }

    IEnumerator LateStart() {
        yield return new WaitForFixedUpdate();
        player = GameObject.Find("PlayerController");
        cursor = GameObject.Find("GunControls");
        bot = GameObject.Find("Monster");
        levelManager = GameObject.Find("LevelManager").GetComponent<LevelManager>();
        previousPlayerPosition = player.transform.position;
        previousCursorPosition = cursor.transform.position;
        previousBotPosition = bot.transform.position;
        botFrustrationComponent = bot.GetComponent<FrustrationComponent>();
        canRecord = true;
        yield return null;
    }
}
