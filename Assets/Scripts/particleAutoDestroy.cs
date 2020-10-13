using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class particleAutoDestroy : MonoBehaviour {
    private ParticleSystem particles;
	// Use this for initialization
	void Awake () {
        particles = gameObject.GetComponent<ParticleSystem>() as ParticleSystem;
	}
	
	// Update is called once per frame
	void LateUpdate () {
        if (particles != null && particles.particleCount == 0) 
        {
            Destroy(gameObject);
        }
    }
}
