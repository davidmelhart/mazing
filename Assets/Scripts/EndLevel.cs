using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EndLevel : MonoBehaviour {

    private LevelManager levelManager;
    ReportGenerator reportGenerator;

    [HideInInspector]
    public bool winCondition = false;
    [HideInInspector]
    public bool LoseCondition = false;

    void Awake() {
        levelManager = GameObject.FindObjectOfType<LevelManager>();
    }

    private void FixedUpdate() {
        if (winCondition) {
            reportGenerator.currentPlaySession.wonTheMatch = true;
        }
        if (LoseCondition) {
            reportGenerator.currentPlaySession.wonTheMatch = false;
        }
 
        if (winCondition || LoseCondition) {
            //levelManager.LoadEndScreen();
            Destroy(gameObject);
        }

        /*if (winCondition) {
            levelManager.LoadLevel("Win");
            Destroy(gameObject);
        }
        if (LoseCondition) {
            levelManager.LoadLevel("Lose");
            Destroy(gameObject);
        }*/
    }
}
