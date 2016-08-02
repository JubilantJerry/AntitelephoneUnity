using UnityEngine;
using System.Collections;

public class SimulationInput : MonoBehaviour
{
	public delegate void moveNotification(WorldData.MoveData move);
	public static event moveNotification moveMadeEvent;
	int energyUse = 0;
	int sign = 1;
	WorldData.Facility facility = WorldData.Facility.SERVER;

	void Start()
	{
	}

	void OnGUI()
	{
		GUI.Label(new Rect(400, 250, 200, 300), string.Concat(facility.ToString(), ", ", energyUse * sign));
	}

	// Temporary input
	void Update()
	{
		if (Input.GetKeyDown(KeyCode.Alpha0)) {
			makeMove(new WorldData.MoveData(facility, energyUse * sign, 0));
		}
		else if (Input.GetKeyDown(KeyCode.Alpha1)) {
			facility = WorldData.Facility.SERVER;
		}
		else if (Input.GetKeyDown(KeyCode.Alpha2)) {
			facility = WorldData.Facility.POWER;
		}
		else if (Input.GetKeyDown(KeyCode.Alpha3)) {
			facility = WorldData.Facility.REDIRECTOR;
		}
		else if (Input.GetKeyDown(KeyCode.Alpha4)) {
			facility = WorldData.Facility.SEISMOMETER;
		}
		else if (Input.GetKeyDown(KeyCode.Minus)) {
			sign = -1;
		}
		else if (Input.GetKeyDown(KeyCode.Equals)) {
			sign = 1;
		}
		else if (Input.GetKeyDown(KeyCode.Q)) {
			energyUse = 0;
		}
		else if (Input.GetKeyDown(KeyCode.W)) {
			energyUse = 1;
		}
		else if (Input.GetKeyDown(KeyCode.E)) {
			energyUse = 2;
		}
		else if (Input.GetKeyDown(KeyCode.R)) {
			energyUse = 10;
		}
	}

	void makeMove(WorldData.MoveData move)
	{
		if (moveMadeEvent != null) {
			moveMadeEvent(move);
		}
	}

}
