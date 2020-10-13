using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FrustrationComponent : MonoBehaviour {
    //ReportGenerator reportGenerator;

    [Tooltip("Disabling this sets Frustration to the Min Frusation value indefinitely.")]
    public bool frustrationIsActive = true;

    public bool lockFrustration = false;
    [Range(0, 100)]
    public int minFrustration = 0;
    [Range(0, 100)]
    public int maxFrustration = 100;
    [Range(0, 100)]
    public float levelOfFrustration = 0f;

    private GameObject agent;
    private FieldOfView fov;
    private ProximityDetector hearing;
    private Movement movement;
    private float viewSectorArea;

    // Variables to assess frustrating events
    private Vector3[] currentPath;
    private int previousPathLength = 0;
    private bool targetWasVisible;
    private bool targetIsVisible;
    private bool decreasingFrustration = false;

    public bool increaseWithTime = false;
    public float incrementDelay;
    private float timeStamp;
    Health status;
    private int healthLost;

    //Agent original attributes
    private float speed;
    private float coreRotationSpeed;

    void Awake() {
        agent = GameObject.Find("Monster");
        fov = agent.GetComponent<FieldOfView>();
        hearing = agent.GetComponent<ProximityDetector>();
        movement = agent.GetComponent<Movement>();
        status = agent.GetComponent<Health>();
        viewSectorArea = (fov.viewAngle / 360) * Mathf.PI * Mathf.Pow(fov.viewRadius, 2f);
        Debug.Log(viewSectorArea);
        //reportGenerator = GameObject.Find("ReportGenerator").GetComponent<ReportGenerator>();
        speed = movement.speed;
        coreRotationSpeed = movement.coreRotationSpeed;
    }

    void FixedUpdate() {
        //reportGenerator.currentPlaySession.agentAvgFrustration.Add(levelOfFrustration);
        healthLost = 100 - status.health;
        fov.viewAngle = Mathf.Abs(levelOfFrustration - 100) * 1.35f;
        fov.viewRadius = Mathf.Clamp(Mathf.Sqrt((viewSectorArea / (fov.viewAngle / 360)) / Mathf.PI), 0, 20);
        hearing.hearingRadius = 4 + Mathf.Abs(levelOfFrustration - 100) * 0.06f;
        hearing.detectionChance = 10 + Mathf.RoundToInt(Mathf.Clamp(levelOfFrustration * 0.90f, 0, 100));
        movement.lookAroundCycle = Mathf.RoundToInt(Mathf.Abs(levelOfFrustration - 100) * 0.03f);
        movement.coreRotationSpeed = 1 + levelOfFrustration * 0.04f;
        movement.speed = speed + levelOfFrustration * 0.005f;

        movement.riskAversion = Mathf.Abs(levelOfFrustration - 100);

        // Checking for frustrating events here
        CheckPathLengths();
        CheckTargetVisible();
        if (movement.isWaiting && !decreasingFrustration) {
            decreasingFrustration = true;
            StartCoroutine("DecreaseFrustration");
        }
        if (!movement.isWaiting && decreasingFrustration) {
            StopCoroutine("DecreaseFrustration");
            decreasingFrustration = false;
        }
        CheckHealth();

        // Checking for frustration-resolving events here

        // Debug mode - Incremental increase of frustration with time (sec)     
        if (timeStamp <= Time.time && increaseWithTime) {
            timeStamp = Time.time + incrementDelay;
            levelOfFrustration++;
            levelOfFrustration = Mathf.Clamp(levelOfFrustration, healthLost, 95);
        }

        // Keeps frustration 0 (off) for the session
        if (!frustrationIsActive) {
            levelOfFrustration = minFrustration;
        }

        // Correcting min-max setup of frustration
        if (maxFrustration < minFrustration) {
            maxFrustration = minFrustration;
        }
        if (levelOfFrustration < minFrustration) {
            levelOfFrustration = minFrustration;
        }
        if (levelOfFrustration > maxFrustration) {
            levelOfFrustration = maxFrustration;
        }
    }

    void OnTriggerEnter2D(Collider2D collision) {
        if (collision.gameObject.CompareTag("projectile")) {
            levelOfFrustration += 3;
        }
    }

    // Checks if the new path is significantly longer than the previous one, which indicates unexpected obstacles
    private void CheckPathLengths() {
        currentPath = agent.GetComponent<Movement>().path;

        if (previousPathLength > 0) {
            if (currentPath.Length > previousPathLength) {
                Debug.Log("Retracing path, getting frustrated");
                levelOfFrustration += (currentPath.Length - previousPathLength);
            }

            if (currentPath.Length > previousPathLength && targetIsVisible) {
                Debug.Log("Getting closer to the player");
                levelOfFrustration -= (currentPath.Length - previousPathLength) / 2;
            }
        }
        previousPathLength = currentPath.Length;
    }

    // Checks if the player is lost after the agent arrived at the "last seen" location
    private void CheckTargetVisible() {
        targetIsVisible = agent.GetComponent<Movement>().targetLastSeen;

        if (!targetIsVisible && targetWasVisible) {
            Debug.Log("Target lost, getting frustrated");
            //reportGenerator.currentPlaySession.agentLostPlayer++;
            levelOfFrustration += 7;
        }

        if (targetIsVisible && !targetWasVisible) {
            Debug.Log("Spotted the player");
            //reportGenerator.currentPlaySession.agentDetectedPlayer++;
            levelOfFrustration += 3;
        }

        targetWasVisible = targetIsVisible;
    }

    // Clamps the level of frustration to the amount of health lost
    private void CheckHealth() {
        levelOfFrustration = Mathf.Clamp(levelOfFrustration, healthLost * 0.5f, 100);
    }

    IEnumerator DecreaseFrustration() {
        //Debug.Log("Decreasing Frustraion");
        while (true) {
            yield return new WaitForSeconds(0.1f);
            levelOfFrustration -= 0.1f;
        }
    }
}