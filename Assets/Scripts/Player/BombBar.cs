using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BombBar : MonoBehaviour {
    private Slider healthBar;
    public GameObject bombIndicator;
    private GunControls gunControls;
    private GameObject[] bombIndicators;
    private RectTransform HPbarSize;
    private Transform player;

    private float bombIndicatorWidth;
    private float HPbarPosition;

    private void Awake() {
        healthBar = GameObject.Find("PlayerHealthBar").GetComponent<Slider>();
        gunControls = GameObject.Find("GunControls").GetComponent<GunControls>();
        HPbarSize = GameObject.Find("PlayerHealthBar").GetComponent<RectTransform>();
        player = GameObject.Find("PlayerController").transform;
        HPbarPosition = healthBar.transform.position.x;
        bombIndicatorWidth = HPbarSize.sizeDelta.x / gunControls.startingBombs + 20.5f;

        for (int i = 0; i < gunControls.startingBombs; i++) {
            GameObject newProjectileIndicator = Instantiate(bombIndicator, Vector3.zero, Quaternion.identity) as GameObject;
            newProjectileIndicator.GetComponent<RectTransform>().sizeDelta = new Vector2(bombIndicatorWidth, HPbarSize.sizeDelta.y);
            newProjectileIndicator.GetComponent<RectTransform>().localScale = HPbarSize.localScale;
            newProjectileIndicator.transform.SetParent(gameObject.transform,false);
        }

        bombIndicators = GameObject.FindGameObjectsWithTag("bomb-indicator");
    }

    // Update is called once per frame
    void Update () {
        
        for (int i = 0; i < bombIndicators.Length; i++) {
            Vector3 newPos = new Vector3((healthBar.transform.position.x - 1.1f) + i * bombIndicatorWidth * 0.015f, player.position.y - 1.5f, -1);
            bombIndicators[i].transform.position = newPos;

            if (i >= gunControls.bombCount) {
                bombIndicators[i].GetComponent<Slider>().value = 0;
            } else {
                bombIndicators[i].GetComponent<Slider>().value = 100;
            }
        }

    }
}
