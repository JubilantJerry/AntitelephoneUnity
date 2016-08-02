using UnityEngine;
using System.Collections;

public class AIScript
{
	public delegate void MoveNotification(WorldData.MoveData move);
	public static event MoveNotification AIMoveCreatedEvent;

	public static void subscribeToUpdates()
	{
		WorldData.worldUpdate += makeMove;
	}

	static void makeMove()
	{
		if (AIMoveCreatedEvent != null) {
			if ((int)(Random.value * 2) == 0) {
				AIMoveCreatedEvent(new WorldData.MoveData(WorldData.Facility.REDIRECTOR));
			}
			else {
				AIMoveCreatedEvent(new WorldData.MoveData(WorldData.Facility.POWER));
			}
		}
	}
}
