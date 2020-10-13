//using System;
//using System.Collections;
//using System.Collections.Generic;
//using System.Linq;
//using UnityEngine;
//using UnityEngine.Events;
//using UnityEngine.Networking;

//public class DataLogger : MonoBehaviour {
//    private GameObject player;
//    private GameObject[] allBots;
//    private GameObject[] allPickUps;
//    private LevelManager levelManager;
//    public string url;

//    private List<GameObject> visibleCharacterList;
//    private List<GameObject> visiblePickUpList;
//    private List<GameObject> playerProjectiles;
//    private List<GameObject> enemyProjectiles;

//    private int tick = 0;
//    private List<string> keyPresses;
//    private int idleTime = 0;
//    private int fullTime = 0;

//    private List<int> playerScore;
//    private List<int> playerHasCollisions;
//    private List<int> playerIsCollidingAbove;
//    private List<int> playerIsCollidingBelow;
//    private List<int> playerIsCollidingLeft;
//    private List<int> playerIsCollidingRight;
//    private List<int> playerIsFalling;
//    private List<int> playerIsGrounded;
//    private List<int> playerIsJumping;
//    private List<float> playerSpeedX;
//    private List<float> playerSpeedY;
//    private List<float> playerDeltaDistance;
//    private List<float> playerHealth;
//    private List<int> playerDamaged;
//    private List<string> playerDamagedBy;
//    private List<int> playerShooting;
//    private List<int> playerProjectileCount;
//    private List<float> playerProjectileDistance;
//    public int playerHealthPickup = 0;
//    public int playerPointPickup = 0;
//    public int playerPowerPickup = 0;
//    public int playerBoostPickup = 0;
//    public int playerSlowPickup = 0;
//    private List<int> playerHasPowerup;
//    public int playerKillCount = 0;
//    public int playerDeath = 0;

//    private List<int> botsVisible;
//    private List<string> botName;
//    private List<int> botHasCollisions;
//    private List<int> botIsCollidingAbove;
//    private List<int> botIsCollidingBelow;
//    private List<int> botIsCollidingLeft;
//    private List<int> botIsCollidingRight;
//    private List<int> botIsFalling;
//    private List<int> botIsGrounded;
//    private List<int> botIsJumping;
//    private List<float> botSpeedX;
//    private List<float> botSpeedY;
//    private List<float> botDeltaDistance;
//    private List<float> botHealth;
//    private List<int> botDamaged;
//    private List<string> botDamagedBy;
//    private List<int> botShooting;
//    private List<int> botCharging;
//    private List<float> botPlayerDistance;
//    private List<int> botProjectileCount;
//    private List<float> botProjectilePlayerDistance;
//    private List<string> botProjectileTypes;

//    private List<string> pickUpTypes;
//    private List<int> pickUpsVisible;
//    private List<float> pickUpPlayerDisctance;

//    public bool record;
//    private bool recordStarted = false;

//    private WWWForm form;

//    private float t = 0.0f;
//    public float period = 0.25f;

//    private void Awake() {
//        form = new WWWForm();
//        ResetForm();
//    }

//    // Start is called before the first frame update
//    void Start() {
//        levelManager = LevelManager.Instance;
//        StartCoroutine(LateStart());
//        levelManager.OnGameEnds.AddListener(delegate {
//            if (record && recordStarted) {
//                record = false;
//                CompressPackage();
//                StartCoroutine(SendPackage(url, form));
//            }
//        });
//        levelManager.OnGameStarts.AddListener(delegate {
//            if (record && !recordStarted) {
//                ResetForm();
//                recordStarted = true;
//                StartCoroutine(UpdatePackage(period));
//            }
//        });
//    }

//    void Update() {
//        allBots = GameObject.FindGameObjectsWithTag("Enemy");
//        allPickUps = GameObject.FindGameObjectsWithTag("PickUp");
//        visibleCharacterList = new List<GameObject>();
//        foreach (GameObject character in allBots) {
//            if (character.name == "RetroBossRightGun" || character.name == "RetroBossLeftGun") {
//                if (character.transform.GetComponent<SpriteRenderer>().isVisible) {
//                    visibleCharacterList.Add(character);
//                }
//            } else {
//                if (levelManager.game == "gun") {
//                    if (character.transform.GetChild(0).GetComponent<SpriteRenderer>().isVisible) {
//                        visibleCharacterList.Add(character);
//                    }
//                } else {
//                    if (character.GetComponent<SpriteRenderer>().isVisible) {
//                        visibleCharacterList.Add(character);
//                    }
//                }
//            }
//        }

