//using System.Collections;
//using System.Collections.Generic;
//using UnityEngine;
//using UnityEngine.UI;

//public class CompileAnswers : MonoBehaviour {
//    ReportGenerator reportGenerator;
//    ReportGenerator.GamePlaySession currentSurvey;

//    Toggle[] q1;
//    Toggle[] q2;

//    int agentQ;
//    int playerQ;

//    private void Awake() {
//        reportGenerator = GameObject.Find("ReportGenerator").GetComponent<ReportGenerator>();
//        currentSurvey = reportGenerator.currentPlaySession;
//    }

//    public void CompileSurvey() {
//       q1 = GameObject.Find("Q1").GetComponentsInChildren<Toggle>();
//       for (int i = 0; i < q1.Length; i++) {
//            if (q1[i].isOn) {
//                agentQ = i+1;
//            }
//       }

//        q2 = GameObject.Find("Q2").GetComponentsInChildren<Toggle>();
//        for (int i = 0; i < q2.Length; i++) {
//            if (q2[i].isOn) {
//                playerQ = i+1;
//            }
//        }

//        switch(agentQ) {
//            case 1:
//                reportGenerator.currentPlaySession.agentMoreFrustrated = 1;
//                break;
//            case 2:
//                reportGenerator.currentPlaySession.agentLessFrustrated = 1;
//                break;
//            case 3:
//                reportGenerator.currentPlaySession.agentBothFrustrated = 1;
//                break;
//        }

//        switch (playerQ) {
//            case 1:
//                reportGenerator.currentPlaySession.playerMoreFrustrated = 1;
//                break;
//            case 2:
//                reportGenerator.currentPlaySession.playerLessFrustrated = 1;
//                break;
//            case 3:
//                reportGenerator.currentPlaySession.playerBothFrustrated = 1;
//                break;
//        }
//        //reportGenerator.currentPlaySession = currentSurvey;
//    }
//}
