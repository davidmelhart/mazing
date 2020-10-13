using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BulletBar : MonoBehaviour {
    private Slider healthBar;
    public GameObject projectileIndicator;
    private GunControls gunControls;
    private GameObject[] projectileIndicators;
    private RectTransform HPbarSize;
    private Transform player;

    private float projectileIndicatorWidth;
    private float HPbarPosition;

    private void Awake() {
        healthBar = GameObject.Find("PlayerHealthBar").GetComponent<Slider>();
        gunControls = GameObject.Find("GunControls").GetComponent<GunControls>();
        HPbarSize = GameObject.Find("PlayerHealthBar").GetComponent<RectTransform>();
        player = GameObject.Find("PlayerController").transform;
        HPbarPosition = healthBar.transform.position.x;
        projectileIndicatorWidth = HPbarSize.sizeDelta.x / gunControls.startingBullets + 8.5f;

        for (int i = 0; i < gunControls.startingBullets; i++) {
            GameObject newProjectileIndicator = Instantiate(projectileIndicator, Vector3.zero, Quaternion.identity) as GameObject;
            newProjectileIndicator.GetComponent<RectTransform>().sizeDelta = new Vector2(projectileIndicatorWidth, HPbarSize.sizeDelta.y);
            newProjectileIndicator.GetComponent<RectTransform>().localScale = HPbarSize.localScale;
            newProjectileIndicator.transform.SetParent(gameObject.transform,false);
        }

        projectileIndicators = GameObject.FindGameObjectsWithTag("projectile-indicator");
    }

    // Update is called once per frame
    void Update () {
       
        for (int i = 0; i < projectileIndicators.Length; i++) {
            Vector3 newPos = new Vector3((healthBar.transform.position.x - 1.35f) + i * projectileIndicatorWidth * 0.0154f, player.position.y - 1.25f, -1);
            projectileIndicators[i].transform.position = newPos;

            if (i >= gunControls.projectileCount) {
                projectileIndicators[i].GetComponent<Slider>().value = 0;
            } else {
                projectileIndicators[i].GetComponent<Slider>().value = 100;
            }
        }
    }
}