//        visiblePickUpList = new List<GameObject>();
//        foreach (GameObject pickup in allPickUps) {
//            if (pickup.transform.GetComponent<SpriteRenderer>().isVisible) {
//                visiblePickUpList.Add(pickup);
//            }
//        }

//        playerProjectiles = new List<GameObject>();
//        enemyProjectiles = new List<GameObject>();
//        GameObject[] allProjectiles = GameObject.FindGameObjectsWithTag("Projectile");
//        foreach (GameObject projectile in allProjectiles) {
//            if (projectile.transform.GetComponent<SpriteRenderer>().isVisible) {
//                if (projectile.layer == 16) {
//                    playerProjectiles.Add(projectile);
//                } else {
//                    enemyProjectiles.Add(projectile);
//                }
//            }
//        }
//        if (record) {
//            UpdateForm();
//        }

//        if (!Input.anyKey) {
//            //Starts counting when no button is being pressed
//            idleTime = idleTime + 1;
//        }
//        fullTime = fullTime + 1;
//    }

//    IEnumerator UpdatePackage(float period) {
//        while (record) {
//            yield return new WaitForSeconds(period);
//            CompressPackage();
//            StartCoroutine(SendPackage(url, form));
//            form = new WWWForm();
//            ResetForm();
//        }
//    }

//    // Update is called once per frame
//    void UpdateForm() {
//        tick++;

//        playerScore.Add(levelManager.Score);
//        playerHasCollisions.Add(player.GetComponent<CorgiController>().State.HasCollisions ? 1 : 0);
//        playerIsCollidingAbove.Add(player.GetComponent<CorgiController>().State.IsCollidingAbove ? 1 : 0);
//        playerIsCollidingBelow.Add(player.GetComponent<CorgiController>().State.IsCollidingBelow ? 1 : 0);
//        playerIsCollidingLeft.Add(player.GetComponent<CorgiController>().State.IsCollidingLeft ? 1 : 0);
//        playerIsCollidingRight.Add(player.GetComponent<CorgiController>().State.IsCollidingRight ? 1 : 0);
//        playerIsFalling.Add(player.GetComponent<CorgiController>().State.IsFalling ? 1 : 0);
//        playerIsGrounded.Add(player.GetComponent<CorgiController>().State.IsGrounded ? 1 : 0);
//        if (levelManager.game == "endless") {
//            playerIsJumping.Add(player.GetComponent<CharacterLadder>().moving ? 1 : 0);
//        } else {
//            playerIsJumping.Add(player.GetComponent<CorgiController>().State.IsJumping ? 1 : 0);
//        }
//        if (levelManager.game == "endless") {
//            playerSpeedX.Add(levelManager.scrollSpeed);
//        } else {
//            playerSpeedX.Add(Mathf.Abs(player.GetComponent<CorgiController>().Speed.x));
//        }
//        playerSpeedY.Add(Mathf.Abs(player.GetComponent<CorgiController>().Speed.y));
//        if (levelManager.game == "endless") {
//            playerDeltaDistance.Add((levelManager._isPlaying ? levelManager.scrollSpeed : 0)
//                + player.GetComponent<CorgiController>().distanceTravelled);
//        } else {
//            playerDeltaDistance.Add(player.GetComponent<CorgiController>().distanceTravelled);
//        }
//        playerHealth.Add((float)player.GetComponent<Health>().CurrentHealth / (float)player.GetComponent<Health>().InitialHealth);
//        playerDamaged.Add(player.GetComponent<Health>().damaged ? 1 : 0);
//        playerDamagedBy.Add(player.GetComponent<Health>().damagedBy);
//        if (levelManager.game == "endless") {
//            playerShooting.Add(player.GetComponent<PlayerAttack>()._attackInProgress ? 1 : 0);
//        } else if (levelManager.game == "platform") {
//            playerShooting.Add(0);
//        } else {
//            playerShooting.Add(player.GetComponent<CharacterHandleWeapon>().shooting ? 1 : 0);
//        }
//        if (levelManager.game == "platform" || levelManager.game == "endless") {
//            playerProjectileCount.Add(0);
//            playerProjectileDistance.Add(0);
//        } else {
//            playerProjectileCount.Add(playerProjectiles.Count);
//            foreach (GameObject projectile in playerProjectiles) {
//                playerProjectileDistance.Add(Vector2.Distance(projectile.transform.position, player.transform.position));
//            }
//        }

