using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class GeneticHandler {
	int geneSize = 11;
	int initPopSize = 32;
	int trials = 5000;
	float mutationRate = 0.05f;
	int fluctationRange = 5;
	int convergenceThreshold = 3;
	//Create initial population.
	//Compute fitness of init pop.
	//Loop
	//	//Select the best two
	//	//Crossover for new pop
	//	//Mutation
	//	//Compute fitness
	//until Pop has converged
	//Stop, print results.
	public List<GeneSequence> population = new List<GeneSequence>(
				new GeneSequence[] {
		(GeneSequence)new int[] { 6,8,3,1,0,2,0,0,0,4,15 },
		(GeneSequence)new int[] { 6,7,0,1,1,2,0,0,0,2,13 },
		(GeneSequence)new int[] { 5,3,1,4,1,2,3,1,0,7,12 },
		(GeneSequence)new int[] { 6,2,1,1,1,2,0,0,0,4,13 },
		(GeneSequence)new int[] { 6,8,3,1,1,2,0,0,0,4,15 },
		(GeneSequence)new int[] { 6,2,1,1,1,2,0,-1,0,4,13},
		(GeneSequence)new int[] { 6,2,3,1,1,2,0,0,0,4,15 },
		(GeneSequence)new int[] { 5,2,1,0,1,2,0,0,0,4,12 },
		(GeneSequence)new int[] { 6,8,1,1,1,2,0,0,0,4,10 },
		(GeneSequence)new int[] { 6,7,3,1,1,2,0,0,0,4,15 },
		(GeneSequence)new int[] { 6,2,3,1,1,2,0,0,0,4,15 },
		(GeneSequence)new int[] { 5,7,1,1,1,2,3,0,0,4,10 },
		(GeneSequence)new int[] { 5,7,1,1,1,2,0,0,0,4,10 },
		(GeneSequence)new int[] { 5,7,1,1,1,2,0,0,0,4,10 },
		(GeneSequence)new int[] { 5,0,1,1,1,2,0,0,0,4,10 },
		(GeneSequence)new int[] { 5,0,1,0,1,2,3,0,0,4,10 },
		(GeneSequence)new int[] { 5,7,1,1,1,2,0,0,0,4,10 },
		(GeneSequence)new int[] { 6,2,1,1,1,2,3,0,0,4,12 },
		(GeneSequence)new int[] { 5,7,1,1,1,2,0,0,0,4,10 },
		(GeneSequence)new int[] { 5,3,1,0,1,2,3,0,0,4,12 },
		(GeneSequence)new int[] { 5,0,1,1,1,2,0,0,0,3,10 },
		(GeneSequence)new int[] { 6,7,1,1,0,0,0,0,0,2,13 },
		(GeneSequence)new int[] { 6,8,1,0,1,2,0,0,0,4,12 },
		(GeneSequence)new int[] { 6,2,1,1,1,2,0,0,0,4,13 },
		(GeneSequence)new int[] { 5,2,3,1,1,2,0,0,0,4,10 },
		(GeneSequence)new int[] { 5,0,1,1,1,2,0,0,0,3,10 },
		(GeneSequence)new int[] { 6,7,3,1,0,2,0,0,0,4,15 },
		(GeneSequence)new int[] { 5,2,1,0,1,2,3,0,0,4,12 },
		(GeneSequence)new int[] { 5,2,1,0,1,2,3,0,0,4,12 },
		(GeneSequence)new int[] { 5,2,1,1,1,2,3,0,0,4,12 },
		(GeneSequence)new int[] { 5,2,3,1,1,2,0,0,0,4,15 },
		(GeneSequence)new int[] { 6,8,3,1,1,2,0,0,0,4,10 },
		(GeneSequence)new int[] { 6,8,3,1,1,2,0,0,0,4,13 },
		(GeneSequence)new int[] { 6,2,1,1,1,2,3,1,0,7,13 },
		(GeneSequence)new int[] { 6,8,3,1,1,2,0,-1,0,4,13},
		(GeneSequence)new int[] { 6,2,1,1,1,2,0,0,0,4,15 },
		(GeneSequence)new int[] { 6,8,1,1,1,2,0,0,0,4,15 },
		(GeneSequence)new int[] { 6,7,1,1,1,2,3,0,0,4,10 },
		(GeneSequence)new int[] { 5,7,1,1,1,2,0,0,0,4,10 },
		(GeneSequence)new int[] { 5,0,1,0,1,2,0,0,0,4,10 },
		(GeneSequence)new int[] { 6,7,1,1,1,2,0,0,0,4,10 },
		(GeneSequence)new int[] { 5,7,1,1,1,2,3,0,0,4,12 },
		(GeneSequence)new int[] { 6,7,1,1,1,0,0,0,0,2,13 },
		(GeneSequence)new int[] { 6,8,1,1,1,2,0,0,0,4,12 },
		(GeneSequence)new int[] { 5,2,1,1,1,2,0,0,0,4,10 },
		(GeneSequence)new int[] { 6,2,1,1,0,2,3,0,0,4,15 },
		(GeneSequence)new int[] { 5,2,1,1,1,2,3,0,0,4,12 },
		(GeneSequence)new int[] { 6,2,3,1,1,2,0,0,0,4,15 }
		}
		);
	public float timeStamp = 0;
	public float originTimeStamp = 0;
	volatile bool reportLockout = false;
	volatile bool getLockout = false;
	public long trialsMasterCount = 0;

	public IEnumerator Log(int[] type, bool? wonGame) {
		trialsMasterCount++;
		while (reportLockout)
			yield return null;
		reportLockout = true;
		GeneSequence ret = population.Find((GeneSequence gs) => gs.Compare((GeneSequence)type) && gs.returned < gs.sent);
		//If the returned game was null, that means it did not happen properly, lower this sent amount by one.
		if (wonGame == null) {
			ret.sent--;
		}
		else {
			//The sequence may have been discarded. (This may no longer be necessary since we wait for trials to complete)
			if (ret != null) {
				if ((bool)wonGame)
					ret.successes++;
				ret.returned++;
				//Debug.Log("Log: " + ret.ToString());
			}
		}
		reportLockout = false;
	}


	public IEnumerator GetSpread(System.Action<int[]> callback) {

		while (getLockout)
			yield return null;
		getLockout = true;

		if (population == null) {
			population = new List<GeneSequence>();
			//Generate initial spread
			while (population.Count < initPopSize * 2) {
				//Randomly generate one
				int[] spread = new int[geneSize];
				int[] defaultSpread = (int[])GameManager.DefaultPointSpread;
				//Fill with random values
				for (int i = 0; i < spread.Length; i++) {
					spread[i] = Mathf.Max(0, defaultSpread[i] + Random.Range(-fluctationRange - 1, fluctationRange + 1));
				}
				//Check if it is the same as one already in there, if so, ditch it.
				bool isDuplicate = false;
				foreach (GeneSequence sequence in population) {
					if (isDuplicate == false)
						isDuplicate = sequence.Compare((GeneSequence)spread);
				}
				if (isDuplicate == false) {
					//Debug override always use default point spread
					//population.Add((GeneSequence)defaultSpread);
					population.Add((GeneSequence)spread);
				}
			}
			timeStamp = Time.time;
			originTimeStamp = Time.time;
			Debug.Log("Simulation start");
			for (int i = 0; i < population.Count; i++) {
				Debug.Log("Init: " + population[i].ToString());
			}
		}
		GeneSequence ret = population.Find((GeneSequence gs) => gs.sent < trials);
		if (ret == null) {
			Debug.LogWarning("All trials sent." + (Time.time - timeStamp));

			//Wait for all trials to complete
			foreach (GeneSequence item in population) {
				while (item.returned < item.sent)
					yield return null;
			}

			Debug.LogWarning("Trial Complete." + (Time.time - timeStamp));
			timeStamp = Time.time;


			//Sort the old population by how well they performed
			population.Sort((GeneSequence a, GeneSequence b) => {
				float aVal = (float)a.successes / (float)a.returned;
				float bVal = (float)b.successes / (float)b.returned;
				if (aVal < bVal) {
					return -1;
				}
				else if (aVal == bVal) {
					return 0;
				}
				else {
					return 1;
				}
			});
			//Trim the worst ones
			while (population.Count > initPopSize) {
				population.RemoveAt(0);
			}
			float lowest = population[0].successes / (float)population[0].returned;
			float highest = population[population.Count - 1].successes / (float)population[population.Count - 1].returned;
			population.Reverse();
			for (int i = 0; i < population.Count; i++) {
				Debug.Log("C Result: " + population[i].ToString());
			}

			int diffCount = 0;
			for (int i = 0; i < population.Count; i++) {
				for (int k = 0; k < population.Count; k++) {
					if (population[i].Compare(population[k]) == false)
						diffCount++;
				}
			}	
				
				
			if (diffCount < convergenceThreshold * initPopSize) {

				//We have converged
				Debug.LogError("Convergence "+diffCount+". Total Time: " + (Time.time - originTimeStamp));
				Debug.Break();
				Application.Quit();
			}
			else {
				//Randomize the population
				int n = population.Count;
				while (n > 1) {
					n--;
					int k = Random.Range(0, n + 1);
					GeneSequence value = population[k];
					population[k] = population[n];
					population[n] = value;
				}

				List<GeneSequence> children = new List<GeneSequence>();
				//Go in pairs and breed new children.
				for (int i = 0; i < population.Count; i += 2) {
					if (i + 1 < population.Count) {
						GeneSequence a = population[i];
						GeneSequence b = population[i+1];
						int[] childArray = new int[a.genes.Length];
						//Get a random value from each parent
						for (int k = 0; k < childArray.Length; k++) {
							childArray[k] = (Random.value < 0.5f) ? a.genes[k] : b.genes[k];
						}
						//Mutate
						if (Random.value < mutationRate) {
							Debug.Log("Mutation");
							//Get a random position
							int pos = Random.Range(0, childArray.Length);
							//Shift it up or down
							if (Random.value < 0.5f) childArray[pos]++; else childArray[pos]--;
						}
						children.Add((GeneSequence)childArray);
					}
				}
				//Add t he children to the population
				foreach (GeneSequence child in children) {
					population.Add(child);
				}
			}

			for (int i = 0; i < population.Count; i++) {
				population[i].sent = 0;
				population[i].returned = 0;
				population[i].successes = 0;
			}

			//select the first thing
			ret = population[0];
		}
		ret.sent++;
		//Select an sequence with not enough trials.
		callback(ret.genes);
		getLockout = false;
	}


	public class GeneSequence {
		public int sent = 0;
		public int returned = 0;
		public int successes = 0;
		public int[] genes;
		public GeneSequence(int[] genes) {
			this.genes = genes;
		}

		public int this[int i] {
			get {
				return genes[i];
			}
			set {
				genes[i] = value;
			}
		}

		public bool Compare(GeneSequence other) {
			for (int i = 0; i < genes.Length; i++) {
				if (other[i] != genes[i])
					return false;
			}
			return true;
		}

		public void swap(int a, int b) {
			int temp = genes[a];
			genes[a] = genes[b];
			genes[b] = temp;
		}

		public override string ToString() {
			return ((float)successes/(float)returned) + ":" + (new PointSpread(genes)).ToString();
		}

		//Allow for an T[] to just convert to this.
		public static explicit operator GeneSequence(int[] d) => new GeneSequence(d);
	}
}
