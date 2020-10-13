//using System;
//using System.Collections;
//using System.Collections.Generic;
//using System.Text;
//using System.IO;
//using UnityEngine;

//public class CompileReport : MonoBehaviour {
//    ReportGenerator reportGenerator;
//    Queue<List<string>> reportData;

//    private void Awake() {
//        reportGenerator = GameObject.Find("ReportGenerator").GetComponent<ReportGenerator>();
//        //reportData = reportGenerator.reportData;
//    }

//    public void AddGamePlay() {
//        reportGenerator.AppendSessionData();
//        reportGenerator.WriteGamePlayRecording();
//    }

//    public void WriteFinalFiles() {
//        //reportGenerator.CompileReport();        
//        reportGenerator.WriteGamePlayData();
//        reportGenerator.WriteGamePlayRecording();

//        Destroy(GameObject.Find("ReportGenerator"));
//    }
//}
