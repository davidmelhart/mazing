using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;


public class EndLevelScreen : MonoBehaviour {
    SpriteRenderer background;
    ReportGenerator reportGenerator;
    Text levelIndicator;
    Text scoreBoard;
    
    void Awake() {
        Debug.Log("Background Loaded.");
        Scene scene = SceneManager.GetActiveScene();
        if (scene.name != "Tutorial") {
            //reportGenerator = GameObject.Find("ReportGenerator").GetComponent<ReportGenerator>();
            levelIndicator = GameObject.Find("LevelIndicator").GetComponent<Text>();
            //levelIndicator.text = ("level " + (4 - reportGenerator.possibleLevels.Count) + "/4");

            scoreBoard = GameObject.Find("Score").GetComponent<Text>();
            //scoreBoard.text = reportGenerator.currentPlaySession.score.ToString();
        }
        

        /*Turned off as there are no win or lose conditions in the current build
         * 
        background = GameObject.Find("Background Screen").GetComponent<SpriteRenderer>();
                Sprite backgroundImage = new Sprite();
        if (reportGenerator.currentPlaySession.wonTheMatch == true) {
            backgroundImage = Resources.Load("win", typeof(Sprite)) as Sprite;
        } else {
            backgroundImage = Resources.Load("lose", typeof(Sprite)) as Sprite;
        }
        background.sprite = backgroundImage;
        */
    }
}
