using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

/// <summary>
///     The first parameter is the sender, 
///     The second parameter is the arguments / info to pass
/// </summary>
using LinkedEvent = System.Action<System.Object, System.Object>;

/// <summary>
/// The SenderTable maps from an object (sender of a notification), 
/// to a List of linkedEvent methods
///     * Note - When no sender is specified for the SenderTable, 
///         the NotificationCenter itself is used as the sender key
/// aka Dictionary<Object_To_Listen_For_Notifications_From, List<linkedEvent>>
/// </summary>
using SourceToEventsMapping = System.Collections.Generic.Dictionary<System.Object, System.Collections.Generic.List<System.Action<System.Object, System.Object>>>;

public class NotificationCenter {
	#region Properties
	/// <summary>
	/// The dictionary "key" (string) represents a notification string to be observed
	/// The dictionary "value" (SenderTable) maps between sender and observer sub tables
	/// [NOTE!] The totality of this table is a notification string -> Sender obj -> List of relevant events
	///			Sender is used to differentiate between listening to specific objects or listening to all objects.
	///			The latter case uses this object as a placeholder in this table.
	/// </summary>
	private Dictionary<string, SourceToEventsMapping> _table = new Dictionary<string, SourceToEventsMapping>();
	private HashSet<List<LinkedEvent>> _invoking = new HashSet<List<LinkedEvent>>();    // Probably is concurrency protection????????????????
	#endregion

	#region Singleton Pattern
	public readonly static NotificationCenter instance = new NotificationCenter();
	private NotificationCenter() { }
	#endregion

	#region Public

	// AddObserver (method_name_no_brackets, notificationName, particular_notification_sender_to_look_for)
	/// <summary>
	/// Add an observer looking for a particular notificationName, and potentialy a particular notification sender.
	/// When receives a notification and sender that match it requirements, it calls the given method
	/// </summary>
	/// <param name="linkedEvent"></param>
	/// <param name="notificationName"></param>
	/// <param name="observationTarget"></param>
	public void AddObserver(LinkedEvent linkedEvent, string notificationName, System.Object observationTarget = null) {
		if (linkedEvent == null) {    // needs to have a method to call

			Debug.LogError("Can't add a null event (linkedEvent) for notification, " + notificationName);
			return;
		}

		if (string.IsNullOrEmpty(notificationName)) { // needs an associated notification name

			Debug.LogError("Can't observe an unnamed notification");
			return;
		}

		if (!_table.ContainsKey(notificationName))  //if we do not have a dictionary for this notification string yet, make one.
			_table.Add(notificationName, new SourceToEventsMapping());

		SourceToEventsMapping subTable = _table[notificationName];    //Get the dictionary for all events registered to this notification

		System.Object key = observationTarget ?? (this);   // use a specific observation target or  this for general listening

		// if subtable(SenderTable, associated w/ given notificationName) does not contain a list of linkedEvents 
		// associated w/ the given key(sender/NotificationCenter.instance), add one
		if (!subTable.ContainsKey(key))
			subTable.Add(key, new List<LinkedEvent>());

		// get reference to list of linkedEvents, that is in the subtable(SenderTable) that is associated w/ given notificationName
		List<LinkedEvent> list = subTable[key];
		if (!list.Contains(linkedEvent)) {   // adds the linkedEvent(method), if it does not exist

			if (_invoking.Contains(list))   // does something????????????
				subTable[key] = list = new List<LinkedEvent>(list);

			list.Add(linkedEvent);
		}
	}


