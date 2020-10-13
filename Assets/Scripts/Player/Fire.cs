using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Fire : MonoBehaviour {
    public float dieOutTime = 5f;
    private float timeStamp;
    private FieldOfView lightSource;

    public float lightRadius;
    public GameObject destroyEffect;

    private void Awake() {
        lightSource = GetComponent<FieldOfView>();
    }

    void Start () {
        timeStamp = Time.time + dieOutTime;
        StartCoroutine(IncreaseLight());
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

    IEnumerator IncreaseLight() {
        float lightSize = 0;
        while (true) {
            if (lightSize < lightRadius) {
                lightSource.viewRadius += 0.2f;
                lightSize += 0.2f;
                yield return new WaitForSeconds(0.0001f);
            } else {
                yield break;
            }
        }
    }
}
