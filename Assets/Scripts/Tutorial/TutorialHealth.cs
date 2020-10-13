using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TutorialHealth : MonoBehaviour {

    public GameObject destroyEffect;
    public LayerMask fireMask;
    private Renderer renderMaterial;


    [HideInInspector]
    public bool isBurning = false;
    private bool renderingDamage = false;

    Color originalColor;

    private void Awake() {
        renderMaterial = GetComponent<Renderer>();
        originalColor = renderMaterial.material.color;
    }

    private void Update() {
        DetectFire();

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

    IEnumerator RenderHitDamage() {
        renderMaterial.material.color = Color.white;
        yield return new WaitForSeconds(0.1f);
        renderMaterial.material.color = originalColor;
        yield break;
    }

    private void DetectFire() {
        if (Physics2D.OverlapCircle(transform.position, 1f, fireMask)) {
            isBurning = true;
        } else {
            isBurning = false;
        }
    }

    void OnTriggerEnter(Collider collision) {
        if (collision.gameObject.CompareTag("projectile")) {
            StartCoroutine("RenderHitDamage");
        }
    }

}

