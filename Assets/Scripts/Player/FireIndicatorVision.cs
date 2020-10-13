using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FireIndicatorVision : MonoBehaviour {
    FieldOfView fov;
    CanvasGroup AIindicators;
    ViewIndicator agent;

    private void Awake() {
        fov = GetComponent<FieldOfView>();
        AIindicators = GameObject.Find("Monster Indicators").GetComponent<CanvasGroup>();
        agent = GameObject.Find("Monster/view visualization").GetComponent<ViewIndicator>();
    }

    private void Update() {
        if (fov.targetDetected) {
            agent.detectedByFire = true;
            AIindicators.alpha = 1;
        } else {
            agent.detectedByFire = false;
            AIindicators.alpha = 0; 
        }
    }
}
