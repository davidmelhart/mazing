using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class PlayerHealth : MonoBehaviour {
    ReportGenerator reportGenerator;
    GameObject agent;
    LevelManager levelManager;

    public GameObject destroyEffect;
    private HealthBar healthBar;
    public LayerMask fireMask;
    private Renderer renderMaterial;
    Color originalColor;
    [Range(1, 100)]
    public int health = 100;
    public float damageFrequency = 0.1f;
    public int fireDamage = 2;

    //Fire damage
    private float timer;
    private float delay;
    private float dmgf;

    //Repelish Health
    public float healthRepelishRate = 0.5f;
    private float healthTimer;
    private float healthDelay;
    private float hfrq;

    [HideInInspector]
    public bool isBurning = false;

    private bool renderingDamage = false;

    private void Awake() {
        renderMaterial = GetComponent<Renderer>();
        originalColor = renderMaterial.material.color;
        dmgf = damageFrequency;
        delay = dmgf;

        hfrq = healthRepelishRate;
        healthDelay = hfrq;

        agent = GameObject.Find("Monster");
        healthBar = GameObject.Find("PlayerHealthBar").GetComponent<HealthBar>();
        reportGenerator = GameObject.Find("ReportGenerator").GetComponent<ReportGenerator>();
        levelManager = GameObject.Find("LevelManager").GetComponent<LevelManager>();
    }

    private void OnCollisionEnter(Collision collision) {
        if (collision.gameObject.tag == "AI") {
            levelManager.ResetStage(-25);

            //GameObject.Find("Player Indicators").GetComponent<CanvasGroup>().alpha = 0;

            //Subtract 8 from the Agent's frustration and the number of player lost to counter the last frame of the game
            //Otherwise if the player is destroyed, the agent registers it as "player lost"
            if (GameObject.Find("Monster").GetComponent<Movement>().targetLastSeen) {
                int playerLost = reportGenerator.currentPlaySession.agentLostPlayer--;
                playerLost = Mathf.Clamp(playerLost,0, reportGenerator.currentPlaySession.agentLostPlayer);
                GameObject.Find("Monster").GetComponent<FrustrationComponent>().levelOfFrustration -= 8;
            }
            reportGenerator.currentPlaySession.playerDied++;
            //Destroy(gameObject);
        }
    }

    private void Update() {
        DetectFire();
        if (health <= 0) {
            levelManager.ResetStage(-25);
            reportGenerator.currentPlaySession.playerDied++;
            //Destroy(gameObject);
        }

        RepelishHealth();

        if (isBurning && !renderingDamage) {
            renderingDamage = true;
            StartCoroutine("RenderTakeDamage");
        }

        if (!isBurning && renderingDamage) {
            renderingDamage = false;
            StopCoroutine("RenderTakeDamage");
            renderMaterial.material.color = originalColor;
        }
    }

    IEnumerator RenderTakeDamage() {
        
        while (true) {
            renderMaterial.material.color = Color.white;
            yield return new WaitForSeconds(0.1f);
            renderMaterial.material.color = originalColor;
            yield return new WaitForSeconds(0.1f);
        }
    }

    void OnDestroy() {
        GameObject.Find("Player Indicators").GetComponent<CanvasGroup>().alpha = 0;
        Destroy(GameObject.Find("PlayerHealthBar"));
        Instantiate(destroyEffect, new Vector3(transform.position.x, transform.position.y, -1), transform.rotation);
    }

    private void DetectFire() {
        if (Physics2D.OverlapCircle(transform.position, 0.5f, fireMask)) {
            timer += Time.deltaTime;
            if (timer > dmgf) {
                health -= fireDamage;
                reportGenerator.currentPlaySession.healthLost += fireDamage;
                dmgf = timer + delay;
            }
            isBurning = true;
        } else {
            isBurning = false;
        }
    }   

    private void RepelishHealth() {
        if (!isBurning) {
            healthTimer += Time.deltaTime;
            if (healthTimer > hfrq) {
                health++;
                hfrq = healthTimer + healthDelay;
            }
        }
        health = Mathf.Clamp(health, 0, 100);
    }

}