//        if (levelManager.game == "platform") {
//            playerHasPowerup.Add(player.GetComponent<SuperHipsterBrosHealth>().hasPowerUp ? 1: 0);
//        } else {
//            playerHasPowerup.Add(0);
//        }

//        botsVisible.Add(visibleCharacterList.Count);
//        foreach (GameObject projectile in enemyProjectiles) {
//            botProjectilePlayerDistance.Add(Vector2.Distance(projectile.transform.position, player.transform.position));
//            botProjectileTypes.Add(projectile.name.Split('-')[0]);
//        }

//        for (int i = 0; i < visibleCharacterList.Count; i++) {
//            GameObject bot = visibleCharacterList[i];
//            botName.Add(bot.name.Split(' ')[0]);
//            if (levelManager.game == "endless") {
//                botHasCollisions.Add(0);
//                botIsCollidingAbove.Add(0);
//                botIsCollidingBelow.Add(1);
//                botIsCollidingLeft.Add(bot.GetComponent<DamageOnTouch>().colliding ? 1 : 0);
//                botIsCollidingRight.Add(0);
//                botIsFalling.Add(0);
//                botIsGrounded.Add(1);
//                botIsJumping.Add(0);
//                botSpeedX.Add(levelManager.scrollSpeed);
//                botSpeedY.Add(0);
//                botDeltaDistance.Add(levelManager.scrollSpeed);
//            } else {
//                botHasCollisions.Add(bot.GetComponent<CorgiController>().State.HasCollisions ? 1 : 0);
//                botIsCollidingAbove.Add(bot.GetComponent<CorgiController>().State.IsCollidingAbove ? 1 : 0);
//                botIsCollidingBelow.Add(bot.GetComponent<CorgiController>().State.IsCollidingBelow ? 1 : 0);
//                botIsCollidingLeft.Add(bot.GetComponent<CorgiController>().State.IsCollidingLeft ? 1 : 0);
//                botIsCollidingRight.Add(bot.GetComponent<CorgiController>().State.IsCollidingRight ? 1 : 0);
//                botIsFalling.Add(bot.GetComponent<CorgiController>().State.IsFalling ? 1 : 0);
//                botIsGrounded.Add(bot.GetComponent<CorgiController>().State.IsGrounded ? 1 : 0);
//                botIsJumping.Add(bot.GetComponent<CorgiController>().State.IsJumping ? 1 : 0);
//                botSpeedX.Add(Mathf.Abs(bot.GetComponent<CorgiController>().Speed.x));
//                botSpeedY.Add(Mathf.Abs(bot.GetComponent<CorgiController>().Speed.y));
//                botDeltaDistance.Add(bot.GetComponent<CorgiController>().distanceTravelled);
//            }
//            botHealth.Add((float)bot.GetComponent<Health>().CurrentHealth / (float)bot.GetComponent<Health>().InitialHealth);
//            if (levelManager.game == "gun") {
//                botDamaged.Add(bot.GetComponent<Health>().damaged ? 1 : 0);
//                botDamagedBy.Add(bot.GetComponent<Health>().damagedBy);
//            }
//            if (levelManager.game == "platform" || levelManager.game == "endless") {
//                botShooting.Add(0);
//                botProjectileCount.Add(0);
//                botCharging.Add(0);
//            } else {
//                botShooting.Add(bot.GetComponent<CharacterHandleWeapon>().shooting ? 1 : 0);
//                botProjectileCount.Add(enemyProjectiles.Count);
//                if (bot.GetComponent<CharacterHandleWeapon>().CurrentWeapon != null) {
//                    botCharging.Add(bot.GetComponent<CharacterHandleWeapon>().CurrentWeapon.charging ? 1 : 0);
//                }
//            }
//            botPlayerDistance.Add(Vector2.Distance(bot.transform.position, player.transform.position));
//        }
        
//        pickUpsVisible.Add(visiblePickUpList.Count);
//        for (int i = 0; i < visiblePickUpList.Count; i++) {
//            GameObject pickup = visiblePickUpList[i];
//            pickUpPlayerDisctance.Add(Vector2.Distance(pickup.transform.position, player.transform.position));
//            pickUpTypes.Add(pickup.name);
//        }

//        foreach (KeyCode kcode in Enum.GetValues(typeof(KeyCode))) {
//            if (Input.GetKey(kcode)) {
//                keyPresses.Add(kcode.ToString());
//            }
//        }
//    }

