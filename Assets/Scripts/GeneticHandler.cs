using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class GeneticHandler {
	int geneSize = 11;
	int initPopSize = 32;
	int trials = 100;
	float mutationRate = 0.02f;

	float convergenceThreshold = 0.01f;
	//Create initial population.
	//Compute fitness of init pop.
	//Loop
	//	//Select the best two
	//	//Crossover for new pop
	//	//Mutation
	//	//Compute fitness
	//until Pop has converged
	//Stop, print results.
	List<GeneSequence> population;

	volatile bool lockout = false;

	public IEnumerator Log(int[] type, bool wonGame) {
		
		while (lockout)
			yield return null;
		lockout = true;

		GeneSequence ret = population.Find((GeneSequence gs) => gs.Compare((GeneSequence)type));
		if (wonGame)
			ret.successes++;
		ret.trials++;
		//Debug.Log("Log: " + ret.ToString());

		lockout = false;
	}


	public IEnumerator GetSpread(System.Action<int[]> callback) {

		while (lockout)
			yield return null;
		lockout = true;

		if (population == null) {
			population = new List<GeneSequence>();
			//Generate initial spread
			while (population.Count < initPopSize) {
				//Randomly generate one
				int[] spread = new int[geneSize];
				//Fill with random values
				for (int i = 0; i < spread.Length; i++) {
					spread[i] = Random.Range(0,11);
				}
				//Check if it is the same as one already in there, if so, ditch it.
				bool isDuplicate = false;
				foreach (GeneSequence sequence in population) {
					if (isDuplicate == false)
						isDuplicate = sequence.Compare((GeneSequence)spread);
				}
				if (isDuplicate == false) {
					population.Add((GeneSequence)spread);
				}
			}
		}
		GeneSequence ret = population.Find((GeneSequence gs) => gs.trials < trials);
		if (ret == null) {
			Debug.LogError("Trial Wave Complete.");
			//Sort the old population by how well they performed
			population.Sort((GeneSequence a, GeneSequence b) => {
				float aVal = (float)a.successes / (float)a.trials;
				float bVal = (float)b.successes / (float)b.trials;
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
			float lowest = population[0].successes / (float)population[0].trials;
			float highest = population[population.Count - 1].successes / (float)population[population.Count - 1].trials;
			population.Reverse();
			for (int i = 0; i < population.Count; i++) {
				Debug.Log("C Result: " + population[i].ToString());
			}
			if (Mathf.Abs(highest - lowest) < convergenceThreshold) {

				//We have converged
				Debug.LogError("Convergence.");
				Debug.Break();
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
							//Get a random position
							int pos = Random.Range(0, childArray.Length);
							//Shift it up or down
							if (Random.value < 0.5f) childArray[pos]++; else childArray[pos]--;
						}
						population.Add((GeneSequence)childArray);
					}
				}
			}

			for (int i = 0; i < population.Count; i++) {
				population[i].trials = 0;
				population[i].successes = 0;
			}

			//select the first thing
			ret = population[0];
		}
		//Select an sequence with not enough trials.
		callback(ret.genes);
		lockout = false;
	}


	class GeneSequence {
		public int trials = 0;
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
			return ((float)successes/(float)trials) + ":" + (new PointSpread(genes)).ToString();
		}

		//Allow for an T[] to just convert to this.
		public static explicit operator GeneSequence(int[] d) => new GeneSequence(d);
	}
}
