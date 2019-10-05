using System.Collections;
using System.Collections.Generic;

public class DynamicContainer {
	
	/// <summary>
	/// The internal storage for this DynamicContainer
	/// </summary>
	private Dictionary<System.Type, Dictionary<string, System.Object>> internalData = new Dictionary<System.Type, Dictionary<string, object>>();

	/// <summary>
	/// Sets a value into internal storage via a type and key
	/// </summary>
	/// <param name="key"></param>
	/// <param name="value"></param>
	public void SetData<T>(string key, T value) {
		if (internalData.ContainsKey(typeof(T)) == false) {
			internalData.Add(typeof(T), new Dictionary<string,object>());
		}
		internalData[typeof(T)][key] = (object)value;
	}

	/// <summary>
	/// Access a value from storage via a type and key
	/// </summary>
	/// <param name="key"></param>
	/// <returns></returns>
	public T GetData<T>(string key) {
		if (typeof(T) == null)
			return default;

		if (internalData.ContainsKey(typeof(T)) == false) {
			return default;
		}
		else {
			Dictionary<string, object> foundDict = internalData[typeof(T)];
			if (foundDict.ContainsKey(key) == false) {
				return default;
			}
			else {
				return (T)foundDict[key];
			}
		}
	}

	/// <summary>
	/// Checks if this container is storing this type of information with that key
	/// </summary>
	/// <typeparam name="T"></typeparam>
	/// <param name="key"></param>
	/// <returns></returns>
	public bool HasKey<T>(string key) {
		if (typeof(T) == null)
			return false;

		if (internalData.ContainsKey(typeof(T)) == false) {
			return false;
		} else {
			Dictionary<string, object> foundDict = internalData[typeof(T)];
			if (foundDict.ContainsKey(key) == false) {
				return false;
			} else {
				return true;
			}
		}
	}

	public bool RemoveData<T>(string key) {
		if (HasKey<T>(key)) {
			internalData[typeof(T)].Remove(key);
			return true;
		}
		else
			return false;
	}

	public override string ToString() {
		string toPrint = "";
		foreach (System.Type type in internalData.Keys) {
			toPrint += type.ToString() + ":";
			foreach (string name in internalData[type].Keys) {
				toPrint += "("+name + "), ";
			}
			toPrint += "\n";
		}

		return toPrint;
	}

	public void Reset() {
		internalData = new Dictionary<System.Type, Dictionary<string, object>>();
	}
}