//    private void CompressPackage() {
//        form.AddField("game", levelManager.game);
//        form.AddField("epoch", ReturnEpochStamp());
//        form.AddField("timeStamp", Time.realtimeSinceStartup.ToString());
//        form.AddField("tick", tick.ToString());

//        form.AddField("keyPressCount", keyPresses.Count.ToString());
//        form.AddField("keyPresses", String.Join("|", keyPresses.ToArray()));
//        form.AddField("idleTime", ((float)idleTime / (float)fullTime).ToString());

//        form.AddField("playerScore", playerScore.DefaultIfEmpty(0).Average().ToString());
//        form.AddField("playerHasCollisions", playerHasCollisions.DefaultIfEmpty(0).Average().ToString());
//        form.AddField("playerIsCollidingAbove", playerIsCollidingAbove.DefaultIfEmpty(0).Average().ToString());
//        form.AddField("playerIsCollidingBelow", playerIsCollidingBelow.DefaultIfEmpty(0).Average().ToString());
//        form.AddField("playerIsCollidingLeft", playerIsCollidingLeft.DefaultIfEmpty(0).Average().ToString());
//        form.AddField("playerIsCollidingRight", playerIsCollidingRight.DefaultIfEmpty(0).Average().ToString());
//        form.AddField("playerIsFalling", playerIsFalling.DefaultIfEmpty(0).Average().ToString());
//        form.AddField("playerIsGrounded", playerIsGrounded.DefaultIfEmpty(0).Average().ToString());
//        form.AddField("playerIsJumping", playerIsJumping.DefaultIfEmpty(0).Average().ToString());
//        form.AddField("playerSpeedX", playerSpeedX.DefaultIfEmpty(0).Average().ToString());
//        form.AddField("playerSpeedY", playerSpeedY.DefaultIfEmpty(0).Average().ToString());
//        form.AddField("playerDeltaDistance", playerDeltaDistance.DefaultIfEmpty(-1).Average().ToString());
//        form.AddField("playerHealth", playerHealth.DefaultIfEmpty(0).Average().ToString());
//        form.AddField("playerDamaged", playerDamaged.DefaultIfEmpty(0).Average().ToString());
//        form.AddField("playerDamagedBy", String.Join("|", playerDamagedBy.DefaultIfEmpty("NONE").Distinct()));
//        form.AddField("playerShooting", playerShooting.DefaultIfEmpty(0).Average().ToString());
//        form.AddField("playerProjectileCount", playerProjectileCount.DefaultIfEmpty(0).Average().ToString());
//        form.AddField("playerProjectileDistance", playerProjectileDistance.DefaultIfEmpty(-1).Average().ToString());
//        form.AddField("playerHealthPickup", playerHealthPickup.ToString());
//        form.AddField("playerPointPickup", playerPointPickup.ToString());
//        form.AddField("playerPowerPickup", playerPowerPickup.ToString());
//        form.AddField("playerBoostPickup", playerBoostPickup.ToString());
//        form.AddField("playerSlowPickup", playerSlowPickup.ToString());
//        form.AddField("playerHasPowerup", playerHasPowerup.DefaultIfEmpty(0).Average().ToString());
//        form.AddField("playerKillCount", playerKillCount.ToString());
//        form.AddField("playerDeath", playerDeath.ToString());
        
