using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EndLevelTrigger : MonoBehaviour {
    public bool toWin;
    public bool toLose;
    private EndLevel endLevelScript;

    void Awake() {
        endLevelScript = GameObject.Find("GoalStatusMonitor").GetComponent<EndLevel>();
    }

    void OnDestroy() {
        if (toWin) {
            endLevelScript.winCondition = true;
        }
        if (toLose) {
            endLevelScript.LoseCondition = true;
        }
    }
}
