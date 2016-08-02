using UnityEngine;
using System.Collections;

public class SimulationGUI : MonoBehaviour
{
	string label1, label2;
	WorldData worldData;

	// Use this for initialization
	void Start()
	{
		label1 = null;
		label2 = null;
		WorldData.worldUpdate += onWorldUpdate;
		worldData = GameObject.FindGameObjectWithTag("GameWorld").GetComponent<WorldData>();
	}
	
	// Update is called once per frame
	void OnGUI()
	{
		GUI.Label(new Rect(300, 350, 200, 300), label1);
		GUI.Label(new Rect(500, 350, 200, 300), label2);
	}

	void onWorldUpdate()
	{
		label1 = worldData.currentNode.playerWorld.ToString();
		label2 = worldData.currentNode.oppWorld.ToString();
	}
}
