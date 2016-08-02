using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class RoundOverview : MonoBehaviour
{
	public string myUsername, oppUsername;
	public Dictionary<string, object> matchParameters;
	string label;
	// Use this for initialization
	void Start()
	{
		label = myUsername + " vs. " + oppUsername + "\n Lives = " + matchParameters ["Lives"];
		Invoke("EnterGame", 5f);
	}

	void EnterGame()
	{
		Application.LoadLevel("Game");
	}

	// Update is called once per frame
	void OnGUI()
	{
		GUI.Label(new Rect(400, 400, 100, 100), label);
	}
}
