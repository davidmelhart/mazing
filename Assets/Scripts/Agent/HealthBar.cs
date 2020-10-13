using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine;

public class HealthBar : MonoBehaviour {
    private Slider healthBar;
    private Health health;
    Transform agent;

    void Awake () {
        healthBar = GetComponent<Slider>();
        agent = GameObject.Find("Monster").transform;
        health = agent.GetComponent<Health>();
    }
	
	void Update () {
        healthBar.value = health.health;
        transform.position = new Vector3 (agent.position.x, agent.position.y-1.5f, -1);      
    }
}
