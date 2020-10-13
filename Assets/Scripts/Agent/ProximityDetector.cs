using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ProximityDetector : MonoBehaviour {
    public LayerMask targetMask;
    public LayerMask obstacleMask;
    public float hearingRadius;
    [Range(0,100)]
    public int detectionChance;
    private int newDetectionChance;
    private int detection;

    [HideInInspector]
    public bool playerDetected;
    [HideInInspector]
    public bool playerInHearingRadius;

    private float frequency = 1f;
    private bool counting = false;
    private bool wallDetected = false;

    private void Awake() {
        newDetectionChance = detectionChance;
    }

    private void FixedUpdate() {
        Collider[] inHearingDistance = Physics.OverlapSphere(transform.position,hearingRadius,targetMask);

        if (inHearingDistance.Length > 0) {
            Transform target = inHearingDistance[0].transform;

            playerInHearingRadius = true;
            Vector3 directionToTarget = (target.position - transform.position).normalized;
            float distanceToTarget = Vector3.Distance(transform.position, target.position);

            if (Physics.Raycast(transform.position, directionToTarget, distanceToTarget, obstacleMask) && !wallDetected) {
                wallDetected = true;
                newDetectionChance = Mathf.RoundToInt((detectionChance-10)/2);
            }
            if (!counting) {
                counting = true;
                StopCoroutine("IncreaseDetectionChance");
                StartCoroutine(IncreaseDetectionChance(frequency));
            }

        } else {
            StopCoroutine("IncreaseDetectionChance");
            newDetectionChance = detectionChance;
            playerDetected = false;
            playerInHearingRadius = false;
            counting = false;
            wallDetected = false;
        }

    }

    IEnumerator IncreaseDetectionChance(float waitTime) {
        while (true) {
            yield return new WaitForSeconds(waitTime);
            detection = Mathf.RoundToInt(Random.Range(0f, 100f));
            if (detection < newDetectionChance) {
                playerDetected = true;
            }
            newDetectionChance++;
        }
    }

    public void OnDrawGizmos() {
        if (transform.GetComponentInParent<Movement>().displayPath) {
                //Gizmos.color = Color.blue;
                //Gizmos.DrawWireSphere(transform.position, hearingRadius);
        }
    }
}
