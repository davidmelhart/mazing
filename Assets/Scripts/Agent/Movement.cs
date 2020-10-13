using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Movement : MonoBehaviour {
    public LayerMask unwalkableMask;
    public LayerMask fireMask;
    public Transform target;
    public bool tutorialActive;

    [Range(2, 3f)]
    public float speed = 2f;
    [HideInInspector]
    public float coreRotationSpeed = 1f;
    [Range(1, 5)]
    public float rotationSpeed = 1f;
    [Range(0, 180)]
    public float lookAroundAngle;
    [Range(1, 3)]
    public int lookAroundCycle = 3;
    [Range(1, 100)]
    public float riskAversion = 100;
    [HideInInspector]
    public Vector3[] path;
    [HideInInspector]
    public Vector3[] riskyPath;
    [HideInInspector]
    public Vector3[] safePath;
    int targetIndex;
    public bool displayPath;

    [HideInInspector]
    public Vector3 lastVisibleTargetPosition;
    [HideInInspector]
    public bool targetLastSeen = false;

    ProximityDetector proximityDetector;
    FieldOfView fov;
    Health status;

    [HideInInspector]
    public bool shouldLookAround = true;
    [HideInInspector]
    public bool shouldWait = true;
    [HideInInspector]
    public bool isWaiting = false;
    [HideInInspector]
    public bool hasTarget = false;
    [HideInInspector]
    public bool searching = true;


    private bool turning = false;
    [HideInInspector]
    public bool fleeing = false;

    Vector3 newTarget;
    Vector3 previousPosition = Vector3.zero;
    Vector3 newRandomPosition;
    [HideInInspector]
    public bool breakBeingStuck = false;
    private float previousRotation = 0;
    private float checkCooldown = 2f;
    private float checkTimeStamp = 0f;

    Vector3 hitDirection;
    [HideInInspector]
    public bool hitRegistered = false;
    private GunControls playerGun;

    FrustrationComponent frustration;

    [HideInInspector]
    public int riskTakingFactor;
    [HideInInspector]
    public bool takingRiskyPath = false;

    void Awake() {
        proximityDetector = transform.GetComponent<ProximityDetector>();
        fov = transform.GetComponent<FieldOfView>();
        status = transform.GetComponent<Health>();
        frustration = transform.GetComponent<FrustrationComponent>();

        playerGun = GameObject.Find("GunControls").GetComponent<GunControls>();
        previousPosition = transform.position;
        previousRotation = transform.eulerAngles.z;
    }
    
    void Update() {
        bool stuckAtPosition = (Vector3.Distance(previousPosition, transform.position) < 0.1f);
        bool stuckInRotation = (Mathf.Abs(transform.eulerAngles.z - previousRotation) < 0.1f);

        if (stuckAtPosition && stuckInRotation) {
            Debug.Log("Agent is stuck...");
            breakBeingStuck = true;
            newRandomPosition = GetRandomPoint(proximityDetector.hearingRadius * 2, 180);
        }
        if (checkTimeStamp <= Time.time) {
            checkTimeStamp = Time.time + checkCooldown;
            previousPosition = transform.position;
            previousRotation = transform.eulerAngles.z;
        }

        if (!frustration.frustrationIsActive) {
            rotationSpeed = coreRotationSpeed;
        }

        if (status.isBurning) {
            fleeing = true;
        }

        //If player is detected follow the player
        if ((proximityDetector.playerDetected || fov.targetDetected)) {
            if (frustration.frustrationIsActive) {
                rotationSpeed = coreRotationSpeed * 3;
            }
            if (!hasTarget) {
                hasTarget = true;
                previousRotation = transform.eulerAngles.z;
                previousPosition = transform.position;
            }
            shouldLookAround = false;
            fleeing = false;
            PathRequestManager.RequestPath(transform.position, target.position, OnPathFound);
            lastVisibleTargetPosition = target.position;
            targetLastSeen = true;
            //If player is lost, travel to the last known location
        } else if ((!proximityDetector.playerDetected || !fov.targetDetected) && targetLastSeen) {
            if (frustration.frustrationIsActive) {
                rotationSpeed = coreRotationSpeed;
            }
            if (Physics2D.OverlapCircle(lastVisibleTargetPosition, 0.5f, fireMask) != null) {
                Debug.Log("Fire!");
                if (!hasTarget) {
                    hasTarget = true;
                }
                Debug.DrawLine(transform.position, newRandomPosition, Color.magenta);
                PathRequestManager.RequestPath(transform.position, newRandomPosition, OnPathFound);
            } else {
                shouldLookAround = false;
                fleeing = false;
                Debug.DrawLine(transform.position, lastVisibleTargetPosition, Color.cyan);
                PathRequestManager.RequestPath(transform.position, lastVisibleTargetPosition, OnPathFound);
            }  
            //If burning, flee from the fire to a new random location
        } else if (fleeing && !breakBeingStuck) {
            if (frustration.frustrationIsActive) {
                rotationSpeed = coreRotationSpeed * 2;
            }
            if (!hasTarget) {
                hasTarget = true;
                Debug.Log("Getting new target");
                GetNewTarget();
                previousRotation = transform.eulerAngles.z;
                previousPosition = transform.position;
            }
            shouldLookAround = false;
            PathRequestManager.RequestPath(transform.position, newTarget, OnPathFound);
            //If hit with a bullet, investigate the direction to the extent of the current hearing distance
        } else if (hitRegistered) {
            if (frustration.frustrationIsActive) {
                rotationSpeed = coreRotationSpeed * 3;
            }
            if (!hasTarget) {
                hasTarget = true;
                lastVisibleTargetPosition = hitDirection;
            }
            Debug.DrawLine(transform.position, hitDirection, Color.black);
            PathRequestManager.RequestPath(transform.position, lastVisibleTargetPosition, OnPathFound);
            //If the agent is stuck at a location it moves to a random position within hearing distance
        } else if (breakBeingStuck && !tutorialActive) {
            if (frustration.frustrationIsActive) {
                rotationSpeed = coreRotationSpeed * 2;
            }
            Debug.Log("...attempting to change position.");
            StopAllCoroutines();
            if (!hasTarget) {
                hasTarget = true;
            }
            Debug.DrawLine(transform.position, newRandomPosition, Color.magenta);
            PathRequestManager.RequestPath(transform.position, newRandomPosition, OnPathFound);
            return;
        //If has nothing to do, commence search behaviour
        } else {
            if (frustration.frustrationIsActive) {
                rotationSpeed = coreRotationSpeed;
            }
            if (!tutorialActive) {
                searching = true;
                SearchBehaviour();
            }
        }
        //Correcting position z index
        transform.position = new Vector3(transform.position.x,transform.position.y, 0);


        if (tutorialActive) {
            speed = 0.5f;
        }
    }
    
    void OnTriggerEnter(Collider collision) {
        if (collision.gameObject.CompareTag("projectile")) {
            hitDirection = transform.position + ((playerGun.firePosition - transform.position).normalized * (target.position - transform.position).magnitude / 2);
            hitRegistered = true;
        }
    }
    
    public void OnPathFound(Vector3[] newPath, Vector3[] newRiskyPath, bool pathSuccessful) {
        if (pathSuccessful) {
            path = FindBestPath(newPath, newRiskyPath);
            Debug.Log("Path found!");
            StopAllCoroutines();
            StartCoroutine("FollowPath");            
        }
    }

    Vector3[] FindBestPath(Vector3[] safePath, Vector3[] riskyPath) {
        if (riskyPath != null && displayPath) {
            for (int i = targetIndex; i < riskyPath.Length; i++) {
                if (i == targetIndex) {
                    Debug.DrawLine(transform.position, riskyPath[i], Color.red);
                } else {
                    Debug.DrawLine(riskyPath[i - 1], riskyPath[i], Color.red);
                }
            }
        }
        if (safePath != null && displayPath) {
            for (int i = targetIndex; i < safePath.Length; i++) {
                if (i == targetIndex) {
                    Debug.DrawLine(transform.position, safePath[i], Color.green);
                } else {
                    Debug.DrawLine(safePath[i - 1], safePath[i], Color.green);
                }
            }
        }
        riskTakingFactor = Mathf.RoundToInt(Mathf.Pow(2, riskAversion / 20)) + Mathf.Clamp(Mathf.Abs(100 - status.health) / 10, 0, 10);

        if (safePath.Length - riskyPath.Length > riskTakingFactor) {
            takingRiskyPath = true;
            return riskyPath;
        } else {
            takingRiskyPath = false;
            return safePath;
        }
    } 

    IEnumerator FollowPath() {
        Vector3 currentWaypoint;
        if (path.Length <= 0) {
            Debug.Log("Path Ends.");
            StopAllCoroutines();
            shouldLookAround = true;
            shouldWait = true;
            hasTarget = false;
            targetLastSeen = false;
            hitRegistered = false;
            takingRiskyPath = false;
            breakBeingStuck = false;
            fleeing = false;
            searching = false;
            yield break;
        }     
        
        while (true) {
            currentWaypoint = path[0];
            if (transform.position == currentWaypoint) {
                targetIndex++;
                if(targetIndex >= path.Length) {
                    yield break;
                }
                currentWaypoint = path[targetIndex];
            }

            Vector3 waypoint = new Vector3(currentWaypoint.x, currentWaypoint.y, transform.position.z);

            Quaternion rot = Quaternion.LookRotation(waypoint - transform.position, Vector3.back);
            transform.rotation = Quaternion.RotateTowards(transform.rotation, rot, rotationSpeed*2);
            transform.eulerAngles = new Vector3(0, 0, transform.eulerAngles.z);
            transform.position = Vector3.MoveTowards(transform.position, waypoint, speed * Time.deltaTime);
            //Correcting position z index
            transform.position = new Vector3(transform.position.x, transform.position.y, 0);
            yield return null;
        }
    }

    void SearchBehaviour () {
        if (shouldWait) {
            StopCoroutine("Wait");
            StartCoroutine("Wait", lookAroundCycle);
        }
        
        if (!isWaiting && !shouldWait) {
            if (!hasTarget) {
                hasTarget = true;
                Debug.Log("Getting new target");
                GetNewTarget();
            }
            PathRequestManager.RequestPath(transform.position, newTarget, OnPathFound);
        }
    }

    IEnumerator Wait(float cycles) {
        shouldWait = false;
        Debug.Log("Searching for target...");
        isWaiting = true;

        StopCoroutine("OscillateView");
        int cyclesPassed = 0;
        while (cyclesPassed < cycles) {

            if (shouldLookAround) {
                StartCoroutine(OscillateView(lookAroundAngle, true, true));
            }
            yield return new WaitUntil(() => shouldLookAround == true);
            cyclesPassed++;
        }
        StopCoroutine("OscillateView");
        isWaiting = false;
        yield break;
    }
    
    IEnumerator OscillateView(float angle, bool do180 = false, bool doOnce = false) {
        shouldLookAround = false;

        StartCoroutine(HandleTurning(transform.rotation, angle));
        while (true) {
            if (!turning) {
                StartCoroutine(HandleTurning(transform.rotation, angle * 2));
            }
            yield return new WaitUntil(() => turning == false);
            if (!turning) {
                StartCoroutine(HandleTurning(transform.rotation, angle * -2));
            }
            yield return new WaitUntil(() => turning == false);

            if (do180) {
                if (!turning) {
                    StartCoroutine(HandleTurning(transform.rotation, 180 + angle));
                }
                yield return new WaitUntil(() => turning == false);
            }
            if (doOnce) {
                shouldLookAround = true;
                yield break;
            }
        }
    }

    IEnumerator HandleTurning(Quaternion rotation, float angle) {
        turning = true;
        while (true) {
            Quaternion newRotation = rotation * Quaternion.Euler(0, 0, angle);
            transform.rotation = Quaternion.RotateTowards(transform.rotation, newRotation, rotationSpeed);
            transform.eulerAngles = new Vector3(0, 0, transform.eulerAngles.z);
            if (Mathf.Approximately(transform.eulerAngles.z, newRotation.eulerAngles.z)) {
                turning = false;
                yield break;
            }
            yield return new WaitForEndOfFrame();
        }
    }

    // This function gets a random point from the circumference of a sector twice the size of the given angle
    Vector3 RandomPointAtDistance(float radius, float angle) {
        float randomAngle = Random.Range(transform.eulerAngles.z + 90 - angle, transform.eulerAngles.z + 90 + angle) * (Mathf.PI / 180);

        float x = Mathf.Clamp(transform.position.x + radius * Mathf.Cos(randomAngle), -18, 18);
        float y = Mathf.Clamp(transform.position.y + radius * Mathf.Sin(randomAngle), -13, 13);

        return new Vector3(x, y, transform.position.z);
    }

    // Builds a stack of random points
    Vector3 GetRandomPoint(float radius, float angle) {
        Stack<Vector3> PointStack = new Stack<Vector3>();
        for (int i = 0; i < 50; i++) {
            PointStack.Push(RandomPointAtDistance(radius, angle));
        }
        for (int j = 0; j < 50; j++) {
            PointStack.Push(RandomPointAtDistance(radius, angle + 180));
        }
        Vector3 randomPoint = PointStack.Pop();

        for (int stackIndex = 0; stackIndex < PointStack.Count; stackIndex++) {
            if (PointIsFree(randomPoint)) {
                return randomPoint;
            } else {
                randomPoint = PointStack.Pop();
            }
        }
        return previousPosition;
    }

    // Checks if a given point is free
    bool PointIsFree(Vector3 point) {
        return !(Physics.CheckSphere(point, 1.5f, unwalkableMask));
    }

    // Gets new target
    Vector3 GetNewTarget() {   
        Vector3 randomPosition = GetRandomPoint(fov.viewRadius, fov.viewAngle);

        RaycastHit hit;
        if (Physics.Raycast(transform.position, randomPosition, out hit, fov.viewRadius, unwalkableMask)) {
            Vector3 distance = new Vector3(hit.point.x, hit.point.y, transform.position.z) - transform.position;
            newTarget = transform.position + (distance.normalized * (distance.magnitude - 2f));
            return newTarget;
        } else {
            newTarget = randomPosition;
            return newTarget;
        }
    }

    public void OnDrawGizmos() {
        if (path != null && displayPath) {
            for (int i=targetIndex; i < path.Length; i++) {
                Gizmos.color = Color.black;

                if (i == targetIndex) {
                    Gizmos.DrawLine(transform.position, path[i]);
                } else {
                    Gizmos.DrawLine(path[i - 1], path[i]);
                }
                //Gizmos.color = Color.blue;
                //Gizmos.DrawCube(path[i], new Vector3(0.2f,0.2f));
            }
        }
    }

}
