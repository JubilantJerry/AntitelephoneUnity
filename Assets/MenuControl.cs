using UnityEngine;
using System.Collections;

public class MenuControl : MonoBehaviour
{
	// Deals with menu GUI, App42, and initialization behavior

	public enum MenuMode
	{
		INIT,
		PLAY,
		STORY,
		OPTIONS,
		INPUTNAME
	}
	; 

	public MenuMode mode;

	// Use this for initialization
	void Start()
	{
		Master.getInstance().Log("Menu scene starts");
		if (string.IsNullOrEmpty(Master.getInstance().username)) {
			mode = MenuMode.INPUTNAME;
		}
		else {
			mode = MenuMode.INIT;
		}
	}
	void Update()
	{
		int choice = 0;
		if (Input.GetKeyDown(KeyCode.Alpha1)) {
			choice = 1;
		}
		else if (Input.GetKeyDown(KeyCode.Alpha2)) {
			choice = 2;
		}
		else if (Input.GetKeyDown(KeyCode.Alpha3)) {
			choice = 3;
		}

		switch (mode) {
			case MenuMode.INIT:
				switch (choice) {
					case 1:
						mode = MenuMode.PLAY;
						break;
					case 2:
						mode = MenuMode.STORY;
						break;
					case 3:
						mode = MenuMode.OPTIONS;
						break;
				}
				break;
			case MenuMode.PLAY:
				switch (choice) {
					case 1:
						enterGame(false);
						break;
					case 2:
						enterGame(true);
						break;
					case 3:
						mode = MenuMode.INIT;
						break;
				}
				break;
			case MenuMode.STORY:
				switch (choice) {
					case 1:
						mode = MenuMode.INIT;
						break;
				}
				break;
			case MenuMode.OPTIONS:
				switch (choice) {
					case 1:
						mode = MenuMode.INPUTNAME;
						break;
					case 2:
						mode = MenuMode.INIT;
						break;
				}
				break;
			case MenuMode.INPUTNAME:
				switch (choice) {
					case 1:
						Master.getInstance().username = "debugUser1";
						Master.getInstance().Log("Master object username set to debugUser1");
						mode = MenuMode.INIT;
						break;
					case 2:
						Master.getInstance().username = "debugUser2";
						Master.getInstance().Log("Master object username set to debugUser2");
						mode = MenuMode.INIT;
						break;
				}
				break;
		}
	}

	void OnGUI()
	{
		string text = "";
		switch (mode) {
			case MenuMode.INIT:
				text = "1: Play\n2: Story Mode\n3: Options";
				break;
			case MenuMode.PLAY:
				text = "1: VS AI\n2. Multiplayer\n3. Back";
				break;
			case MenuMode.STORY:
				text = "1. Back";
				break;
			case MenuMode.OPTIONS:
				text = "1. Log in\n3. Back";
				break;
			case MenuMode.INPUTNAME:
				text = "1. Log in as debugUser1\n2. Log in as debugUser2";
				break;

		}
		GUI.Label(new Rect(400, 10, 300, 600), text);

	}

	void enterGame(bool multiplayer)
	{
		Master.getInstance().multiplayer = multiplayer;
		Application.LoadLevel("LevelSelect");
	}

}
