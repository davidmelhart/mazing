using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BombTrigger : MonoBehaviour {
    private float timeStamp;

    void OnTriggerEnter(Collider trigger)
    {
        if (trigger.transform.IsChildOf(transform))
        {
            Destroy(gameObject);
        }
    }

    void Start()
    {
        //Timestamp for auto-destruct 20 seconds afer creation
        timeStamp = Time.time + 20f;
    }

    void Update()
    {
        //Clear old, unused triggers
        if (timeStamp <= Time.time)
        {
            Destroy(gameObject);
        }
    }
}
