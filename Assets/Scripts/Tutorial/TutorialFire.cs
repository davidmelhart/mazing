using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TutorialFire : MonoBehaviour {
    public float dieOutTime = 5f;
    private float timeStamp;

    public float lightRadius;
    public GameObject destroyEffect;

    void Start () {
        timeStamp = Time.time + dieOutTime;
    }
	
	// Update is called once per frame
	void Update () {
		if(timeStamp <= Time.time)
        {
            Destroy(gameObject);
        }
	}

    void OnDestroy() {
        Instantiate(destroyEffect, new Vector3(transform.position.x, transform.position.y, -1), transform.rotation);
    }
}
