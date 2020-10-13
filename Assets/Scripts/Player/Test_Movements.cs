using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Test_Movements : MonoBehaviour {
    public bool autoMoveInDirection;
    public float movementSpeed = 20;
    [Range(-1, 1)]
    public float inputX;
    [Range(-1, 1)]
    public float inputY;

    public bool spawnFire;
    public bool invulnerableToFire;

    public float fireFrequency;
    private float timer;
    private float delay;
    private float fFreq;
    public GameObject fire;
    Quaternion fireRot = Quaternion.LookRotation(Vector3.forward, Vector3.up);

    private Vector2 direction;
    private Rigidbody2D rigidBody2D;
    private PlayerHealth health;

    private void Awake() {
        fFreq = fireFrequency;
        delay = fFreq;
        health = gameObject.GetComponent<PlayerHealth>();
    }

    void Start() {
        rigidBody2D = GetComponent<Rigidbody2D>();
    }

    // Update is called once per frame
    void Update () {
        if (invulnerableToFire) {
            health.health = 100;
        }

        if (autoMoveInDirection) { 
            direction = new Vector2(inputX * movementSpeed, inputY* movementSpeed);
            rigidBody2D.AddForce(direction);
        }
        if (spawnFire) {   
            timer += Time.deltaTime;
            if (timer > fFreq) {
                Instantiate(fire, transform.position, fireRot);
                fFreq = timer + delay;
            }
        }
	}
}
