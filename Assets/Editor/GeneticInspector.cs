using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.IO;

public class GeneticInspector : EditorWindow {

	List<string> display = new List<string>();
	Vector2 scrollPos = Vector2.zero;
	[MenuItem("Window/Genetic Inspecttor")]

	public static void ShowWindow() {
		EditorWindow.GetWindow(typeof(GeneticInspector));
	}

	private void OnGUI() {
		if (GameManager.AccessInstance() == null)
			GUILayout.Label("Game not running, UI disabled.", EditorStyles.boldLabel);
		else {
			EditorGUILayout.BeginHorizontal();
			if (GUILayout.Button("Update")) {
				//Gather data
				display = new List<string>();
				GeneticHandler gH = GameManager.GeneticHandler;
				display.Add("Count: " + gH.population.Count + "\tTime:" + (Time.time - gH.timeStamp) + "\t Full Time: " + (Time.time - gH.originTimeStamp) + "\tAllTrials: " + gH.trialsMasterCount + "\tTPS:" + (gH.trialsMasterCount / (Time.time - gH.originTimeStamp)));
				//pull strings for all the info
				for (int i = 0; i < gH.population.Count; i++) {
					GeneticHandler.GeneSequence item = gH.population[i];
					display.Add(i+": "+item.returned + "/"+ item.sent + " @ "+ item.ToString());
				}
			}
			if (GUILayout.Button("DumpPopulation")) {
				//Count all the current stats files
				string fileName = "PopDump";
				string fileExtenstion = ".txt";
				int num = 0;
				while (File.Exists(Application.persistentDataPath + fileName + num + fileExtenstion)) {
					num++;
				}
				Debug.LogError("Logged to : \n"+Application.persistentDataPath + fileName + num + fileExtenstion);

				//Prepare the file string
				string output = "= new List<GeneSequence>(\nnew GeneSequence[] {\n";
				foreach (GeneticHandler.GeneSequence gS in GameManager.GeneticHandler.population) {
					output += "(GeneticSequence)new int[] {";
					for (int i = 0; i < gS.genes.Length; i++) {
						output += ""+gS.genes[i];
						if (i != gS.genes.Length - 1)
							output += ",";
						output += " ";
					}
					output += "},\n";
				}
				output += "\n}\n);";
				//Only save the file if we have something to report.
				if (output != "")
					File.WriteAllText(Application.persistentDataPath + fileName + num + fileExtenstion, output);
			}
			EditorGUILayout.EndHorizontal();

			scrollPos = EditorGUILayout.BeginScrollView(scrollPos);
			//Draw most recent data
			for (int i = 0; i < display.Count; i++) {
				GUILayout.Label(display[i], EditorStyles.label);
			}
			EditorGUILayout.EndScrollView();

		}
	}
}
