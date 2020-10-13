using System.Collections.Generic;
using System.Collections;
using UnityEngine;


public class HearingDistance : MonoBehaviour {
    ParticleSystem PS;
    Movement status;
    ProximityDetector hearing;
    FieldOfView fov;
    
    // Use this for initialization
    void Awake () {
        PS = gameObject.GetComponent<ParticleSystem>();
        hearing = GameObject.Find("Monster").GetComponent<ProximityDetector>();
        fov = GameObject.Find("Monster").GetComponent<FieldOfView>();
        status = GameObject.Find("Monster").GetComponent<Movement>();

    }
	
	// Update is called once per frame
	void Update () {
        PS.startLifetime = hearing.hearingRadius / 2;

        // Turned off for now. Testplayers' attention was drawn to the colour change too much.
        //
        //if (hearing.playerDetected || fov.targetDetected || status.targetLastSeen || status.hitRegistered) {
        //    PS.startColor = Color.red;
        //} else {
        //    PS.startColor = Color.white;
        //}
    }
    
}
