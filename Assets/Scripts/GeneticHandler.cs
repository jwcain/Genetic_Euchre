using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class GeneticHandler<T> {





	class GeneSequence {
		T[] genes;
		public GeneSequence(T[] genes) {
			this.genes = genes;
		}

		public T this[int i] {
			get {
				return genes[i];
			}
			set {
				genes[i] = value;
			}
		}

		public void swap(int a, int b) {
			T temp = genes[a];
			genes[a] = genes[b];
			genes[b] = temp;
		}

		//Allow for an T[] to just convert to this.
		public static explicit operator GeneSequence(T[] d) => new GeneSequence(d);
	}
}