//        form.AddField("botsVisible", botsVisible.DefaultIfEmpty(0).Average().ToString());
//        form.AddField("botTypes", String.Join("|", botName.DefaultIfEmpty("NONE").Distinct()));
//        form.AddField("botHasCollisions", botHasCollisions.DefaultIfEmpty(0).Average().ToString());
//        form.AddField("botIsCollidingAbove", botIsCollidingAbove.DefaultIfEmpty(0).Average().ToString());
//        form.AddField("botIsCollidingBelow", botIsCollidingBelow.DefaultIfEmpty(0).Average().ToString());
//        form.AddField("botIsCollidingLeft", botIsCollidingLeft.DefaultIfEmpty(0).Average().ToString());
//        form.AddField("botIsCollidingRight", botIsCollidingRight.DefaultIfEmpty(0).Average().ToString());
//        form.AddField("botIsFalling", botIsFalling.DefaultIfEmpty(0).Average().ToString());
//        form.AddField("botIsGrounded", botIsGrounded.DefaultIfEmpty(0).Average().ToString());
//        form.AddField("botIsJumping", botIsJumping.DefaultIfEmpty(0).Average().ToString());
//        form.AddField("botSpeedX", botSpeedX.DefaultIfEmpty(0).Average().ToString());
//        form.AddField("botSpeedY", botSpeedY.DefaultIfEmpty(0).Average().ToString());
//        form.AddField("botDeltaDistance", botDeltaDistance.DefaultIfEmpty(0).Average().ToString());
//        form.AddField("botHealth", botHealth.DefaultIfEmpty(0).Average().ToString());
//        form.AddField("botDamaged", botDamaged.DefaultIfEmpty(0).Average().ToString());
//        form.AddField("botShooting", botShooting.DefaultIfEmpty(0).Average().ToString());
//        form.AddField("botProjectileCount", botProjectileCount.DefaultIfEmpty(0).Average().ToString());
//        form.AddField("botCharging", botCharging.DefaultIfEmpty(0).Average().ToString());
//        form.AddField("botPlayerDistance", botPlayerDistance.DefaultIfEmpty(0).Average().ToString());
//        form.AddField("botProjectilePlayerDistance", botProjectilePlayerDistance.DefaultIfEmpty(0).Average().ToString());
//        form.AddField("pickUpTypes", String.Join("|", pickUpTypes.DefaultIfEmpty("NONE").Distinct()));
//        form.AddField("pickUpsVisible", pickUpsVisible.DefaultIfEmpty(0).Average().ToString());
//        form.AddField("pickUpPlayerDisctance", pickUpPlayerDisctance.DefaultIfEmpty(0).Average().ToString());
//    }

//    void ResetForm() {
//        tick = 0;
//        keyPresses = new List<string>();
//        idleTime = 0;
//        fullTime = 0;

//        playerScore = new List<int>();
//        playerHasCollisions = new List<int>();
//        playerIsCollidingAbove = new List<int>();
//        playerIsCollidingBelow = new List<int>();
//        playerIsCollidingLeft = new List<int>();
//        playerIsCollidingRight = new List<int>();
//        playerIsFalling = new List<int>();
//        playerIsGrounded = new List<int>();
//        playerIsJumping = new List<int>();
//        playerSpeedX = new List<float>();
//        playerSpeedY = new List<float>();
//        playerDeltaDistance = new List<float>();
//        playerHealth = new List<float>();
//        playerDamaged = new List<int>();
//        playerDamagedBy = new List<string>();
//        playerShooting = new List<int>();
//        playerProjectileCount = new List<int>();
//        playerProjectileDistance = new List<float>();
//        playerHealthPickup = 0;
//        playerPointPickup = 0;
//        playerPowerPickup = 0;
//        playerBoostPickup = 0;
//        playerSlowPickup = 0;
//        playerHasPowerup = new List<int>();
//        playerKillCount = 0;
//        playerDeath = 0;

//        botsVisible = new List<int>();
//        botName = new List<string>();
//        botHasCollisions = new List<int>();
//        botIsCollidingAbove = new List<int>();
//        botIsCollidingBelow = new List<int>();
//        botIsCollidingLeft = new List<int>();
//        botIsCollidingRight = new List<int>();
//        botIsFalling = new List<int>();
//        botIsGrounded = new List<int>();
//        botIsJumping = new List<int>();
//        botSpeedX = new List<float>();
//        botSpeedY = new List<float>();
//        botDeltaDistance = new List<float>();
//        botHealth = new List<float>();
//        botDamaged = new List<int>();
//        botDamagedBy = new List<string>();
//        botShooting = new List<int>();
//        botCharging = new List<int>();
//        botPlayerDistance = new List<float>();
//        botProjectileCount = new List<int>();
//        botProjectilePlayerDistance = new List<float>();
//        botProjectileTypes = new List<string>();

//        pickUpTypes = new List<string>();
//        pickUpsVisible = new List<int>();
//        pickUpPlayerDisctance = new List<float>();
//    }

//    private string ReturnEpochStamp() {
//        DateTime epochStart = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
//        string currentEpochTime = ((long)(DateTime.UtcNow - epochStart).TotalMilliseconds).ToString();
//        return currentEpochTime;
//    }

//    IEnumerator SendPackage(string destination, WWWForm package) {
//        if (package.data.Count() > 0) {
//            UnityWebRequest www = UnityWebRequest.Post(destination, package);
//            yield return www.SendWebRequest();
//        } else {
//            yield return null;
//        }
//    }

//    IEnumerator LateStart() {
//        yield return new WaitForFixedUpdate();
//        player = GameObject.FindWithTag("Player");
//        yield return null;
//    }
//}
