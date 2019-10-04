using UnityEngine;
using System;
using System.Collections;
/// <summary>
/// An alias for Action<Object, Object>, where the arguments are sender and args
/// </summary>
using LinkedEvent = System.Action<System.Object, System.Object>;

public static class NotificationExtensions {
	public static void PostNotification (this object sender, string notificationName) {
		NotificationCenter.instance.PostNotification(notificationName, sender);
	}
	
	public static void PostNotification (this object sender, string notificationName, object args) {
		NotificationCenter.instance.PostNotification(notificationName, sender, args);
	}
	



	public static void AddObserver (this object observingObj, LinkedEvent linkedEvent, string notificationName) {
		NotificationCenter.instance.AddObserver(linkedEvent, notificationName);
	}
	
	public static void AddObserver (this object observingObj, LinkedEvent linkedEvent, string notificationName, object observationTarget) {
		NotificationCenter.instance.AddObserver(linkedEvent, notificationName, observationTarget);
	}
	



	public static void RemoveObserver (this object observingObj, LinkedEvent linkedEvent, string notificationName) {
		NotificationCenter.instance.RemoveObserver(linkedEvent, notificationName);
	}
	
	public static void RemoveObserver (this object observingObj, LinkedEvent linkedEvent, string notificationName, System.Object observationTarget) {
		NotificationCenter.instance.RemoveObserver(linkedEvent, notificationName, observationTarget);
	}
}