	/// <summary>
	/// removes observer with given attributes
	/// </summary>
	/// <param name="linkedEvent"></param>
	/// <param name="notificationName"></param>
	/// <param name="observationTarget"></param>
	public void RemoveObserver(LinkedEvent linkedEvent, string notificationName, System.Object observationTarget = null) {
		if (linkedEvent == null) {
			Debug.LogError("Can't remove a null event linkedEvent for notification, " + notificationName);
			return;
		}

		if (string.IsNullOrEmpty(notificationName)) {
			Debug.LogError("A notification name is required to stop observation");
			return;
		}

		// No need to take action if we dont monitor this notification
		if (_table.ContainsKey(notificationName) == false)
			return;

		SourceToEventsMapping subTable = _table[notificationName];
		System.Object key = observationTarget ?? (this);
		if (!subTable.ContainsKey(key))
			return;

		List<LinkedEvent> list = subTable[key];
		int index = list.IndexOf(linkedEvent);
		if (index != -1) {
			if (_invoking.Contains(list))
				subTable[key] = list = new List<LinkedEvent>(list);
			list.RemoveAt(index);
		}
	}


	/// <summary>
	/// Post notification w/ given notificationName, sender, and method arguments
	/// </summary>
	/// <param name="notificationName"></param>
	/// <param name="sender"></param>
	/// <param name="arg"></param>
	public void PostNotification(string notificationName, System.Object sender = null, System.Object arg = null) {
		//Debug.Log("Posting notification: \"" + notificationName + "\"");


		if (string.IsNullOrEmpty(notificationName)) {
			Debug.LogError("A notification name is required");
			return;
		}

		// No need to take action if we dont monitor this notification
		if (!_table.ContainsKey(notificationName))
			return;

		// Post to subscribers who specified a sender to observe
		SourceToEventsMapping subTable = _table[notificationName];
		if (sender != null && subTable.ContainsKey(sender)) {
			List<LinkedEvent> linkedEvents = subTable[sender];
			_invoking.Add(linkedEvents);
			for (int i = 0; i < linkedEvents.Count; ++i)
				linkedEvents[i](sender, arg);
			_invoking.Remove(linkedEvents);
		}

		// Post to subscribers who did not specify a sender to observe
		if (subTable.ContainsKey(this)) {
			List<LinkedEvent> linkedEvents = subTable[this];
			_invoking.Add(linkedEvents);
			for (int i = 0; i < linkedEvents.Count; ++i)
				linkedEvents[i](sender, arg);
			_invoking.Remove(linkedEvents);
		}
	}

	/// <summary>
	/// Sanitizes the Notification center.
	/// </summary>
	public void Clean() {
		string[] notKeys = new string[_table.Keys.Count];
		_table.Keys.CopyTo(notKeys, 0);

		for (int i = notKeys.Length - 1; i >= 0; --i) {
			string notificationName = notKeys[i];
			SourceToEventsMapping senderTable = _table[notificationName];

			object[] senKeys = new object[senderTable.Keys.Count];
			senderTable.Keys.CopyTo(senKeys, 0);

			for (int j = senKeys.Length - 1; j >= 0; --j) {
				object sender = senKeys[j];
				List<LinkedEvent> linkedEvents = senderTable[sender];
				if (linkedEvents.Count == 0)
					senderTable.Remove(sender);
			}

			if (senderTable.Count == 0)
				_table.Remove(notificationName);
		}
	}

	/// <summary>
	/// Yields a coroutine until a message is recieved by the Notification center
	/// </summary>
	/// <param name="message"></param>
	/// <returns></returns>
	public IEnumerator<object> WaitForMessage(string message, System.Object observationTarget = null) {
		bool messageFlag = false;
		LinkedEvent callback = (object sender, object args) => { messageFlag = true; };
		AddObserver(callback, message, observationTarget);
		while (messageFlag == false)
			yield return null;

		RemoveObserver(callback, message, observationTarget);
	}

	/// <summary>
	/// Returns a list of keys currently resting inside the notification center. (Intended for debugging purposes)
	/// </summary>
	/// <returns></returns>
	public List<string> GetNotificationKeys() {
		Clean();
		List<string> keys = new List<string>();
		foreach (string itm in _table.Keys) {
			//if (_table[itm].Count > 0)
			keys.Add(itm);
		}
		return keys;
	}
	#endregion
}