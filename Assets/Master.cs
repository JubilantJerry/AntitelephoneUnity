using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Master : MonoBehaviour
	//Stores & retrieves global persistent data and keeps a debug log
{
	public static Master instance;

	public Dictionary<string, object> matchParameters;
	public bool multiplayer;
	public string username;
	public string roomName;

	bool debugOn = true;
	Rect bounds = new Rect(10, 10, 300, 600);
	string debugText = "";

	public static Master getInstance()
	{
		if (instance == null) {
			instance = GameObject.FindWithTag("Master").GetComponent<Master>();
			instance.initiate();
		}
		return instance;
	}

	void Start()
	{
		getInstance();
	}
		
	void initiate()
	{
		Application.runInBackground = true;
		DontDestroyOnLoad(this.gameObject);

		username = PlayerPrefs.GetString("username");
		Log("Player Prefs loaded, username is " + username);
	}

	void OnApplicationQuit()
	{
		PlayerPrefs.SetString("username", username);
		Log("Player Prefs has been set to " + PlayerPrefs.GetString("username"));
	}

	void OnGUI()
	{
		if (debugOn) {
			GUI.Label(bounds, debugText);
		}
	}

	public void Log(string msg)
	{
		Debug.Log(msg);
		debugText = msg + "\n" + debugText;
	}

	public void LogWarning(string msg)
	{
		Debug.LogWarning(msg);
		debugText = "!!" + msg + "\n" + debugText;
	}

}
