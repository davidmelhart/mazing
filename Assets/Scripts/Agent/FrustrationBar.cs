using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine;

public class FrustrationBar : MonoBehaviour {
    private Slider frustrationBar;
    Transform agent;

    void Awake() {
        frustrationBar = GetComponent<Slider>();
        agent = GameObject.Find("Monster").transform;
    }

    void Update() {
        frustrationBar.value = agent.GetComponent<FrustrationComponent>().levelOfFrustration;
        transform.position = new Vector3(agent.position.x, agent.position.y - 1.9f, -1);
    }
}