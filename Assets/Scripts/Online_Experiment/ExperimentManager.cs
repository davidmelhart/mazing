using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using UnityEngine.Events;
using UnityEngine.Networking;
using System.Runtime.InteropServices;
using MoreMountains.CorgiEngine;

public class ExperimentManager : MonoBehaviour
{
    [DllImport("__Internal")]
    private static extern void InitRecord();
    [DllImport("__Internal")]
    private static extern void StartRecord();
    [DllImport("__Internal")]
    private static extern void StopRecord();

    public Text endCountdown;
    public int endCountdownTimer;
    public string nextLevel;
    public bool lastLevel;
    public bool recordLevel;

    private void Awake() {
        if (recordLevel) {
            InitRecord();
        }
    }

    void Start() {
        LevelManager.Instance.OnGameEnds.AddListener(EndLevel);
        if (recordLevel) {
            LevelManager.Instance.OnGameStarts.AddListener(StartRecord);
        }
    }

    private void EndLevel() {
        endCountdown.gameObject.SetActive(true);
        StartCoroutine(EndLevelProcess());
    }

    /// <summary>
    /// Starts the game shutdown coroutine.
    /// </summary>
    /// <returns>yield enumerator</returns>
    public IEnumerator EndLevelProcess() {
        LevelManager.Instance.FreezeCharacters();
        GameObject.FindWithTag("Player").SetActive(false);
        if (!lastLevel) {
            for (int i = endCountdownTimer; i > -1; i--) {
                endCountdown.text = String.Format("Next level loads in {0} sec...", i);
                if (i <= 0) {
                    SceneManager.LoadScene(nextLevel);
                }
                yield return new WaitForSeconds(1f);
            }
        } else {
            for (int i = endCountdownTimer; i > -1; i--) {
                endCountdown.text = String.Format("Game ends in {0} sec...", i);
                if (i <= 0) {
                    if (recordLevel) {
                        endCountdown.text = String.Format("Please stand by... \n Your gameplay is being submitted...");
                        StopRecord();
                    }
                }
                yield return new WaitForSeconds(1f);
            }
        }
    }
}
