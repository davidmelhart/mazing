using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Health : MonoBehaviour {
    //ReportGenerator reportGenerator;
    LevelManager levelManager;

    public GameObject destroyEffect;
    private HealthBar healthBar;
    public LayerMask fireMask;
    private Renderer renderMaterial;

    [Range(1, 100)]
    public int health = 100;
    public float damageFrequency = 0.1f;
    public int fireDamage = 1;
    public int bulletDamage = 2;

    private float timer;
    private float delay;
    private float dmgf;

    [HideInInspector]
    public bool isBurning = false;
    [HideInInspector]
    public bool botDied = false;

    private bool renderingDamage = false;

    private int lastHealth;
    public int deltaHealth;

    Color originalColor;

    //Fix for multiple bullet hits
    private bool bulletColliding = false;

    private void Awake() {
        renderMaterial = GetComponent<Renderer>();
        originalColor = renderMaterial.material.color;
        dmgf = damageFrequency;
        delay = dmgf;
        healthBar = GameObject.Find("HealthBar").GetComponent<HealthBar>();
        //reportGenerator = GameObject.Find("ReportGenerator").GetComponent<ReportGenerator>();
        levelManager = GameObject.Find("LevelManager").GetComponent<LevelManager>();
        lastHealth = health;
    }

    private void Update() {
        deltaHealth = health - lastHealth;
        lastHealth = health;

        bulletColliding = false;

        DetectFire();
        if (health <= 0) {
            botDied = true;
            levelManager.ResetStage(15);
            //reportGenerator.currentPlaySession.agentDied++;
            //Destroy(gameObject);
        }

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

    private void LateUpdate() {
        botDied = false;
    }

    IEnumerator RenderTakeDamage() {
        while (true) {
            renderMaterial.material.color = Color.white;
            yield return new WaitForSeconds(0.1f);
            renderMaterial.material.color = originalColor;
            yield return new WaitForSeconds(0.1f);
        }
    }

    IEnumerator RenderHitDamage() {
        renderMaterial.material.color = Color.white;
        yield return new WaitForSeconds(0.1f);
        renderMaterial.material.color = originalColor;
        yield break;
    }

    void OnDestroy() {
        Destroy(GameObject.Find("HealthBar"));
        Destroy(GameObject.Find("FrustrationBar"));
        Instantiate(destroyEffect, new Vector3(transform.position.x, transform.position.y, -1), transform.rotation);
    }

    private void DetectFire() {
        if (Physics2D.OverlapCircle(transform.position, 1f, fireMask)) {
            timer += Time.deltaTime;
            if (timer > dmgf) {
                health -= fireDamage;

                //reportGenerator.currentPlaySession.agentHealthLost += fireDamage;
                levelManager.score += fireDamage;
                dmgf = timer + delay;
            }
            isBurning = true;
        } else {
            isBurning = false;
        }
    }

    void OnTriggerEnter(Collider collision) {
        if (collision.gameObject.CompareTag("projectile")) {
            if (bulletColliding) {
                return;
            } else {
                bulletColliding = true;
                Debug.Log("Hit");
                StartCoroutine("RenderHitDamage");
                health -= bulletDamage;
                //reportGenerator.currentPlaySession.agentHealthLost += bulletDamage;
                levelManager.score += bulletDamage;
            }
        }
    }

}
